using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;
using Winform_RobotArmUI_new.Models;

namespace Winform_RobotArmUI_new
{
    public partial class MainForm : Form
    {
        // 팔 설정
        private readonly float overlap = 10f;     // 팔 겹치는 정도
        private readonly float waferOffset = 15f; // 집게와 웨이퍼가 겹치는 정도
        // 길이
        private const float L1 = 85f;    // 루트에서 시작
        private const float L2 = 85f;
        private const float L3 = 85f;

        private readonly float loadAngle = 90f; // load 시 바라볼 각도

        // 속도 설정
        private readonly float foldStrength = 2f;     // fold 강도 (낮을수록 강함)
        private readonly float rotationSpeed = 0.05f; // ARC 모드 회전 속도
        private readonly float straightSpeed = 3.0f;  // LINEAR 모드 이동 속도

        // 웨이퍼 폰트
        private readonly Font waferFont = new Font("Tahoma", 8, FontStyle.Bold);

        // 리소스 이미지
        private readonly float rootScale = 0.25f;  // 루트 이미지 크기
        private readonly Image imgRoot = Properties.Resources.Robot_Root;

        private readonly float waferScale = 0.15f; // 웨이퍼 이미지 크기
        private readonly Image imgWaferIdle = Properties.Resources.Wafer_Idle; // 사용할 이미지
        private readonly Image imgWaferEnd = Properties.Resources.Wafer_End;   // 사용할 이미지
        private readonly Image imgLink1 = Properties.Resources.Robot_Link1;
        private readonly Image imgLink2 = Properties.Resources.Robot_Link2;
        private readonly Image imgLink3 = Properties.Resources.Robot_Link3;
        //private readonly Image imgRoot = Properties.Resources.Robot_Root2;
        //private readonly Image imgLink1 = Properties.Resources.Robot_Link1_2;
        //private readonly Image imgLink2 = Properties.Resources.Robot_Link2_2;
        //private readonly Image imgLink3 = Properties.Resources.Robot_Link3_2;

        // 좌표 설정
        private readonly PointF root = new PointF(343, 330);
        private readonly PointF root2 = new PointF(343, 330);
        private readonly PointF pm1 = new PointF(81, 322);
        private readonly PointF pm2 = new PointF(222, 119);
        private readonly PointF pm3 = new PointF(472, 117);
        private readonly PointF pm4 = new PointF(607, 325);
        private readonly PointF ll1 = new PointF(263, 548);
        private readonly PointF ll2 = new PointF(426, 549);

        private const string WAFER_TAG = "LP_";
        private int waferNum = 1;

        private float waferWidth;
        private float waferHeight;
        private float rootAngle; // 루트 이미지 각도
        private float rootWidth;
        private float rootHeight;
        private const float RAD_TO_DEG = 180f / (float)Math.PI;

        private PointF endPoint;                    // 팔 현재 끝점
        private PointF targetEndPoint;              // 팔 목표점
        private PointF currentEndPoint;             // Lower 팔 계산 중 위치
        private PointF prevElbowPos = PointF.Empty; // Lower 팔 이전 팔꿈치 위치
        private bool isFolded = true;               // 접었나?

        private PointF endPoint2;
        private PointF targetEndPoint2;
        private PointF currentEndPoint2;
        private PointF prevElbowPos2 = PointF.Empty;
        private bool isFolded2 = true;

        private PointF loadDef; // 로드시 끝점

        // 이동 명령 시 목표로 삼은 스테이션 이름을 저장
        private string pendingStationLower = null;
        private string pendingStationUpper = null;

        private Dictionary<string, Station> stations = new Dictionary<string, Station>();
        // 각 팔의 웨이퍼 보유 상태 (true: 들고 있음, false: 비어 있음)
        private bool hasWaferLower = false;
        private bool hasWaferUpper = false;
        // 이동 방식
        private string moveMode = "ARC";

        // 두 개의 BackgroundWorker (Lower/Upper 팔 각각)
        private readonly BackgroundWorker mover = new BackgroundWorker();
        private readonly BackgroundWorker mover2 = new BackgroundWorker();

