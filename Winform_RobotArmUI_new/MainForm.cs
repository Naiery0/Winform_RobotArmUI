using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;

namespace Winform_RobotArmUI_new
{
    public partial class MainForm : Form
    {
        private float overlap = 10f; // 팔 겹치는 정도
        private float L1 => 85f;    // 루트에서 시작
        private float L2 => 85f;
        private float L3 => 85f;

        private float loadAngle = 90f; // load 시 바라볼 각도

        // 속도 설정
        private float foldStrength = 2f;     // fold 강도 (낮을수록 강함)
        private float rotationSpeed = 0.05f; // ARC 모드 회전 속도
        private float straightSpeed = 4.0f;   // LINEAR 모드 이동 속도

        // 리소스 이미지
        private readonly Image imgRoot = Properties.Resources.Robot_Root;
        private readonly Image imgLink1 = Properties.Resources.Robot_Link1;
        private readonly Image imgLink2 = Properties.Resources.Robot_Link2;
        private readonly Image imgLink3 = Properties.Resources.Robot_Link3;
        //private readonly Image imgRoot = Properties.Resources.Robot_Root2;
        //private readonly Image imgLink1 = Properties.Resources.Robot_Link1_2;
        //private readonly Image imgLink2 = Properties.Resources.Robot_Link2_2;
        //private readonly Image imgLink3 = Properties.Resources.Robot_Link3_2;
        private float rootScale = 0.25f; // 이미지 크기
        private float rootAngle; // 루트 이미지 각도
        private float rootWidth /*= imgRoot.Width * 0.5*/;
        private float rootHeight /*= imgRoot.Height * 0.5*/;

        // 계산 필드
        private PointF root = new PointF(343, 330);
        private PointF root2 = new PointF(343, 330);

        private PointF def;                         // 팔 현재 끝점
        private PointF targetDef;                   // 팔 목표점
        private PointF currentDef;                  // Lower 팔 계산 중 위치
        private PointF prevElbowPos = PointF.Empty; // Lower 팔 이전 팔꿈치 위치
        private bool isFolded = true;               // 접었나?

        private PointF def2;      
        private PointF targetDef2; 
        private PointF currentDef2; 
        private PointF prevElbowPos2 = PointF.Empty;
        private bool isFolded2 = true;

        private readonly PointF pm1 = new PointF(81, 322);
        private readonly PointF pm2 = new PointF(222, 119);
        private readonly PointF pm3 = new PointF(472, 117);
        private readonly PointF pm4 = new PointF(607, 325);
        private readonly PointF ll1 = new PointF(263, 548);
        private readonly PointF ll2 = new PointF(426, 549);

        private PointF loadDef; // 로드시 끝점

        // 이동 방식
        private string moveMode = "ARC";

        // 두 개의 BackgroundWorker (Lower/Upper 팔 각각)
        private readonly BackgroundWorker mover = new BackgroundWorker();
        private readonly BackgroundWorker mover2 = new BackgroundWorker();

        private const float RAD_TO_DEG = 180f / (float)Math.PI;

        public MainForm()
        {
            rootWidth = imgRoot.Width * rootScale;
            rootHeight = imgRoot.Height * rootScale;
            rootAngle = loadAngle;

            this.DoubleBuffered = true;
            InitializeComponent();

            // Panel 더블 버퍼링 활성화
            typeof(Panel).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, pnl_MainPaint, new object[] { true });

            // 초기 끝점 설정
            float startRad = loadAngle * (float)Math.PI / 180f;
            loadDef = new PointF(
                root.X + (float)Math.Cos(startRad) * foldStrength,
                root.Y + (float)Math.Sin(startRad) * foldStrength
            );

            def = loadDef;
            def2 = loadDef;

            // BackgroundWorker 설정
            ConfigureMover(mover);
            ConfigureMover(mover2);

