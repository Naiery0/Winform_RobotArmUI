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
        private PointF def, ankle;
        private float stoA = 90f, atoD = 110f; // 링크 길이

        // image
        private Image imgRoot = Properties.Resources.Robot_Root;
        private Image imgAnkle = Properties.Resources.Robot_Ankle;
        private Image imgLink1 = Properties.Resources.Robot_Link1;
        private Image imgLink2 = Properties.Resources.Robot_Link2;

        // 중앙점
        private PointF root = new PointF(345, 309);
        //private float middleX = 345; 
        //private float middleY = 309;

        private BackgroundWorker mover = new BackgroundWorker();
        private PointF targetDef;     // 최종 목표점
        private PointF currentDef;    // 현재 로봇팔 끝 위치
        private bool isMoving = false;
        private bool isFolded = true;
        private string moveMode = "ARC";

        private PointF pm1 = new PointF(84, 312);
        private PointF pm2 = new PointF(220, 105);
        private PointF pm3 = new PointF(466, 103);
        private PointF pm4 = new PointF(605, 312);
        private PointF ll1 = new PointF(264, 526);
        private PointF ll2 = new PointF(425, 532);

        private float lockedAngle;

        public MainForm()
        {
            this.DoubleBuffered = true; // 깜빡임 방지 처리
            InitializeComponent();

            mover.WorkerReportsProgress = true;
            mover.DoWork += Mover_DoWork;
            mover.ProgressChanged += Mover_ProgressChanged;
            mover.RunWorkerCompleted += Mover_RunWorkerCompleted;
            ankle = CalculateAnklePos(root, def);
            pnl_MainPaint.Invalidate();
            btn_NewArm.Enabled = false;

            // 최초 실행 시 팔 접기
            RobotMove(ankle, "LINEAR");
        }
        #region button funtion
        private void btn_MousePoint_Click(object sender, EventArgs e)
        {
            Frm_Kinamatics_MousePoint frm = new Frm_Kinamatics_MousePoint();
            frm.Show();
        }

        private void pnl_MainPaint_MouseMove(object sender, MouseEventArgs e)
        {
            lbl_Point_X.Text = $"X: {e.X}";
            lbl_Point_Y.Text = $"Y: {e.Y}";

            // 마우스 움직임에 따라 즉시 계산하고 싶다면 아래 추가
            //def = e.Location;
            //if (root != PointF.Empty)
            //{
            //    ankle = CalculateAnklePos(root, def);
            //    pnl_MainPaint.Invalidate(); // 폼 전체가 아닌 '패널'만 다시 그리도록 요청
            //}
        }

        private void btn_NewArm_Click(object sender, EventArgs e)
        {
            //root = new PointF(middleX, middleY);
            ankle = CalculateAnklePos(root, def);
            pnl_MainPaint.Invalidate();
        }

        private void pnl_MainPaint_MouseClick(object sender, MouseEventArgs e)
        {
            lbl_MiddelSpot.Text = $"중앙: {e.X}, {e.Y}";
            //middleX = e.X;
            //middleY = e.Y;
        }
        private void btn_Pm1_Click(object sender, EventArgs e) => RobotMove(pm1, "ARC");
        private void btn_Pm2_Click(object sender, EventArgs e) => RobotMove(pm2, "ARC");
        private void btn_Pm3_Click(object sender, EventArgs e) => RobotMove(pm3, "ARC");

        private void btn_Pm4_Click(object sender, EventArgs e) => RobotMove(pm4, "ARC");


        private void btn_Ll1_Click(object sender, EventArgs e) => RobotMove(ll1, "ARC");

        private void btn_Ll2_Click(object sender, EventArgs e) => RobotMove(ll2, "ARC");
        private void btn_Fold_Click(object sender, EventArgs e) // btn fold
        {
            if (ankle == PointF.Empty) return;

            // 접기 직전의 정확한 방향을 저장
            lockedAngle = (float)Math.Atan2(def.Y - root.Y, def.X - root.X);

            // end-effector(def)을 ankle 위치로 이동시키기
            isFolded = true;
            RobotMove(ankle, "LINEAR");
        }
        private void btn_Unfold_Click(object sender, EventArgs e)
        {
            if (root == PointF.Empty || isMoving) return;

            // 1. 현재 어깨에서 끝점이 바라보는 각도 계산
            // 만약 완전히 접혀서 root와 def가 겹쳐있다면 ankle 방향을 기준으로 함
            float dx = def.X - root.X;
            float dy = def.Y - root.Y;

            if (Distance(root, def) < 5f) // 너무 많이 접혀있을 경우 대비
            {
                dx = ankle.X - root.X;
                dy = ankle.Y - root.Y;
            }

            float angle = (float)Math.Atan2(dy, dx);

            // 2. 최대 길이만큼 뻗은 좌표 계산 (stoA + atoD)
            // 약간의 여유를 위해 0.99를 곱해 완전히 직선이 되기 직전까지 펴지게 할 수도 있습니다.
            float maxLength = (stoA + atoD);
            float targetX = root.X + (float)Math.Cos(angle) * maxLength;
            float targetY = root.Y + (float)Math.Sin(angle) * maxLength;

            // 3. 해당 지점으로 이동 (직선으로 펴지게 하려면 LINEAR 사용)
            isFolded = false;
            RobotMove(new PointF(targetX, targetY), "LINEAR");
        }
        private void ButtonEnable(bool isFolded)
        {
            this.isFolded = isFolded;
            btn_Fold.Enabled = !isFolded;
            btn_Unfold.Enabled = isFolded;
            gb_Control.Enabled = isFolded;
        }
        #endregion

        private void RobotMove(PointF def, string mode)
        {
            //this.def = def;
            //if (root == PointF.Empty)
            //    return;
            //ankle = CalculateAnklePos(root, def);
            //pnl_MainPaint.Invalidate();

            if (root == PointF.Empty || isMoving)
                return;

            targetDef = def;
            currentDef = this.def;

            moveMode = mode;
            isMoving = true;
            mover.RunWorkerAsync();
        }
        private void Mover_DoWork(object sender, DoWorkEventArgs e)
        {
            float t = 0f;

            // 1. 기초 값 계산
            PointF startPoint = currentDef;
            float startAngle = (float)Math.Atan2(startPoint.Y - root.Y, startPoint.X - root.X);
            float targetAngle = (float)Math.Atan2(targetDef.Y - root.Y, targetDef.X - root.X);
            float startRadius = Distance(root, startPoint);

            if (moveMode is "ARC") // 루트에서 원하는 방향, 일정거리 이동 시 찍히는 좌표 
                targetDef = GetPointByDistance(targetDef, Distance(root, startPoint));

            float targetRadius = Distance(root, targetDef);

            // 2. 각도 차이 보정 및 절대량 계산
            float angleDiff = targetAngle - startAngle;
            while (angleDiff > Math.PI) angleDiff -= (float)(Math.PI * 2);
            while (angleDiff < -Math.PI) angleDiff += (float)(Math.PI * 2);

            float angleAmount = Math.Abs(angleDiff);
            float distAmount = Distance(startPoint, targetDef);

            // 3. 동적 속도(step) 계산 알고리즘 적용
            // 기본 속도 설정 (기존 0.02f 기준)
            float baseSpeed = 0.03f;

            // 이동량(각도 혹은 거리)이 클수록 step을 작게 만들어 오래 걸리게 함
            // ARC 모드일 때는 각도 중심, LINEAR일 때는 거리 중심으로 가중치 계산
            float weight = (moveMode == "ARC") ? angleAmount * 3f : distAmount / 100f;

            float dynamicStep = baseSpeed / (1f + weight);

            // 너무 느려지는 것 방지 (최소 속도 제한)
            dynamicStep = Math.Max(dynamicStep, baseSpeed * 0.2f);

            // 4. 애니메이션 루프
            while (t < 1.0f)
            {
                t += dynamicStep; // 계산된 동적 속도 적용
                if (t > 1.0f) t = 1.0f;

                switch (moveMode)
                {
                    case "LINEAR":
                        // 직선 보간
                        currentDef = new PointF(
                            startPoint.X + (targetDef.X - startPoint.X) * t,
                            startPoint.Y + (targetDef.Y - startPoint.Y) * t
                        );
                        break;

                    case "ARC":
                        // 각도와 반지름 보간
                        float curAngle = startAngle + angleDiff * t;
                        float curRadius = startRadius + (targetRadius - startRadius) * t;
                        currentDef = new PointF(
                            root.X + (float)Math.Cos(curAngle) * curRadius,
                            root.Y + (float)Math.Sin(curAngle) * curRadius
                        );
                        break;
                }
                mover.ReportProgress(0);
                System.Threading.Thread.Sleep(1);
            }
        }

        private void Mover_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.def = currentDef;
            ankle = CalculateAnklePos(root, def);
            pnl_MainPaint.Invalidate();
        }
        private void Mover_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.def = targetDef;
            ankle = CalculateAnklePos(root, def);
            pnl_MainPaint.Invalidate();

            isMoving = false;
            ButtonEnable(isFolded);
        }
        private float Distance(PointF a, PointF b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
        // 접은 채로 돌리기 위해 만듦
        private PointF GetPointByDistance(PointF target, float distance)
        {
            // 1. 두 점 사이의 각도 계산 (라디안)
            float dx = target.X - root.X;
            float dy = target.Y - root.Y;
            float angle = (float)Math.Atan2(dy, dx);

            // 2. 각도와 거리를 이용하여 새로운 X, Y 계산
            float newX = root.X + (float)Math.Cos(angle) * distance;
            float newY = root.Y + (float)Math.Sin(angle) * distance;

            return new PointF(newX, newY);
        }

        private PointF CalculateAnklePos(PointF p1, PointF p2)
        {
            float d = Distance(p1, p2);

            // 최대/최소 범위 제한
            if (d >= stoA + atoD) d = (stoA + atoD) - 0.01f;
            if (d <= Math.Abs(stoA - atoD)) d = Math.Abs(stoA - atoD) + 0.01f;

            float cosA = (stoA * stoA + d * d - atoD * atoD) / (2 * stoA * d);

            //  -1.0 ~ 1.0 사이로 강제 고정
            cosA = Math.Max(-1f, Math.Min(1f, cosA));

            float angleA = (float)Math.Acos(cosA);
            float baseAngle = (float)Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);

            return new PointF(p1.X + (float)Math.Cos(baseAngle + angleA) * stoA,
                             p1.Y + (float)Math.Sin(baseAngle + angleA) * stoA);
        }

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
            DrawRotatedImage(g, imgLink1, root, angle1 - offset, stoA); 
            DrawRotatedImage(g, imgLink2, ankle, angle2 - offset, atoD); 

            float jx = ankle.X - imgAnkle.Width / 7f;
            float jy = ankle.Y - imgAnkle.Height / 7f;
            g.DrawImage(imgAnkle, jx, jy);

            float baseX = root.X - imgRoot.Width / 7f;
            float baseY = root.Y - imgRoot.Height / 7f;
            g.DrawImage(imgRoot, baseX, baseY);
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

            // length가 너무 작으면 에러가 날 수 있으므로 최소값 1 설정
            float drawWidth = Math.Max(1f, length);
            g.DrawImage(img, 0, -img.Height / 2, drawWidth, img.Height);

            g.Restore(state);
        }

    }
}