using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Winform_RobotArmUI_.Modals;

namespace Winform_RobotArmUI_
{
    public partial class MainForm : Form
    {
        // 위치 관련
        public float stoA = 100f, atoD = 125f; // 링크 길이
        public float armOffsetDeg = 40f; // 두 번째 팔의 각도 차이 (높을수록 줄어듦)

        public float minElbowAngle = 40f; // 가장 많이 접혔을 때의 각도 (minDistLimit에 의해 무시될 확률이 높음)

        public float maxElbowAngle = 180f; // 가장 많이 펴졌을 때의 각도
        public float minDistLimit = 60f; // 루트와 끝점(손)의 최소 거리 제한

        public float constantSpeed = 1.5f; // 로봇 움직이는 속도 조절

        private float FoldedSymmetryAngle;


        // image
        public Image imgRoot = Properties.Resources.Robot_Root3;
        public Image imgAnkle = Properties.Resources.Robot_Ankle;
        public Image imgLink1 = Properties.Resources.Robot_Link1;
        public Image imgLink2 = Properties.Resources.Robot_Link2;

        // 루트 이미지 크기 조절
        private const float NUM = 40f;
        //private const float NUM = 50f; 
        public float rootImageWidth = NUM;
        public float rootImageHeight = NUM;
        // 루트 이미지 각도 조절
        public float rootImageAngle = 199.5f;

        // 중앙점
        public PointF root = new PointF(345, 309);
        // 모듈
        public PointF pm1 = new PointF(84, 312);
        public PointF pm2 = new PointF(220, 105);
        public PointF pm3 = new PointF(466, 103);
        public PointF pm4 = new PointF(605, 312);
        public PointF ll1 = new PointF(264, 526);
        public PointF ll2 = new PointF(425, 532);

        //private float middleX = 345; 
        //private float middleY = 309;

        private BackgroundWorker mover = new BackgroundWorker();
        private PointF def;
        private PointF ankle;
        private PointF targetDef;     // 최종 목표점
        private PointF currentDef;    // 현재 로봇팔 끝 위치
        private bool isMoving = false;
        private bool isFolded = true;
        private string moveMode = "ARC";

        public MainForm()
        {
            this.DoubleBuffered = true; // 깜빡임 방지 처리
            InitializeComponent();

            mover.WorkerReportsProgress = true;
            mover.DoWork += Mover_DoWork;
            mover.ProgressChanged += Mover_ProgressChanged;
            mover.RunWorkerCompleted += Mover_RunWorkerCompleted;
            pnl_MainPaint.Invalidate();

            FoldedSymmetryAngle = 0.5f * (stoA + atoD + minDistLimit) - Math.Abs(stoA - atoD);

            // 최초 실행 시 팔 접기
            FoldArm();
        }
        #region button funtion
        private void btn_MousePoint_Click(object sender, EventArgs e)
        {
            Frm_Kinamatics_MousePoint frm = new Frm_Kinamatics_MousePoint();
            frm.Show();
        }
        private void pnl_MainPaint_MouseClick(object sender, MouseEventArgs e)
        {
            lbl_MiddelSpot.Text = $"중앙: {e.X}, {e.Y}";
            //middleX = e.X;
            //middleY = e.Y;
        }
        private void btn_Pm1_Click(object sender, EventArgs e) => RobotMove(GetArcPoint(root, pm1, armOffsetDeg / 2), "ARC");
        private void btn_Pm2_Click(object sender, EventArgs e) => RobotMove(GetArcPoint(root, pm2, armOffsetDeg / 2), "ARC");
        private void btn_Pm3_Click(object sender, EventArgs e) => RobotMove(GetArcPoint(root, pm3, armOffsetDeg / 2), "ARC");
        private void btn_Pm4_Click(object sender, EventArgs e) => RobotMove(GetArcPoint(root, pm4, armOffsetDeg / 2), "ARC");
        private void btn_Ll1_Click(object sender, EventArgs e) => RobotMove(GetArcPoint(root, ll1, armOffsetDeg / 2), "ARC");
        private void btn_Ll2_Click(object sender, EventArgs e) => RobotMove(GetArcPoint(root, ll2, armOffsetDeg / 2), "ARC");

