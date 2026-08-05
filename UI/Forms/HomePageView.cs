using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using RetroLauncher.UI.Controls;
using RetroLauncher.UI.Theme;

namespace RetroLauncher.UI.Forms
{
    public class HomePageView : UserControl
    {
        private readonly GameLibraryManager _libraryManager = new();

        public event EventHandler? AddGameRequested;
        public event EventHandler? ManageEmulatorsRequested;
        public event EventHandler? SyncBiosRequested;
        public event EventHandler? SyncControllersRequested;
        public event EventHandler<Game>? PlayGameRequested;

        public HomePageView()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Dock = DockStyle.Fill;
            AutoScroll = true;
            Padding = new Padding(28, 24, 28, 28);
            BackColor = AppTheme.Current.Colors.Background;

            BuildDashboardLayout();
        }

        public void BuildDashboardLayout()
        {
            Controls.Clear();

            var container = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(0),
                Margin = new Padding(0),
                BackColor = Color.Transparent
            };

            var tlpMain = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1,
                RowCount = 7,
                Margin = new Padding(0),
                Padding = new Padding(0),
                BackColor = Color.Transparent
            };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // -----------------------------------------------------------------
            // 1. Full-Width Premium Hero Banner Card
            // -----------------------------------------------------------------
            var pnlHero = new Panel
            {
                Height = 210,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 0, 0, 24),
                BackColor = AppTheme.Current.Colors.SurfaceCard
            };

            pnlHero.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                Rectangle rect = new Rectangle(0, 0, pnlHero.Width, pnlHero.Height);
                using (var path = GetRoundedPath(new Rectangle(0, 0, pnlHero.Width - 1, pnlHero.Height - 1), 12))
                {
                    using (var brush = new LinearGradientBrush(
                        rect,
                        Color.FromArgb(79, 70, 229),      // Indigo
                        Color.FromArgb(24, 27, 36),       // Surface Dark
                        35F))
                    {
                        g.FillPath(brush, path);
                    }

                    // Soft accent glow overlay
                    using (var glowBrush = new LinearGradientBrush(
                        new Rectangle(0, 0, pnlHero.Width, 60),
                        Color.FromArgb(40, 139, 92, 246),
                        Color.Transparent,
                        90F))
                    {
                        g.FillRectangle(glowBrush, 0, 0, pnlHero.Width, 60);
                    }

                    using (var borderPen = new Pen(AppTheme.Current.Colors.Border, 1.5f))
                    {
                        g.DrawPath(borderPen, path);
                    }
                }

                // Draw Status Pill Badge
                using (var badgeBg = new SolidBrush(Color.FromArgb(40, 16, 185, 129)))
                using (var badgeBorder = new Pen(Color.FromArgb(16, 185, 129), 1))
                using (var dotBrush = new SolidBrush(Color.FromArgb(16, 185, 129)))
                {
                    Rectangle badgeRect = new Rectangle(28, 24, 210, 26);
                    using (var badgePath = GetRoundedPath(badgeRect, 13))
                    {
                        g.FillPath(badgeBg, badgePath);
                        g.DrawPath(badgeBorder, badgePath);
                    }
                    g.FillEllipse(dotBrush, 38, 33, 8, 8);
                    TextRenderer.DrawText(g, "RETRO ENGINE v2.0 • ONLINE", AppTheme.Current.Fonts.ButtonSmall, new Point(52, 28), Color.FromArgb(16, 185, 129));
                }

                // Title & Subtitle
                TextRenderer.DrawText(g, "WELCOME BACK, RETRO GAMER", AppTheme.Current.Fonts.TitleLarge, new Point(28, 62), AppTheme.Current.Colors.TextPrimary);
                TextRenderer.DrawText(g, "Jump straight back into your classic retro gaming collection", AppTheme.Current.Fonts.BodyLarge, new Point(28, 104), AppTheme.Current.Colors.TextSecondary);
            };

            var btnHeroPlay = new ModernButton
            {
                Text = "▶  Launch Library",
                IsPrimary = true,
                Size = new Size(175, 42),
                Location = new Point(28, 144)
            };
            btnHeroPlay.Click += (s, e) => AddGameRequested?.Invoke(this, EventArgs.Empty);

            var btnHeroAdd = new ModernButton
            {
                Text = "➕  Add Game",
                IsPrimary = false,
                Size = new Size(135, 42),
                Location = new Point(215, 144)
            };
            btnHeroAdd.Click += (s, e) => AddGameRequested?.Invoke(this, EventArgs.Empty);

            pnlHero.Controls.Add(btnHeroPlay);
            pnlHero.Controls.Add(btnHeroAdd);
            tlpMain.Controls.Add(pnlHero, 0, 0);

            // -----------------------------------------------------------------
            // 2. Dashboard Stat Cards (4 Columns)
            // -----------------------------------------------------------------
            var tlpStats = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 84,
                ColumnCount = 4,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 24),
                BackColor = Color.Transparent
            };
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpStats.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            int totalGames = _libraryManager.Games.Count;
            tlpStats.Controls.Add(CreateStatCard("📚  Library Games", $"{totalGames} Games", "Across 4 Platforms", 0), 0, 0);
            tlpStats.Controls.Add(CreateStatCard("🎮  Emulators", "4 Installed", "PCSX2, DuckStation, RPCS3, Dolphin", 1), 1, 0);
            tlpStats.Controls.Add(CreateStatCard("🔄  BIOS Status", "100% Synced", "All System Firmware Verified", 2), 2, 0);
            tlpStats.Controls.Add(CreateStatCard("⚡  System Health", "100% Ready", "All Engine Pipelines Operational", 3), 3, 0);

            tlpMain.Controls.Add(tlpStats, 0, 1);

            // -----------------------------------------------------------------
            // 3. Quick Actions Chips Bar
            // -----------------------------------------------------------------
            var secQuickActions = new SectionHeader { Title = "Quick Actions", Subtitle = "Manage games, emulators, BIOS and controllers" };
            tlpMain.Controls.Add(secQuickActions, 0, 2);

            var flpQuickActions = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                WrapContents = true,
                Margin = new Padding(0, 0, 0, 24)
            };

            var btnAdd = new ModernButton { Text = "➕ Add Game", Size = new Size(135, 38), IsPrimary = true };
            btnAdd.Click += (s, e) => AddGameRequested?.Invoke(this, EventArgs.Empty);

            var btnEmu = new ModernButton { Text = "🎮 Emulators", Size = new Size(140, 38), IsPrimary = false };
            btnEmu.Click += (s, e) => ManageEmulatorsRequested?.Invoke(this, EventArgs.Empty);

            var btnBios = new ModernButton { Text = "🔄 Sync BIOS", Size = new Size(140, 38), IsPrimary = false };
            btnBios.Click += (s, e) => SyncBiosRequested?.Invoke(this, EventArgs.Empty);

            var btnCtrl = new ModernButton { Text = "🕹️ Controllers", Size = new Size(145, 38), IsPrimary = false };
            btnCtrl.Click += (s, e) => SyncControllersRequested?.Invoke(this, EventArgs.Empty);

            flpQuickActions.Controls.Add(btnAdd);
            flpQuickActions.Controls.Add(btnEmu);
            flpQuickActions.Controls.Add(btnBios);
            flpQuickActions.Controls.Add(btnCtrl);

            tlpMain.Controls.Add(flpQuickActions, 0, 3);

            // -----------------------------------------------------------------
            // 4. Continue Playing / Recent Games
            // -----------------------------------------------------------------
            var secContinue = new SectionHeader { Title = "Continue Playing", Subtitle = "Pick up where you left off" };
            tlpMain.Controls.Add(secContinue, 0, 4);

            var games = _libraryManager.Games.Take(6).ToList();
            if (games.Count > 0)
            {
                var flpGames = new FlowLayoutPanel
                {
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    WrapContents = true,
                    Margin = new Padding(0, 0, 0, 24)
                };

                foreach (var g in games)
                {
                    var card = new GameCard(g) { Size = new Size(175, 230), Margin = new Padding(0, 0, 16, 16) };
                    card.CardSelected += (s, e) => PlayGameRequested?.Invoke(this, g);
                    flpGames.Controls.Add(card);
                }

                tlpMain.Controls.Add(flpGames, 0, 5);
            }
            else
            {
                var emptyState = new EmptyStatePanel();
                emptyState.Configure("👾", "Your Library is Empty", "Add your ROMs to start playing retro games.", "Add First Game");
                emptyState.ActionClicked += (s, e) => AddGameRequested?.Invoke(this, EventArgs.Empty);
                tlpMain.Controls.Add(emptyState, 0, 5);
            }

            // -----------------------------------------------------------------
            // 5. System Health & Engine Status Widget
            // -----------------------------------------------------------------
            var pnlStatusCard = new Panel
            {
                Dock = DockStyle.Top,
                Height = 110,
                Margin = new Padding(0, 0, 0, 20),
                Padding = new Padding(20, 16, 20, 16),
                BackColor = AppTheme.Current.Colors.SurfaceCard
            };

            pnlStatusCard.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle r = new Rectangle(0, 0, pnlStatusCard.Width - 1, pnlStatusCard.Height - 1);
                using var path = GetRoundedPath(r, 10);
                using var bgBrush = new SolidBrush(AppTheme.Current.Colors.SurfaceCard);
                using var borderPen = new Pen(AppTheme.Current.Colors.Border, 1.2f);
                g.FillPath(bgBrush, path);
                g.DrawPath(borderPen, path);
            };

            var tlpStatusContent = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Margin = new Padding(0),
                BackColor = Color.Transparent
            };
            tlpStatusContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            tlpStatusContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tlpStatusContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tlpStatusContent.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var lblStatusTitle = new Label
            {
                Text = "⚡ System Health & Engine Status",
                Font = AppTheme.Current.Fonts.TitleSmall,
                ForeColor = AppTheme.Current.Colors.TextPrimary,
                AutoSize = true,
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblStatusDesc = new Label
            {
                Text = "PCSX2: 🟢 Ready   •   DuckStation: 🟢 Ready   •   RPCS3: 🟢 Ready   •   Dolphin: 🟢 Ready",
                Font = AppTheme.Current.Fonts.BodyMedium,
                ForeColor = AppTheme.Current.Colors.TextSecondary,
                AutoSize = true,
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var btnManageEngines = new ModernButton
            {
                Text = "⚙️ Manage Engines",
                IsPrimary = false,
                Size = new Size(150, 34),
                Anchor = AnchorStyles.Right
            };
            btnManageEngines.Click += (s, e) => ManageEmulatorsRequested?.Invoke(this, EventArgs.Empty);

            tlpStatusContent.Controls.Add(lblStatusTitle, 0, 0);
            tlpStatusContent.Controls.Add(btnManageEngines, 1, 0);
            tlpStatusContent.Controls.Add(lblStatusDesc, 0, 1);

            pnlStatusCard.Controls.Add(tlpStatusContent);
            tlpMain.Controls.Add(pnlStatusCard, 0, 6);

            container.Controls.Add(tlpMain);
            Controls.Add(container);
        }

        private Panel CreateStatCard(string label, string metric, string subtitle, int index)
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(index == 0 ? 0 : 8, 0, index == 3 ? 0 : 8, 0),
                Padding = new Padding(14, 10, 14, 10),
                BackColor = AppTheme.Current.Colors.SurfaceCard
            };

            pnl.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle r = new Rectangle(0, 0, pnl.Width - 1, pnl.Height - 1);
                using var path = GetRoundedPath(r, 10);
                using var bgBrush = new SolidBrush(AppTheme.Current.Colors.SurfaceCard);
                using var borderPen = new Pen(AppTheme.Current.Colors.Border, 1f);
                g.FillPath(bgBrush, path);
                g.DrawPath(borderPen, path);
            };

            var lblLabel = new Label
            {
                Text = label,
                Font = AppTheme.Current.Fonts.ButtonSmall,
                ForeColor = AppTheme.Current.Colors.TextMuted,
                Dock = DockStyle.Top,
                Height = 18
            };

            var lblMetric = new Label
            {
                Text = metric,
                Font = AppTheme.Current.Fonts.TitleMedium,
                ForeColor = AppTheme.Current.Colors.TextPrimary,
                Dock = DockStyle.Top,
                Height = 26
            };

            var lblSub = new Label
            {
                Text = subtitle,
                Font = AppTheme.Current.Fonts.BodySmall,
                ForeColor = AppTheme.Current.Colors.TextSecondary,
                Dock = DockStyle.Top,
                Height = 18
            };

            pnl.Controls.Add(lblSub);
            pnl.Controls.Add(lblMetric);
            pnl.Controls.Add(lblLabel);

            return pnl;
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
