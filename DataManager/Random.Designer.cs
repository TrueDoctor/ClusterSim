namespace DataManager
{
    partial class Random
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.VelBar = new System.Windows.Forms.TrackBar();
            this.PosBar = new System.Windows.Forms.TrackBar();
            this.StarCount = new System.Windows.Forms.NumericUpDown();
            this.MassBar = new System.Windows.Forms.TrackBar();
            this.PosAns = new System.Windows.Forms.Label();
            this.VelAns = new System.Windows.Forms.Label();
            this.MassAns = new System.Windows.Forms.Label();
            this.Start = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.VelBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PosBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.StarCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MassBar)).BeginInit();
            this.SuspendLayout();
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(21, 114);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(120, 23);
            this.progressBar.TabIndex = 0;
            // 
            // VelBar
            // 
            this.VelBar.Location = new System.Drawing.Point(172, 63);
            this.VelBar.Maximum = 3;
            this.VelBar.Minimum = -8;
            this.VelBar.Name = "VelBar";
            this.VelBar.Size = new System.Drawing.Size(104, 45);
            this.VelBar.SmallChange = 5;
            this.VelBar.TabIndex = 2;
            this.VelBar.Value = -4;
            this.VelBar.ValueChanged += new System.EventHandler(this.VelChange);
            // 
            // PosBar
            // 
            this.PosBar.LargeChange = 100;
            this.PosBar.Location = new System.Drawing.Point(172, 12);
            this.PosBar.Minimum = 1;
            this.PosBar.Name = "PosBar";
            this.PosBar.Size = new System.Drawing.Size(104, 45);
            this.PosBar.SmallChange = 10;
            this.PosBar.TabIndex = 30;
            this.PosBar.Value = 5;
            this.PosBar.ValueChanged += new System.EventHandler(this.PosChange);
            // 
            // StarCount
            // 
            this.StarCount.Location = new System.Drawing.Point(21, 22);
            this.StarCount.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.StarCount.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.StarCount.Name = "StarCount";
            this.StarCount.Size = new System.Drawing.Size(120, 20);
            this.StarCount.TabIndex = 4;
            this.StarCount.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // MassBar
            // 
            this.MassBar.Location = new System.Drawing.Point(172, 114);
            this.MassBar.Maximum = 13;
            this.MassBar.Minimum = -1;
            this.MassBar.Name = "MassBar";
            this.MassBar.Size = new System.Drawing.Size(104, 45);
            this.MassBar.TabIndex = 5;
            this.MassBar.Value = 3;
            this.MassBar.ValueChanged += new System.EventHandler(this.MassChange);
            // 
            // PosAns
            // 
            this.PosAns.AutoSize = true;
            this.PosAns.Location = new System.Drawing.Point(283, 22);
            this.PosAns.Name = "PosAns";
            this.PosAns.Size = new System.Drawing.Size(35, 13);
            this.PosAns.TabIndex = 6;
            this.PosAns.Text = "label1";
            // 
            // VelAns
            // 
            this.VelAns.AutoSize = true;
            this.VelAns.Location = new System.Drawing.Point(282, 72);
            this.VelAns.Name = "VelAns";
            this.VelAns.Size = new System.Drawing.Size(35, 13);
            this.VelAns.TabIndex = 7;
            this.VelAns.Text = "label2";
            // 
            // MassAns
            // 
            this.MassAns.AutoSize = true;
            this.MassAns.Location = new System.Drawing.Point(282, 124);
            this.MassAns.Name = "MassAns";
            this.MassAns.Size = new System.Drawing.Size(35, 13);
            this.MassAns.TabIndex = 8;
            this.MassAns.Text = "label3";
            // 
            // Start
            // 
            this.Start.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Start.Location = new System.Drawing.Point(21, 63);
            this.Start.Name = "Start";
            this.Start.Size = new System.Drawing.Size(120, 36);
            this.Start.TabIndex = 9;
            this.Start.Text = "Start";
            this.Start.UseVisualStyleBackColor = true;
            this.Start.Click += new System.EventHandler(this.Start_Click);
            // 
            // Random
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(342, 171);
            this.Controls.Add(this.Start);
            this.Controls.Add(this.MassAns);
            this.Controls.Add(this.VelAns);
            this.Controls.Add(this.PosAns);
            this.Controls.Add(this.MassBar);
            this.Controls.Add(this.StarCount);
            this.Controls.Add(this.PosBar);
            this.Controls.Add(this.VelBar);
            this.Controls.Add(this.progressBar);
            this.Name = "Random";
            this.Text = "Random";
            ((System.ComponentModel.ISupportInitialize)(this.VelBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PosBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.StarCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MassBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TrackBar VelBar;
        private System.Windows.Forms.TrackBar PosBar;
        private System.Windows.Forms.NumericUpDown StarCount;
        private System.Windows.Forms.TrackBar MassBar;
        private System.Windows.Forms.Label PosAns;
        private System.Windows.Forms.Label VelAns;
        private System.Windows.Forms.Label MassAns;
        private System.Windows.Forms.Button Start;
    }
}