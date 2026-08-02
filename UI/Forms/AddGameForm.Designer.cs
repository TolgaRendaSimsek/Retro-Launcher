namespace RetroLauncher.UI.Forms
{
    partial class AddGameForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Label lblConsole;
        private System.Windows.Forms.ComboBox cbConsole;
        private System.Windows.Forms.Label lblEmulator;
        private System.Windows.Forms.TextBox tbEmulator;
        private System.Windows.Forms.Button btnBrowseEmulator;
        private System.Windows.Forms.Button btnChooseManual;
        private System.Windows.Forms.Label lblRom;
        private System.Windows.Forms.TextBox tbRom;
        private System.Windows.Forms.Button btnBrowseRom;
        private System.Windows.Forms.Label lblCover;
        private System.Windows.Forms.TextBox tbCover;
        private System.Windows.Forms.Button btnBrowseCover;
        private System.Windows.Forms.Label lblHero;
        private System.Windows.Forms.TextBox tbHero;
        private System.Windows.Forms.Button btnBrowseHero;
        private System.Windows.Forms.Label lblLogo;
        private System.Windows.Forms.TextBox tbLogo;
        private System.Windows.Forms.Button btnBrowseLogo;
        private System.Windows.Forms.Label lblIcon;
        private System.Windows.Forms.TextBox tbIcon;
        private System.Windows.Forms.Button btnBrowseIcon;
        private System.Windows.Forms.Label lblScreenshots;
        private System.Windows.Forms.TextBox tbScreenshots;
        private System.Windows.Forms.Button btnBrowseScreenshots;
        private System.Windows.Forms.Label lblTrailer;
        private System.Windows.Forms.TextBox tbTrailer;
        private System.Windows.Forms.Button btnBrowseTrailer;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label lblAutoDetect;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblName = new System.Windows.Forms.Label();
            this.tbName = new System.Windows.Forms.TextBox();
            this.lblConsole = new System.Windows.Forms.Label();
            this.cbConsole = new System.Windows.Forms.ComboBox();
            this.lblEmulator = new System.Windows.Forms.Label();
            this.tbEmulator = new System.Windows.Forms.TextBox();
            this.btnBrowseEmulator = new System.Windows.Forms.Button();
            this.btnChooseManual = new System.Windows.Forms.Button();
            this.lblRom = new System.Windows.Forms.Label();
            this.tbRom = new System.Windows.Forms.TextBox();
            this.btnBrowseRom = new System.Windows.Forms.Button();
            this.lblCover = new System.Windows.Forms.Label();
            this.tbCover = new System.Windows.Forms.TextBox();
            this.btnBrowseCover = new System.Windows.Forms.Button();
            this.lblHero = new System.Windows.Forms.Label();
            this.tbHero = new System.Windows.Forms.TextBox();
            this.btnBrowseHero = new System.Windows.Forms.Button();
            this.lblLogo = new System.Windows.Forms.Label();
            this.tbLogo = new System.Windows.Forms.TextBox();
            this.btnBrowseLogo = new System.Windows.Forms.Button();
            this.lblIcon = new System.Windows.Forms.Label();
            this.tbIcon = new System.Windows.Forms.TextBox();
            this.btnBrowseIcon = new System.Windows.Forms.Button();
            this.lblScreenshots = new System.Windows.Forms.Label();
            this.tbScreenshots = new System.Windows.Forms.TextBox();
            this.btnBrowseScreenshots = new System.Windows.Forms.Button();
            this.lblTrailer = new System.Windows.Forms.Label();
            this.tbTrailer = new System.Windows.Forms.TextBox();
            this.btnBrowseTrailer = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.lblAutoDetect = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblName.Location = new System.Drawing.Point(20, 23);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(78, 15);
            this.lblName.TabIndex = 0;
            this.lblName.Text = "Game Name:";
            // 
            // tbName
            // 
            this.tbName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(52)))));
            this.tbName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbName.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.tbName.ForeColor = System.Drawing.Color.White;
            this.tbName.Location = new System.Drawing.Point(130, 20);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(350, 24);
            this.tbName.TabIndex = 1;
            // 
            // lblConsole
            // 
            this.lblConsole.AutoSize = true;
            this.lblConsole.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblConsole.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblConsole.Location = new System.Drawing.Point(20, 63);
            this.lblConsole.Name = "lblConsole";
            this.lblConsole.Size = new System.Drawing.Size(53, 15);
            this.lblConsole.TabIndex = 2;
            this.lblConsole.Text = "Console:";
            // 
            // cbConsole
            // 
            this.cbConsole.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(52)))));
            this.cbConsole.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbConsole.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbConsole.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cbConsole.ForeColor = System.Drawing.Color.White;
            this.cbConsole.FormattingEnabled = true;
            this.cbConsole.Location = new System.Drawing.Point(130, 60);
            this.cbConsole.Name = "cbConsole";
            this.cbConsole.Size = new System.Drawing.Size(350, 25);
            this.cbConsole.TabIndex = 3;
            // 
            // lblEmulator
            // 
            this.lblEmulator.AutoSize = true;
            this.lblEmulator.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblEmulator.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblEmulator.Location = new System.Drawing.Point(20, 103);
            this.lblEmulator.Name = "lblEmulator";
            this.lblEmulator.Size = new System.Drawing.Size(60, 15);
            this.lblEmulator.TabIndex = 4;
            this.lblEmulator.Text = "Emulator:";
            // 
            // tbEmulator
            // 
            this.tbEmulator.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(52)))));
            this.tbEmulator.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbEmulator.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.tbEmulator.ForeColor = System.Drawing.Color.White;
            this.tbEmulator.Location = new System.Drawing.Point(130, 100);
            this.tbEmulator.Name = "tbEmulator";
            this.tbEmulator.Size = new System.Drawing.Size(260, 24);
            this.tbEmulator.TabIndex = 5;
            // 
            // btnBrowseEmulator
            // 
            this.btnBrowseEmulator.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(52)))));
            this.btnBrowseEmulator.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(85)))), ((int)(((byte)(99)))));
            this.btnBrowseEmulator.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowseEmulator.ForeColor = System.Drawing.Color.White;
            this.btnBrowseEmulator.Location = new System.Drawing.Point(400, 100);
            this.btnBrowseEmulator.Name = "btnBrowseEmulator";
            this.btnBrowseEmulator.Size = new System.Drawing.Size(35, 24);
            this.btnBrowseEmulator.TabIndex = 6;
            this.btnBrowseEmulator.Text = "📁";
            this.btnBrowseEmulator.UseVisualStyleBackColor = false;
            // 
            // btnChooseManual
            // 
            this.btnChooseManual.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(52)))));
            this.btnChooseManual.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(85)))), ((int)(((byte)(99)))));
            this.btnChooseManual.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnChooseManual.ForeColor = System.Drawing.Color.White;
            this.btnChooseManual.Location = new System.Drawing.Point(440, 100);
            this.btnChooseManual.Name = "btnChooseManual";
            this.btnChooseManual.Size = new System.Drawing.Size(40, 24);
            this.btnChooseManual.TabIndex = 7;
            this.btnChooseManual.Text = "⚙️";
            this.btnChooseManual.UseVisualStyleBackColor = false;
            // 
            // lblRom
            // 
            this.lblRom.AutoSize = true;
            this.lblRom.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblRom.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblRom.Location = new System.Drawing.Point(20, 143);
            this.lblRom.Name = "lblRom";
            this.lblRom.Size = new System.Drawing.Size(66, 15);
            this.lblRom.TabIndex = 8;
            this.lblRom.Text = "ROM Path:";
            // 
            // tbRom
            // 
            this.tbRom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(52)))));
            this.tbRom.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbRom.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.tbRom.ForeColor = System.Drawing.Color.White;
            this.tbRom.Location = new System.Drawing.Point(130, 140);
            this.tbRom.Name = "tbRom";
            this.tbRom.Size = new System.Drawing.Size(310, 24);
            this.tbRom.TabIndex = 9;
            // 
            // btnBrowseRom
            // 
            this.btnBrowseRom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(52)))));
            this.btnBrowseRom.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(85)))), ((int)(((byte)(99)))));
            this.btnBrowseRom.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowseRom.ForeColor = System.Drawing.Color.White;
            this.btnBrowseRom.Location = new System.Drawing.Point(445, 140);
            this.btnBrowseRom.Name = "btnBrowseRom";
            this.btnBrowseRom.Size = new System.Drawing.Size(35, 24);
            this.btnBrowseRom.TabIndex = 10;
            this.btnBrowseRom.Text = "📁";
            this.btnBrowseRom.UseVisualStyleBackColor = false;
            // 
            // lblCover
            // 
            this.lblCover.AutoSize = true;
            this.lblCover.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblCover.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblCover.Location = new System.Drawing.Point(20, 183);
            this.lblCover.Name = "lblCover";
            this.lblCover.Size = new System.Drawing.Size(68, 15);
            this.lblCover.TabIndex = 11;
            this.lblCover.Text = "Cover Art:";
            // 
            // tbCover
            // 
            this.tbCover.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(52)))));
            this.tbCover.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbCover.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.tbCover.ForeColor = System.Drawing.Color.White;
            this.tbCover.Location = new System.Drawing.Point(130, 180);
            this.tbCover.Name = "tbCover";
            this.tbCover.Size = new System.Drawing.Size(310, 24);
            this.tbCover.TabIndex = 12;
            // 
            // btnBrowseCover
            // 
            this.btnBrowseCover.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(52)))));
            this.btnBrowseCover.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(85)))), ((int)(((byte)(99)))));
            this.btnBrowseCover.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowseCover.ForeColor = System.Drawing.Color.White;
            this.btnBrowseCover.Location = new System.Drawing.Point(445, 180);
            this.btnBrowseCover.Name = "btnBrowseCover";
            this.btnBrowseCover.Size = new System.Drawing.Size(35, 24);
            this.btnBrowseCover.TabIndex = 13;
            this.btnBrowseCover.Text = "📁";
            this.btnBrowseCover.UseVisualStyleBackColor = false;
            // 
            // lblHero
            // 
            this.lblHero.AutoSize = true;
            this.lblHero.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblHero.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblHero.Location = new System.Drawing.Point(20, 223);
            this.lblHero.Name = "lblHero";
            this.lblHero.Size = new System.Drawing.Size(81, 15);
            this.lblHero.TabIndex = 14;
            this.lblHero.Text = "Hero Banner:";
            // 
            // tbHero
            // 
            this.tbHero.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(52)))));
            this.tbHero.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbHero.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.tbHero.ForeColor = System.Drawing.Color.White;
            this.tbHero.Location = new System.Drawing.Point(130, 220);
            this.tbHero.Name = "tbHero";
            this.tbHero.Size = new System.Drawing.Size(310, 24);
            this.tbHero.TabIndex = 15;
            // 
            // btnBrowseHero
            // 
            this.btnBrowseHero.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(52)))));
            this.btnBrowseHero.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(85)))), ((int)(((byte)(99)))));
            this.btnBrowseHero.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowseHero.ForeColor = System.Drawing.Color.White;
            this.btnBrowseHero.Location = new System.Drawing.Point(445, 220);
            this.btnBrowseHero.Name = "btnBrowseHero";
            this.btnBrowseHero.Size = new System.Drawing.Size(35, 24);
            this.btnBrowseHero.TabIndex = 16;
            this.btnBrowseHero.Text = "📁";
            this.btnBrowseHero.UseVisualStyleBackColor = false;
            // 
            // lblLogo
            // 
            this.lblLogo.AutoSize = true;
            this.lblLogo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblLogo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblLogo.Location = new System.Drawing.Point(20, 263);
            this.lblLogo.Name = "lblLogo";
            this.lblLogo.Size = new System.Drawing.Size(76, 15);
            this.lblLogo.TabIndex = 17;
            this.lblLogo.Text = "Logo Image:";
            // 
            // tbLogo
            // 
            this.tbLogo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(52)))));
            this.tbLogo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbLogo.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.tbLogo.ForeColor = System.Drawing.Color.White;
            this.tbLogo.Location = new System.Drawing.Point(130, 260);
            this.tbLogo.Name = "tbLogo";
            this.tbLogo.Size = new System.Drawing.Size(310, 24);
            this.tbLogo.TabIndex = 18;
            // 
            // btnBrowseLogo
            // 
            this.btnBrowseLogo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(52)))));
            this.btnBrowseLogo.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(85)))), ((int)(((byte)(99)))));
            this.btnBrowseLogo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowseLogo.ForeColor = System.Drawing.Color.White;
            this.btnBrowseLogo.Location = new System.Drawing.Point(445, 260);
            this.btnBrowseLogo.Name = "btnBrowseLogo";
            this.btnBrowseLogo.Size = new System.Drawing.Size(35, 24);
            this.btnBrowseLogo.TabIndex = 19;
            this.btnBrowseLogo.Text = "📁";
            this.btnBrowseLogo.UseVisualStyleBackColor = false;
            // 
            // lblIcon
            // 
            this.lblIcon.AutoSize = true;
            this.lblIcon.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblIcon.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblIcon.Location = new System.Drawing.Point(20, 303);
            this.lblIcon.Name = "lblIcon";
            this.lblIcon.Size = new System.Drawing.Size(73, 15);
            this.lblIcon.TabIndex = 20;
            this.lblIcon.Text = "Icon Image:";
            // 
            // tbIcon
            // 
            this.tbIcon.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(52)))));
            this.tbIcon.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbIcon.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.tbIcon.ForeColor = System.Drawing.Color.White;
            this.tbIcon.Location = new System.Drawing.Point(130, 300);
            this.tbIcon.Name = "tbIcon";
            this.tbIcon.Size = new System.Drawing.Size(310, 24);
            this.tbIcon.TabIndex = 21;
            // 
            // btnBrowseIcon
            // 
            this.btnBrowseIcon.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(52)))));
            this.btnBrowseIcon.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(85)))), ((int)(((byte)(99)))));
            this.btnBrowseIcon.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowseIcon.ForeColor = System.Drawing.Color.White;
            this.btnBrowseIcon.Location = new System.Drawing.Point(445, 300);
            this.btnBrowseIcon.Name = "btnBrowseIcon";
            this.btnBrowseIcon.Size = new System.Drawing.Size(35, 24);
            this.btnBrowseIcon.TabIndex = 22;
            this.btnBrowseIcon.Text = "📁";
            this.btnBrowseIcon.UseVisualStyleBackColor = false;
            // 
            // lblScreenshots
            // 
            this.lblScreenshots.AutoSize = true;
            this.lblScreenshots.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblScreenshots.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblScreenshots.Location = new System.Drawing.Point(20, 343);
            this.lblScreenshots.Name = "lblScreenshots";
            this.lblScreenshots.Size = new System.Drawing.Size(78, 15);
            this.lblScreenshots.TabIndex = 23;
            this.lblScreenshots.Text = "Screenshots:";
            // 
            // tbScreenshots
            // 
            this.tbScreenshots.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(52)))));
            this.tbScreenshots.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbScreenshots.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.tbScreenshots.ForeColor = System.Drawing.Color.White;
            this.tbScreenshots.Location = new System.Drawing.Point(130, 340);
            this.tbScreenshots.Name = "tbScreenshots";
            this.tbScreenshots.Size = new System.Drawing.Size(310, 24);
            this.tbScreenshots.TabIndex = 24;
            // 
            // btnBrowseScreenshots
            // 
            this.btnBrowseScreenshots.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(52)))));
            this.btnBrowseScreenshots.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(85)))), ((int)(((byte)(99)))));
            this.btnBrowseScreenshots.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowseScreenshots.ForeColor = System.Drawing.Color.White;
            this.btnBrowseScreenshots.Location = new System.Drawing.Point(445, 340);
            this.btnBrowseScreenshots.Name = "btnBrowseScreenshots";
            this.btnBrowseScreenshots.Size = new System.Drawing.Size(35, 24);
            this.btnBrowseScreenshots.TabIndex = 25;
            this.btnBrowseScreenshots.Text = "📁";
            this.btnBrowseScreenshots.UseVisualStyleBackColor = false;
            // 
            // lblTrailer
            // 
            this.lblTrailer.AutoSize = true;
            this.lblTrailer.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTrailer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblTrailer.Location = new System.Drawing.Point(20, 383);
            this.lblTrailer.Name = "lblTrailer";
            this.lblTrailer.Size = new System.Drawing.Size(81, 15);
            this.lblTrailer.TabIndex = 26;
            this.lblTrailer.Text = "Trailer Video:";
            // 
            // tbTrailer
            // 
            this.tbTrailer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(52)))));
            this.tbTrailer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbTrailer.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.tbTrailer.ForeColor = System.Drawing.Color.White;
            this.tbTrailer.Location = new System.Drawing.Point(130, 380);
            this.tbTrailer.Name = "tbTrailer";
            this.tbTrailer.Size = new System.Drawing.Size(310, 24);
            this.tbTrailer.TabIndex = 27;
            // 
            // btnBrowseTrailer
            // 
            this.btnBrowseTrailer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(52)))));
            this.btnBrowseTrailer.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(85)))), ((int)(((byte)(99)))));
            this.btnBrowseTrailer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowseTrailer.ForeColor = System.Drawing.Color.White;
            this.btnBrowseTrailer.Location = new System.Drawing.Point(445, 380);
            this.btnBrowseTrailer.Name = "btnBrowseTrailer";
            this.btnBrowseTrailer.Size = new System.Drawing.Size(35, 24);
            this.btnBrowseTrailer.TabIndex = 28;
            this.btnBrowseTrailer.Text = "📁";
            this.btnBrowseTrailer.UseVisualStyleBackColor = false;
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatAppearance.BorderSize = 0;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Location = new System.Drawing.Point(250, 445);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(110, 35);
            this.btnCancel.TabIndex = 29;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(185)))), ((int)(((byte)(129)))));
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(370, 445);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(110, 35);
            this.btnSave.TabIndex = 30;
            this.btnSave.Text = "Save Game";
            this.btnSave.UseVisualStyleBackColor = false;
            // 
            // lblAutoDetect
            // 
            this.lblAutoDetect.AutoSize = true;
            this.lblAutoDetect.Font = new System.Drawing.Font("Segoe UI", 8.2F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
            this.lblAutoDetect.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(165)))), ((int)(((byte)(180)))), ((int)(((byte)(252)))));
            this.lblAutoDetect.Location = new System.Drawing.Point(20, 425);
            this.lblAutoDetect.Name = "lblAutoDetect";
            this.lblAutoDetect.Size = new System.Drawing.Size(0, 13);
            this.lblAutoDetect.TabIndex = 31;
            // 
            // AddGameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(28)))));
            this.ClientSize = new System.Drawing.Size(504, 500);
            this.Controls.Add(this.lblAutoDetect);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.tbName);
            this.Controls.Add(this.lblConsole);
            this.Controls.Add(this.cbConsole);
            this.Controls.Add(this.lblEmulator);
            this.Controls.Add(this.tbEmulator);
            this.Controls.Add(this.btnBrowseEmulator);
            this.Controls.Add(this.btnChooseManual);
            this.Controls.Add(this.lblRom);
            this.Controls.Add(this.tbRom);
            this.Controls.Add(this.btnBrowseRom);
            this.Controls.Add(this.lblCover);
            this.Controls.Add(this.tbCover);
            this.Controls.Add(this.btnBrowseCover);
            this.Controls.Add(this.lblHero);
            this.Controls.Add(this.tbHero);
            this.Controls.Add(this.btnBrowseHero);
            this.Controls.Add(this.lblLogo);
            this.Controls.Add(this.tbLogo);
            this.Controls.Add(this.btnBrowseLogo);
            this.Controls.Add(this.lblIcon);
            this.Controls.Add(this.tbIcon);
            this.Controls.Add(this.btnBrowseIcon);
            this.Controls.Add(this.lblScreenshots);
            this.Controls.Add(this.tbScreenshots);
            this.Controls.Add(this.btnBrowseScreenshots);
            this.Controls.Add(this.lblTrailer);
            this.Controls.Add(this.tbTrailer);
            this.Controls.Add(this.btnBrowseTrailer);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddGameForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add New Game";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
