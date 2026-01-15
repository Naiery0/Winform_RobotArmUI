using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Winform_RobotArmUI_.Modals
{
    public partial class Frm_Kinamatics_MousePoint : Form
    {
        private PointF root = new PointF(400, 300);
        private PointF def, ankle, ankle2;
        private float stoA = 90f, atoD = 110f;
        private bool flipDirection = false;
        
        // 두 링크 사이의 최소 각도 (이미지의 빨간색 표시 부분)
        private float minJointAngle = 30f;  // 예: 30도 미만으로 접히지 않음
                                            // 두 링크 사이의 최대 각도 (팔이 일자로 펴지는 정도 제한)
        private float maxJointAngle = 180f; // 예: 170도 이상 펴지지 않음 (완전 직선 방지)
        //private float minStopDistance = 80f;

        private float armOffsetDeg = 35f;
        // image
        private Image imgRoot = Properties.Resources.Robot_Root;
        private Image imgAnkle = Properties.Resources.Robot_Ankle;
        private Image imgLink1 = Properties.Resources.Robot_Link1;
        private Image imgLink2 = Properties.Resources.Robot_Link2;

        public Frm_Kinamatics_MousePoint()
        {
            this.DoubleBuffered = true;
            this.Size = new Size(800, 600);

            // 테스트용 임시 비트맵 생성 (이미지가 없을 경우 대비)
            //imgBone1 = CreatePlaceholderImage(Color.Gray, (int)stoA, 20);
            //imgBone2 = CreatePlaceholderImage(Color.DarkGray, (int)atoD, 20);
        }

        // 이미지를 특정 지점을 기준으로 회전시켜 그리는 핵심 함수
        private void DrawRotatedImage(Graphics g, Image img, PointF position, float angleInRad, float length)
        {
            // NaN 또는 무한대 값이 들어오면 그리지 않고 리턴
            if (float.IsNaN(angleInRad) || float.IsInfinity(angleInRad)) return;
            if (img == null) return;

            GraphicsState state = g.Save();

            g.TranslateTransform(position.X, position.Y);

            // 도 단위로 변환
            float angleInDeg = angleInRad * (180f / (float)Math.PI);
            g.RotateTransform(angleInDeg);

            // 3. 이미지 그리기
            // length가 너무 작으면 에러가 날 수 있으므로 최소값 1 설정
            float drawWidth = Math.Max(1f, length);
            g.DrawImage(img, 0, -img.Height / 2, drawWidth, img.Height);

            g.Restore(state);
        }

        // 2. 마우스 이동 시 두 팔의 좌표를 모두 계산하도록 수정
        protected override void OnMouseMove(MouseEventArgs e)
        {
            def = e.Location;

            // 첫 번째 팔 (현재 설정된 방향)
            ankle = CalculateAnklePos(root, def, flipDirection);

            // 두 번째 팔 (반대 방향 - flipDirection을 반전시켜 전달)
            ankle2 = CalculateAnklePos(root, def, !flipDirection);

            this.Invalidate();
        }

        // 4. OnPaint에서 두 번째 팔 그리기 추가
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (def == PointF.Empty) return;

            // 1. 공통 계산
            float dist = (float)Math.Sqrt(Math.Pow(def.X - root.X, 2) + Math.Pow(def.Y - root.Y, 2));
            float baseAngle = (float)Math.Atan2(def.Y - root.Y, def.X - root.X);
            float offsetRad = armOffsetDeg * (float)Math.PI / 180f;

            // 두 팔의 최대 길이를 합산 (90 + 110 = 200)
            float maxRange = stoA + atoD;

            // --- [첫 번째 팔: 마우스 거리를 그대로 추적] ---
            PointF target1 = CalculateTargetPoint(root, baseAngle, dist);
            PointF ankle1 = CalculateAnklePos(root, target1, flipDirection);

            float ang1_1 = (float)Math.Atan2(ankle1.Y - root.Y, ankle1.X - root.X);
            float ang1_2 = (float)Math.Atan2(target1.Y - ankle1.Y, target1.X - ankle1.X);

            DrawRotatedImage(g, imgLink1, root, ang1_1, stoA);
            DrawRotatedImage(g, imgLink2, ankle1, ang1_2, atoD);

            // --- [두 번째 팔: 거리 반전 로직] ---
            // 마우스가 멀어질수록(dist가 커질수록) targetDist2는 작아짐
            // 최소 거리 보장을 위해 Math.Max 사용 (0 이하로 내려가지 않게)
            float targetDist2 = Math.Max(10f, maxRange - dist + 50f);
            

            // 타겟 지점 계산 (각도는 유지하되 거리만 반전)
            PointF target2 = CalculateTargetPoint(root, baseAngle + offsetRad, targetDist2);
            PointF ankle2 = CalculateAnklePos(root, target2, !flipDirection);

            float ang2_1 = (float)Math.Atan2(ankle2.Y - root.Y, ankle2.X - root.X);
            float ang2_2 = (float)Math.Atan2(target2.Y - ankle2.Y, target2.X - ankle2.X);

            DrawRotatedImage(g, imgLink1, root, ang2_1, stoA);
            DrawRotatedImage(g, imgLink2, ankle2, ang2_2, atoD);

            // --- [조인트 그리기] ---
            DrawJoint(g, imgAnkle, ankle1);
            DrawJoint(g, imgAnkle, ankle2);
            DrawJoint(g, imgRoot, root);
        }

        private void DrawJoint(Graphics g, Image img, PointF pos)
        {
            if (img == null) return;
            float jx = pos.X - img.Width / 7f; // 기존 코드의 /7f 유지
            float jy = pos.Y - img.Height / 7f;
            g.DrawImage(img, jx, jy);
        }

        // 각도와 거리를 이용해 좌표를 구하는 보조 함수
        private PointF CalculateTargetPoint(PointF origin, float angleRad, float distance)
        {
            return new PointF(
                origin.X + (float)Math.Cos(angleRad) * distance,
                origin.Y + (float)Math.Sin(angleRad) * distance
            );
        }


        //// 3. 계산 함수 수정 (전역 변수 flipDirection 대신 파라미터를 받도록 변경)
        //private PointF CalculateAnklePos(PointF p1, PointF p2, bool isFlipped)
        //{
        //    float length = (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));

        //    if (length >= stoA + atoD)
        //    {
        //        float angle = (float)Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
        //        return new PointF(p1.X + (float)Math.Cos(angle) * stoA, p1.Y + (float)Math.Sin(angle) * stoA);
        //    }

        //    float cosAngle = (stoA * stoA + length * length - atoD * atoD) / (2 * stoA * length);
        //    float angleA = (float)Math.Acos(Math.Max(-1, Math.Min(1, cosAngle)));
        //    float baseAngle = (float)Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);

        //    // 파라미터로 받은 isFlipped에 따라 각도 방향 결정
        //    float finalAngle = isFlipped ? (baseAngle - angleA) : (baseAngle + angleA);

        //    return new PointF(p1.X + (float)Math.Cos(finalAngle) * stoA, p1.Y + (float)Math.Sin(finalAngle) * stoA);
        //}

        private PointF CalculateAnklePos(PointF p1, PointF p2, bool isFlipped = false)
        {
            float d = (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));

            // --- [관절 내부 각도 제한 로직] ---

            // 1. 최소 각도(minJointAngle)를 거리로 변환
            float minRad = minJointAngle * (float)Math.PI / 180f;
            float minDist = (float)Math.Sqrt(
                stoA * stoA + atoD * atoD - 2 * stoA * atoD * (float)Math.Cos(minRad)
            );

            // 2. 최대 각도(maxJointAngle)를 거리로 변환
            float maxRad = maxJointAngle * (float)Math.PI / 180f;
            float maxDist = (float)Math.Sqrt(
                stoA * stoA + atoD * atoD - 2 * stoA * atoD * (float)Math.Cos(maxRad)
            );

            // 3. 물리적 한계값(두 팔의 길이 차이 및 합)과 비교하여 최종 범위 결정
            float finalMin = Math.Max(minDist, Math.Abs(stoA - atoD));
            float finalMax = Math.Min(maxDist, stoA + atoD);

            // 4. 현재 거리 d를 제한 범위 내로 고정
            if (d < finalMin) d = finalMin + 0.01f;
            if (d > finalMax) d = finalMax - 0.01f;

            // --- [이후는 기존 역기구학 계산] ---
            float cosA = (stoA * stoA + d * d - atoD * atoD) / (2 * stoA * d);
            cosA = Math.Max(-1f, Math.Min(1f, cosA));

            float angleA = (float)Math.Acos(cosA);
            float baseAngle = (float)Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);

            float finalAngle = isFlipped ? (baseAngle - angleA) : (baseAngle + angleA);

            return new PointF(p1.X + (float)Math.Cos(finalAngle) * stoA,
                              p1.Y + (float)Math.Sin(finalAngle) * stoA);
        }


        private Image CreatePlaceholderImage(Color color, int w, int h)
        {
            Bitmap bmp = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(bmp)) g.Clear(color);
            return bmp;
        }
    }
}
