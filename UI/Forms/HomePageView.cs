using System;
using System.Drawing;
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
            Padding = new Padding(20);
            BackColor = AppTheme.Current.Colors.Background;

            BuildLayout();
        }

        private void BuildLayout()
        {
            Controls.Clear();

            var tlpMain = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1,
                RowCount = 6,
                Margin = new Padding(0)
            };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // 1. Featured Hero Banner
            var pnlHero = new Panel
            {
                Height = 180,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 0, 0, 20),
                BackColor = AppTheme.Current.Colors.SurfaceCard
            };
            pnlHero.Paint += (s, e) =>
            {
                using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    pnlHero.ClientRectangle,
                    AppTheme.Current.Colors.AccentPrimary,
                    AppTheme.Current.Colors.SurfaceCard,
                    45F);
                e.Graphics.FillRectangle(brush, pnlHero.ClientRectangle);

                TextRenderer.DrawText(e.Graphics, "RETRO LAUNCHER", AppTheme.Current.Fonts.TitleLarge, new Point(24, 28), AppTheme.Current.Colors.TextPrimary);
                TextRenderer.DrawText(e.Graphics, "Welcome back! Ready to jump into your favorite retro classics?", AppTheme.Current.Fonts.BodyLarge, new Point(24, 70), AppTheme.Current.Colors.TextSecondary);
            };

            var btnHeroPlay = new ModernButton
            {
                Text = "▶  Launch Library",
                IsPrimary = true,
                Size = new Size(160, 38),
                Location = new Point(24, 115)
            };
            btnHeroPlay.Click += (s, e) => AddGameRequested?.Invoke(this, EventArgs.Empty);
            pnlHero.Controls.Add(btnHeroPlay);

            tlpMain.Controls.Add(pnlHero, 0, 0);

            // 2. Quick Actions Bar
            var secQuickActions = new SectionHeader { Title = "Quick Actions", Subtitle = "Manage games, emulators, BIOS and controllers" };
            tlpMain.Controls.Add(secQuickActions, 0, 1);

            var flpQuickActions = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                WrapContents = true,
                Margin = new Padding(0, 0, 0, 24)
            };

            var btnAdd = new ModernButton { Text = "➕ Add Game", Size = new Size(130, 36), IsPrimary = true };
            btnAdd.Click += (s, e) => AddGameRequested?.Invoke(this, EventArgs.Empty);

            var btnEmu = new ModernButton { Text = "🎮 Emulators", Size = new Size(130, 36), IsPrimary = false };
            btnEmu.Click += (s, e) => ManageEmulatorsRequested?.Invoke(this, EventArgs.Empty);

            var btnBios = new ModernButton { Text = "🔄 Sync BIOS", Size = new Size(130, 36), IsPrimary = false };
            btnBios.Click += (s, e) => SyncBiosRequested?.Invoke(this, EventArgs.Empty);

            var btnCtrl = new ModernButton { Text = "🕹️ Controllers", Size = new Size(140, 36), IsPrimary = false };
            btnCtrl.Click += (s, e) => SyncControllersRequested?.Invoke(this, EventArgs.Empty);

            flpQuickActions.Controls.Add(btnAdd);
            flpQuickActions.Controls.Add(btnEmu);
            flpQuickActions.Controls.Add(btnBios);
            flpQuickActions.Controls.Add(btnCtrl);

            tlpMain.Controls.Add(flpQuickActions, 0, 2);

            // 3. Continue Playing / Recent Games
            var secContinue = new SectionHeader { Title = "Continue Playing", Subtitle = "Pick up where you left off" };
            tlpMain.Controls.Add(secContinue, 0, 3);

            var games = _libraryManager.Games.Take(4).ToList();
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
                    var card = new GameCard(g) { Size = new Size(160, 220) };
                    card.CardSelected += (s, e) => PlayGameRequested?.Invoke(this, g);
                    flpGames.Controls.Add(card);
                }

                tlpMain.Controls.Add(flpGames, 0, 4);
            }
            else
            {
                var emptyState = new EmptyStatePanel();
                emptyState.Configure("👾", "Your Library is Empty", "Add your ROMs to start playing retro games.", "Add First Game");
                emptyState.ActionClicked += (s, e) => AddGameRequested?.Invoke(this, EventArgs.Empty);
                tlpMain.Controls.Add(emptyState, 0, 4);
            }

            // 4. System Status Summary Card
            var pnlStatusCard = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(16),
                Margin = new Padding(0, 0, 0, 20),
                BackColor = AppTheme.Current.Colors.SurfaceCard
            };
            var lblStatusTitle = new Label
            {
                Text = "⚡ System Health Summary",
                Font = AppTheme.Current.Fonts.TitleSmall,
                ForeColor = AppTheme.Current.Colors.TextPrimary,
                AutoSize = true,
                Dock = DockStyle.Top
            };
            var lblStatusDesc = new Label
            {
                Text = "All installed emulator engines, BIOS packages, and controller profiles are synchronized.",
                Font = AppTheme.Current.Fonts.BodyMedium,
                ForeColor = AppTheme.Current.Colors.TextSecondary,
                AutoSize = true,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 6, 0, 0)
            };
            pnlStatusCard.Controls.Add(lblStatusDesc);
            pnlStatusCard.Controls.Add(lblStatusTitle);

            tlpMain.Controls.Add(pnlStatusCard, 0, 5);

            Controls.Add(tlpMain);
        }
    }
}
