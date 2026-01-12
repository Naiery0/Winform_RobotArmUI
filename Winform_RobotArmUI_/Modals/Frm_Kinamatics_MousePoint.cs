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
        private PointF posStart = new PointF(400, 300);
        private PointF posDef, posAnkle;
        private float stoA = 150f, atoD = 150f;
        private bool flipDirection = false;

        // 이미지 변수
        private Image imgBone1;
        private Image imgBone2;

        public Frm_Kinamatics_MousePoint()
        {
            this.DoubleBuffered = true;
            this.Size = new Size(800, 600);

            // 이미지 로드 (파일 경로에 맞게 수정하세요)
            imgBone1 = Properties.Resources.Robot_Link1;
            imgBone2 = Properties.Resources.Robot_Link2;
            if (imgBone1 == null || imgBone2 == null)
            {
                MessageBox.Show("리소스를 찾았으나 이미지가 비어있습니다.");
            }
            // 테스트용 임시 비트맵 생성 (이미지가 없을 경우 대비)
            //imgBone1 = CreatePlaceholderImage(Color.Gray, (int)stoA, 20);
            //imgBone2 = CreatePlaceholderImage(Color.DarkGray, (int)atoD, 20);

            this.MouseDown += (s, e) => { if (e.Button == MouseButtons.Right) flipDirection = !flipDirection; };
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

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (posAnkle == PointF.Empty) return;

            // 각도 계산
            float angle1 = (float)Math.Atan2(posAnkle.Y - posStart.Y, posAnkle.X - posStart.X);
            float angle2 = (float)Math.Atan2(posDef.Y - posAnkle.Y, posDef.X - posAnkle.X);
            float offset = 0;
            //float offset = (float)(Math.PI / 2);

            // 이미지 그리기
            DrawRotatedImage(g, imgBone1, posStart, angle1 - offset, stoA); // 첫 번째 뼈대
            DrawRotatedImage(g, imgBone2, posAnkle, angle2 - offset, atoD); // 두 번째 뼈대

            // 관절 포인트(선택 사항)
            g.FillEllipse(Brushes.Red, posAnkle.X - 5, posAnkle.Y - 5, 10, 10);
        }

        // --- 이하 로직은 이전과 동일 (CalculateAnklePos, OnMouseMove 등) ---

        protected override void OnMouseMove(MouseEventArgs e)
        {
            posDef = e.Location;
            posAnkle = CalculateAnklePos(posStart, posDef);
            this.Invalidate();
        }

        private PointF CalculateAnklePos(PointF p1, PointF p2)
        {
            float length = (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
            if (length >= stoA + atoD)
            {
                float angle = (float)Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
                return new PointF(p1.X + (float)Math.Cos(angle) * stoA, p1.Y + (float)Math.Sin(angle) * stoA);
            }
            float cosAngle = (stoA * stoA + length * length - atoD * atoD) / (2 * stoA * length);
            float angleA = (float)Math.Acos(Math.Max(-1, Math.Min(1, cosAngle)));
            float baseAngle = (float)Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
            float finalAngle = flipDirection ? (baseAngle - angleA) : (baseAngle + angleA);
            return new PointF(p1.X + (float)Math.Cos(finalAngle) * stoA, p1.Y + (float)Math.Sin(finalAngle) * stoA);
        }

        private Image CreatePlaceholderImage(Color color, int w, int h)
        {
            Bitmap bmp = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(bmp)) g.Clear(color);
            return bmp;
        }
    }
}
