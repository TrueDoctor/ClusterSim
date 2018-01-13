namespace ClusterSim.DataManager
{
    partial class DataManager
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.ServerList = new System.Windows.Forms.ListBox();
            this.AddTable = new System.Windows.Forms.Button();
            this.SchritteAns = new System.Windows.Forms.Label();
            this.SterneAns = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.newTableContainer = new System.Windows.Forms.GroupBox();
            this.randomTable = new System.Windows.Forms.Button();
            this.newTableName = new System.Windows.Forms.TextBox();
            this.ListRefresh = new System.Windows.Forms.Timer(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.DataView = new System.Windows.Forms.Button();
            this.ClusterSim = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.Check = new System.Windows.Forms.Button();
            this.Download = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.Analysis = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.newTableContainer.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // ServerList
            // 
            this.ServerList.BackColor = System.Drawing.Color.DimGray;
            this.ServerList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ServerList.FormattingEnabled = true;
            this.ServerList.Items.AddRange(new object[] {
            "Lade Liste...",
            "Warscheinlich ",
            "Verbindungsproblem"});
            this.ServerList.Location = new System.Drawing.Point(33, 12);
            this.ServerList.Name = "ServerList";
            this.ServerList.Size = new System.Drawing.Size(125, 169);
            this.ServerList.TabIndex = 1;
            this.ServerList.SelectedIndexChanged += new System.EventHandler(this.ListIndexChange);
            this.ServerList.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ServerList_KeyDown);
            // 
            // AddTable
            // 
            this.AddTable.Location = new System.Drawing.Point(6, 54);
            this.AddTable.Name = "AddTable";
            this.AddTable.Size = new System.Drawing.Size(62, 23);
            this.AddTable.TabIndex = 4;
            this.AddTable.Text = "Leer";
            this.AddTable.UseVisualStyleBackColor = true;
            this.AddTable.Click += new System.EventHandler(this.AddTable_Click);
            // 
            // SchritteAns
            // 
            this.SchritteAns.AutoSize = true;
            this.SchritteAns.Location = new System.Drawing.Point(6, 34);
            this.SchritteAns.Name = "SchritteAns";
            this.SchritteAns.Size = new System.Drawing.Size(49, 13);
            this.SchritteAns.TabIndex = 5;
            this.SchritteAns.Text = "Schritte :";
            // 
            // SterneAns
            // 
            this.SterneAns.AutoSize = true;
            this.SterneAns.Location = new System.Drawing.Point(6, 21);
            this.SterneAns.Name = "SterneAns";
            this.SterneAns.Size = new System.Drawing.Size(44, 13);
            this.SterneAns.TabIndex = 6;
            this.SterneAns.Text = "Sterne :";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.SchritteAns);
            this.groupBox1.Controls.Add(this.SterneAns);
            this.groupBox1.Location = new System.Drawing.Point(188, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(146, 60);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Details";
            // 
            // newTableContainer
            // 
            this.newTableContainer.Controls.Add(this.randomTable);
            this.newTableContainer.Controls.Add(this.newTableName);
            this.newTableContainer.Controls.Add(this.AddTable);
            this.newTableContainer.Location = new System.Drawing.Point(188, 96);
            this.newTableContainer.Name = "newTableContainer";
            this.newTableContainer.Size = new System.Drawing.Size(146, 85);
            this.newTableContainer.TabIndex = 10;
            this.newTableContainer.TabStop = false;
            this.newTableContainer.Text = "Neue Tabelle";
            // 
            // randomTable
            // 
            this.randomTable.Location = new System.Drawing.Point(74, 54);
            this.randomTable.Name = "randomTable";
            this.randomTable.Size = new System.Drawing.Size(66, 23);
            this.randomTable.TabIndex = 12;
            this.randomTable.Text = "Zufällig";
            this.randomTable.UseVisualStyleBackColor = true;
            this.randomTable.Click += new System.EventHandler(this.randomTable_Click);
            // 
            // newTableName
            // 
            this.newTableName.Location = new System.Drawing.Point(6, 28);
            this.newTableName.Name = "newTableName";
            this.newTableName.Size = new System.Drawing.Size(134, 20);
            this.newTableName.TabIndex = 11;
            this.newTableName.Text = "Name";
            // 
            // ListRefresh
            // 
            this.ListRefresh.Interval = 500;
            this.ListRefresh.Tick += new System.EventHandler(this.ListRefresh_Tick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(-6, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 31);
            this.label1.TabIndex = 11;
            this.label1.Text = "↻";
            this.label1.Click += new System.EventHandler(this.Refresh_Click);
            // 
            // DataView
            // 
            this.DataView.Location = new System.Drawing.Point(6, 14);
            this.DataView.Name = "DataView";
            this.DataView.Size = new System.Drawing.Size(62, 23);
            this.DataView.TabIndex = 12;
            this.DataView.Text = "DataView";
            this.DataView.UseVisualStyleBackColor = true;
            this.DataView.Click += new System.EventHandler(this.DataView_Click);
            // 
            // ClusterSim
            // 
            this.ClusterSim.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ClusterSim.Location = new System.Drawing.Point(74, 14);
            this.ClusterSim.Name = "ClusterSim";
            this.ClusterSim.Size = new System.Drawing.Size(66, 23);
            this.ClusterSim.TabIndex = 13;
            this.ClusterSim.Text = "ClusterSim";
            this.ClusterSim.UseVisualStyleBackColor = true;
            this.ClusterSim.Click += new System.EventHandler(this.ClusterSim_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(12, 237);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(322, 23);
            this.progressBar.TabIndex = 14;
            this.progressBar.Visible = false;
            // 
            // Check
            // 
            this.Check.Location = new System.Drawing.Point(12, 208);
            this.Check.Name = "Check";
            this.Check.Size = new System.Drawing.Size(70, 23);
            this.Check.TabIndex = 13;
            this.Check.Text = "Überprüfen";
            this.Check.UseVisualStyleBackColor = true;
            this.Check.Click += new System.EventHandler(this.Check_Click);
            // 
            // Download
            // 
            this.Download.Location = new System.Drawing.Point(88, 208);
            this.Download.Name = "Download";
            this.Download.Size = new System.Drawing.Size(70, 23);
            this.Download.TabIndex = 15;
            this.Download.Text = "Download";
            this.Download.UseVisualStyleBackColor = true;
            this.Download.Click += new System.EventHandler(this.Download_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.ClusterSim);
            this.groupBox2.Controls.Add(this.DataView);
            this.groupBox2.Location = new System.Drawing.Point(188, 187);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(146, 43);
            this.groupBox2.TabIndex = 16;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Export";
            // 
            // Analysis
            // 
            this.Analysis.Location = new System.Drawing.Point(13, 177);
            this.Analysis.Name = "Analysis";
            this.Analysis.Size = new System.Drawing.Size(69, 23);
            this.Analysis.TabIndex = 17;
            this.Analysis.Text = "Analyse";
            this.Analysis.UseVisualStyleBackColor = true;
            this.Analysis.Click += new System.EventHandler(this.ClusterAnalysis);
            // 
            // DataManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.ClientSize = new System.Drawing.Size(342, 266);
            this.Controls.Add(this.Analysis);
            this.Controls.Add(this.Download);
            this.Controls.Add(this.Check);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.newTableContainer);
            this.Controls.Add(this.ServerList);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.Name = "DataManager";
            this.Text = "DataManager";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.newTableContainer.ResumeLayout(false);
            this.newTableContainer.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox ServerList;
        private System.Windows.Forms.Button AddTable;
        private System.Windows.Forms.Label SchritteAns;
        private System.Windows.Forms.Label SterneAns;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox newTableContainer;
        private System.Windows.Forms.TextBox newTableName;
        private System.Windows.Forms.Timer ListRefresh;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button DataView;
        private System.Windows.Forms.Button randomTable;
        private System.Windows.Forms.Button ClusterSim;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Button Check;
        private System.Windows.Forms.Button Download;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button Analysis;
    }
}

