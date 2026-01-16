
namespace Winform_RobotArmUI_new
{
    partial class MainForm
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btn_Fold2 = new System.Windows.Forms.Button();
            this.btn_Unfold2 = new System.Windows.Forms.Button();
            this.gb_LowerFold = new System.Windows.Forms.GroupBox();
            this.btn_Fold = new System.Windows.Forms.Button();
            this.btn_Unfold = new System.Windows.Forms.Button();
            this.gb_Control = new System.Windows.Forms.GroupBox();
            this.btn_Pm1 = new System.Windows.Forms.Button();
            this.btn_Ll2 = new System.Windows.Forms.Button();
            this.btn_Pm2 = new System.Windows.Forms.Button();
            this.btn_Ll1 = new System.Windows.Forms.Button();
            this.btn_Pm3 = new System.Windows.Forms.Button();
            this.btn_Pm4 = new System.Windows.Forms.Button();
            this.pnl_MainPaint = new System.Windows.Forms.Panel();
            this.lbl_MiddelSpot = new System.Windows.Forms.Label();
            this.lbl_Point_Y = new System.Windows.Forms.Label();
            this.lbl_Point_X = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btn_MousePoint = new System.Windows.Forms.Button();
            this.btn_Load = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.gb_LowerFold.SuspendLayout();
            this.gb_Control.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btn_Fold2);
            this.groupBox1.Controls.Add(this.btn_Unfold2);
            this.groupBox1.Location = new System.Drawing.Point(708, 503);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(113, 83);
            this.groupBox1.TabIndex = 25;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Upper";
            // 
            // btn_Fold2
            // 
            this.btn_Fold2.Enabled = false;
            this.btn_Fold2.Location = new System.Drawing.Point(7, 22);
            this.btn_Fold2.Name = "btn_Fold2";
            this.btn_Fold2.Size = new System.Drawing.Size(100, 23);
            this.btn_Fold2.TabIndex = 14;
            this.btn_Fold2.Text = "fold";
            this.btn_Fold2.UseVisualStyleBackColor = true;
            this.btn_Fold2.Click += new System.EventHandler(this.btn_Fold2_Click);
            // 
            // btn_Unfold2
            // 
            this.btn_Unfold2.Enabled = false;
            this.btn_Unfold2.Location = new System.Drawing.Point(7, 51);
            this.btn_Unfold2.Name = "btn_Unfold2";
            this.btn_Unfold2.Size = new System.Drawing.Size(100, 23);
            this.btn_Unfold2.TabIndex = 15;
            this.btn_Unfold2.Text = "unfold";
            this.btn_Unfold2.UseVisualStyleBackColor = true;
            this.btn_Unfold2.Click += new System.EventHandler(this.btn_Unfold2_Click);
            // 
            // gb_LowerFold
            // 
            this.gb_LowerFold.Controls.Add(this.btn_Fold);
            this.gb_LowerFold.Controls.Add(this.btn_Unfold);
            this.gb_LowerFold.Location = new System.Drawing.Point(710, 414);
            this.gb_LowerFold.Name = "gb_LowerFold";
            this.gb_LowerFold.Size = new System.Drawing.Size(113, 83);
            this.gb_LowerFold.TabIndex = 23;
            this.gb_LowerFold.TabStop = false;
            this.gb_LowerFold.Text = "Lower";
            // 
            // btn_Fold
            // 
            this.btn_Fold.Enabled = false;
            this.btn_Fold.Location = new System.Drawing.Point(7, 20);
            this.btn_Fold.Name = "btn_Fold";
            this.btn_Fold.Size = new System.Drawing.Size(100, 23);
            this.btn_Fold.TabIndex = 14;
            this.btn_Fold.Text = "fold";
            this.btn_Fold.UseVisualStyleBackColor = true;
            this.btn_Fold.Click += new System.EventHandler(this.btn_Fold_Click);
            // 
            // btn_Unfold
            // 
            this.btn_Unfold.Enabled = false;
            this.btn_Unfold.Location = new System.Drawing.Point(7, 49);
            this.btn_Unfold.Name = "btn_Unfold";
            this.btn_Unfold.Size = new System.Drawing.Size(100, 23);
            this.btn_Unfold.TabIndex = 15;
            this.btn_Unfold.Text = "unfold";
            this.btn_Unfold.UseVisualStyleBackColor = true;
            this.btn_Unfold.Click += new System.EventHandler(this.btn_Unfold_Click);
            // 
            // gb_Control
            // 
            this.gb_Control.Controls.Add(this.btn_Load);
            this.gb_Control.Controls.Add(this.btn_Pm1);
            this.gb_Control.Controls.Add(this.btn_Ll2);
            this.gb_Control.Controls.Add(this.btn_Pm2);
            this.gb_Control.Controls.Add(this.btn_Ll1);
            this.gb_Control.Controls.Add(this.btn_Pm3);
            this.gb_Control.Controls.Add(this.btn_Pm4);
            this.gb_Control.Enabled = false;
            this.gb_Control.Location = new System.Drawing.Point(710, 160);
            this.gb_Control.Name = "gb_Control";
            this.gb_Control.Size = new System.Drawing.Size(113, 226);
            this.gb_Control.TabIndex = 24;
            this.gb_Control.TabStop = false;
            this.gb_Control.Text = "Control_Lower";
            // 
            // btn_Pm1
            // 
            this.btn_Pm1.Location = new System.Drawing.Point(6, 20);
            this.btn_Pm1.Name = "btn_Pm1";
            this.btn_Pm1.Size = new System.Drawing.Size(101, 20);
            this.btn_Pm1.TabIndex = 7;
            this.btn_Pm1.Text = "PM1";
            this.btn_Pm1.UseVisualStyleBackColor = true;
            this.btn_Pm1.Click += new System.EventHandler(this.btn_Pm1_Click);
            // 
            // btn_Ll2
            // 
            this.btn_Ll2.Location = new System.Drawing.Point(6, 165);
            this.btn_Ll2.Name = "btn_Ll2";
            this.btn_Ll2.Size = new System.Drawing.Size(101, 20);
            this.btn_Ll2.TabIndex = 12;
            this.btn_Ll2.Text = "LL2";
            this.btn_Ll2.UseVisualStyleBackColor = true;
            this.btn_Ll2.Click += new System.EventHandler(this.btn_Ll2_Click);
            // 
            // btn_Pm2
            // 
            this.btn_Pm2.Location = new System.Drawing.Point(6, 49);
            this.btn_Pm2.Name = "btn_Pm2";
            this.btn_Pm2.Size = new System.Drawing.Size(101, 20);
            this.btn_Pm2.TabIndex = 8;
            this.btn_Pm2.Text = "PM2";
            this.btn_Pm2.UseVisualStyleBackColor = true;
            this.btn_Pm2.Click += new System.EventHandler(this.btn_Pm2_Click);
            // 
            // btn_Ll1
            // 
            this.btn_Ll1.Location = new System.Drawing.Point(6, 136);
            this.btn_Ll1.Name = "btn_Ll1";
            this.btn_Ll1.Size = new System.Drawing.Size(101, 20);
            this.btn_Ll1.TabIndex = 11;
            this.btn_Ll1.Text = "LL1";
            this.btn_Ll1.UseVisualStyleBackColor = true;
            this.btn_Ll1.Click += new System.EventHandler(this.btn_Ll1_Click);
            // 
            // btn_Pm3
            // 
            this.btn_Pm3.Location = new System.Drawing.Point(6, 78);
            this.btn_Pm3.Name = "btn_Pm3";
            this.btn_Pm3.Size = new System.Drawing.Size(101, 20);
            this.btn_Pm3.TabIndex = 9;
            this.btn_Pm3.Text = "PM3";
            this.btn_Pm3.UseVisualStyleBackColor = true;
            this.btn_Pm3.Click += new System.EventHandler(this.btn_Pm3_Click);
            // 
            // btn_Pm4
            // 
            this.btn_Pm4.Location = new System.Drawing.Point(6, 107);
            this.btn_Pm4.Name = "btn_Pm4";
            this.btn_Pm4.Size = new System.Drawing.Size(101, 20);
            this.btn_Pm4.TabIndex = 10;
            this.btn_Pm4.Text = "PM4";
            this.btn_Pm4.UseVisualStyleBackColor = true;
            this.btn_Pm4.Click += new System.EventHandler(this.btn_Pm4_Click);
            // 
            // pnl_MainPaint
            // 
            this.pnl_MainPaint.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.pnl_MainPaint.BackgroundImage = global::Winform_RobotArmUI_new.Properties.Resources.MainPaint;
            this.pnl_MainPaint.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pnl_MainPaint.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnl_MainPaint.Location = new System.Drawing.Point(12, 12);
            this.pnl_MainPaint.Name = "pnl_MainPaint";
            this.pnl_MainPaint.Size = new System.Drawing.Size(690, 617);
            this.pnl_MainPaint.TabIndex = 18;
            this.pnl_MainPaint.Paint += new System.Windows.Forms.PaintEventHandler(this.pnl_MainPaint_Paint);
            this.pnl_MainPaint.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pnl_MainPaint_MouseClick);
            this.pnl_MainPaint.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.pnl_MainPaint_MouseDoubleClick);
            this.pnl_MainPaint.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pnl_MainPaint_MouseMove);
            // 
            // lbl_MiddelSpot
            // 
            this.lbl_MiddelSpot.AutoSize = true;
            this.lbl_MiddelSpot.Location = new System.Drawing.Point(708, 96);
            this.lbl_MiddelSpot.Name = "lbl_MiddelSpot";
            this.lbl_MiddelSpot.Size = new System.Drawing.Size(57, 12);
            this.lbl_MiddelSpot.TabIndex = 22;
            this.lbl_MiddelSpot.Text = "중앙: 0, 0";
            // 
            // lbl_Point_Y
            // 
            this.lbl_Point_Y.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lbl_Point_Y.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_Point_Y.Location = new System.Drawing.Point(710, 62);
            this.lbl_Point_Y.Name = "lbl_Point_Y";
            this.lbl_Point_Y.Size = new System.Drawing.Size(100, 25);
            this.lbl_Point_Y.TabIndex = 21;
            this.lbl_Point_Y.Text = "y:";
            this.lbl_Point_Y.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbl_Point_X
            // 
            this.lbl_Point_X.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lbl_Point_X.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_Point_X.Location = new System.Drawing.Point(710, 30);
            this.lbl_Point_X.Name = "lbl_Point_X";
            this.lbl_Point_X.Size = new System.Drawing.Size(100, 23);
            this.lbl_Point_X.TabIndex = 20;
            this.lbl_Point_X.Text = "x:";
            this.lbl_Point_X.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(708, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 12);
            this.label1.TabIndex = 19;
            this.label1.Text = "Mouse Point";
            // 
            // btn_MousePoint
            // 
            this.btn_MousePoint.Location = new System.Drawing.Point(708, 111);
            this.btn_MousePoint.Name = "btn_MousePoint";
            this.btn_MousePoint.Size = new System.Drawing.Size(75, 23);
            this.btn_MousePoint.TabIndex = 17;
            this.btn_MousePoint.Text = "마우스";
            this.btn_MousePoint.UseVisualStyleBackColor = true;
            this.btn_MousePoint.Click += new System.EventHandler(this.btn_MousePoint_Click);
            // 
            // btn_Load
            // 
            this.btn_Load.Location = new System.Drawing.Point(6, 194);
            this.btn_Load.Name = "btn_Load";
            this.btn_Load.Size = new System.Drawing.Size(101, 20);
            this.btn_Load.TabIndex = 13;
            this.btn_Load.Text = "Load";
            this.btn_Load.UseVisualStyleBackColor = true;
            this.btn_Load.Click += new System.EventHandler(this.btn_Load_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.ClientSize = new System.Drawing.Size(830, 656);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.gb_LowerFold);
            this.Controls.Add(this.gb_Control);
            this.Controls.Add(this.pnl_MainPaint);
            this.Controls.Add(this.lbl_MiddelSpot);
            this.Controls.Add(this.lbl_Point_Y);
            this.Controls.Add(this.lbl_Point_X);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btn_MousePoint);
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.groupBox1.ResumeLayout(false);
            this.gb_LowerFold.ResumeLayout(false);
            this.gb_Control.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btn_Fold2;
        private System.Windows.Forms.Button btn_Unfold2;
        private System.Windows.Forms.GroupBox gb_LowerFold;
        private System.Windows.Forms.Button btn_Fold;
        private System.Windows.Forms.Button btn_Unfold;
        private System.Windows.Forms.GroupBox gb_Control;
        private System.Windows.Forms.Button btn_Pm1;
        private System.Windows.Forms.Button btn_Ll2;
        private System.Windows.Forms.Button btn_Pm2;
        private System.Windows.Forms.Button btn_Ll1;
        private System.Windows.Forms.Button btn_Pm3;
        private System.Windows.Forms.Button btn_Pm4;
        private System.Windows.Forms.Panel pnl_MainPaint;
        private System.Windows.Forms.Label lbl_MiddelSpot;
        private System.Windows.Forms.Label lbl_Point_Y;
        private System.Windows.Forms.Label lbl_Point_X;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btn_MousePoint;
        private System.Windows.Forms.Button btn_Load;
    }
}