        private void btn_Fold_Click(object sender, EventArgs e) // btn fold
        {
            FoldArm();
        }
        private void btn_Unfold_Click(object sender, EventArgs e)
        {
            if (root == PointF.Empty || isMoving) return;

            // 현재 어깨에서 끝점이 바라보는 각도 계산
            // 만약 완전히 접혀서 root와 def가 겹쳐있다면 ankle 방향을 기준으로 함
            float dx = def.X - root.X;
            float dy = def.Y - root.Y;

            if (Distance(root, def) < 5f) // 너무 많이 접혀있을 경우 대비
            {
                dx = ankle.X - root.X;
                dy = ankle.Y - root.Y;
            }

            float angle = (float)Math.Atan2(dy, dx);

            // 최대 길이만큼 뻗은 좌표 계산 (stoA + atoD)
            float maxLength = (stoA + atoD);
            float targetX = root.X + (float)Math.Cos(angle) * maxLength;
            float targetY = root.Y + (float)Math.Sin(angle) * maxLength;

            // 해당 지점으로 이동
            isFolded = false;
            // RobotMove(new PointF(targetX, targetY), "LINEAR");
            // 팔 동작 개선f
            RobotMove(GetArcPoint(root, new PointF(targetX, targetY), -armOffsetDeg / 2), "LINEAR");
        }
        private void btn_Fold2_Click(object sender, EventArgs e)
        {
            FoldArm();
        }

        private void btn_Unfold2_Click(object sender, EventArgs e)
        {
            // [두 번째 팔을 펴낸다] = [첫 번째 팔을 최소로 접는다]
            // 아까 설정한 minDistLimit(예: 70f) 위치로 이동
            //MoveArmByDistance(minDistLimit);
            isFolded = false;
            MoveArmByDistance(1f);
        }

        private void ButtonEnable(bool isFolded)
        {
            this.isFolded = isFolded;

            btn_Fold.Enabled = !isFolded;
            btn_Unfold.Enabled = isFolded;
            btn_Fold2.Enabled = !isFolded;
            btn_Unfold2.Enabled = isFolded;

            gb_Control.Enabled = isFolded;
            //gb_Control2.Enabled = isFolded;
        }
        #endregion

