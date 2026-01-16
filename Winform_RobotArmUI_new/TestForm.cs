using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Winform_RobotArmUI_new
{
    public partial class TestForm : Form
    {
        // 설정값 
        private float overlap = 10f;
        private float L1 => 120f - (overlap * 2);
        private float L2 => 120f - (overlap * 2);
        private float L3 => 120f - overlap;

        private PointF root = new PointF(400, 300);
        private PointF def; // 마우스 위치 기록용

        // 리소스 이미지 
        private Image imgLink1 = Properties.Resources.Robot_Link1;
        private Image imgLink2 = Properties.Resources.Robot_Link2;
        private Image imgLink3 = Properties.Resources.Robot_Link3;

        public TestForm()
        {
            this.DoubleBuffered = true;
            this.Size = new Size(800, 600);
            this.BackColor = Color.Gray;
        }

        private float GetDistance(PointF a, PointF b)
        {
            return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        private PointF GetPointAtDistance(PointF start, PointF target, float dist)
        {
            float angle = (float)Math.Atan2(target.Y - start.Y, target.X - start.X);
            return new PointF(
                start.X + (float)Math.Cos(angle) * dist,
                start.Y + (float)Math.Sin(angle) * dist
            );
        }

        private PointF prevElbowPos = PointF.Empty;

        private PointF CalculateElbowPos(PointF p1, PointF p2)
        {
            float d = GetDistance(p1, p2);
            float dClamped = Math.Max(Math.Abs(L1 - L2) + 0.1f, Math.Min(d, L1 + L2 - 0.1f));

            float cosA = (L1 * L1 + dClamped * dClamped - L2 * L2) / (2 * L1 * dClamped);
            float alpha = (float)Math.Acos(Math.Max(-1, Math.Min(1, cosA)));

            float baseAngle = (float)Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);

            // 두 후보 해
            PointF elbow1 = new PointF(
                p1.X + (float)Math.Cos(baseAngle + alpha) * L1,
                p1.Y + (float)Math.Sin(baseAngle + alpha) * L1
            );
            PointF elbow2 = new PointF(
                p1.X + (float)Math.Cos(baseAngle - alpha) * L1,
                p1.Y + (float)Math.Sin(baseAngle - alpha) * L1
            );

            // 이전 프레임과의 연속성 유지
            PointF chosen;
            if (prevElbowPos == PointF.Empty)
                chosen = elbow2; // 초기값
            else
            {
                float dist1 = GetDistance(prevElbowPos, elbow1);
                float dist2 = GetDistance(prevElbowPos, elbow2);
                chosen = (dist1 < dist2) ? elbow1 : elbow2;
            }

            prevElbowPos = chosen;
            return chosen;
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (def == PointF.Empty) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 1. 목표 지점 제한 (최대 사거리)
            float maxReach = L1 + L2 + L3;
            PointF finalTarget = (GetDistance(root, def) > maxReach)
                ? GetPointAtDistance(root, def, maxReach) : def;

            // 2. 손목 위치 결정 (Atan2는 분모가 0인 경우를 처리하므로 체크 불필요)
            float dx = finalTarget.X - root.X;
            float dy = finalTarget.Y - root.Y;
            float angToTarget = (float)Math.Atan2(dy, dx);

            PointF wristPos = new PointF(
                finalTarget.X - (float)Math.Cos(angToTarget) * L3,
                finalTarget.Y - (float)Math.Sin(angToTarget) * L3
            );

            // 3. 손목-루트 최소 거리 유지 (팔이 꼬이는 것 방지)
            float minWristDist = Math.Abs(L1 - L2) + 2.0f;
            if (GetDistance(root, wristPos) < minWristDist)
                wristPos = GetPointAtDistance(root, wristPos, minWristDist);

            // 4. 엘보우 위치 계산
            PointF elbowPos = CalculateElbowPos(root, wristPos);

            // 5. 각도 추출
            float a1 = (float)Math.Atan2(elbowPos.Y - root.Y, elbowPos.X - root.X);
            float a2 = (float)Math.Atan2(wristPos.Y - elbowPos.Y, wristPos.X - elbowPos.X);
            float a3 = (float)Math.Atan2(finalTarget.Y - wristPos.Y, finalTarget.X - wristPos.X);

            // 6. 그리기
            DrawLink(g, imgLink1, root, a1, L1, false);
            DrawLink(g, imgLink2, elbowPos, a2, L2, false);
            DrawLink(g, imgLink3, wristPos, a3, L3, true);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            def = e.Location;
            this.Invalidate(); // 화면 갱신
        }


        private void DrawLink(Graphics g, Image img, PointF pos, float rad, float len, bool isLast)
        {
            if (img == null) return;
            GraphicsState state = g.Save();

            g.TranslateTransform(pos.X, pos.Y);
            g.RotateTransform(rad * (180f / (float)Math.PI));

            float drawW = isLast ? len + overlap : len + (overlap * 2);
            float drawH = drawW * ((float)img.Height / img.Width);

            // 관절 위치가 정확히 겹치도록 -overlap 지점부터 그림
            g.DrawImage(img, -overlap, -drawH / 2, drawW, drawH);

            g.Restore(state);
        }
    }
}