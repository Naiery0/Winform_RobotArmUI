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
using Winform_RobotArmUI_.Modals;

namespace Winform_RobotArmUI_
{
    public partial class MainForm : Form
    {
        // 위치 관련
        private PointF root;
        private PointF def, ankle;
        private float stoA = 100f, atoD = 100f; // 링크 길이

        private Image imgLink1 = Properties.Resources.Robot_Link1;
        private Image imgLink2 = Properties.Resources.Robot_Link2;

        private float middleX, middleY;

        private PointF pm1 = new PointF(84, 312);
        private PointF pm2 = new PointF(220, 105);
        private PointF pm3 = new PointF(466, 103);
        private PointF pm4 = new PointF(605, 312);
        private PointF ll1 = new PointF(264, 526);
        private PointF ll2 = new PointF(425, 532);

        public MainForm()
        {
            this.DoubleBuffered = true; // 깜빡임 방지 처리
            InitializeComponent();
        }

        private void btn_MousePoint_Click(object sender, EventArgs e)
        {
            Frm_Kinamatics_MousePoint frm = new Frm_Kinamatics_MousePoint();
            frm.Show();
        }

        private void pnl_MainPaint_MouseMove(object sender, MouseEventArgs e)
        {
            lbl_Point_X.Text = $"X: {e.X}";
            lbl_Point_Y.Text = $"Y: {e.Y}";
            def = e.Location;
            // 마우스 움직임에 따라 즉시 계산하고 싶다면 아래 추가
            if (root != PointF.Empty)
            {
                ankle = CalculateAnklePos(root, def);
                pnl_MainPaint.Invalidate(); // 폼 전체가 아닌 '패널'만 다시 그리도록 요청
            }
        }

        private void btn_NewArm_Click(object sender, EventArgs e)
        {
            root = new PointF(middleX, middleY);
            ankle = CalculateAnklePos(root, def);
            pnl_MainPaint.Invalidate();
        }

        private void pnl_MainPaint_MouseClick(object sender, MouseEventArgs e)
        {
            lbl_MiddelSpot.Text = $"중앙: {e.X}, {e.Y}";
            middleX = e.X;
            middleY = e.Y;
        }
        #region to module
        private void btn_Pm1_Click(object sender, EventArgs e) => RobotMove(pm1);
        private void btn_Pm2_Click(object sender, EventArgs e) => RobotMove(pm2);
        private void btn_Pm3_Click(object sender, EventArgs e) => RobotMove(pm3);

        private void btn_Pm4_Click(object sender, EventArgs e) => RobotMove(pm4);


        private void btn_Ll1_Click(object sender, EventArgs e) => RobotMove(ll1);

        private void btn_Ll2_Click(object sender, EventArgs e) => RobotMove(ll2);
        private void RobotMove(PointF def)
        {
            this.def = def;
            if (root == PointF.Empty)
                return;
            ankle = CalculateAnklePos(root, def);
            pnl_MainPaint.Invalidate();
        }
        private void straight(bool isPut)
        {

        }
        private void Rotation()
        {

        }


        #endregion

        private void pnl_MainPaint_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (ankle == PointF.Empty) return;

            // 각도 계산
            float angle1 = (float)Math.Atan2(ankle.Y - root.Y, ankle.X - root.X);
            float angle2 = (float)Math.Atan2(def.Y - ankle.Y, def.X - ankle.X);
            float offset = 0;
            //float offset = (float)(Math.PI / 2);

            // 이미지 그리기
            DrawRotatedImage(g, imgLink1, root, angle1 - offset, stoA); // 첫 번째 뼈대
            DrawRotatedImage(g, imgLink2, ankle, angle2 - offset, atoD); // 두 번째 뼈대

            // 관절 포인트(선택 사항)
            g.FillEllipse(Brushes.Red, ankle.X - 5, ankle.Y - 5, 10, 10);
        }


        #region 로봇팔 함수

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
            float finalAngle = baseAngle + angleA;
            return new PointF(p1.X + (float)Math.Cos(finalAngle) * stoA, p1.Y + (float)Math.Sin(finalAngle) * stoA);
        }
        #endregion
    }
}
