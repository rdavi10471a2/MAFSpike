namespace CodexFileQuery;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        this.lblTitle = new System.Windows.Forms.Label();
        this.lblDescription = new System.Windows.Forms.Label();
        this.btnConnect = new System.Windows.Forms.Button();
        this.txtFilePath = new System.Windows.Forms.TextBox();
        this.btnBrowseFile = new System.Windows.Forms.Button();
        this.lblQuestion = new System.Windows.Forms.Label();
        this.txtQuestion = new System.Windows.Forms.TextBox();
        this.btnSend = new System.Windows.Forms.Button();
        this.lblResponse = new System.Windows.Forms.Label();
        this.txtResponse = new System.Windows.Forms.TextBox();
        this.lblStatus = new System.Windows.Forms.Label();
        this.btnCancel = new System.Windows.Forms.Button();
        this.panelTop = new System.Windows.Forms.Panel();
        this.panelConnection = new System.Windows.Forms.Panel();
        this.panelFile = new System.Windows.Forms.Panel();
        this.panelQuestion = new System.Windows.Forms.Panel();
        this.panelResponse = new System.Windows.Forms.Panel();
        this.panelBottom = new System.Windows.Forms.Panel();
        this.panelTop.SuspendLayout();
        this.panelConnection.SuspendLayout();
        this.panelFile.SuspendLayout();
        this.panelQuestion.SuspendLayout();
        this.panelResponse.SuspendLayout();
        this.panelBottom.SuspendLayout();
        this.SuspendLayout();
        // 
        // lblTitle
        // 
        this.lblTitle.AutoSize = true;
        this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
        this.lblTitle.Location = new System.Drawing.Point(15, 15);
        this.lblTitle.Name = "lblTitle";
        this.lblTitle.Size = new System.Drawing.Size(312, 30);
        this.lblTitle.TabIndex = 0;
        this.lblTitle.Text = "Codex File Query";
        // 
        // lblDescription
        // 
        this.lblDescription.Location = new System.Drawing.Point(15, 50);
        this.lblDescription.Name = "lblDescription";
        this.lblDescription.Size = new System.Drawing.Size(600, 20);
        this.lblDescription.TabIndex = 1;
        this.lblDescription.Text = "Query ChatGPT about your local files using your browser session (no API key needed).";
        // 
        // panelTop
        // 
        this.panelTop.Controls.Add(this.lblTitle);
        this.panelTop.Controls.Add(this.lblDescription);
        this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
        this.panelTop.Location = new System.Drawing.Point(0, 0);
        this.panelTop.Name = "panelTop";
        this.panelTop.Size = new System.Drawing.Size(700, 75);
        this.panelTop.TabIndex = 2;
        // 
        // panelConnection
        // 
        this.panelConnection.Controls.Add(this.btnConnect);
        this.panelConnection.Dock = System.Windows.Forms.DockStyle.Top;
        this.panelConnection.Location = new System.Drawing.Point(0, 75);
        this.panelConnection.Name = "panelConnection";
        this.panelConnection.Padding = new System.Windows.Forms.Padding(10);
        this.panelConnection.Size = new System.Drawing.Size(700, 50);
        this.panelConnection.TabIndex = 3;
        // 
        // btnConnect
        // 
        this.btnConnect.BackColor = System.Drawing.Color.FromArgb(16, 163, 74);
        this.btnConnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnConnect.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        this.btnConnect.ForeColor = System.Drawing.Color.White;
        this.btnConnect.Location = new System.Drawing.Point(10, 8);
        this.btnConnect.Name = "btnConnect";
        this.btnConnect.Size = new System.Drawing.Size(200, 34);
        this.btnConnect.TabIndex = 0;
        this.btnConnect.Text = "Open ChatGPT";
        this.btnConnect.UseVisualStyleBackColor = false;
        this.btnConnect.Click += new System.EventHandler(this.BtnConnect_Click);
        // 
        // panelFile
        // 
        this.panelFile.Controls.Add(this.txtFilePath);
        this.panelFile.Controls.Add(this.btnBrowseFile);
        this.panelFile.Dock = System.Windows.Forms.DockStyle.Top;
        this.panelFile.Location = new System.Drawing.Point(0, 125);
        this.panelFile.Name = "panelFile";
        this.panelFile.Padding = new System.Windows.Forms.Padding(10);
        this.panelFile.Size = new System.Drawing.Size(700, 50);
        this.panelFile.TabIndex = 4;
        // 
        // txtFilePath
        // 
        this.txtFilePath.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtFilePath.Location = new System.Drawing.Point(10, 10);
        this.txtFilePath.Name = "txtFilePath";
        this.txtFilePath.PlaceholderText = "Select a file to analyze...";
        this.txtFilePath.Size = new System.Drawing.Size(590, 23);
        this.txtFilePath.TabIndex = 0;
        // 
        // btnBrowseFile
        // 
        this.btnBrowseFile.Dock = System.Windows.Forms.DockStyle.Right;
        this.btnBrowseFile.Location = new System.Drawing.Point(600, 10);
        this.btnBrowseFile.Name = "btnBrowseFile";
        this.btnBrowseFile.Size = new System.Drawing.Size(90, 30);
        this.btnBrowseFile.TabIndex = 1;
        this.btnBrowseFile.Text = "Browse...";
        this.btnBrowseFile.UseVisualStyleBackColor = true;
        this.btnBrowseFile.Click += new System.EventHandler(this.BtnBrowseFile_Click);
        // 
        // panelQuestion
        // 
        this.panelQuestion.Controls.Add(this.txtQuestion);
        this.panelQuestion.Dock = System.Windows.Forms.DockStyle.Top;
        this.panelQuestion.Location = new System.Drawing.Point(0, 175);
        this.panelQuestion.Name = "panelQuestion";
        this.panelQuestion.Padding = new System.Windows.Forms.Padding(10);
        this.panelQuestion.Size = new System.Drawing.Size(700, 80);
        this.panelQuestion.TabIndex = 5;
        // 
        // lblQuestion
        // 
        this.lblQuestion.Location = new System.Drawing.Point(15, 5);
        this.lblQuestion.Name = "lblQuestion";
        this.lblQuestion.Size = new System.Drawing.Size(200, 15);
        this.lblQuestion.TabIndex = 0;
        this.lblQuestion.Text = "Question (optional):";
        // 
        // txtQuestion
        // 
        this.txtQuestion.Dock = System.Windows.Forms.DockStyle.Top;
        this.txtQuestion.Location = new System.Drawing.Point(10, 25);
        this.txtQuestion.Multiline = true;
        this.txtQuestion.Name = "txtQuestion";
        this.txtQuestion.PlaceholderText = "What would you like to know about this file? Leave blank for auto-analysis.";
        this.txtQuestion.Size = new System.Drawing.Size(680, 45);
        this.txtQuestion.TabIndex = 1;
        // 
        // btnSend
        // 
        this.btnSend.BackColor = System.Drawing.Color.FromArgb(16, 163, 74);
        this.btnSend.Dock = System.Windows.Forms.DockStyle.Top;
        this.btnSend.Enabled = false;
        this.btnSend.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnSend.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        this.btnSend.ForeColor = System.Drawing.Color.White;
        this.btnSend.Location = new System.Drawing.Point(0, 0);
        this.btnSend.Margin = new System.Windows.Forms.Padding(10);
        this.btnSend.Name = "btnSend";
        this.btnSend.Size = new System.Drawing.Size(700, 40);
        this.btnSend.TabIndex = 0;
        this.btnSend.Text = "Send to ChatGPT";
        this.btnSend.UseVisualStyleBackColor = false;
        this.btnSend.Click += new System.EventHandler(this.BtnSend_Click);
        // 
        // panelResponse
        // 
        this.panelResponse.Controls.Add(this.lblResponse);
        this.panelResponse.Controls.Add(this.txtResponse);
        this.panelResponse.Dock = System.Windows.Forms.DockStyle.Fill;
        this.panelResponse.Location = new System.Drawing.Point(0, 255);
        this.panelResponse.Name = "panelResponse";
        this.panelResponse.Padding = new System.Windows.Forms.Padding(10);
        this.panelResponse.Size = new System.Drawing.Size(700, 295);
        this.panelResponse.TabIndex = 6;
        // 
        // lblResponse
        // 
        this.lblResponse.Location = new System.Drawing.Point(15, 5);
        this.lblResponse.Name = "lblResponse";
        this.lblResponse.Size = new System.Drawing.Size(200, 15);
        this.lblResponse.TabIndex = 0;
        this.lblResponse.Text = "ChatGPT Response:";
        // 
        // txtResponse
        // 
        this.txtResponse.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
        this.txtResponse.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtResponse.Font = new System.Drawing.Font("Consolas", 10F);
        this.txtResponse.ForeColor = System.Drawing.Color.LightGray;
        this.txtResponse.Location = new System.Drawing.Point(10, 25);
        this.txtResponse.Multiline = true;
        this.txtResponse.Name = "txtResponse";
        this.txtResponse.ReadOnly = true;
        this.txtResponse.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.txtResponse.Size = new System.Drawing.Size(680, 260);
        this.txtResponse.TabIndex = 1;
        // 
        // panelBottom
        // 
        this.panelBottom.Controls.Add(this.btnCancel);
        this.panelBottom.Controls.Add(this.lblStatus);
        this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.panelBottom.Location = new System.Drawing.Point(0, 550);
        this.panelBottom.Name = "panelBottom";
        this.panelBottom.Size = new System.Drawing.Size(700, 35);
        this.panelBottom.TabIndex = 7;
        // 
        // btnCancel
        // 
        this.btnCancel.Dock = System.Windows.Forms.DockStyle.Right;
        this.btnCancel.Location = new System.Drawing.Point(620, 0);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(80, 35);
        this.btnCancel.TabIndex = 0;
        this.btnCancel.Text = "Cancel";
        this.btnCancel.UseVisualStyleBackColor = true;
        // 
        // lblStatus
        // 
        this.lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblStatus.Location = new System.Drawing.Point(0, 0);
        this.lblStatus.Name = "lblStatus";
        this.lblStatus.Size = new System.Drawing.Size(620, 35);
        this.lblStatus.TabIndex = 1;
        this.lblStatus.Text = "Ready";
        this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this.lblStatus.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
        // 
        // MainForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(700, 585);
        this.Controls.Add(this.panelResponse);
        this.Controls.Add(this.btnSend);
        this.Controls.Add(this.panelQuestion);
        this.Controls.Add(this.panelFile);
        this.Controls.Add(this.panelConnection);
        this.Controls.Add(this.panelTop);
        this.Controls.Add(this.panelBottom);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.Name = "MainForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Codex File Query - ChatGPT Browser Session";
        this.panelTop.ResumeLayout(false);
        this.panelTop.PerformLayout();
        this.panelConnection.ResumeLayout(false);
        this.panelFile.ResumeLayout(false);
        this.panelFile.PerformLayout();
        this.panelQuestion.ResumeLayout(false);
        this.panelQuestion.PerformLayout();
        this.panelResponse.ResumeLayout(false);
        this.panelResponse.PerformLayout();
        this.panelBottom.ResumeLayout(false);
        this.ResumeLayout(false);

    }

    #endregion

    private Label lblTitle;
    private Label lblDescription;
    private Panel panelTop;
    private Panel panelConnection;
    private Button btnConnect;
    private Panel panelFile;
    private TextBox txtFilePath;
    private Button btnBrowseFile;
    private Panel panelQuestion;
    private Label lblQuestion;
    private TextBox txtQuestion;
    private Button btnSend;
    private Panel panelResponse;
    private Label lblResponse;
    private TextBox txtResponse;
    private Panel panelBottom;
    private Button btnCancel;
    private Label lblStatus;
}