            // 시작 시 팔 접기
            isFolded = true;
            isFolded2 = true;
            FoldArm("Lower");
            FoldArm("Upper");
        }
        #region button
        private void ButtonEnable(string armName)
        {
            if (armName == "Lower")
            {
                btn_Fold.Enabled = !isFolded;
                btn_Unfold.Enabled = isFolded;
            }
            else
            {
                btn_Fold2.Enabled = !isFolded2;
                btn_Unfold2.Enabled = isFolded2;
            }

            // 두 팔 모두 접혀있어야 제어 가능
            gb_Control.Enabled = (isFolded && isFolded2);
        }

        private void btn_MousePoint_Click(object sender, EventArgs e)
        {
            new TestForm().Show();
        }
        
        private void btn_Pm1_Click(object sender, EventArgs e) => RobotMove(pm1, "ARC", "Lower");
        private void btn_Pm2_Click(object sender, EventArgs e) => RobotMove(pm2, "ARC", "Lower");
        private void btn_Pm3_Click(object sender, EventArgs e) => RobotMove(pm3, "ARC", "Lower");
        private void btn_Pm4_Click(object sender, EventArgs e) => RobotMove(pm4, "ARC", "Lower");
        private void btn_Ll1_Click(object sender, EventArgs e) => RobotMove(ll1, "ARC", "Lower");
        private void btn_Ll2_Click(object sender, EventArgs e) => RobotMove(ll2, "ARC", "Lower");
        private void btn_Load_Click(object sender, EventArgs e) => RobotMove(loadDef, "ARC", "Lower");

        private void btn_Fold_Click(object sender, EventArgs e) => FoldArm("Lower");
        private void btn_Unfold_Click(object sender, EventArgs e) => UnfoldArm("Lower");
        private void btn_Fold2_Click(object sender, EventArgs e) => FoldArm("Upper");
        private void btn_Unfold2_Click(object sender, EventArgs e) => UnfoldArm("Upper");
        #endregion

        #region worker
        // BackgroundWorker 이동 처리
        private void ConfigureMover(BackgroundWorker bg)
        {
            bg.WorkerReportsProgress = true;
            bg.DoWork += Mover_DoWork;
            bg.ProgressChanged += Mover_ProgressChanged;
            bg.RunWorkerCompleted += Mover_RunWorkerCompleted;
        }

        /// <summary>
        /// 팔 접었다 펴기
        /// </summary>
        /// <param name="armName">어떤 팔을 골랐는가</param>
        public void UnfoldArm(string armName) 
        {
            PointF armDef = (armName == "Lower") ? def : def2;

            float angle = (float)Math.Atan2(armDef.Y - root.Y, armDef.X - root.X);
            float maxReach = L1 + L2 + L3; // 최대 길이

            PointF unfoldTarget = new PointF(
                root.X + (float)Math.Cos(angle) * maxReach,
                root.Y + (float)Math.Sin(angle) * maxReach
            );

            RobotMove(unfoldTarget, "LINEAR", armName);
            if (armName == "Lower") isFolded = false; else isFolded2 = false;
        }

        // 팔 접기
        public void FoldArm(string armName)
        {
            PointF armDef = (armName == "Lower") ? def : def2;

            float angle = (float)Math.Atan2(armDef.Y - root.Y, armDef.X - root.X);
            PointF foldTarget = new PointF(
                root.X + (float)Math.Cos(angle) * foldStrength,
                root.Y + (float)Math.Sin(angle) * foldStrength
            );

            RobotMove(foldTarget, "LINEAR", armName);
            if (armName == "Lower") isFolded = true; else isFolded2 = true;
        }

        // 이동 명령 실행
        private void RobotMove(PointF def, string mode, string armName)
        {
            if (armName == "Lower")
            {
                if (mover.IsBusy) return;
                targetDef = def;
                currentDef = this.def;
                moveMode = mode;
                mover.RunWorkerAsync("Lower"); // 어떤 팔인지 보냄
            }
            else
            {
                if (mover2.IsBusy) return;
                targetDef2 = def;
                currentDef2 = this.def2;
                moveMode = mode;
                mover2.RunWorkerAsync("Upper");
            }
        }

