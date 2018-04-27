namespace ClusterSim.ClusterLib.Analysis
{
    partial class Analysis
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
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
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
            this.ClusterName = new System.Windows.Forms.Label();
            this.Energy = new System.Windows.Forms.Button();
            this.Density = new System.Windows.Forms.Button();
            this.Relaxation = new System.Windows.Forms.Button();
            this.Efficiency = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.ClusterLifetime = new System.Windows.Forms.Label();
            this.DatapointInput = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // ClusterName
            // 
            this.ClusterName.AutoSize = true;
            this.ClusterName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ClusterName.Location = new System.Drawing.Point(13, 13);
            this.ClusterName.Name = "ClusterName";
            this.ClusterName.Size = new System.Drawing.Size(104, 20);
            this.ClusterName.TabIndex = 0;
            this.ClusterName.Text = "Analyse für:";
            // 
            // Energy
            // 
            this.Energy.Location = new System.Drawing.Point(28, 79);
            this.Energy.Name = "Energy";
            this.Energy.Size = new System.Drawing.Size(101, 23);
            this.Energy.TabIndex = 1;
            this.Energy.Text = "Energy Analysis";
            this.Energy.UseVisualStyleBackColor = true;
            this.Energy.Click += new System.EventHandler(this.EnergyAnalysis);
            // 
            // Density
            // 
            this.Density.Location = new System.Drawing.Point(28, 119);
            this.Density.Name = "Density";
            this.Density.Size = new System.Drawing.Size(101, 23);
            this.Density.TabIndex = 2;
            this.Density.Text = "Dichte";
            this.Density.UseVisualStyleBackColor = true;
            this.Density.Click += new System.EventHandler(this.DensityAnalysis);
            // 
            // Relaxation
            // 
            this.Relaxation.Location = new System.Drawing.Point(161, 78);
            this.Relaxation.Name = "Relaxation";
            this.Relaxation.Size = new System.Drawing.Size(75, 23);
            this.Relaxation.TabIndex = 3;
            this.Relaxation.Text = "Relaxation";
            this.Relaxation.UseVisualStyleBackColor = true;
            this.Relaxation.Click += new System.EventHandler(this.RelaxationTime);
            // 
            // Efficiency
            // 
            this.Efficiency.Location = new System.Drawing.Point(161, 118);
            this.Efficiency.Name = "Efficiency";
            this.Efficiency.Size = new System.Drawing.Size(75, 23);
            this.Efficiency.TabIndex = 4;
            this.Efficiency.Text = "Efficiency";
            this.Efficiency.UseVisualStyleBackColor = true;
            this.Efficiency.Click += new System.EventHandler(this.EfficiencyAnalysis);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(28, 207);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(208, 23);
            this.progressBar.TabIndex = 5;
            // 
            // ClusterLifetime
            // 
            this.ClusterLifetime.AutoSize = true;
            this.ClusterLifetime.Location = new System.Drawing.Point(17, 46);
            this.ClusterLifetime.Name = "ClusterLifetime";
            this.ClusterLifetime.Size = new System.Drawing.Size(61, 13);
            this.ClusterLifetime.TabIndex = 6;
            this.ClusterLifetime.Text = "Lebenszeit:";
            this.ClusterLifetime.Click += new System.EventHandler(this.CalcLivetime);
            // 
            // DatapointInput
            // 
            this.DatapointInput.Location = new System.Drawing.Point(161, 38);
            this.DatapointInput.Name = "DatapointInput";
            this.DatapointInput.Size = new System.Drawing.Size(75, 20);
            this.DatapointInput.TabIndex = 7;
            this.DatapointInput.Text = "200";
            this.DatapointInput.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // Analysis
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.DatapointInput);
            this.Controls.Add(this.ClusterLifetime);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.Efficiency);
            this.Controls.Add(this.Relaxation);
            this.Controls.Add(this.Density);
            this.Controls.Add(this.Energy);
            this.Controls.Add(this.ClusterName);
            this.Name = "Analysis";
            this.Text = "Analysis";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label ClusterName;
        private System.Windows.Forms.Button Energy;
        private System.Windows.Forms.Button Density;
        private System.Windows.Forms.Button Relaxation;
        private System.Windows.Forms.Button Efficiency;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label ClusterLifetime;
        private System.Windows.Forms.TextBox DatapointInput;
    }
}