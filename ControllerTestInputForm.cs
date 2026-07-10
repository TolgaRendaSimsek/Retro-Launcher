using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace RetroLauncher
{
    public partial class ControllerTestInputForm : Form
    {
        public ControllerTestInputForm()
        {
            InitializeComponent();
            SetupCustomEvents();
        }

        private void SetupCustomEvents()
        {
            this.Load += ControllerTestInputForm_Load;
            this.FormClosing += (s, e) => tmrPoll.Stop();
            tmrPoll.Tick += tmrPoll_Tick;
            btnCloseTest.Click += (s, e) => this.Close();

            // Set button hover color
            btnCloseTest.BackColor = Color.FromArgb(55, 65, 81);
            btnCloseTest.MouseEnter += (s, e) => btnCloseTest.BackColor = Color.FromArgb(31, 41, 55);
            btnCloseTest.MouseLeave += (s, e) => btnCloseTest.BackColor = Color.FromArgb(55, 65, 81);
        }

        private void ControllerTestInputForm_Load(object? sender, EventArgs e)
        {
            PopulateDevices();
            if (cbDevices.Items.Count > 0)
            {
                cbDevices.SelectedIndex = 0;
                tmrPoll.Start();
            }
            else
            {
                lblButtonsPressed.Text = "No controllers available to test.";
                lblAxesState.Text = "";
                lblPOVState.Text = "";
            }
        }

        private void PopulateDevices()
        {
            cbDevices.Items.Clear();
            var list = ControllerManager.Instance.DetectConnectedControllers();
            foreach (var dev in list)
            {
                cbDevices.Items.Add(new DeviceComboItem { Id = dev.Id, Name = dev.ProductName });
            }
        }

        private void tmrPoll_Tick(object? sender, EventArgs e)
        {
            var selectedItem = cbDevices.SelectedItem as DeviceComboItem;
            if (selectedItem == null) return;

            ControllerManager.JOYINFOEX info = new ControllerManager.JOYINFOEX();
            if (ControllerManager.Instance.GetJoystickState(selectedItem.Id, ref info))
            {
                // Parse pressed buttons (dwButtons has bit flags for buttons 0 to 31)
                List<string> pressed = new List<string>();
                for (int i = 0; i < 32; i++)
                {
                    if ((info.dwButtons & (1U << i)) != 0)
                    {
                        pressed.Add($"Button {i}");
                    }
                }

                lblButtonsPressed.Text = pressed.Count > 0 
                    ? "Pressed Buttons: " + string.Join(", ", pressed) 
                    : "Pressed Buttons: None";

                // Parse axes
                lblAxesState.Text = $"Axes Coordinates:\n" +
                                    $"X: {info.dwXpos,-8} Y: {info.dwYpos,-8}\n" +
                                    $"Z: {info.dwZpos,-8} R: {info.dwRpos,-8}";

                // Parse POV (returns 0xFFFF if centered; otherwise returns direction in hundredths of a degree, e.g. 0 = North, 9000 = East, etc.)
                if (info.dwPOV == 0xFFFF)
                {
                    lblPOVState.Text = "D-Pad / POV: Centered";
                }
                else
                {
                    double degrees = info.dwPOV / 100.0;
                    string direction = GetPOVDirection(degrees);
                    lblPOVState.Text = $"D-Pad / POV: {degrees}° ({direction})";
                }
            }
            else
            {
                lblButtonsPressed.Text = "Error: Controller unplugged or disconnected.";
                lblAxesState.Text = "";
                lblPOVState.Text = "";
            }
        }

        private string GetPOVDirection(double degrees)
        {
            if (degrees >= 337.5 || degrees < 22.5) return "Up / North";
            if (degrees >= 22.5 && degrees < 67.5) return "Up-Right";
            if (degrees >= 67.5 && degrees < 112.5) return "Right / East";
            if (degrees >= 112.5 && degrees < 157.5) return "Down-Right";
            if (degrees >= 157.5 && degrees < 202.5) return "Down / South";
            if (degrees >= 202.5 && degrees < 247.5) return "Down-Left";
            if (degrees >= 247.5 && degrees < 292.5) return "Left / West";
            return "Up-Left";
        }

        private class DeviceComboItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";

            public override string ToString()
            {
                return $"[Port {Id}] {Name}";
            }
        }
    }
}
