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
            this.ClusterName = new System.Windows.Forms.Label();
            this.Energy = new System.Windows.Forms.Button();
            this.Density = new System.Windows.Forms.Button();
            this.Relaxation = new System.Windows.Forms.Button();
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
            // Analysis
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
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
    }
}