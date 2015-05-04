namespace CardGame
{
    partial class Form1
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
            this.Board = new System.Windows.Forms.PictureBox();
            this.PlayersCard = new System.Windows.Forms.PictureBox();
            this.ServerList = new System.Windows.Forms.ComboBox();
            this.FindButton = new System.Windows.Forms.Button();
            this.ConnectButton = new System.Windows.Forms.Button();
            this.PlayerName = new System.Windows.Forms.TextBox();
            this.ServerStart = new System.Windows.Forms.Button();
            this.ServerLog = new System.Windows.Forms.RichTextBox();
            this.ClientLog = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.Board)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PlayersCard)).BeginInit();
            this.SuspendLayout();
            // 
            // Board
            // 
            this.Board.Location = new System.Drawing.Point(12, 12);
            this.Board.Name = "Board";
            this.Board.Size = new System.Drawing.Size(570, 290);
            this.Board.TabIndex = 0;
            this.Board.TabStop = false;
            this.Board.Click += new System.EventHandler(this.Board_Click);
            // 
            // PlayersCard
            // 
            this.PlayersCard.Location = new System.Drawing.Point(12, 322);
            this.PlayersCard.Name = "PlayersCard";
            this.PlayersCard.Size = new System.Drawing.Size(570, 156);
            this.PlayersCard.TabIndex = 1;
            this.PlayersCard.TabStop = false;
            this.PlayersCard.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PlayersCard_MouseDown);
            // 
            // ServerList
            // 
            this.ServerList.FormattingEnabled = true;
            this.ServerList.Location = new System.Drawing.Point(601, 31);
            this.ServerList.Name = "ServerList";
            this.ServerList.Size = new System.Drawing.Size(121, 21);
            this.ServerList.TabIndex = 2;
            // 
            // FindButton
            // 
            this.FindButton.Location = new System.Drawing.Point(601, 58);
            this.FindButton.Name = "FindButton";
            this.FindButton.Size = new System.Drawing.Size(121, 23);
            this.FindButton.TabIndex = 3;
            this.FindButton.Text = "Найти сервера";
            this.FindButton.UseVisualStyleBackColor = true;
            this.FindButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // ConnectButton
            // 
            this.ConnectButton.Location = new System.Drawing.Point(601, 115);
            this.ConnectButton.Name = "ConnectButton";
            this.ConnectButton.Size = new System.Drawing.Size(121, 23);
            this.ConnectButton.TabIndex = 4;
            this.ConnectButton.Text = "Подключиться";
            this.ConnectButton.UseVisualStyleBackColor = true;
            this.ConnectButton.Click += new System.EventHandler(this.ConnectButton_Click);
            // 
            // PlayerName
            // 
            this.PlayerName.Location = new System.Drawing.Point(601, 89);
            this.PlayerName.Name = "PlayerName";
            this.PlayerName.Size = new System.Drawing.Size(121, 20);
            this.PlayerName.TabIndex = 5;
            // 
            // ServerStart
            // 
            this.ServerStart.Location = new System.Drawing.Point(601, 144);
            this.ServerStart.Name = "ServerStart";
            this.ServerStart.Size = new System.Drawing.Size(121, 23);
            this.ServerStart.TabIndex = 6;
            this.ServerStart.Text = "Создать сервер";
            this.ServerStart.UseVisualStyleBackColor = true;
            this.ServerStart.Click += new System.EventHandler(this.ServerStart_Click);
            // 
            // ServerLog
            // 
            this.ServerLog.Location = new System.Drawing.Point(601, 185);
            this.ServerLog.Name = "ServerLog";
            this.ServerLog.Size = new System.Drawing.Size(175, 147);
            this.ServerLog.TabIndex = 7;
            this.ServerLog.Text = "";
            // 
            // ClientLog
            // 
            this.ClientLog.Location = new System.Drawing.Point(601, 338);
            this.ClientLog.Name = "ClientLog";
            this.ClientLog.Size = new System.Drawing.Size(175, 147);
            this.ClientLog.TabIndex = 7;
            this.ClientLog.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(788, 491);
            this.Controls.Add(this.ClientLog);
            this.Controls.Add(this.ServerLog);
            this.Controls.Add(this.ServerStart);
            this.Controls.Add(this.PlayerName);
            this.Controls.Add(this.ConnectButton);
            this.Controls.Add(this.FindButton);
            this.Controls.Add(this.ServerList);
            this.Controls.Add(this.PlayersCard);
            this.Controls.Add(this.Board);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.Board)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PlayersCard)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox Board;
        private System.Windows.Forms.PictureBox PlayersCard;
        private System.Windows.Forms.ComboBox ServerList;
        private System.Windows.Forms.Button FindButton;
        private System.Windows.Forms.Button ConnectButton;
        private System.Windows.Forms.TextBox PlayerName;
        private System.Windows.Forms.Button ServerStart;
        private System.Windows.Forms.RichTextBox ServerLog;
        private System.Windows.Forms.RichTextBox ClientLog;
    }
}

