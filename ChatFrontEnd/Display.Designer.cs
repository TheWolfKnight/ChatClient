namespace ChatFrontEnd {
    partial class Display {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            rtb_MessageDisplay = new RichTextBox();
            txb_MessageInput = new TextBox();
            btn_Send = new Button();
            txb_IpInput = new TextBox();
            txb_PortInput = new TextBox();
            txb_NicknameInput = new TextBox();
            btn_Connect = new Button();
            this.SuspendLayout();
            // 
            // rtb_MessageDisplay
            // 
            rtb_MessageDisplay.Location = new Point(408, 12);
            rtb_MessageDisplay.Name = "rtb_MessageDisplay";
            rtb_MessageDisplay.ReadOnly = true;
            rtb_MessageDisplay.Size = new Size(380, 426);
            rtb_MessageDisplay.TabIndex = 0;
            rtb_MessageDisplay.TabStop = false;
            rtb_MessageDisplay.Text = "";
            // 
            // txb_MessageInput
            // 
            txb_MessageInput.Location = new Point(12, 415);
            txb_MessageInput.Name = "txb_MessageInput";
            txb_MessageInput.PlaceholderText = "Type something";
            txb_MessageInput.Size = new Size(306, 23);
            txb_MessageInput.TabIndex = 1;
            // 
            // btn_Send
            // 
            btn_Send.Location = new Point(327, 415);
            btn_Send.Name = "btn_Send";
            btn_Send.Size = new Size(75, 23);
            btn_Send.TabIndex = 2;
            btn_Send.Text = "Send";
            btn_Send.UseVisualStyleBackColor = true;
            btn_Send.MouseClick += this.on_SendClick;
            // 
            // txb_IpInput
            // 
            txb_IpInput.Location = new Point(12, 12);
            txb_IpInput.Name = "txb_IpInput";
            txb_IpInput.PlaceholderText = "IP";
            txb_IpInput.Size = new Size(306, 23);
            txb_IpInput.TabIndex = 3;
            txb_IpInput.Leave += this.on_IPValidate;
            // 
            // txb_PortInput
            // 
            txb_PortInput.Location = new Point(12, 41);
            txb_PortInput.Name = "txb_PortInput";
            txb_PortInput.PlaceholderText = "Port";
            txb_PortInput.Size = new Size(306, 23);
            txb_PortInput.TabIndex = 4;
            txb_PortInput.Leave += this.on_PortValidate;
            // 
            // txb_NicknameInput
            // 
            txb_NicknameInput.Location = new Point(12, 92);
            txb_NicknameInput.Name = "txb_NicknameInput";
            txb_NicknameInput.PlaceholderText = "Nickname";
            txb_NicknameInput.Size = new Size(306, 23);
            txb_NicknameInput.TabIndex = 5;
            // 
            // btn_Connect
            // 
            btn_Connect.Location = new Point(12, 121);
            btn_Connect.Name = "btn_Connect";
            btn_Connect.Size = new Size(306, 23);
            btn_Connect.TabIndex = 6;
            btn_Connect.Text = "Connect";
            btn_Connect.UseVisualStyleBackColor = true;
            btn_Connect.MouseClick += this.on_ButtonMouseClick;
            // 
            // Display
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(800, 450);
            this.Controls.Add(btn_Connect);
            this.Controls.Add(txb_NicknameInput);
            this.Controls.Add(txb_PortInput);
            this.Controls.Add(txb_IpInput);
            this.Controls.Add(btn_Send);
            this.Controls.Add(txb_MessageInput);
            this.Controls.Add(rtb_MessageDisplay);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.Name = "Display";
            this.Text = "Display";
            this.FormClosing += this.on_FormClosed;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private RichTextBox rtb_MessageDisplay;
        private TextBox txb_MessageInput;
        private Button btn_Send;
        private TextBox txb_IpInput;
        private TextBox txb_PortInput;
        private TextBox txb_NicknameInput;
        private Button btn_Connect;
    }
}