        // BackgroundWorker 실제 이동 계산
        private void Mover_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            string arm = e.Argument as string; // 어떤 팔인지 추출

            // 현재-목표점 설정
            PointF startPos = arm == "Lower" ? currentDef : currentDef2;
            PointF endPos = arm == "Lower" ? targetDef : targetDef2;

            // ARC 모드용 각도 계산
            float startAngle = (float)Math.Atan2(startPos.Y - root.Y, startPos.X - root.X);
            float targetAngle = (float)Math.Atan2(endPos.Y - root.Y, endPos.X - root.X);
            float startRadius = Distance(root, startPos);

            // ARC 모드에서는 목표점을 현재 반지름으로 맞춤
            if (moveMode == "ARC")
                endPos = GetPointByDistance(root, endPos, startRadius);

            float targetRadius = Distance(root, endPos);

            // 각도 차이 조정
            float angleDiff = targetAngle - startAngle;
            while (angleDiff > Math.PI) angleDiff -= (float)(Math.PI * 2);
            while (angleDiff < -Math.PI) angleDiff += (float)(Math.PI * 2);

            float totalDistance = Distance(startPos, endPos); // 움직일 거리

            // 움직이는 거리에 따른 step수 결정
            float steps = (moveMode == "ARC")
                ? Math.Abs(angleDiff) / rotationSpeed
                : totalDistance / straightSpeed;

            steps = Math.Max(10f, steps);
            float t = 0f;
            float dynamicStep = 1.0f / steps;
             
