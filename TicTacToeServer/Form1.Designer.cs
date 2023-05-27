namespace TicTacToeServer
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            inputBox_port = new TextBox();
            port_label = new Label();
            listen_button = new Button();
            log_textbox = new RichTextBox();
            btn_00 = new Button();
            btn_01 = new Button();
            btn_02 = new Button();
            btn_12 = new Button();
            btn_11 = new Button();
            btn_10 = new Button();
            btn_22 = new Button();
            btn_21 = new Button();
            btn_20 = new Button();
            dataGridView_learderboard = new DataGridView();
            leaderBoardLabel = new Label();
            currentPlayers_label = new Label();
            serverlog_label = new Label();
            disconnect_button = new Button();
            clearlogs_btn = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridView_learderboard).BeginInit();
            SuspendLayout();
            // 
            // inputBox_port
            // 
            inputBox_port.Location = new Point(65, 51);
            inputBox_port.Margin = new Padding(3, 4, 3, 4);
            inputBox_port.Name = "inputBox_port";
            inputBox_port.Size = new Size(103, 27);
            inputBox_port.TabIndex = 0;
            // 
            // port_label
            // 
            port_label.AutoSize = true;
            port_label.BackColor = SystemColors.Desktop;
            port_label.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            port_label.ForeColor = SystemColors.Control;
            port_label.Location = new Point(14, 51);
            port_label.Name = "port_label";
            port_label.Size = new Size(50, 23);
            port_label.TabIndex = 1;
            port_label.Text = "Port :";
            // 
            // listen_button
            // 
            listen_button.Cursor = Cursors.Hand;
            listen_button.Location = new Point(14, 89);
            listen_button.Margin = new Padding(3, 4, 3, 4);
            listen_button.Name = "listen_button";
            listen_button.Size = new Size(117, 39);
            listen_button.TabIndex = 2;
            listen_button.Text = "Listen";
            listen_button.UseVisualStyleBackColor = true;
            listen_button.Click += listen_button_Click;
            // 
            // log_textbox
            // 
            log_textbox.BackColor = SystemColors.MenuText;
            log_textbox.ForeColor = SystemColors.Control;
            log_textbox.Location = new Point(14, 172);
            log_textbox.Margin = new Padding(3, 4, 3, 4);
            log_textbox.Name = "log_textbox";
            log_textbox.ReadOnly = true;
            log_textbox.Size = new Size(284, 340);
            log_textbox.TabIndex = 3;
            log_textbox.Text = "";
            // 
            // btn_00
            // 
            btn_00.BackColor = SystemColors.ControlLight;
            btn_00.Enabled = false;
            btn_00.Font = new Font("Segoe UI", 27F, FontStyle.Bold, GraphicsUnit.Point);
            btn_00.Location = new Point(315, 199);
            btn_00.Margin = new Padding(3, 4, 3, 4);
            btn_00.Name = "btn_00";
            btn_00.Size = new Size(103, 120);
            btn_00.TabIndex = 4;
            btn_00.UseVisualStyleBackColor = false;
            // 
            // btn_01
            // 
            btn_01.BackColor = SystemColors.ControlLight;
            btn_01.Enabled = false;
            btn_01.Font = new Font("Segoe UI", 27F, FontStyle.Bold, GraphicsUnit.Point);
            btn_01.Location = new Point(425, 199);
            btn_01.Margin = new Padding(3, 4, 3, 4);
            btn_01.Name = "btn_01";
            btn_01.Size = new Size(103, 120);
            btn_01.TabIndex = 7;
            btn_01.UseVisualStyleBackColor = false;
            // 
            // btn_02
            // 
            btn_02.BackColor = SystemColors.ControlLight;
            btn_02.Enabled = false;
            btn_02.Font = new Font("Segoe UI", 27F, FontStyle.Bold, GraphicsUnit.Point);
            btn_02.Location = new Point(535, 199);
            btn_02.Margin = new Padding(3, 4, 3, 4);
            btn_02.Name = "btn_02";
            btn_02.Size = new Size(103, 120);
            btn_02.TabIndex = 8;
            btn_02.UseVisualStyleBackColor = false;
            // 
            // btn_12
            // 
            btn_12.BackColor = SystemColors.ControlLight;
            btn_12.Enabled = false;
            btn_12.Font = new Font("Segoe UI", 27F, FontStyle.Bold, GraphicsUnit.Point);
            btn_12.Location = new Point(535, 327);
            btn_12.Margin = new Padding(3, 4, 3, 4);
            btn_12.Name = "btn_12";
            btn_12.Size = new Size(103, 120);
            btn_12.TabIndex = 11;
            btn_12.UseVisualStyleBackColor = false;
            // 
            // btn_11
            // 
            btn_11.BackColor = SystemColors.ControlLight;
            btn_11.Enabled = false;
            btn_11.Font = new Font("Segoe UI", 27F, FontStyle.Bold, GraphicsUnit.Point);
            btn_11.Location = new Point(425, 327);
            btn_11.Margin = new Padding(3, 4, 3, 4);
            btn_11.Name = "btn_11";
            btn_11.Size = new Size(103, 120);
            btn_11.TabIndex = 10;
            btn_11.UseVisualStyleBackColor = false;
            // 
            // btn_10
            // 
            btn_10.BackColor = SystemColors.ControlLight;
            btn_10.Enabled = false;
            btn_10.Font = new Font("Segoe UI", 27F, FontStyle.Bold, GraphicsUnit.Point);
            btn_10.Location = new Point(315, 327);
            btn_10.Margin = new Padding(3, 4, 3, 4);
            btn_10.Name = "btn_10";
            btn_10.Size = new Size(103, 120);
            btn_10.TabIndex = 9;
            btn_10.UseVisualStyleBackColor = false;
            // 
            // btn_22
            // 
            btn_22.BackColor = SystemColors.ControlLight;
            btn_22.Enabled = false;
            btn_22.Font = new Font("Segoe UI", 27F, FontStyle.Bold, GraphicsUnit.Point);
            btn_22.Location = new Point(535, 455);
            btn_22.Margin = new Padding(3, 4, 3, 4);
            btn_22.Name = "btn_22";
            btn_22.Size = new Size(103, 120);
            btn_22.TabIndex = 14;
            btn_22.UseVisualStyleBackColor = false;
            // 
            // btn_21
            // 
            btn_21.BackColor = SystemColors.ControlLight;
            btn_21.Enabled = false;
            btn_21.Font = new Font("Segoe UI", 27F, FontStyle.Bold, GraphicsUnit.Point);
            btn_21.Location = new Point(425, 455);
            btn_21.Margin = new Padding(3, 4, 3, 4);
            btn_21.Name = "btn_21";
            btn_21.Size = new Size(103, 120);
            btn_21.TabIndex = 13;
            btn_21.UseVisualStyleBackColor = false;
            // 
            // btn_20
            // 
            btn_20.BackColor = SystemColors.ControlLight;
            btn_20.Enabled = false;
            btn_20.Font = new Font("Segoe UI", 27F, FontStyle.Bold, GraphicsUnit.Point);
            btn_20.Location = new Point(315, 455);
            btn_20.Margin = new Padding(3, 4, 3, 4);
            btn_20.Name = "btn_20";
            btn_20.Size = new Size(103, 120);
            btn_20.TabIndex = 12;
            btn_20.UseVisualStyleBackColor = false;
            // 
            // dataGridView_learderboard
            // 
            dataGridView_learderboard.BackgroundColor = SystemColors.HighlightText;
            dataGridView_learderboard.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView_learderboard.Location = new Point(315, 36);
            dataGridView_learderboard.Margin = new Padding(3, 4, 3, 4);
            dataGridView_learderboard.Name = "dataGridView_learderboard";
            dataGridView_learderboard.ReadOnly = true;
            dataGridView_learderboard.RowHeadersWidth = 51;
            dataGridView_learderboard.RowTemplate.Height = 25;
            dataGridView_learderboard.ScrollBars = ScrollBars.Vertical;
            dataGridView_learderboard.Size = new Size(322, 132);
            dataGridView_learderboard.TabIndex = 15;
            // 
            // leaderBoardLabel
            // 
            leaderBoardLabel.AutoSize = true;
            leaderBoardLabel.BackColor = SystemColors.Desktop;
            leaderBoardLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            leaderBoardLabel.ForeColor = SystemColors.Control;
            leaderBoardLabel.Location = new Point(315, 9);
            leaderBoardLabel.Name = "leaderBoardLabel";
            leaderBoardLabel.Size = new Size(110, 23);
            leaderBoardLabel.TabIndex = 16;
            leaderBoardLabel.Text = "Leaderboard:";
            // 
            // currentPlayers_label
            // 
            currentPlayers_label.BackColor = SystemColors.Desktop;
            currentPlayers_label.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            currentPlayers_label.ForeColor = SystemColors.Control;
            currentPlayers_label.Location = new Point(315, 172);
            currentPlayers_label.Name = "currentPlayers_label";
            currentPlayers_label.Size = new Size(322, 23);
            currentPlayers_label.TabIndex = 17;
            currentPlayers_label.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // serverlog_label
            // 
            serverlog_label.AutoSize = true;
            serverlog_label.BackColor = SystemColors.Desktop;
            serverlog_label.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            serverlog_label.ForeColor = SystemColors.Control;
            serverlog_label.Location = new Point(14, 145);
            serverlog_label.Name = "serverlog_label";
            serverlog_label.Size = new Size(101, 23);
            serverlog_label.TabIndex = 18;
            serverlog_label.Text = "Server Logs:";
            // 
            // disconnect_button
            // 
            disconnect_button.BackColor = Color.Red;
            disconnect_button.Cursor = Cursors.Hand;
            disconnect_button.Enabled = false;
            disconnect_button.ForeColor = SystemColors.Control;
            disconnect_button.Location = new Point(137, 89);
            disconnect_button.Margin = new Padding(3, 4, 3, 4);
            disconnect_button.Name = "disconnect_button";
            disconnect_button.Size = new Size(32, 39);
            disconnect_button.TabIndex = 19;
            disconnect_button.Text = "X";
            disconnect_button.UseVisualStyleBackColor = false;
            disconnect_button.Click += disconnect_button_Click;
            // 
            // clearlogs_btn
            // 
            clearlogs_btn.Location = new Point(14, 521);
            clearlogs_btn.Margin = new Padding(3, 4, 3, 4);
            clearlogs_btn.Name = "clearlogs_btn";
            clearlogs_btn.Size = new Size(155, 35);
            clearlogs_btn.TabIndex = 20;
            clearlogs_btn.Text = "Clear Logs";
            clearlogs_btn.UseVisualStyleBackColor = true;
            clearlogs_btn.Click += clearlogs_btn_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Desktop;
            ClientSize = new Size(649, 591);
            Controls.Add(clearlogs_btn);
            Controls.Add(disconnect_button);
            Controls.Add(serverlog_label);
            Controls.Add(currentPlayers_label);
            Controls.Add(leaderBoardLabel);
            Controls.Add(dataGridView_learderboard);
            Controls.Add(btn_22);
            Controls.Add(btn_21);
            Controls.Add(btn_20);
            Controls.Add(btn_12);
            Controls.Add(btn_11);
            Controls.Add(btn_10);
            Controls.Add(btn_02);
            Controls.Add(btn_01);
            Controls.Add(btn_00);
            Controls.Add(log_textbox);
            Controls.Add(listen_button);
            Controls.Add(port_label);
            Controls.Add(inputBox_port);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(3, 4, 3, 4);
            MaximizeBox = false;
            Name = "Form1";
            Text = "TicTacToe Server";
            ((System.ComponentModel.ISupportInitialize)dataGridView_learderboard).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox inputBox_port;
        private Label port_label;
        private Button listen_button;
        private RichTextBox log_textbox;
        private Button btn_00;
        private Button btn_01;
        private Button btn_02;
        private Button btn_12;
        private Button btn_11;
        private Button btn_10;
        private Button btn_22;
        private Button btn_21;
        private Button btn_20;
        private DataGridView dataGridView_learderboard;
        private Label leaderBoardLabel;
        private Label currentPlayers_label;
        private Label serverlog_label;
        private Button disconnect_button;
        private Button clearlogs_btn;
    }
}