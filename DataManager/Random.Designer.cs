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
            this.components = new System.ComponentModel.Container();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.VelBar = new System.Windows.Forms.TrackBar();
            this.PosBar = new System.Windows.Forms.TrackBar();
            this.StarCount = new System.Windows.Forms.NumericUpDown();
            this.MassVBar = new System.Windows.Forms.TrackBar();
            this.PosAns = new System.Windows.Forms.Label();
            this.VelAns = new System.Windows.Forms.Label();
            this.MassVAns = new System.Windows.Forms.Label();
            this.Start = new System.Windows.Forms.Button();
            this.StarCountLabel = new System.Windows.Forms.Label();
            this.normalVariance = new System.Windows.Forms.Label();
            this.MassMBar = new System.Windows.Forms.TrackBar();
            this.MassMAns = new System.Windows.Forms.Label();
            this.BarAns = new System.Windows.Forms.Label();
            this.Kroupa = new System.Windows.Forms.CheckBox();
            this.Relaxation = new System.Windows.Forms.Label();
            this.calc = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.VelBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PosBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.StarCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MassVBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MassMBar)).BeginInit();
            this.SuspendLayout();
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(12, 163);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(120, 23);
            this.progressBar.TabIndex = 0;
            // 
            // VelBar
            // 
            this.VelBar.LargeChange = 1;
            this.VelBar.Location = new System.Drawing.Point(163, 71);
            this.VelBar.Maximum = 2;
            this.VelBar.Minimum = -10;
            this.VelBar.Name = "VelBar";
            this.VelBar.Size = new System.Drawing.Size(104, 45);
            this.VelBar.SmallChange = 5;
            this.VelBar.TabIndex = 2;
            this.VelBar.Value = -4;
            this.VelBar.ValueChanged += new System.EventHandler(this.VelChange);
            // 
            // PosBar
            // 
            this.PosBar.LargeChange = 1;
            this.PosBar.Location = new System.Drawing.Point(163, 32);
            this.PosBar.Minimum = -2;
            this.PosBar.Name = "PosBar";
            this.PosBar.Size = new System.Drawing.Size(104, 45);
            this.PosBar.SmallChange = 10;
            this.PosBar.TabIndex = 30;
            this.PosBar.Value = 3;
            this.PosBar.ValueChanged += new System.EventHandler(this.PosChange);
            // 
            // StarCount
            // 
            this.StarCount.Location = new System.Drawing.Point(12, 32);
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
            // MassVBar
            // 
            this.MassVBar.LargeChange = 1;
            this.MassVBar.Location = new System.Drawing.Point(163, 122);
            this.MassVBar.Maximum = 8;
            this.MassVBar.Minimum = -2;
            this.MassVBar.Name = "MassVBar";
            this.MassVBar.Size = new System.Drawing.Size(104, 45);
            this.MassVBar.TabIndex = 5;
            this.MassVBar.Value = 3;
            this.MassVBar.ValueChanged += new System.EventHandler(this.MassChange);
            // 
            // PosAns
            // 
            this.PosAns.AutoSize = true;
            this.PosAns.Location = new System.Drawing.Point(274, 39);
            this.PosAns.Name = "PosAns";
            this.PosAns.Size = new System.Drawing.Size(21, 13);
            this.PosAns.TabIndex = 6;
            this.PosAns.Text = "Ort";
            // 
            // VelAns
            // 
            this.VelAns.AutoSize = true;
            this.VelAns.Location = new System.Drawing.Point(273, 80);
            this.VelAns.Name = "VelAns";
            this.VelAns.Size = new System.Drawing.Size(85, 13);
            this.VelAns.TabIndex = 7;
            this.VelAns.Text = "Geschwindigkeit";
            // 
            // MassVAns
            // 
            this.MassVAns.AutoSize = true;
            this.MassVAns.Location = new System.Drawing.Point(273, 126);
            this.MassVAns.Name = "MassVAns";
            this.MassVAns.Size = new System.Drawing.Size(100, 13);
            this.MassVAns.TabIndex = 8;
            this.MassVAns.Text = "Masse Abweichung";
            // 
            // Start
            // 
            this.Start.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Start.Location = new System.Drawing.Point(12, 103);
            this.Start.Name = "Start";
            this.Start.Size = new System.Drawing.Size(120, 36);
            this.Start.TabIndex = 9;
            this.Start.Text = "Start";
            this.Start.UseVisualStyleBackColor = true;
            this.Start.Click += new System.EventHandler(this.Start_Click);
            // 
            // StarCountLabel
            // 
            this.StarCountLabel.AutoSize = true;
            this.StarCountLabel.Location = new System.Drawing.Point(9, 9);
            this.StarCountLabel.Name = "StarCountLabel";
            this.StarCountLabel.Size = new System.Drawing.Size(66, 13);
            this.StarCountLabel.TabIndex = 31;
            this.StarCountLabel.Text = "Sternanzahl:";
            // 
            // normalVariance
            // 
            this.normalVariance.AutoSize = true;
            this.normalVariance.Location = new System.Drawing.Point(160, 9);
            this.normalVariance.Name = "normalVariance";
            this.normalVariance.Size = new System.Drawing.Size(115, 13);
            this.normalVariance.TabIndex = 32;
            this.normalVariance.Text = "Standard Abweichung:";
            // 
            // MassMBar
            // 
            this.MassMBar.LargeChange = 1;
            this.MassMBar.Location = new System.Drawing.Point(163, 163);
            this.MassMBar.Maximum = 8;
            this.MassMBar.Minimum = -2;
            this.MassMBar.Name = "MassMBar";
            this.MassMBar.Size = new System.Drawing.Size(104, 45);
            this.MassMBar.TabIndex = 33;
            this.MassMBar.Value = 3;
            this.MassMBar.Scroll += new System.EventHandler(this.MassMBar_Scroll);
            // 
            // MassMAns
            // 
            this.MassMAns.AutoSize = true;
            this.MassMAns.Location = new System.Drawing.Point(273, 171);
            this.MassMAns.Name = "MassMAns";
            this.MassMAns.Size = new System.Drawing.Size(66, 13);
            this.MassMAns.TabIndex = 34;
            this.MassMAns.Text = "Masse Mittel";
            // 
            // BarAns
            // 
            this.BarAns.AutoSize = true;
            this.BarAns.Location = new System.Drawing.Point(12, 142);
            this.BarAns.Name = "BarAns";
            this.BarAns.Size = new System.Drawing.Size(40, 13);
            this.BarAns.TabIndex = 35;
            this.BarAns.Text = "Status:";
            this.BarAns.Visible = false;
            // 
            // Kroupa
            // 
            this.Kroupa.AutoSize = true;
            this.Kroupa.Location = new System.Drawing.Point(12, 80);
            this.Kroupa.Name = "Kroupa";
            this.Kroupa.Size = new System.Drawing.Size(95, 17);
            this.Kroupa.TabIndex = 36;
            this.Kroupa.Text = "Kroupa nutzen";
            this.Kroupa.UseVisualStyleBackColor = true;
            this.Kroupa.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // Relaxation
            // 
            this.Relaxation.AutoSize = true;
            this.Relaxation.Location = new System.Drawing.Point(9, 64);
            this.Relaxation.Name = "Relaxation";
            this.Relaxation.Size = new System.Drawing.Size(57, 13);
            this.Relaxation.TabIndex = 37;
            this.Relaxation.Text = "Relaxation";
            // 
            // calc
            // 
            this.calc.Enabled = true;
            this.calc.Tick += new System.EventHandler(this.calc_Tick);
            // 
            // Random
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(373, 193);
            this.Controls.Add(this.Relaxation);
            this.Controls.Add(this.Kroupa);
            this.Controls.Add(this.BarAns);
            this.Controls.Add(this.MassMAns);
            this.Controls.Add(this.MassMBar);
            this.Controls.Add(this.normalVariance);
            this.Controls.Add(this.StarCountLabel);
            this.Controls.Add(this.Start);
            this.Controls.Add(this.MassVAns);
            this.Controls.Add(this.VelAns);
            this.Controls.Add(this.PosAns);
            this.Controls.Add(this.MassVBar);
            this.Controls.Add(this.StarCount);
            this.Controls.Add(this.PosBar);
            this.Controls.Add(this.VelBar);
            this.Controls.Add(this.progressBar);
            this.Name = "Random";
            this.Text = "Random";
            ((System.ComponentModel.ISupportInitialize)(this.VelBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PosBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.StarCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MassVBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MassMBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TrackBar VelBar;
        private System.Windows.Forms.TrackBar PosBar;
        private System.Windows.Forms.NumericUpDown StarCount;
        private System.Windows.Forms.TrackBar MassVBar;
        private System.Windows.Forms.Label PosAns;
        private System.Windows.Forms.Label VelAns;
        private System.Windows.Forms.Label MassVAns;
        private System.Windows.Forms.Button Start;
        private System.Windows.Forms.Label StarCountLabel;
        private System.Windows.Forms.Label normalVariance;
        private System.Windows.Forms.TrackBar MassMBar;
        private System.Windows.Forms.Label MassMAns;
        private System.Windows.Forms.Label BarAns;
        private System.Windows.Forms.CheckBox Kroupa;
        private System.Windows.Forms.Label Relaxation;
        private System.Windows.Forms.Timer calc;
    }
}