        public MainForm()
        {
            stations.Add("PM1", new Station("PM1", pm1, false));
            stations.Add("PM2", new Station("PM2", pm2, false));
            stations.Add("PM3", new Station("PM3", pm3, false));
            stations.Add("PM4", new Station("PM4", pm4, false));
            stations.Add("LL1", new Station("LL1", ll1, false));
            stations.Add("LL2", new Station("LL2", ll2, false));
            stations.Add("Lower", new Station("Lower", PointF.Empty, false));
            stations.Add("Upper", new Station("Upper", PointF.Empty, false));

            waferWidth = imgWaferIdle.Width * waferScale;
            waferHeight = imgWaferIdle.Height * waferScale;
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

            endPoint = loadDef;
            endPoint2 = loadDef;

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

        private void btn_Pm1_Click(object sender, EventArgs e) => RobotMove(pm1, "ARC", "Lower", "PM1");
        private void btn_Pm2_Click(object sender, EventArgs e) => RobotMove(pm2, "ARC", "Lower", "PM2");
        private void btn_Pm3_Click(object sender, EventArgs e) => RobotMove(pm3, "ARC", "Lower", "PM3");
        private void btn_Pm4_Click(object sender, EventArgs e) => RobotMove(pm4, "ARC", "Lower", "PM4");
        private void btn_Ll1_Click(object sender, EventArgs e) => RobotMove(ll1, "ARC", "Lower", "LL1");
        private void btn_Ll2_Click(object sender, EventArgs e) => RobotMove(ll2, "ARC", "Lower", "LL2");
        private void btn_Load_Click(object sender, EventArgs e) => RobotMove(loadDef, "ARC", "Lower");

        private void btn_Fold_Click(object sender, EventArgs e) => FoldArm("Lower");
        private void btn_Fold2_Click(object sender, EventArgs e) => FoldArm("Upper");
        private void btn_Unfold_Click(object sender, EventArgs e)
        {
            string nearestStation = GetNearestStationName("Lower");
            if (!string.IsNullOrEmpty(nearestStation))
            {
                UnfoldArm("Lower", stations[nearestStation].Location, nearestStation);
            }
        }
        private void btn_Unfold2_Click(object sender, EventArgs e)
        {
            string nearestStation = GetNearestStationName("Upper");
            if (!string.IsNullOrEmpty(nearestStation))
            {
                UnfoldArm("Upper", stations[nearestStation].Location, nearestStation);
            }
        }

        private void btnl_Pm1_End_Click(object sender, EventArgs e)
        {
            PmWaferEnd("PM1");
        }

        private void btnl_Pm2_End_Click(object sender, EventArgs e)
        {
            PmWaferEnd("PM2");
        }

        private void btnl_Pm3_End_Click(object sender, EventArgs e)
        {
            PmWaferEnd("PM3");
        }

        private void btnl_Pm4_End_Click(object sender, EventArgs e)
        {
            PmWaferEnd("PM4");
        }
        private void PmWaferEnd(string name)
        {
            stations[name].Image = imgWaferEnd;
            pnl_MainPaint.Invalidate();
        }
        private void btn_Ll1_Idle_Click(object sender, EventArgs e)
        {
            stations["LL1"].Id = WAFER_TAG + waferNum++;
            stations["LL1"].HasWafer = true;
            stations["LL1"].Image = imgWaferIdle;

            pnl_MainPaint.Invalidate();
        }

        private void btn_Ll2_Idle_Click(object sender, EventArgs e)
        {
            stations["LL2"].Id = WAFER_TAG + waferNum++;
            stations["LL2"].HasWafer = true;
            stations["LL2"].Image = imgWaferIdle;

            pnl_MainPaint.Invalidate();
        }

        private void btnl_Ll1_Delete_Click(object sender, EventArgs e)
        {
            stations["LL1"].HasWafer = false;
            stations["LL1"].Image = null;

            pnl_MainPaint.Invalidate();
        }

        private void btnl_Ll2_Delete_Click(object sender, EventArgs e)
        {
            stations["LL2"].HasWafer = false;
            stations["LL2"].Image = null;

            pnl_MainPaint.Invalidate();
        }

        #endregion

        #region worker & arm, wafer move
        // BackgroundWorker 이동 처리
        private void ConfigureMover(BackgroundWorker bg)
        {
            bg.WorkerReportsProgress = true;
            bg.DoWork += Mover_DoWork;
            bg.ProgressChanged += Mover_ProgressChanged;
            bg.RunWorkerCompleted += Mover_RunWorkerCompleted;
        }
        // BackgroundWorker 실제 이동 계산
        private void Mover_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            string arm = e.Argument as string; // 어떤 팔인지 추출
            e.Result = arm;

            // 현재-목표점 설정
            PointF startPos = arm == "Lower" ? currentEndPoint : currentEndPoint2;
            PointF endPos = arm == "Lower" ? targetEndPoint : targetEndPoint2;

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
                    currentEndPoint = nextPos;

                    float r2 = Distance(root2, currentEndPoint2);
                    currentEndPoint2 = new PointF(
                        root2.X + (float)Math.Cos(curAngle) * r2,
                        root2.Y + (float)Math.Sin(curAngle) * r2
                    );
                }
                else
                {
                    currentEndPoint2 = nextPos;

                    float r1 = Distance(root, currentEndPoint);
                    currentEndPoint = new PointF(
                        root.X + (float)Math.Cos(curAngle) * r1,
                        root.Y + (float)Math.Sin(curAngle) * r1
                    );
                }