        #region worker
        private void RobotMove(PointF def, string mode)
        {
            if (root == PointF.Empty || isMoving)
                return;
            // --- [끝점 정지 로직 추가] ---
            float d = Distance(root, def);

            if (d < minDistLimit)
            {
                // 목표점이 너무 가까우면 root에서 목표점 방향의 직선상에 있는 
                // minDistLimit 거리의 좌표를 새로운 목표점으로 설정
                float angle = (float)Math.Atan2(def.Y - root.Y, def.X - root.X);
                def = new PointF(
                    root.X + (float)Math.Cos(angle) * minDistLimit,
                    root.Y + (float)Math.Sin(angle) * minDistLimit
                );
            }

            targetDef = def;
            currentDef = this.def;
            moveMode = mode;
            isMoving = true;

            gb_Control.Enabled = false;
            //gb_Control2.Enabled = false;
            mover.RunWorkerAsync();
        }
        private void Mover_DoWork(object sender, DoWorkEventArgs e)
        {
            float t = 0f;

            // 기초 값 계산
            PointF startPoint = currentDef;
            float startAngle = (float)Math.Atan2(startPoint.Y - root.Y, startPoint.X - root.X);
            float targetAngle = (float)Math.Atan2(targetDef.Y - root.Y, targetDef.X - root.X);
            float startRadius = Distance(root, startPoint);

            if (moveMode is "ARC") // 루트에서 원하는 방향, 일정거리 이동 시 찍히는 좌표 
                targetDef = GetPointByDistance(root, targetDef, Distance(root, startPoint));

            float targetRadius = Distance(root, targetDef);

            // 각도 차이 보정 및 절대량 계산
            float angleDiff = targetAngle - startAngle;
            while (angleDiff > Math.PI) angleDiff -= (float)(Math.PI * 2);
            while (angleDiff < -Math.PI) angleDiff += (float)(Math.PI * 2);

            // 1. 총 이동량 계산
            float totalMovement;
            if (moveMode == "ARC") // 호 이동일 때는 '각도 차이'와 '반지름'을 이용한 호의 길이를 기준으로 함
                totalMovement = Math.Abs(angleDiff) * startRadius;
            else // 직선 이동일 때는 두 점 사이의 거리 기준
                totalMovement = Distance(startPoint, targetDef);

            float constantSpeed = this.constantSpeed;
            if (moveMode == "LINEAR")
                constantSpeed = this.constantSpeed / 2f;

            // 3. 이동량에 따라 필요한 총 단계수(Steps) 결정
            // 이동량이 클수록 totalSteps가 커지므로 t가 증가하는 속도(dynamicStep)는 작아짐 -> 오래 걸림
            float totalSteps = Math.Max(10f, totalMovement / constantSpeed);
            float dynamicStep = 1.0f / totalSteps;

            // 애니메이션 루프
            while (t < 1.0f)
            {
                t += dynamicStep; // 계산된 동적 속도 적용
                if (t > 1.0f) t = 1.0f; 

                float smoothT = (float)(Math.Sin((t - 0.5) * Math.PI) + 1) / 2f;

                switch (moveMode)
                {
                    case "LINEAR":
                        // 직선 보간
                        currentDef = new PointF(
                            startPoint.X + (targetDef.X - startPoint.X) * smoothT,
                            startPoint.Y + (targetDef.Y - startPoint.Y) * smoothT
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
            //ankle = CalculateAnklePos(root, def);
            pnl_MainPaint.Invalidate();
        }
        private void Mover_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.def = targetDef;
            //ankle = CalculateAnklePos(root, def);
            pnl_MainPaint.Invalidate();

            gb_Control.Enabled = true;
            //gb_Control2.Enabled = true;
            isMoving = false;
            ButtonEnable(isFolded);
        }
        #endregion
        private float Distance(PointF a, PointF b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
        private PointF GetPointByDistance(PointF start, PointF target, float distance)
        {
            // 접은 채로 돌리기 위해 만듦
            // 1. 두 점 사이의 각도 계산 (라디안)
            float dx = target.X - start.X;
            float dy = target.Y - start.Y;
            float angle = (float)Math.Atan2(dy, dx);

            // 2. 각도와 거리를 이용하여 새로운 X, Y 계산
            float newX = start.X + (float)Math.Cos(angle) * distance;
            float newY = start.Y + (float)Math.Sin(angle) * distance;

            return new PointF(newX, newY);
        }
        private PointF GetArcPoint(PointF start, PointF target, float offsetDeg)
        {
            // 1. 반지름(r) 계산: 루트와 타겟 사이의 거리
            float dx = target.X - start.X;
            float dy = target.Y - start.Y;
            float radius = (float)Math.Sqrt(dx * dx + dy * dy);

            // 2. 기준 각도(radian) 계산: Atan2는 -PI ~ PI 사이의 값을 반환합니다.
            float baseAngleRad = (float)Math.Atan2(dy, dx);

            // 3. 오프셋 각도를 라디안으로 변환하여 더함
            float offsetRad = offsetDeg * (float)Math.PI / 180f;
            float finalAngleRad = baseAngleRad - offsetRad;

            // 4. 새로운 좌표 계산 (삼각함수)
            float newX = start.X + (float)Math.Cos(finalAngleRad) * radius;
            float newY = start.Y + (float)Math.Sin(finalAngleRad) * radius;

            return new PointF(newX, newY);
        }
        private float GetDynamicFoldAngle()
        {
            // 기존의 고정값 20f나 0.5 같은 비율을 팔 길이의 합(MaxRange)에 비례하게 변경
            float maxRange = stoA + atoD;
            return (float)(0.5 * maxRange - (maxRange * 0.1f) + minDistLimit / 2);
        }
        private PointF CalculateAnklePos(PointF p1, PointF p2, bool isFlipped = false)
        {
            float d = Distance(p1, p2);

            // --- [각도 제한을 거리 제한으로 변환] ---

            // 1. 최소 각도(minElbowAngle)일 때의 거리 계산 -> 팔이 이보다 더 접히지 않음
            float minRad = minElbowAngle * (float)Math.PI / 180f;
            float minDist = (float)Math.Sqrt(
                stoA * stoA + atoD * atoD - 2 * stoA * atoD * Math.Cos(minRad)
            );

            // 2. 최대 각도(maxElbowAngle)일 때의 거리 계산 -> 팔이 이보다 더 펴지지 않음
            float maxRad = maxElbowAngle * (float)Math.PI / 180f;
            float maxDist = (float)Math.Sqrt(
                stoA * stoA + atoD * atoD - 2 * stoA * atoD * Math.Cos(maxRad)
            );

            // 3. 물리적 한계치와 비교 (안전장치)
            float physicalMin = Math.Abs(stoA - atoD);
            float physicalMax = stoA + atoD;

            float finalMin = Math.Max(minDist, physicalMin);
            float finalMax = Math.Min(maxDist, physicalMax);

            // 4. 계산된 범위를 현재 거리 d에 적용 (Clamp)
            if (d < finalMin) d = finalMin + 0.01f;
            if (d > finalMax) d = finalMax - 0.01f;

            // --- [기존 IK 계산 로직] ---
            float cosA = (stoA * stoA + d * d - atoD * atoD) / (2 * stoA * d);
            cosA = Math.Max(-1f, Math.Min(1f, cosA));

            float angleA = (float)Math.Acos(cosA);
            float baseAngle = (float)Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);

            float finalAngle = isFlipped ? (baseAngle - angleA) : (baseAngle + angleA);

            return new PointF(p1.X + (float)Math.Cos(finalAngle) * stoA,
                              p1.Y + (float)Math.Sin(finalAngle) * stoA);
        }

        // 거리 값만 주면 현재 방향을 유지한 채 targetDef를 계산해 이동시키는 편의 함수
        private void MoveArmByDistance(float targetDistance)
        {
            if (root == PointF.Empty || isMoving) return;

            // 현재 팔이 바라보고 있는 방향(각도) 계산
            float currentAngle = (float)Math.Atan2(def.Y - root.Y, def.X - root.X);

            // 해당 방향으로 목표 거리만큼 떨어진 좌표 계산
            PointF newTarget = new PointF(
                root.X + (float)Math.Cos(currentAngle) * targetDistance,
                root.Y + (float)Math.Sin(currentAngle) * targetDistance
            );

            // 기존의 RobotMove 호출 (LINEAR 모드로 부드럽게 이동)
            RobotMove(GetArcPoint(root, newTarget, armOffsetDeg / 2), "LINEAR");
        }

        private void pnl_MainPaint_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (def == PointF.Empty) return;

            float maxRange = stoA + atoD;
            float realDist = Distance(root, def); 
            float clampedDist = Math.Max(minDistLimit, Math.Min(realDist, maxRange));
            float baseAngle = (float)Math.Atan2(def.Y - root.Y, def.X - root.X);
            float offsetRad = armOffsetDeg * (float)Math.PI / 180f;

            // 첫 번째 팔
            PointF target1 = GetPointByDistance(root, def, clampedDist); // 현재 방향 그대로
            PointF currentAnkle1 = CalculateAnklePos(root, target1, false);

            float ang1_1 = (float)Math.Atan2(currentAnkle1.Y - root.Y, currentAnkle1.X - root.X);
            float ang1_2 = (float)Math.Atan2(target1.Y - currentAnkle1.Y, target1.X - currentAnkle1.X);

            DrawRotatedImage(g, imgLink1, root, ang1_1, stoA);
            DrawRotatedImage(g, imgLink2, currentAnkle1, ang1_2, atoD);
            float targetDist2 = (maxRange + minDistLimit) - clampedDist;
            targetDist2 = Math.Min(targetDist2, maxRange - 0.01f);

            if (targetDist2 < minDistLimit)
            {
                targetDist2 = minDistLimit;
            }

            // 타겟 지점을 offset만큼 틀고 거리는 반전시킴
            PointF target2 = new PointF(
                root.X + (float)Math.Cos(baseAngle + offsetRad) * targetDist2,
                root.Y + (float)Math.Sin(baseAngle + offsetRad) * targetDist2
            );
            PointF currentAnkle2 = CalculateAnklePos(root, target2, true); // true로 반전

            float ang2_1 = (float)Math.Atan2(currentAnkle2.Y - root.Y, currentAnkle2.X - root.X);
            float ang2_2 = (float)Math.Atan2(target2.Y - currentAnkle2.Y, target2.X - currentAnkle2.X);
            
            // 1. 두 팔의 Link1 각도 차이가 180도를 넘어갈 경우를 대비한 보정 로직
            float diff = ang2_1 - ang1_1;
            while (diff > Math.PI) diff -= (float)(Math.PI * 2);
            while (diff < -Math.PI) diff += (float)(Math.PI * 2);

            // 2. 평균 각도(이등분선) 구하기
            float midAngle = ang1_1 + (diff / 2f) + rootImageAngle;

            DrawRotatedImage(g, imgLink1, root, ang2_1, stoA);
            DrawRotatedImage(g, imgLink2, currentAnkle2, ang2_2, atoD); 
            DrawRotatedImage(g, imgRoot, root, midAngle, rootImageWidth, rootImageHeight);

            // --- [조인트 그리기] ---
            DrawJoint(g, imgAnkle, currentAnkle1);
            DrawJoint(g, imgAnkle, currentAnkle2);
            //DrawJoint(g, imgRoot, root);
        }

        // 조인트 그리기 함수
        private void DrawJoint(Graphics g, Image img, PointF pos)
        {
            float jx = pos.X - img.Width / 7f;
            float jy = pos.Y - img.Height / 7f;
            g.DrawImage(img, jx, jy);
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
            //0이 아니라 -10 정도의 오프셋을 주면 link2가 link1 안쪽으로 파고듭니다.
            //float overlapOffset = -15f; // 겹치고 싶은 픽셀 양
            //g.DrawImage(img, overlapOffset, -img.Height / 2, length + Math.Abs(overlapOffset), img.Height);
            g.Restore(state);
        }
        // 루트 전용
        private void DrawRotatedImage(Graphics g, Image img, PointF position, float angleInRad, float width, float height)
        {
            if (float.IsNaN(angleInRad) || float.IsInfinity(angleInRad)) return;
            if (img == null) return;

            GraphicsState state = g.Save();

            // 1. 원점을 지정된 좌표로 이동
            g.TranslateTransform(position.X, position.Y);

            // 2. 회전 적용
            float angleInDeg = angleInRad * (180f / (float)Math.PI);
            g.RotateTransform(angleInDeg);

            // 3. 이미지를 중앙 정렬하여 지정된 크기로 그리기
            // (0, 0) 지점이 이미지의 정중앙이 되도록 좌표를 반만큼 뺍니다.
            g.DrawImage(img, -width / 2f, -height / 2f, width, height);

            g.Restore(state);
        }

        private void FoldArm()
        {
            if (root == PointF.Empty || isMoving) return;

            float baseAngle = (float)Math.Atan2(def.Y - root.Y, def.X - root.X);

            float foldDist = Math.Max(minDistLimit, Math.Abs(stoA - atoD) + FoldedSymmetryAngle);

            PointF symmetricFoldPoint = new PointF(
                root.X + (float)Math.Cos(baseAngle) * foldDist,
                root.Y + (float)Math.Sin(baseAngle) * foldDist
            );

            float defDist = Distance(root, def);
            isFolded = true;
            if (Math.Round(defDist) != stoA + atoD)
                RobotMove(GetArcPoint(root, symmetricFoldPoint, -armOffsetDeg / 2), "LINEAR");
            else
                RobotMove(GetArcPoint(root, symmetricFoldPoint, armOffsetDeg / 2), "LINEAR");
        }

        private void pnl_MainPaint_MouseMove(object sender, MouseEventArgs e)
        {
            lbl_Point_X.Text = $"X: {e.X}";
            lbl_Point_Y.Text = $"Y: {e.Y}";


            //// 첫 번째 팔 (현재 설정된 방향)
            //def = e.Location;
            //ankle = CalculateAnklePos(root, def, false);
            //pnl_MainPaint.Invalidate(); // 폼 전체가 아닌 '패널'만 다시 그리도록 요청
        }
    }
}