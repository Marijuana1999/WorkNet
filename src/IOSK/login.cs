using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TextBox = System.Windows.Forms.TextBox;
using Microsoft.Data.Sqlite;


namespace IOSK
{
    public partial class login : Form
    {
        private Form1 form1;
        class UpdateInfo
        {
            public string version { get; set; }
            public string url { get; set; }
        }
        private async Task CheckForUpdateAsync()
        {
            try
            {
                string jsonUrl =
                    "https://raw.githubusercontent.com/Marijuana1999/DNSchanger/main/update.json";

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);

                    string json = await client.GetStringAsync(jsonUrl);

                    var info = System.Text.Json.JsonSerializer.Deserialize<UpdateInfo>(json);

                    if (info == null || string.IsNullOrWhiteSpace(info.version))
                        return;

                    Version currentVersion =
                        new Version(Application.ProductVersion);

                    Version latestVersion =
                        new Version(info.version);

                    if (latestVersion > currentVersion)
                    {
                        var result = MessageBox.Show(
                            $"🚀 New version available!\n\n" +
                            $"Current: {currentVersion}\n" +
                            $"Latest: {latestVersion}\n\n" +
                            $"Do you want to update now?",
                            "Update Available",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Information
                        );

                        if (result == DialogResult.Yes)
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = info.url,
                                UseShellExecute = true
                            });
                        }
                    }
                }
            }
            catch
            {
                // ❌ هیچ پیامی نده — لاگین نباید اذیت شود
            }
        }

        public login()
        {
            InitializeComponent();
        }
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        const int WM_NCLBUTTONDOWN = 0xA1;
        const int HTCAPTION = 0x2;

        /// <summary>
        /// این تابع رو به MouseDown یه کنترل (مثلاً پنل) وصل کن تا فرم قابل جابجا شدن بشه.
        /// </summary>
        void EnableDrag(Control targetControl, Form form)
        {
            targetControl.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(form.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
                }
            };
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (Environment.OSVersion.Version.Major >= 6) // Vista or later
            {
                MARGINS margins = new MARGINS()
                {
                    leftWidth = -1,
                    rightWidth = -1,
                    topHeight = -1,
                    bottomHeight = -1
                };
                DwmExtendFrameIntoClientArea(this.Handle, ref margins);
                this.BackColor = Color.Black;
            }
        }
        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }

        private void login_Load(object sender, EventArgs e)
        {
            _ = CheckForUpdateAsync();

            ServerConfig.Initialize();

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = this.pictureBox1.BackColor;
            this.TransparencyKey = this.pictureBox1.BackColor;
            this.BackColor = Color.Black;
            this.TransparencyKey = Color.Empty;
            this.AllowTransparency = false;

            // ساخت دیتابیس یوزرها
            UserDatabaseManager.InitializeUserDatabase();

            EnableDrag(panelb, this);
            EnableDrag(panelf, this);

            tb_level.Value = 0;
        }

        private void btnMinimized_click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btn_login_click(object sender, EventArgs e)
        {
            if (!tx_pass2.Visible == true && !tx_Rcode.Visible == true)
            {
                string username = tx_username.Text.Trim();
                string password = tx_password.Text.Trim();

                if (UserDatabaseManager.CheckLogin(username, password))
                {
                    if (UserDatabaseManager.CheckLogin(username, password))
                    {
                        // گرفتن UserID از دیتابیس
                        int userId = GetUserId(username);
                        UserDatabaseManager.CurrentUser.UserId = userId;
                        UserDatabaseManager.CurrentUser.Username = username;



                        OnlineUsersManager.SetUserOnline(username);

                        (this.Owner as Form1)?.LoginUser(username);

                        // باز کردن فرم اصلی
                        Form1 mainForm = new Form1();
                        mainForm.Show();

                        // بستن فرم لاگین
                        this.Hide();// یا this.Close(); اگر فرم اصلی رو به عنوان فرم اصلی برنامه تنظیم کرده باشی
                    }
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("❌ Username or password is incorrect");
                }
            }
            else { tx_pass2.Visible = false; tx_Rcode.Visible = false;
                btn_back.Visible = btn_next.Visible = false;
                tb_level.Value = 0;
                if (tx_Rcode.Location.X == 50)
                {
                    tx_username.Location = new Point(500, 150);
                    tx_password.Location = new Point(500, 210);
                }
                else
                {
                    tx_username.Location = new Point(50, 150);
                    tx_password.Location = new Point(50, 210);
                }

                tx_family.Location = new Point(500, 150);
                tx_Rcode.Location = new Point(500, 150);
                tx_Nc.Location = new Point(500, 150);
                tx_number.Location = new Point(500, 150);
                tx_card.Location = new Point(500, 150);
                tx_addres.Location = new Point(500, 150);
                tx_email.Location = new Point(500, 150);
                tx_pass2.Location = new Point(500, 150);
                BlindOpen(tx_username, 300);
                BlindOpen(tx_password, 300);
            }
        }
        private void btn_register(object sender, EventArgs e)
        {
            if (tx_pass2.Visible && tx_Rcode.Visible)
            {
                if (tx_password.Text != tx_pass2.Text)
                {
                    MessageBox.Show("Password and its repetition do not match ❌");
                    return;
                }

                if (
                    string.IsNullOrWhiteSpace(tx_username.Text) ||
                    string.IsNullOrWhiteSpace(tx_pass2.Text) ||
                    string.IsNullOrWhiteSpace(tx_family.Text) ||
                    string.IsNullOrWhiteSpace(tx_Nc.Text) ||
                    string.IsNullOrWhiteSpace(tx_number.Text) ||
                    string.IsNullOrWhiteSpace(tx_card.Text) ||
                    string.IsNullOrWhiteSpace(tx_addres.Text) ||
                    string.IsNullOrWhiteSpace(tx_email.Text)
                )
                {
                    MessageBox.Show("Please fill in all fields ❌");
                    return;
                }


                // ثبت کاربر با همه فیلدها
                bool success = UserDatabaseManager.AddUser(
                    tx_username.Text.Trim(),
                    tx_pass2.Text.Trim(),
                    tx_family.Text.Trim(),
                    tx_Nc.Text.Trim(),
                    tx_number.Text.Trim(),
                    tx_card.Text.Trim(),
                    tx_addres.Text.Trim(),
                    tx_email.Text.Trim()
                );

                if (success)
                {
                    MessageBox.Show("Your registration was successful ✅");
                }
                else
                {
                    MessageBox.Show("❌ This username is already registered");
                }
            }
            else
            {
                // مرحله اول فرم و باز کردن فیلدها
                btn_back.Location = new Point(140, 270);
                btn_next.Location = new Point(140, 270);
                tb_level.Value = 10;
                tx_password.Location = new Point(450, 150);
                tx_family.Location = new Point(50, 210);

                BlindOpen(tx_username, 300);
                BlindOpen(tx_family, 300);
                btn_back.Visible = btn_next.Visible = true;
                tx_Rcode.Visible = tx_pass2.Visible = true;
            }
        }


        private string VerifyCodeFromServer(string code)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    client.Connect("127.0.0.1", 5000); // اتصال دستی
                    using (NetworkStream stream = client.GetStream())
                    {
                        byte[] dataToSend = Encoding.UTF8.GetBytes(code);
                        stream.Write(dataToSend, 0, dataToSend.Length);

                        byte[] buffer = new byte[64];
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                        return response;
                    }
                }
            }
            catch (SocketException)
            {
                return "NO_SERVER";
            }
            catch
            {
                return "FAIL";
            }
        }
        private void BlindOpen(Control TextBox, int targetHeight)
        {
            if (tx_username.Width != 0 || tx_family.Width != 0)
            {
                TextBox.Width = 0;
                TextBox.Visible = true;
                Timer timer = new Timer();
                timer.Interval = 10;
                timer.Tick += (s, e) =>
                {
                    if (TextBox.Width < targetHeight)
                        TextBox.Width += 10;
                    else
                        timer.Stop();
                };
                timer.Start();
            }
            else TextBox.Width = 300;
        }
        int currentStep = 1;

        private void btn_next_Click(object sender, EventArgs e)
        {
            int CX = 31;
            int OUT = 500;

            switch (currentStep)
            {
                case 1:
                    if (string.IsNullOrWhiteSpace(tx_username.Text) || tx_username.Text.Length < 3)
                    {
                        MessageBox.Show("Username must be at least 3 letters ❌");
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(tx_family.Text) || tx_family.Text.Length < 3)
                    {
                        MessageBox.Show("Last name must be at least 3 letters ❌");
                        return;
                    }

                    // برن بیرون
                    tx_username.Location = new Point(OUT, 142);
                    tx_family.Location = new Point(OUT, 202);

                    // بیان وسط
                    tx_password.Location = new Point(CX, 142);
                    tx_pass2.Location = new Point(CX, 202);

                    BlindOpen(tx_password, 300);
                    BlindOpen(tx_pass2, 300);

                    tb_level.Value = 10;
                    currentStep++;
                    break;

                case 2:
                    string password = tx_password.Text.Trim();
                    string pass2 = tx_pass2.Text.Trim();

                    if (password.Length < 4 || password.Length > 10)
                    {
                        MessageBox.Show("Password must be between 4 and 10 characters ❌");
                        return;
                    }
                    if (password != pass2)
                    {
                        MessageBox.Show("Password and its repetition do not match ❌");
                        return;
                    }

                    // برن بیرون
                    tx_password.Location = new Point(OUT, 142);
                    tx_pass2.Location = new Point(OUT, 202);

                    // بیان وسط
                    tx_number.Location = new Point(CX, 142);
                    tx_card.Location = new Point(CX, 202);
                    tx_Nc.Location = new Point(CX, 262);

                    BlindOpen(tx_number, 300);
                    BlindOpen(tx_card, 300);
                    BlindOpen(tx_Nc, 300);

                    btn_back.Location = new Point(CX, 322);
                    btn_next.Location = new Point(CX, 322);

                    tb_level.Value = 40;
                    currentStep++;
                    break;

                case 3:
                    string number = tx_number.Text.Trim();
                    string card = tx_card.Text.Trim();
                    string nc = tx_Nc.Text.Trim();

                    if (number.Length != 11 || !number.All(char.IsDigit))
                    {
                        MessageBox.Show("The phone number must be exactly 11 digits long ❌");
                        return;
                    }
                    if (card.Length < 4 || card.Length > 16 || !card.All(char.IsDigit))
                    {
                        MessageBox.Show("The card number must be numeric and between 4 and 16 digits ❌");
                        return;
                    }
                    if (nc.Length != 10 || !nc.All(char.IsDigit))
                    {
                        MessageBox.Show("The national code must be exactly 10 numeric digits ❌");
                        return;
                    }

                    // برن بیرون
                    tx_number.Location = new Point(OUT, 142);
                    tx_card.Location = new Point(OUT, 202);
                    tx_Nc.Location = new Point(OUT, 262);

                    // بیان وسط
                    tx_email.Location = new Point(CX, 142);
                    tx_addres.Location = new Point(CX, 202);
                    tx_addres.Height = 115;

                    BlindOpen(tx_email, 300);
                    BlindOpen(tx_addres, 300);

                    tb_level.Value = 70;
                    currentStep++;
                    break;

                case 4:
                    string email = tx_email.Text.Trim();
                    string address = tx_addres.Text.Trim();

                    if (!email.Contains("@") || !email.Contains("."))
                    {
                        MessageBox.Show("The email entered is not valid ❌");
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(address) || address.Length < 5)
                    {
                        MessageBox.Show("Address must be at least 5 characters long ❌");
                        return;
                    }

                    // برن بیرون
                    tx_email.Location = new Point(OUT, 142);
                    tx_addres.Location = new Point(OUT, 202);

                    // بیان وسط
                    tx_Rcode.Location = new Point(CX, 142);
                    BlindOpen(tx_Rcode, 300);

                    btn_back.Location = new Point(CX, 270);
                    btn_next.Location = new Point(CX, 270);

                    tb_level.Value = 100;
                    currentStep++;
                    MessageBox.Show("All steps completed successfully ✅");
                    break;
            }
        }

        private void btn_back_Click(object sender, EventArgs e)
        {
            int CX = 31;
            int OUT = 500;

            switch (currentStep)
            {
                case 2:
                    tx_password.Location = new Point(OUT, 142);
                    tx_pass2.Location = new Point(OUT, 202);

                    tx_username.Location = new Point(CX, 142);
                    tx_family.Location = new Point(CX, 202);

                    BlindOpen(tx_username, 300);
                    BlindOpen(tx_family, 300);

                    tb_level.Value = 0;
                    currentStep--;
                    break;

                case 3:
                    tx_number.Location = new Point(OUT, 142);
                    tx_card.Location = new Point(OUT, 202);
                    tx_Nc.Location = new Point(OUT, 262);

                    tx_password.Location = new Point(CX, 142);
                    tx_pass2.Location = new Point(CX, 202);

                    BlindOpen(tx_password, 300);
                    BlindOpen(tx_pass2, 300);

                    btn_back.Location = new Point(CX, 270);
                    btn_next.Location = new Point(CX, 270);

                    tb_level.Value = 10;
                    currentStep--;
                    break;

                case 4:
                    tx_email.Location = new Point(OUT, 142);
                    tx_addres.Location = new Point(OUT, 202);

                    tx_number.Location = new Point(CX, 142);
                    tx_card.Location = new Point(CX, 202);
                    tx_Nc.Location = new Point(CX, 262);

                    BlindOpen(tx_number, 300);
                    BlindOpen(tx_card, 300);
                    BlindOpen(tx_Nc, 300);

                    btn_back.Location = new Point(CX, 330);
                    btn_next.Location = new Point(CX, 330);

                    tb_level.Value = 40;
                    currentStep--;
                    break;

                case 5:
                    tx_Rcode.Location = new Point(OUT, 142);

                    tx_email.Location = new Point(CX, 142);
                    tx_addres.Location = new Point(CX, 202);
                    tx_addres.Height = 115;

                    BlindOpen(tx_email, 300);
                    BlindOpen(tx_addres, 300);

                    btn_back.Location = new Point(CX, 330);
                    btn_next.Location = new Point(CX, 330);

                    tb_level.Value = 70;
                    currentStep--;
                    break;
            }
        }


        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Alt | Keys.F4) || keyData == Keys.Escape)
            {
                return true; // نادیده گرفتن کلید
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.TaskManagerClosing)
            {
                e.Cancel = true;
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to leave?", "Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private int GetUserId(string username)
        {
            using (var conn = new System.Data.SQLite.SQLiteConnection("Data Source=users.db;Version=3;"))
            {
                conn.Open();
                string query = "SELECT UserID FROM Users WHERE Username = @Username";
                using (var cmd = new System.Data.SQLite.SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    var result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : -1;
                }
            }
        }
    }
}