                worker.ReportProgress(0, arm);
                System.Threading.Thread.Sleep(1);
            }
        }

        private void Mover_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            endPoint = currentEndPoint;
            endPoint2 = currentEndPoint2;
            pnl_MainPaint.Invalidate();
        }

        private void Mover_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string armName = e.Result as string;
            bool isLower = (armName == "Lower");

            // 팔이 펴진 상태에서만 상호작용 발생
            bool foldedState = isLower ? isFolded : isFolded2;

            if (!foldedState)
            {
                // 펴진 상태라면 현재 위치에서 가장 가까운 스테이션을 찾아 작업 수행
                TryPickOrPlace(armName);
            }

            pnl_MainPaint.Invalidate(); // 상태 변화 반영을 위해 재그리기
            ButtonEnable("Lower");
            ButtonEnable("Upper");
        }

        /// <summary>
        /// 팔 접었다 펴기
        /// </summary>
        /// <param name="armName">어떤 팔을 골랐는가</param>
        // 목표 좌표까지만 뻗음
        public void UnfoldArm(string armName, PointF targetPos, string stationName)
        {
            float angle = (float)Math.Atan2(targetPos.Y - root.Y, targetPos.X - root.X);

            // 웨이퍼 중심을 맞추기 위한 끝점 확장 계산
            PointF extendedTarget = new PointF(
                targetPos.X + (float)Math.Cos(angle) * waferOffset,
                targetPos.Y + (float)Math.Sin(angle) * waferOffset
            );

            // 의도: 확장된 좌표로 가되, 목표 스테이션 이름을 명확히 전달
            RobotMove(extendedTarget, "LINEAR", armName, stationName);

            if (armName == "Lower") isFolded = false;
            else isFolded2 = false;
        }

        // 팔 접기
        public void FoldArm(string armName)
        {
            PointF armDef = (armName == "Lower") ? endPoint : endPoint2;

            float angle = (float)Math.Atan2(armDef.Y - root.Y, armDef.X - root.X);
            PointF foldTarget = new PointF(
              root.X + (float)Math.Cos(angle) * foldStrength,
              root.Y + (float)Math.Sin(angle) * foldStrength
            );

            RobotMove(foldTarget, "LINEAR", armName);
            if (armName == "Lower") isFolded = true; else isFolded2 = true;
        }

        // 이동 명령 실행
        private void RobotMove(PointF endPoint, string mode, string armName = "Lower", string stationName = null)
        {
            if (armName == "Lower")
            {
                if (mover.IsBusy) return;
                pendingStationLower = stationName; // 목표 저장
                targetEndPoint = endPoint;
                currentEndPoint = this.endPoint;
                moveMode = mode;
                mover.RunWorkerAsync("Lower");
            }
            else
            {
                if (mover2.IsBusy) return;
                pendingStationUpper = stationName; // 목표 저장
                targetEndPoint2 = endPoint;
                currentEndPoint2 = this.endPoint2;
                moveMode = mode;
                mover2.RunWorkerAsync("Upper");
            }
        }
        
        private void TryPickOrPlace(string armName)
        {
            bool isLower = (armName == "Lower");
            string reservedStation = isLower ? pendingStationLower : pendingStationUpper;

            // 작업 시작 전 예약 정보 초기화 (중복 작업 방지)
            if (isLower) pendingStationLower = null; else pendingStationUpper = null;

            // 지정된 스테이션이 없으면 아무것도 안 함
            if (string.IsNullOrEmpty(reservedStation) || !stations.ContainsKey(reservedStation)) return;

            ProcessWaferTransfer(stations[reservedStation], isLower);
        }
        // 팔이 바라보는 각도와 가장 일치하는 각도에 있는 station 리턴
        private string GetNearestStationName(string armName)
        {
            PointF currentPos = (armName == "Lower") ? endPoint : endPoint2;
            float minAngleDiff = float.MaxValue;
            string targetName = "";

            float currentAngle = (float)Math.Atan2(currentPos.Y - root.Y, currentPos.X - root.X);

            foreach (var st in stations)
            {
                float stAngle = (float)Math.Atan2(st.Value.Location.Y - root.Y, st.Value.Location.X - root.X);
                float diff = Math.Abs(currentAngle - stAngle);

                if (diff < minAngleDiff)
                {
                    minAngleDiff = diff;
                    targetName = st.Key;
                }
            }
            return targetName;
        }

        // Pick/Place
        private void ProcessWaferTransfer(Station targetStation, bool isLower)
        {
            // 제어할 팔의 스테이션 객체 선택
            Station arm = isLower ? stations["Lower"] : stations["Upper"];

            // 1. Pick (팔이 비었고 스테이션에 웨이퍼가 있을 때)
            if (!arm.HasWafer && targetStation.HasWafer)
            {
                arm.HasWafer = true;
                arm.Id = targetStation.Id;       // ID 전송
                arm.Image = targetStation.Image; // 이미지 전송

                targetStation.HasWafer = false;
                targetStation.Id = string.Empty; // 원본 데이터 삭제
            }
            // 2. Place (팔에 웨이퍼가 있고 스테이션이 비었을 때)
            else if (arm.HasWafer && !targetStation.HasWafer)
            {
                targetStation.HasWafer = true;
                targetStation.Id = arm.Id;       // ID 전송
                targetStation.Image = arm.Image; // 이미지 전송

                arm.HasWafer = false;
                arm.Id = string.Empty;           // 팔 데이터 비움
            }
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
            float maxReach = L1 + L2 + L3; // 팔을 뻗을 수 있는 최대 길이
            // 목표 좌표가 팔 길이보다 짧으면 그 지점에 멈추고, 길면 최대치까지만 뻗음
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
            float a3 = (float)Math.Atan2(finalTarget.Y - wrist.Y, finalTarget.X - wrist.X); // 손목 각도를 타겟 방향과 일치시킴

            return (basePos, elbow, wrist, a1, a2, a3);
        }
        #endregion

        #region paint
        private void pnl_MainPaint_Paint(object sender, PaintEventArgs e)
        {
            if (endPoint == PointF.Empty || endPoint2 == PointF.Empty) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var arm1 = CalculateArmPoints(root, endPoint, ref prevElbowPos, "Lower");
            var arm2 = CalculateArmPoints(root2, endPoint2, ref prevElbowPos2, "Upper");

            // 1. 루트(Base) 그리기
            float targetWristAngle = mover2.IsBusy ? arm2.Angle3 : arm1.Angle3;
            DrawRoot(g, targetWristAngle);

            // 2. 링크 그리기 (Z-Order: 아래에서 위로 쌓기)
            // [Layer 1] 각 팔의 첫 번째 링크
            DrawLink(g, imgLink1, arm1.Base, arm1.Angle1, L1, false, true);
            DrawLink(g, imgLink1, arm2.Base, arm2.Angle1, L1, false, true);

            // [Layer 2] Lower 팔의 팔꿈치와 손목
            DrawLink(g, imgLink2, arm1.Elbow, arm1.Angle2, L2, false, false);
            DrawLink(g, imgLink3, arm1.Wrist, arm1.Angle3, L3, true, false);

            // [Layer 3] Lower 팔 웨이퍼 (손목 위에 얹힘)
            float lX = endPoint.X - (float)Math.Cos(arm1.Angle3) * waferOffset;
            float lY = endPoint.Y - (float)Math.Sin(arm1.Angle3) * waferOffset;
            DrawWafer(g, stations["Lower"], new PointF(lX, lY));

            // [Layer 4] Upper 팔의 팔꿈치와 손목 (Lower 위에 그려짐)
            DrawLink(g, imgLink2, arm2.Elbow, arm2.Angle2, L2, false, false);
            DrawLink(g, imgLink3, arm2.Wrist, arm2.Angle3, L3, true, false);

            // [Layer 5] Upper 팔 웨이퍼 (모든 팔 구조물 위에 얹힘)
            float uX = endPoint2.X - (float)Math.Cos(arm2.Angle3) * waferOffset;
            float uY = endPoint2.Y - (float)Math.Sin(arm2.Angle3) * waferOffset;
            DrawWafer(g, stations["Upper"], new PointF(uX, uY));

            // [Layer 6] 스테이션 웨이퍼 (바닥 또는 가장 위에 그리기 - 설계에 따라 선택)
            // 의도: stations 딕셔너리에서 'Lower', 'Upper'만 제외하고 출력하여 (0,0) 버그 해결
            foreach (var kvp in stations)
            {
                if (kvp.Key == "Lower" || kvp.Key == "Upper") continue;

                DrawWafer(g, kvp.Value, kvp.Value.Location);
            }
        }

        // 루트 그리기
        private void DrawRoot(Graphics g, float targetWristAngle)
        {
            if (imgRoot == null) return;
            GraphicsState state = g.Save();
            g.TranslateTransform(root.X, root.Y);
            g.RotateTransform(targetWristAngle * RAD_TO_DEG - rootAngle);
            g.DrawImage(imgRoot, -rootWidth / 2f, -rootHeight / 2f, rootWidth, rootHeight);
            g.Restore(state);
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

        private void DrawWafer(Graphics g, Station st, PointF center)
        {
            if (!st.HasWafer || st.Image == null) return;

            // 웨이퍼 이미지 그리기
            g.DrawImage(st.Image,
                center.X - waferWidth / 2f,
                center.Y - waferHeight / 2f,
                waferWidth, waferHeight);

            // ID 텍스트 중앙 출력
            if (!string.IsNullOrEmpty(st.Id))
            {
                // 성능 최적화: 문자열 크기 측정
                SizeF textSize = g.MeasureString(st.Id, waferFont);

                // 정가운데 좌표 계산
                PointF textPos = new PointF(
                    center.X - textSize.Width / 2f,
                    center.Y - textSize.Height / 2f
                );

                // 가독성을 위해 외곽선 효과(선택 사항) 또는 단순 출력
                g.DrawString(st.Id, waferFont, Brushes.Black, textPos);
            }
        }
        #endregion


        // 마우스 입력 처리
        private void pnl_MainPaint_MouseMove(object sender, MouseEventArgs e)
        {
            lbl_Point_X.Text = $"X: {e.X}";
            lbl_Point_Y.Text = $"Y: {e.Y}";
        }

        private void pnl_MainPaint_MouseClick(object sender, MouseEventArgs e)
        {
            // 마우스 클릭 위치와 끝점 사이의 거리 계산
            float distToPm1 = Distance(e.Location, pm1);
            float distToPm2 = Distance(e.Location, pm2);
            float distToPm3 = Distance(e.Location, pm3);
            float distToPm4 = Distance(e.Location, pm4);
            float distToLl1 = Distance(e.Location, ll1);
            float distToLl2 = Distance(e.Location, ll2);
            float distToLower = Distance(e.Location, endPoint);
            float distToUpper = Distance(e.Location, endPoint2);

            // 타겟 이미지의 반지름 범위 내를 클릭했는지 확인
            float clickRadius = waferWidth / 2f;

            if (distToUpper <= clickRadius && hasWaferUpper && hasWaferLower)
            {
                MessageBox.Show("Lower, Upper 모두 웨이퍼를 들고 있습니다.");
                return;
            }
            if (distToUpper <= clickRadius && hasWaferUpper)
            {
                MessageBox.Show("Upper 웨이퍼 이미지를 클릭했습니다!");
                return;
            }
            if (distToLower <= clickRadius && hasWaferLower)
            {
                MessageBox.Show("Lower 웨이퍼 이미지를 클릭했습니다!");
                return;
            }
            if (distToPm1 <= clickRadius && stations["PM1"].HasWafer)
            {
                MessageBox.Show("PM1 웨이퍼 이미지를 클릭했습니다!");
            }
            if (distToPm2 <= clickRadius && stations["PM2"].HasWafer)
            {
                MessageBox.Show("PM2 웨이퍼 이미지를 클릭했습니다!");
            }
            if (distToPm3 <= clickRadius && stations["PM3"].HasWafer)
            {
                MessageBox.Show("PM3 웨이퍼 이미지를 클릭했습니다!");
            }
            if (distToPm4 <= clickRadius && stations["PM4"].HasWafer)
            {
                MessageBox.Show("PM4 웨이퍼 이미지를 클릭했습니다!");
            }
            if (distToLl1 <= clickRadius && stations["LL1"].HasWafer)
            {
                MessageBox.Show("LL1 웨이퍼 이미지를 클릭했습니다!");
            }
            if (distToLl2 <= clickRadius && stations["LL2"].HasWafer)
            {
                MessageBox.Show("LL2 웨이퍼 이미지를 클릭했습니다!");
            }

            lbl_MiddelSpot.Text = $"중앙: {e.X}, {e.Y}";
        }
    }
}