            // 보간 이동
            while (t < 1.0f)
            {
                t += dynamicStep;
                if (t > 1f) t = 1f;

                // 직선동작(팔 뻗기접기)이라면 부드럽게, 아니라면 딱딱하게(?)
                float progress = (moveMode == "LINEAR")
                    ? (float)((Math.Sin((t - 0.5) * Math.PI) + 1) / 2f)
                    : t;

                PointF nextPos;
                float curAngle;

                if (moveMode == "LINEAR")
                {
                    nextPos = new PointF(
                        startPos.X + (endPos.X - startPos.X) * progress,
                        startPos.Y + (endPos.Y - startPos.Y) * progress
                    );
                    curAngle = (float)Math.Atan2(nextPos.Y - root.Y, nextPos.X - root.X);
                }
                else
                {
                    curAngle = startAngle + angleDiff * progress;
                    float curRadius = startRadius + (targetRadius - startRadius) * progress;

                    nextPos = new PointF(
                        root.X + (float)Math.Cos(curAngle) * curRadius,
                        root.Y + (float)Math.Sin(curAngle) * curRadius
                    );
                }

                // 두 팔 동기화 회전
                if (arm == "Lower")
                {
                    currentDef = nextPos;

                    float r2 = Distance(root2, currentDef2);
                    currentDef2 = new PointF(
                        root2.X + (float)Math.Cos(curAngle) * r2,
                        root2.Y + (float)Math.Sin(curAngle) * r2
                    );
                }
                else
                {
                    currentDef2 = nextPos;

                    float r1 = Distance(root, currentDef);
                    currentDef = new PointF(
                        root.X + (float)Math.Cos(curAngle) * r1,
                        root.Y + (float)Math.Sin(curAngle) * r1
                    );
                }

                worker.ReportProgress(0, arm);
                System.Threading.Thread.Sleep(2);
            }
        }

        private void Mover_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            def = currentDef;
            def2 = currentDef2;
            pnl_MainPaint.Invalidate();
        }

        private void Mover_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            pnl_MainPaint.Invalidate();
            ButtonEnable("Lower");
            ButtonEnable("Upper");
        }
        #endregion

        #region calculate
        /// <summary>
        /// 두 좌표간 거리 도출
        /// </summary>
        private float Distance(PointF a, PointF b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// 목표지점을 향해 회전만 시킬 수 있는 메서드
        /// </summary>
        /// <param name="start">기준점</param>
        /// <param name="target">목표지점</param>
        /// <param name="dist">기준점에서 가고 싶은 거리</param>
        /// <returns></returns>
        private PointF GetPointByDistance(PointF start, PointF target, float dist)
        {
            float angle = (float)Math.Atan2(target.Y - start.Y, target.X - start.X);
            return new PointF(
                start.X + (float)Math.Cos(angle) * dist,
                start.Y + (float)Math.Sin(angle) * dist
            );
        }

        // 팔꿈치 위치 계산
        private PointF CalculateElbowPos(PointF p1, PointF p2, ref PointF prevElbow, string armName)
        {
            float d = Distance(p1, p2);
            float dClamped = Math.Max(Math.Abs(L1 - L2) + 0.1f, Math.Min(d, L1 + L2 - 0.1f));

            float cosA = (L1 * L1 + dClamped * dClamped - L2 * L2) / (2 * L1 * dClamped);
            float alpha = (float)Math.Acos(Math.Max(-1f, Math.Min(1f, cosA)));

            float baseAngle = (float)Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);

            PointF elbow1 = new PointF(
                p1.X + (float)Math.Cos(baseAngle + alpha) * L1,
                p1.Y + (float)Math.Sin(baseAngle + alpha) * L1
            );

            PointF elbow2 = new PointF(
                p1.X + (float)Math.Cos(baseAngle - alpha) * L1,
                p1.Y + (float)Math.Sin(baseAngle - alpha) * L1
            );

            PointF chosen;

            // 팔별로 엘보 방향 고정
            // 최근 엘보 위치와 가까운 엘보로 결정
            // 수학적으로 항상 두 가지의 해(팔꿈치 위치)가 나오기 때문에 이러한 처리가 없으면 팔꿈치가 뒤집히는 현상이 나타남
            if (armName == "Lower")
            {
                chosen = prevElbow == PointF.Empty
                    ? elbow2
                    : (Distance(prevElbow, elbow1) < Distance(prevElbow, elbow2) ? elbow1 : elbow2);
            }
            else
            {
                chosen = prevElbow == PointF.Empty
                    ? elbow1
                    : (Distance(prevElbow, elbow2) < Distance(prevElbow, elbow1) ? elbow2 : elbow1);
            }

            prevElbow = chosen;
            return chosen;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="basePos">루트</param>
        /// <param name="targetPos">끝점</param>
        /// <param name="prevElbow">엘보</param>
        /// <param name="armName">어떤 팔?</param>
        /// <returns>루트 지점과 엘보우, 손목, 각 축의 좌표/각도를 리턴</returns>
        private (PointF Base, PointF Elbow, PointF Wrist, float Angle1, float Angle2, float Angle3)
            CalculateArmPoints(PointF basePos, PointF targetPos, ref PointF prevElbow, string armName)
        {
            float maxReach = L1 + L2 + L3;
            PointF finalTarget =
                Distance(basePos, targetPos) > maxReach
                ? GetPointByDistance(basePos, targetPos, maxReach)
                : targetPos;

            float angToTarget = (float)Math.Atan2(finalTarget.Y - basePos.Y, finalTarget.X - basePos.X);

            PointF wrist = new PointF(
                finalTarget.X - (float)Math.Cos(angToTarget) * L3,
                finalTarget.Y - (float)Math.Sin(angToTarget) * L3
            );

            PointF elbow = CalculateElbowPos(basePos, wrist, ref prevElbow, armName);

            float a1 = (float)Math.Atan2(elbow.Y - basePos.Y, elbow.X - basePos.X);
            float a2 = (float)Math.Atan2(wrist.Y - elbow.Y, wrist.X - elbow.X);
            float a3 = (float)Math.Atan2(finalTarget.Y - wrist.Y, finalTarget.X - wrist.X);

            return (basePos, elbow, wrist, a1, a2, a3);
        }
        #endregion

        #region paint
        private void pnl_MainPaint_Paint(object sender, PaintEventArgs e)
        {
            if (def == PointF.Empty || def2 == PointF.Empty) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 두 팔의 관절 좌표 계산
            var arm1 = CalculateArmPoints(root, def, ref prevElbowPos, "Lower");
            var arm2 = CalculateArmPoints(root2, def2, ref prevElbowPos2, "Upper");

            // 현재 움직이고 있는 팔의 Angle3(손목 각도)를 가져옴
            float targetWristAngle = mover2.IsBusy ? arm2.Angle3 : arm1.Angle3;

            // 루트 이미지 회전하며 그리기
            if (imgRoot != null)
            {
                GraphicsState state = g.Save();

                // 회전 중심을 root 좌표로 이동
                g.TranslateTransform(root.X, root.Y);

                // 손목이 바라보는 각도(Angle3)만큼 루트를 회전
                g.RotateTransform(targetWristAngle * RAD_TO_DEG - rootAngle);

                // 루트 이미지 크기 설정
                g.DrawImage(imgRoot, -rootWidth / 2f, -rootHeight / 2f, rootWidth, rootHeight);

                g.Restore(state);
            }

            // 순서대로 이미지 출력 (순서 중요함)
            DrawLink(g, imgLink1, arm1.Base, arm1.Angle1, L1, false, true);
            DrawLink(g, imgLink1, arm2.Base, arm2.Angle1, L1, false, true);

            DrawLink(g, imgLink2, arm1.Elbow, arm1.Angle2, L2, false, false);
            DrawLink(g, imgLink3, arm1.Wrist, arm1.Angle3, L3, true, false);

            DrawLink(g, imgLink2, arm2.Elbow, arm2.Angle2, L2, false, false);
            DrawLink(g, imgLink3, arm2.Wrist, arm2.Angle3, L3, true, false);
        }

        // 링크 이미지 그리기
        private void DrawLink(Graphics g, Image img, PointF pos, float rad, float len, bool isLast, bool isFirst)
        {
            if (img == null) return;

            GraphicsState state = g.Save();
            g.TranslateTransform(pos.X, pos.Y);
            g.RotateTransform(rad * RAD_TO_DEG);

            float startX = isFirst ? 0f : -overlap; // 링크1은 겹치지 않고 나머지 팔은 오버랩 만큼 겹쳐 그림
            float drawW = (isFirst || isLast) ? len + overlap : len + (overlap * 2);
            float drawH = drawW * ((float)img.Height / img.Width);

            g.DrawImage(img, startX, -drawH / 2f, drawW, drawH);
            g.Restore(state);
        }
        #endregion


        // 마우스 입력 처리 -> 지워도 무관
        private void pnl_MainPaint_MouseMove(object sender, MouseEventArgs e)
        {
            lbl_Point_X.Text = $"X: {e.X}";
            lbl_Point_Y.Text = $"Y: {e.Y}";
        }

        private void pnl_MainPaint_MouseClick(object sender, MouseEventArgs e)
        {
            lbl_MiddelSpot.Text = $"중앙: {e.X}, {e.Y}";

            if (e.Button == MouseButtons.Left)
            {
                isFolded = false;
                RobotMove(new Point(e.X, e.Y), "LINEAR", "Lower");
            }
            else if (e.Button == MouseButtons.Right)
            {
                isFolded2 = false;
                RobotMove(new Point(e.X, e.Y), "LINEAR", "Upper");
            }
        }

        private void pnl_MainPaint_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            RobotMove(new PointF(e.X, e.Y), "LINEAR", "Upper");
        }
    }
}
