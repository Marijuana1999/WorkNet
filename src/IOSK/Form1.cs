using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;
using static IOSK.Form1;
using System.Net.Http;
using System.Net;
using System.Diagnostics;
using System.Text.Json;
using System.Net.Http.Headers;
using Excel = Microsoft.Office.Interop.Excel;
using System.Drawing.Printing;
using System.Globalization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using Microsoft.Data.Sqlite;

namespace IOSK
{
    public partial class Form1 : Form
    {
        private DateTime _lastScanTime = DateTime.MinValue;
        private readonly TimeSpan _scanInterval = TimeSpan.FromSeconds(10);

        private HashSet<string> _notifiedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private bool isServerConnected = true; // وضعیت فعلی اتصال به سرور

        private string dailyReportsFilePath;
        public Form1()
        {
            InitializeComponent();

            this.TransparencyKey = Color.Empty;
            this.AllowTransparency = false;
            this.BackColor = Color.Black;
            this.Shown += Form1_Shown; // هندلر رو وصل کن


            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string companyFolder = Path.Combine(desktopPath, "Companies");
            if (!Directory.Exists(companyFolder))
            {
                Directory.CreateDirectory(companyFolder);
            }
            dailyReportsFilePath = Path.Combine(companyFolder, "DailyReports.xlsx");
        }

        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        private void Form1_Shown(object sender, EventArgs e)
        {
            string filePath = Path.Combine(desktopPath, "Companies", GlobalFileManager.ProjectsFile);
            if (File.Exists(filePath))
            {
                LoadInitialProjects(filePath, 3);
            }
            else
            {
                MessageBox.Show("Project file not found.");
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            GlobalFileManager.Initialize();
            LoadInitialProjects(GlobalFileManager.ProjectsFile, 10);

            ServerConfig.Initialize();
            //LoadExcelFiles(@"C:\Users\Milad\Desktop\Companies\پروژه سیمان.xlsx",
            //   @"C:\Users\Milad\Desktop\Companies\پروژه فولاد.xlsx");

            // مسیر پوشه آنلاین‌ها
            string onlinePath = ServerConfig.GetNetworkPath("OnlineUsers");
            if (!Directory.Exists(onlinePath))
            {
                //MessageBox.Show("⚠️ Unable to connect to server — application is running in local mode.");
                onlinePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LocalUsers");
                Directory.CreateDirectory(onlinePath);
            }

            // مسیر را به فیلد سراسری اختصاص بده
            userFolderPath = onlinePath;

            LoadOnlineUsers();



            string folderPath = ServerConfig.GetNetworkPath("");
            string fileName = "onlineusers.txt";

            if (string.IsNullOrEmpty(folderPath) || string.IsNullOrEmpty(fileName))
            {
                MessageBox.Show("The path or file name has no value.");
                return;
            }   

            //userFolderPath = Path.Combine(@"\\192.168.1.101\iosk\OnlineUsers", CurrentUsername);
            //// اطمینان از وجود پوشه
            //if (!Directory.Exists(userFolderPath))
            //    Directory.CreateDirectory(userFolderPath);

            //string fullPath = Path.Combine(folderPath, fileName);

            Timer t = new Timer();
            t.Interval = 5000; // هر 5 ثانیه
            t.Tick += (s, ev) => LoadOnlineUsers();
            t.Start();

            fileCheckTimer = new Timer();
            fileCheckTimer.Interval = 5000; // هر ۵ ثانیه
            fileCheckTimer.Tick += FileCheckTimer_Tick;
            fileCheckTimer.Start();

            LoadCurrentUserData();
            panelMiniCompanies.Location = new Point(20000, 0);
            panel_profile.Location = new Point(20000, 0);


            EnableDrag(panel_profile,this);
            EnableDrag(panelR, this);
            EnableDrag(panelcompanies, this);
            EnableDrag(panelMiniCompanies, this);
            EnableDrag(flowlayoutcompanis, this);
            EnableDrag(flowLayoutMiniCompanies, this);

            SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.BackColor = this.pictureBox1.BackColor;
            this.TransparencyKey = this.pictureBox1.BackColor;

            string filePath = Path.Combine(desktopPath, "Companies", GlobalFileManager.ProjectsFile);

            panelcompanies.Visible = true;

            if (System.IO.File.Exists(filePath))
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    LoadInitialProjects(filePath, 5);
                });
            }
            else
            {
                MessageBox.Show("Project file not found.");
            }


            //foreach (string printer in PrinterSettings.InstalledPrinters)
            //{
            //    cmbPrinters.Items.Add(printer);
            //}

            //// انتخاب پرینتر پیش‌فرض
            //PrinterSettings settings = new PrinterSettings();
            //cmbPrinters.SelectedItem = settings.PrinterName;

        }

        public void LoginUser(string username) // Changed from private to public
        {
            txtUsername.Text = username.Trim();
            OnlineUsersManager.SetUserOnline(txtUsername.Text);

            userFolderPath = Path.Combine(ServerConfig.GetNetworkPath("OnlineUsers"), txtUsername.Text);
            if (!Directory.Exists(userFolderPath))
                Directory.CreateDirectory(userFolderPath);

            // حالا تایمر رو اینجا استارت کن
            fileCheckTimer = new Timer();
            fileCheckTimer.Interval = 5000;
            fileCheckTimer.Tick += FileCheckTimer_Tick;
            fileCheckTimer.Start();
        }


        private void Home_Click(object sender, EventArgs e)
        {
            // مخفی کردن همه پنل‌ها
            Point hiddenLocation = new Point(20000, 0);
            panelcompanies.Location = hiddenLocation;
            panelMiniCompanies.Location = hiddenLocation;
            panel_profile.Location = hiddenLocation;
            panelForDailyReportInput.Location = hiddenLocation;


            panelcompanies.Visible = false;
            panelMiniCompanies.Visible = false;
            panel_profile.Visible = false;
            panelForDailyReportInput.Visible = false;
            

            // نمایش پنل کمپانی
            panelcompanies.Location = new Point(206, 0);
            panelcompanies.Visible = true;

            // لود فقط ۳ پروژه اول
            string filePath = Path.Combine(desktopPath, "Companies", GlobalFileManager.ProjectsFile);
            if (System.IO.File.Exists(filePath))
            {
                LoadInitialProjects(filePath, 5);
            }
            else
            {
                MessageBox.Show("Project file not found.");
            }
        }
        private DateTime? ParsePersianDate(string dateStr)
        {
            try
            {
                var parts = dateStr.Split('/', '-', '.', ' ');
                if (parts.Length < 3) return null;
                int day = int.Parse(parts[0]);
                int month = int.Parse(parts[1]);
                int year = int.Parse(parts[2]);

                var persianCalendar = new System.Globalization.PersianCalendar();
                return persianCalendar.ToDateTime(year, month, day, 0, 0, 0, 0);
            }
            catch
            {
                return null;
            }
        }
        //private void LoadInitialProjects(string filePath, int maxItems)
        //{
        //    flowlayoutcompanis.Controls.Clear();
        //    companies.Clear();

        //    if (!File.Exists(filePath))
        //    {
        //        MessageBox.Show("Projects file not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        return;
        //    }

        //    using (var workbook = new XLWorkbook(filePath))
        //    {
        //        IXLWorksheet worksheet;
        //        try
        //        {
        //            worksheet = workbook.Worksheet("Projects");
        //        }
        //        catch
        //        {
        //            MessageBox.Show("Worksheet 'Projects' not found in the file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //            return;
        //        }

        //        string[] headers = { "Company Name", "Project Name", "Delivery Date", "Request Number", "Final Result", "Description" };
        //        var lastCell = worksheet.LastCellUsed();
        //        if (lastCell == null) return;

        //        int colCount = lastCell.Address.ColumnNumber;
        //        int rowCount = lastCell.Address.RowNumber;

        //        var headerIndices = new Dictionary<string, int>();
        //        Func<string, string> normalize = s => s.Trim().Replace("\u200C", " ").Replace("\u00A0", " ").Trim();

        //        for (int col = 1; col <= colCount; col++)
        //        {
        //            string header = normalize(worksheet.Cell(1, col).GetValue<string>());
        //            foreach (var wanted in headers)
        //            {
        //                if (normalize(header).Equals(normalize(wanted), StringComparison.OrdinalIgnoreCase))
        //                    headerIndices[wanted] = col;
        //            }
        //        }

        //        DateTime today = DateTime.Now.Date;
        //        var tempList = new List<(CompanyData data, Panel panel, DateTime? deadline)>();

        //        for (int row = 2; row <= rowCount; row++)
        //        {
        //            bool rowHasAny = headerIndices.Values.Any(col => !string.IsNullOrWhiteSpace(worksheet.Cell(row, col).GetValue<string>()));
        //            if (!rowHasAny) continue;

        //            var data = new CompanyData();
        //            DateTime? deliveryDate = null;

        //            foreach (var header in headers)
        //            {
        //                if (headerIndices.ContainsKey(header))
        //                {
        //                    string cellValue = worksheet.Cell(row, headerIndices[header]).GetValue<string>().Trim();
        //                    switch (header)
        //                    {
        //                        case "Company Name": data.CompanyName = cellValue; break;
        //                        case "Project Name": data.ProjectName = cellValue; break;
        //                        case "Delivery Date":
        //                            data.DeliveryDate = cellValue;
        //                            if (DateTime.TryParse(cellValue, out DateTime parsedDate))
        //                                deliveryDate = parsedDate;
        //                            break;
        //                        case "Request Number": data.RequestNumber = cellValue; break;
        //                        case "Final Result": data.FinalResult = cellValue; break;
        //                        case "Description": data.Description = cellValue; break;
        //                    }
        //                }
        //            }

        //            if (!deliveryDate.HasValue) continue;

        //            // Only this month
        //            if (deliveryDate.Value.Year != today.Year || deliveryDate.Value.Month != today.Month)
        //                continue;

        //            // Create project panel
        //            Panel itemPanel = new Panel
        //            {
        //                Width = 300,
        //                AutoSize = true,
        //                BackColor = Color.FromArgb(46, 46, 46),
        //                Margin = new Padding(10),
        //                Padding = new Padding(10),
        //                BorderStyle = BorderStyle.None
        //            };

        //            // Rounded corners
        //            itemPanel.Paint += (s, e) =>
        //            {
        //                int radius = 15;
        //                var path = new System.Drawing.Drawing2D.GraphicsPath();
        //                path.AddArc(0, 0, radius, radius, 180, 90);
        //                path.AddArc(itemPanel.Width - radius, 0, radius, radius, 270, 90);
        //                path.AddArc(itemPanel.Width - radius, itemPanel.Height - radius, radius, radius, 0, 90);
        //                path.AddArc(0, itemPanel.Height - radius, radius, radius, 90, 90);
        //                path.CloseAllFigures();
        //                itemPanel.Region = new Region(path);
        //            };

        //            int yOffset = 0;
        //            foreach (var header in headers)
        //            {
        //                if (headerIndices.ContainsKey(header))
        //                {
        //                    string cellValue = worksheet.Cell(row, headerIndices[header]).GetValue<string>().Trim();
        //                    Label lblField = new Label
        //                    {
        //                        AutoSize = false,
        //                        Width = itemPanel.Width - 20,
        //                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
        //                        ForeColor = Color.Gray,
        //                        Text = $"{header}: {cellValue}",
        //                        Location = new Point(5, yOffset),
        //                        Height = 25
        //                    };
        //                    yOffset += 28;
        //                    itemPanel.Controls.Add(lblField);
        //                }
        //            }

        //            tempList.Add((data, itemPanel, deliveryDate));
        //        }

        //        // Sort by deadline
        //        var sorted = tempList.OrderBy(t => t.deadline ?? DateTime.MaxValue).Take(maxItems).ToList();

        //        foreach (var (data, itemPanel, deliveryDate) in sorted)
        //        {
        //            int daysLeft = deliveryDate.HasValue ? (deliveryDate.Value - today).Days : int.MaxValue;
        //            Color circleColor = Color.Green;
        //            if (daysLeft <= 1) circleColor = Color.Red;
        //            else if (daysLeft <= 7) circleColor = Color.Orange;

        //            Panel circleIndicator = new Panel
        //            {
        //                Width = 12,
        //                Height = 12,
        //                BackColor = Color.Transparent,
        //                Location = new Point(5, 5)
        //            };
        //            circleIndicator.Paint += (s, e) =>
        //            {
        //                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        //                using (Brush b = new SolidBrush(circleColor))
        //                {
        //                    e.Graphics.FillEllipse(b, 0, 0, circleIndicator.Width - 1, circleIndicator.Height - 1);
        //                }
        //            };
        //            itemPanel.Controls.Add(circleIndicator);
        //            circleIndicator.BringToFront();

        //            data.Panel = itemPanel;
        //            companies.Add(data);
        //            flowlayoutcompanis.Controls.Add(itemPanel);
        //        }
        //    }
        //}
        private void LoadInitialProjects(string filePath, int maxItems)
        {
            flowlayoutcompanis.Controls.Clear();
            companies.Clear();

            if (!File.Exists(filePath))
            {
                MessageBox.Show("Projects file not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var workbook = new XLWorkbook(filePath))
            {
                IXLWorksheet worksheet;
                try
                {
                    worksheet = workbook.Worksheet("Projects");
                }
                catch
                {
                    MessageBox.Show("Worksheet 'Projects' not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string[] headers =
                {
            "Company Name",
            "Project Name",
            "Delivery Date",
            "Request Number",
            "Final Result",
            "Description"
        };

                var lastCell = worksheet.LastCellUsed();
                if (lastCell == null) return;

                int colCount = lastCell.Address.ColumnNumber;
                int rowCount = lastCell.Address.RowNumber;

                var headerIndices = new Dictionary<string, int>();
                Func<string, string> normalize =
                    s => s.Trim().Replace("\u200C", " ").Replace("\u00A0", " ").Trim();

                // پیدا کردن ایندکس ستون‌ها
                for (int col = 1; col <= colCount; col++)
                {
                    string header = normalize(worksheet.Cell(1, col).GetValue<string>());
                    foreach (var wanted in headers)
                    {
                        if (normalize(header)
                            .Equals(normalize(wanted), StringComparison.OrdinalIgnoreCase))
                        {
                            headerIndices[wanted] = col;
                        }
                    }
                }

                DateTime today = DateTime.Now.Date;
                var tempList = new List<(CompanyData data, Panel panel, DateTime deadline)>();

                for (int row = 2; row <= rowCount; row++)
                {
                    bool rowHasAny =
                        headerIndices.Values.Any(col =>
                            !string.IsNullOrWhiteSpace(
                                worksheet.Cell(row, col).GetValue<string>()));

                    if (!rowHasAny) continue;

                    var data = new CompanyData();
                    DateTime? deliveryDate = null;

                    foreach (var header in headers)
                    {
                        if (!headerIndices.ContainsKey(header)) continue;

                        string cellValue =
                            worksheet.Cell(row, headerIndices[header])
                                     .GetValue<string>()
                                     .Trim();

                        switch (header)
                        {
                            case "Company Name":
                                data.CompanyName = cellValue;
                                break;

                            case "Project Name":
                                data.ProjectName = cellValue;
                                break;

                            case "Delivery Date":
                                data.DeliveryDate = cellValue;

                                // میلادی
                                if (DateTime.TryParse(cellValue, out DateTime gDate))
                                    deliveryDate = gDate;
                                else
                                    deliveryDate = ParsePersianDate(cellValue); // شمسی

                                break;

                            case "Request Number":
                                data.RequestNumber = cellValue;
                                break;

                            case "Final Result":
                                data.FinalResult = cellValue;
                                break;

                            case "Description":
                                data.Description = cellValue;
                                break;
                        }
                    }

                    // تاریخ نامعتبر → حذف
                    if (!deliveryDate.HasValue)
                        continue;

                    // پروژه‌های گذشته حذف شوند
                    int daysLeft = (deliveryDate.Value.Date - today).Days;
                    if (daysLeft < 0)
                        continue;

                    // ---------- UI Card ----------
                    Panel itemPanel = new Panel
                    {
                        Width = 300,
                        AutoSize = true,
                        BackColor = Color.FromArgb(46, 46, 46),
                        Margin = new Padding(10),
                        Padding = new Padding(10),
                        BorderStyle = BorderStyle.None
                    };

                    // Rounded corners
                    itemPanel.Paint += (s, e) =>
                    {
                        int radius = 15;
                        var path = new System.Drawing.Drawing2D.GraphicsPath();
                        path.AddArc(0, 0, radius, radius, 180, 90);
                        path.AddArc(itemPanel.Width - radius, 0, radius, radius, 270, 90);
                        path.AddArc(itemPanel.Width - radius, itemPanel.Height - radius, radius, radius, 0, 90);
                        path.AddArc(0, itemPanel.Height - radius, radius, radius, 90, 90);
                        path.CloseAllFigures();
                        itemPanel.Region = new Region(path);
                    };

                    int yOffset = 0;
                    foreach (var header in headers)
                    {
                        if (!headerIndices.ContainsKey(header)) continue;

                        string cellValue =
                            worksheet.Cell(row, headerIndices[header])
                                     .GetValue<string>()
                                     .Trim();

                        Label lblField = new Label
                        {
                            AutoSize = false,
                            Width = itemPanel.Width - 20,
                            Font = new Font("Segoe UI", 10, FontStyle.Bold),
                            ForeColor = Color.Gray,
                            Text = $"{header}: {cellValue}",
                            Location = new Point(5, yOffset),
                            Height = 25
                        };

                        yOffset += 28;
                        itemPanel.Controls.Add(lblField);
                    }

                    tempList.Add((data, itemPanel, deliveryDate.Value));
                }

                // مرتب‌سازی بر اساس نزدیک‌ترین Deadline
                var sorted =
                    tempList
                    .OrderBy(t => t.deadline)
                    .Take(maxItems)
                    .ToList();

                foreach (var (data, itemPanel, deadline) in sorted)
                {
                    int daysLeft = (deadline - today).Days;

                    Color circleColor =
                        daysLeft <= 1 ? Color.Red :
                        daysLeft <= 7 ? Color.Orange :
                        Color.Green;

                    Panel circleIndicator = new Panel
                    {
                        Width = 12,
                        Height = 12,
                        BackColor = Color.Transparent,
                        Location = new Point(5, 5)
                    };

                    circleIndicator.Paint += (s, e) =>
                    {
                        e.Graphics.SmoothingMode =
                            System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                        using (Brush b = new SolidBrush(circleColor))
                            e.Graphics.FillEllipse(
                                b, 0, 0,
                                circleIndicator.Width - 1,
                                circleIndicator.Height - 1);
                    };

                    itemPanel.Controls.Add(circleIndicator);
                    circleIndicator.BringToFront();

                    data.Panel = itemPanel;
                    companies.Add(data);
                    flowlayoutcompanis.Controls.Add(itemPanel);
                }
            }
        }


        private void SwitchPanel(Control targetPanel)
        {
            Point visibleLocation = new Point(206, 0);
            Point hiddenLocation = new Point(20000, 0);

            // لیست همه پنل‌ها
            var panels = new List<Control> { panelcompanies, panelMiniCompanies, panel_profile ,panelForDailyReportInput};

            foreach (var panel in panels)
            {
                bool isTarget = panel == targetPanel;
                panel.Location = isTarget ? visibleLocation : hiddenLocation;
                panel.Visible = isTarget;
            }
        }

        private void LoadExcelFiles(params string[] filePaths)
        {
            flowlayoutcompanis.Controls.Clear();
            companies.Clear();
            //flowlayoutcompanis.FlowDirection = FlowDirection.TopDown;
            //flowlayoutcompanis.WrapContents = false;
            //flowlayoutcompanis.AutoScroll = true;
            //flowlayoutcompanis.Padding = new Padding(10);
            //flowlayoutcompanis.BackColor = Color.FromArgb(40, 40, 40);

            var seenKeys = new HashSet<string>();

            foreach (var filePath in filePaths)
            {
                if (!File.Exists(filePath)) continue;

                using (var workbook = new XLWorkbook(filePath))
                {
                    IXLWorksheet worksheet;
                    try
                    {
                        worksheet = workbook.Worksheet("Projects");
                    }
                    catch
                    {
                        MessageBox.Show($"Worksheet 'Projects' not found in: {Path.GetFileName(filePath)}",
                            "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        continue;
                    }

                    string[] headers = { "Company Name", "Project Name", "Delivery Date", "Request Number", "Final Result", "Description" };
                    var lastCell = worksheet.LastCellUsed();
                    if (lastCell == null) continue;

                    int colCount = lastCell.Address.ColumnNumber;
                    int rowCount = lastCell.Address.RowNumber;

                    var headerIndices = new Dictionary<string, int>();
                    Func<string, string> normalize = s => s.Trim().Replace("\u200C", " ").Replace("\u00A0", " ").Trim();

                    // Find header columns
                    for (int col = 1; col <= colCount; col++)
                    {
                        string header = normalize(worksheet.Cell(1, col).GetValue<string>());
                        foreach (var wanted in headers)
                        {
                            if (normalize(header).Equals(normalize(wanted), StringComparison.OrdinalIgnoreCase))
                                headerIndices[wanted] = col;
                        }
                    }

                    string lastCompany = null;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var data = new CompanyData();
                        foreach (var header in headers)
                        {
                            if (headerIndices.ContainsKey(header))
                            {
                                string cellValue = worksheet.Cell(row, headerIndices[header]).GetValue<string>().Trim();
                                switch (header)
                                {
                                    case "Company Name": data.CompanyName = cellValue; break;
                                    case "Project Name": data.ProjectName = cellValue; break;
                                    case "Delivery Date": data.DeliveryDate = cellValue; break;
                                    case "Request Number": data.RequestNumber = cellValue; break;
                                    case "Final Result": data.FinalResult = cellValue; break;
                                    case "Description": data.Description = cellValue; break;
                                }
                            }
                        }

                        string uniqueKey = $"{data.CompanyName}|{data.ProjectName}|{data.RequestNumber}";
                        if (seenKeys.Contains(uniqueKey)) continue;
                        seenKeys.Add(uniqueKey);
                        // --- Company header ---
                        // --- Company header ---
                        if (!string.IsNullOrEmpty(data.CompanyName) && data.CompanyName != lastCompany)
                        {
                            Label lblCompany = new Label
                            {
                                AutoSize = true,
                                Text = data.CompanyName,
                                Font = new Font("Segoe UI Semibold", 13, FontStyle.Bold),
                                ForeColor = Color.FromArgb(255, 180, 0), // gold accent
                                Margin = new Padding(10, 20, 10, 5)
                            };
                            flowlayoutcompanis.Controls.Add(lblCompany);
                            lastCompany = data.CompanyName;
                        }

                        // --- Project card ---
                        Panel itemPanel = new Panel
                        {
                            Width = flowlayoutcompanis.ClientSize.Width - 40,
                            Height = 105,
                            BackColor = Color.FromArgb(55, 55, 55),
                            Margin = new Padding(10, 5, 10, 10),
                            Padding = new Padding(15, 10, 15, 10),
                            BorderStyle = BorderStyle.None,
                            Anchor = AnchorStyles.Left | AnchorStyles.Right,
                            Tag = data.ProjectName
                        };

                        // rounded corners
                        itemPanel.Paint += (s, e) =>
                        {
                            int radius = 12;
                            var path = new System.Drawing.Drawing2D.GraphicsPath();
                            path.AddArc(0, 0, radius, radius, 180, 90);
                            path.AddArc(itemPanel.Width - radius, 0, radius, radius, 270, 90);
                            path.AddArc(itemPanel.Width - radius, itemPanel.Height - radius, radius, radius, 0, 90);
                            path.AddArc(0, itemPanel.Height - radius, radius, radius, 90, 90);
                            path.CloseAllFigures();
                            itemPanel.Region = new Region(path);
                        };

                        // --- Labels ---
                        Label lblProject = new Label
                        {
                            AutoSize = false,
                            Width = itemPanel.Width - 120,
                            Height = 24,
                            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                            ForeColor = Color.Gainsboro,
                            Text = $"📁 Project: {data.ProjectName}",
                            Location = new Point(15, 10),
                            BackColor = Color.Transparent
                        };
                        itemPanel.Controls.Add(lblProject);

                        Label lblDeadline = new Label
                        {
                            AutoSize = false,
                            Width = itemPanel.Width - 120,
                            Height = 22,
                            Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                            ForeColor = Color.Silver,
                            Text = $"⏱ Delivery: {data.DeliveryDate}",
                            Location = new Point(15, 35),
                            BackColor = Color.Transparent
                        };
                        itemPanel.Controls.Add(lblDeadline);

                        Label lblResult = new Label
                        {
                            AutoSize = false,
                            Width = itemPanel.Width - 120,
                            Height = 22,
                            Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                            ForeColor = Color.DarkGray,
                            Text = $"✅ Result: {data.FinalResult}",
                            Location = new Point(15, 58),
                            BackColor = Color.Transparent
                        };
                        itemPanel.Controls.Add(lblResult);

                        // --- Button (right aligned, top layer) ---
                        Button btnExpand = new Button
                        {
                            Text = "Open",
                            Width = 70,
                            Height = 35,
                            Anchor = AnchorStyles.Top | AnchorStyles.Right,
                            Location = new Point(itemPanel.Width - 90, 10),
                            BackColor = Color.FromArgb(75, 75, 75),
                            ForeColor = Color.White,
                            Font = new Font("Segoe UI", 9, FontStyle.Bold),
                            FlatStyle = FlatStyle.Flat,
                            Cursor = Cursors.Hand
                        };
                        btnExpand.FlatAppearance.BorderSize = 0;
                        btnExpand.FlatAppearance.MouseOverBackColor = Color.FromArgb(95, 95, 95);
                        btnExpand.FlatAppearance.MouseDownBackColor = Color.FromArgb(110, 110, 110);

                        // always on top
                        itemPanel.Controls.Add(btnExpand);
                        btnExpand.BringToFront();

                        // click event
                        string sheetNameCopy = "Projects";
                        string filePathCopy = filePath;
                        btnExpand.Click += (s, e) =>
                        {
                            try
                            {
                                var excelApp = new Excel.Application { Visible = true };
                                var wb = excelApp.Workbooks.Open(filePathCopy);
                                var sheet = wb.Sheets[sheetNameCopy] as Excel.Worksheet;
                                if (sheet != null)
                                    sheet.Activate();
                                else
                                    MessageBox.Show($"Sheet '{sheetNameCopy}' not found!");
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error opening Excel file:\n{ex.Message}");
                            }
                        };

                        // add to flow
                        data.Panel = itemPanel;
                        data.FilePath = filePath;
                        companies.Add(data);
                        flowlayoutcompanis.Controls.Add(itemPanel);

                    }
                }
            }
        }


        private void txsearchcompanis_DoubleClick(object sender, EventArgs e)
        {

        }


        private void btnLoad_Click(object sender, EventArgs e)
        {
            SwitchPanel(panelcompanies);

            // مسیر فایل‌ها
            string path1 = Path.Combine(desktopPath, "Companies", GlobalFileManager.ProjectsFile);
            string path2 = Path.Combine(desktopPath, "Companies", GlobalFileManager.ProjectsFile);

            List<string> filePaths = new List<string>();

            if (File.Exists(path1)) filePaths.Add(path1);
            if (File.Exists(path2)) filePaths.Add(path2);

            if (filePaths.Count > 0)
            {
                // متد جدید که چند فایل می‌خونه
                LoadExcelFiles(filePaths.ToArray());
            }
            else
            {
                MessageBox.Show("هیچ‌کدوم از فایل‌ها پیدا نشد.");
            }
        }

        private List<CompanyData> companies = new List<CompanyData>();

        public class CompanyData
        {
            public string CompanyName { get; set; }
            public string ProjectName { get; set; }
            public string DeliveryDate { get; set; }
            public string RequestNumber { get; set; }
            public string FinalResult { get; set; }
            public string Description { get; set; }
            public Panel Panel { get; set; }
            public string FilePath { get; set; } // مسیر فایل اکسل
        }
        private void btn_LoadExcelCompaniesData_Click(object sender, EventArgs e)
        {
            SwitchPanel(panelMiniCompanies);

            string filePathHoldings = GlobalFileManager.DataBankFile;

            // Check default global file first
            if (!File.Exists(filePathHoldings))
            {
                // Ask user to select a file manually
                OpenFileDialog ofd = new OpenFileDialog
                {
                    Filter = "Excel Files|*.xlsx;*.xls",
                    Title = "Please select the DataBank.xlsx file"
                };

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    filePathHoldings = ofd.FileName;
                }
                else
                {
                    MessageBox.Show("Data Bank file not found or not selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            try
            {
                LoadExcelData_Holdings(filePathHoldings);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading Data Bank file:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void LoadExcelData_Holdings(string filePathHoldings)
        {
            flowLayoutMiniCompanies.Controls.Clear();
            flowLayoutMiniCompanies.FlowDirection = FlowDirection.TopDown;
            flowLayoutMiniCompanies.WrapContents = false;
            flowLayoutMiniCompanies.AutoScroll = true;

            Label lblHeader = new Label
            {
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.Gray,
                AutoSize = true,
                Margin = new Padding(5)
            };
            flowLayoutMiniCompanies.Controls.Add(lblHeader);

            if (!File.Exists(filePathHoldings))
            {
                MessageBox.Show("Data Bank file not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var workbookData = new XLWorkbook(filePathHoldings))
            {
                IXLWorksheet worksheet;
                try
                {
                    worksheet = workbookData.Worksheet("Reviewed");
                }
                catch
                {
                    MessageBox.Show("Worksheet 'Reviewed' not found in Data Bank file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int lastRow = worksheet.LastRowUsed().RowNumber();
                string lastHoldingName = "";

                for (int row = 2; row <= lastRow; row++)
                {
                    string holdingName = worksheet.Cell(row, 1).GetString().Trim();
                    string companyName = worksheet.Cell(row, 2).GetString().Trim();
                    string targetSheet = worksheet.Cell(row, 3).GetString().Trim(); // new column for link target

                    if (string.IsNullOrEmpty(holdingName) && row > 2)
                        holdingName = worksheet.Cell(row - 1, 1).GetString().Trim();

                    if (!string.IsNullOrEmpty(holdingName) && holdingName != lastHoldingName)
                    {
                        Label lblHolding = new Label
                        {
                            Text = holdingName,
                            Font = new Font("Segoe UI Semibold", 13, FontStyle.Bold),
                            ForeColor = Color.FromArgb(255, 180, 0), // طلایی ملایم
                            AutoSize = true,
                            Margin = new Padding(10, 25, 10, 5)
                        };
                        flowLayoutMiniCompanies.Controls.Add(lblHolding);
                        lastHoldingName = holdingName;
                    }

                    if (!string.IsNullOrEmpty(companyName))
                    {
                        Panel itemPanel = new Panel
                        {
                            Width = 580,
                            Height = 65,
                            BackColor = Color.FromArgb(55, 55, 55),
                            Margin = new Padding(10, 5, 10, 10),
                            Padding = new Padding(15, 10, 15, 10),
                            BorderStyle = BorderStyle.None,
                            Tag = companyName
                        };

                        // گرد کردن گوشه‌های پنل
                        itemPanel.Paint += (s, e) =>
                        {
                            int radius = 12;
                            var path = new System.Drawing.Drawing2D.GraphicsPath();
                            path.AddArc(0, 0, radius, radius, 180, 90);
                            path.AddArc(itemPanel.Width - radius, 0, radius, radius, 270, 90);
                            path.AddArc(itemPanel.Width - radius, itemPanel.Height - radius, radius, radius, 0, 90);
                            path.AddArc(0, itemPanel.Height - radius, radius, radius, 90, 90);
                            path.CloseAllFigures();
                            itemPanel.Region = new Region(path);
                        };

                        // نام شرکت
                        Label lblCompany = new Label
                        {
                            AutoSize = false,
                            Width = itemPanel.Width - 110,
                            Height = 40,
                            Location = new Point(15, 12),
                            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                            ForeColor = Color.Gainsboro,
                            Text = $"🏢  {companyName}",
                            TextAlign = ContentAlignment.MiddleLeft,
                            BackColor = Color.Transparent
                        };
                        itemPanel.Controls.Add(lblCompany);

                        // دکمه باز کردن فایل
                        Button btnOpenSheet = new Button
                        {
                            Text = "Open",
                            Width = 70,
                            Height = 35,
                            Location = new Point(itemPanel.Width - 85, 15),
                            BackColor = Color.FromArgb(80, 80, 80),
                            ForeColor = Color.White,
                            Font = new Font("Segoe UI", 9, FontStyle.Bold),
                            FlatStyle = FlatStyle.Flat,
                            Cursor = Cursors.Hand,
                            TabStop = false,
                            Tag = companyName
                        };
                        btnOpenSheet.FlatAppearance.BorderSize = 0;
                        btnOpenSheet.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, 100, 100);
                        btnOpenSheet.FlatAppearance.MouseDownBackColor = Color.FromArgb(120, 120, 120);

                        string sheetNameCopy = targetSheet;
                        string filePathCopy = filePathHoldings;

                        btnOpenSheet.Click += (s, e) =>
                        {
                            try
                            {
                                if (string.IsNullOrEmpty(sheetNameCopy))
                                {
                                    MessageBox.Show("Target sheet name not found or empty!");
                                    return;
                                }

                                var excelApp = new Excel.Application();
                                excelApp.Visible = true;
                                var workbook = excelApp.Workbooks.Open(filePathCopy);
                                var sheet = workbook.Sheets[sheetNameCopy] as Excel.Worksheet;
                                sheet?.Activate();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error opening target sheet: {ex.Message}");
                            }
                        };

                        itemPanel.Controls.Add(btnOpenSheet);
                        flowLayoutMiniCompanies.Controls.Add(itemPanel);
                    }
                }
            }
        }
        private void txtSearchMiniCompanies_Double_click(object sender, EventArgs e)
        {

        }

        private List<CompanyData2> companies2 = new List<CompanyData2>();

        public class CompanyData2
        {
            public string CompanyName { get; set; }
            public string OfficeNumber { get; set; }
            public string Website { get; set; }
            public Panel Panel { get; set; }
        }



        private void btn_edit_Click(object sender, EventArgs e)
        {
            txtAddress.Enabled = txtCardNum.Enabled = txtEmail.Enabled = txtFamily.Enabled = txtNationalCode.Enabled = txtNewPassword.Enabled = txtOldPassword.Enabled = txtTell.Enabled = txtUsername.Enabled = true;
            txtNewPassword.Visible = txtOldPassword.Visible = true;
        }

        private void btn_profile_Click(object sender, EventArgs e)
        {

        }

        private void btnSaveChanges_Click(object sender, EventArgs e)
        {
            int userId = UserDatabaseManager.CurrentUser.UserId;

            string username = txtUsername.Text.Trim();
            string family = txtFamily.Text.Trim();
            string tell = txtTell.Text.Trim();
            string email = txtEmail.Text.Trim();
            string nc = txtNationalCode.Text.Trim();
            string address = txtAddress.Text.Trim();
            string card = txtCardNum.Text.Trim();

            UserDatabaseManager.UpdateUserInfo(userId, username, family, tell, email, nc, address, card);

            string oldPass = txtOldPassword.Text.Trim();
            string newPass = txtNewPassword.Text.Trim();

            if (!string.IsNullOrEmpty(oldPass) && !string.IsNullOrEmpty(newPass))
            {
                bool changed = UserDatabaseManager.ChangePassword(userId, oldPass, newPass);
                if (changed)
                {
                    MessageBox.Show("✅ Information and password have been successfully updated.");
                }
                else
                {
                    MessageBox.Show("❌ The current password is incorrect. The information was saved but the password was not changed..");
                }
            }
            else
            {
                MessageBox.Show("✅ Information updated successfully.");
            }

            txtOldPassword.Text = "";
            txtNewPassword.Text = "";
            txtAddress.Enabled = txtCardNum.Enabled = txtEmail.Enabled = txtFamily.Enabled = txtNationalCode.Enabled = txtNewPassword.Enabled = txtOldPassword.Enabled = txtTell.Enabled = txtUsername.Enabled = false;
        }

        private void LoadCurrentUserData()
        {
            int userId = UserDatabaseManager.CurrentUser.UserId;
            Dictionary<string, string> userDetails = UserDatabaseManager.GetUserDetails(userId);

            if (userDetails.Count > 0)
            {
                txtUsername.Text = userDetails["Username"];
                txtFamily.Text = userDetails["Family"];
                txtEmail.Text = userDetails["Email"];
                txtCardNum.Text = userDetails["CardNum"];
                txtTell.Text = userDetails["Tell"];
                txtNationalCode.Text = userDetails["NationalCode"];
                txtAddress.Text = userDetails["Address"];
            }
            else
            {
                MessageBox.Show("User information not found");
            }
        }

        private void BlindOpen(Control panel, int targetHeight)
        {
            if (!panel_profile.Visible)
            {
                panel.Height = 0;
                panel.Visible = true;
                Timer timer = new Timer();
                timer.Interval = 5;
                timer.Tick += (s, e) =>
                {
                    if (panel.Height < targetHeight)
                        panel.Height += 30;
                    else
                        timer.Stop();
                };
                timer.Start();
            }
            else panel.Height = targetHeight;
        }

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        const int WM_NCLBUTTONDOWN = 0xA1;
        const int HTCAPTION = 0x2;

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

        private void btn_profile_Click_1(object sender, EventArgs e)
        {
            SwitchPanel(panel_profile);

        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentUsername))
            {
                OnlineUsersManager.SetUserOffline(txtUsername.Text); // پاک کردن فایل .online
            }
            Application.Exit();
        }


        private void button_marijuana1999_ui4_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {

                WindowState = FormWindowState.Maximized;
            }
            else { WindowState = FormWindowState.Normal;}
        }

        private void button_marijuana1999_ui3_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Normal;
            }
            else WindowState = FormWindowState.Minimized;
        }

        private Timer fileCheckTimer;
        private string userFolderPath = ServerConfig.GetNetworkPath("OnlineUsers")
;

        private void FileCheckTimer_Tick(object sender, EventArgs e)
        {
            // ⏱️ جلوگیری از اسکن بیش از حد
            if (DateTime.Now - _lastScanTime < _scanInterval)
                return;

            _lastScanTime = DateTime.Now;

            string username = txtUsername.Text.Trim();
            if (string.IsNullOrEmpty(username))
                return; // ❌ دیگه MessageBox توی Timer نداریم

            string onlineRoot = ServerConfig.GetNetworkPath("OnlineUsers");
            bool networkAvailable = Directory.Exists(onlineRoot);

            string userFolder;

            if (networkAvailable)
            {
                if (!isServerConnected)
                {
                    isServerConnected = true;
                    MessageBox.Show(
                        "✅ Connection to server restored. Back to online mode.",
                        "Connection Restored",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                userFolder = Path.Combine(onlineRoot, username);
            }
            else
            {
                if (isServerConnected)
                {
                    isServerConnected = false;
                    MessageBox.Show(
                        "⚠️ Cannot connect to server — switched to local mode.",
                        "Connection Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }

                userFolder = Path.Combine(GlobalFileManager.LocalUsersFolder, username);
            }

            // 📁 آماده‌سازی فولدرها
            string seenFolder = Path.Combine(userFolder, "Seen");

            try
            {
                Directory.CreateDirectory(userFolder);
                Directory.CreateDirectory(seenFolder);
            }
            catch
            {
                return; // اگر فولدر نشد، همون Tick بی‌سروصدا رد شه
            }

            string[] files;
            try
            {
                files = Directory.GetFiles(userFolder)
                                 .Where(f => !f.StartsWith(seenFolder, StringComparison.OrdinalIgnoreCase))
                                 .ToArray();
            }
            catch
            {
                return;
            }

            if (files.Length == 0)
                return; // ❌ لاگ بی‌خودی نداریم

            foreach (var file in files)
            {
                string fileName = Path.GetFileName(file);

                // 🧠 اگر قبلاً به این فایل نوتیفای شده، رد شو
                if (_notifiedFiles.Contains(fileName))
                    continue;

                _notifiedFiles.Add(fileName);

                var result = MessageBox.Show(
                    $"📩 New file received:\n\n{fileName}\n\nOpen it now?",
                    "New File",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(file)
                        {
                            UseShellExecute = true
                        });

                        string destPath = Path.Combine(seenFolder, fileName);

                        if (File.Exists(destPath))
                            File.Delete(destPath);

                        File.Move(file, destPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"❌ Error handling file:\n{ex.Message}",
                            "File Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
        }


        private void btn_sendfile_Click(object sender, EventArgs e)
        {
            if (cmbOnlineUser1.SelectedItem == null)
            {
                MessageBox.Show("⚠️ Please select a user to send the file to.",
                                "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedUser = cmbOnlineUser1.SelectedItem.ToString().Trim();
            if (string.IsNullOrEmpty(selectedUser))
            {
                MessageBox.Show("Invalid user selection.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select a file to send";
                ofd.Filter = "All Files (*.*)|*.*";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string selectedFilePath = ofd.FileName;

                    try
                    {
                        SendFileToUser(selectedFilePath, selectedUser);
                        MessageBox.Show($"✅ File sent successfully to '{selectedUser}'.",
                                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"❌ Error sending file:\n{ex.Message}",
                                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void SendFileToUser(string filePath, string username)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                {
                    MessageBox.Show("⚠️ Please select a valid user.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!File.Exists(filePath))
                {
                    MessageBox.Show("❌ The selected file does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // مسیر سرور
                string networkBase = ServerConfig.GetNetworkPath("OnlineUsers");
                string userFolder;
                bool useLocal = false;

                // بررسی در دسترس بودن مسیر شبکه
                try
                {
                    if (Directory.Exists(networkBase))
                    {
                        userFolder = Path.Combine(networkBase, username);
                    }
                    else
                    {
                        useLocal = true;
                        userFolder = Path.Combine(GlobalFileManager.LocalUsersFolder, username);
                    }
                }
                catch
                {
                    useLocal = true;
                    userFolder = Path.Combine(GlobalFileManager.LocalUsersFolder, username);
                }

                // ساخت مسیر کاربر در مقصد (در صورت نیاز)
                if (!Directory.Exists(userFolder))
                    Directory.CreateDirectory(userFolder);

                // مسیر نهایی فایل
                string destFile = Path.Combine(userFolder, Path.GetFileName(filePath));

                // کپی فایل
                File.Copy(filePath, destFile, true);

                if (useLocal)
                    MessageBox.Show($"📁 Server not reachable.\nFile saved locally for user: {username}",
                                    "Local Mode", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show($"✅ File successfully sent to {username} via server.",
                                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error sending file:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadOnlineUsers()
        {
            cmbOnlineUser1.Items.Clear();
            var onlineUsers = OnlineUsersManager.GetOnlineUsers();
            foreach (var user in onlineUsers)
            {
                if (!string.IsNullOrEmpty(user) && user != CurrentUsername) // حذف خود کاربر و null
                    cmbOnlineUser1.Items.Add(user);
            }
        }

        public string CurrentUsername { get; set; }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!string.IsNullOrEmpty(CurrentUsername))
            {
                OnlineUsersManager.SetUserOffline(txtUsername.Text);
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Supported Files|*.pdf;*.docx;*.xlsx";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                string extension = Path.GetExtension(filePath).ToLower();

                switch (extension)
                {
                    case ".pdf":
                        PrintPDF(filePath);
                        break;
                    case ".xlsx":
                        PrintExcel(filePath);
                        break;
                    default:
                        MessageBox.Show("فرمت فایل پشتیبانی نمی‌شود.");
                        break;
                }
            }
        }

        private void PrintPDF(string filePath)
        {
            string selectedPrinter = cmbPrinters.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedPrinter))
            {
                MessageBox.Show("لطفاً یک پرینتر انتخاب کنید.");
                return;
            }

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "AcroRd32.exe", // مسیر Adobe Reader
                Arguments = $"/t \"{filePath}\" \"{selectedPrinter}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            Process.Start(psi);
        }

        private void PrintExcel(string filePath)
        {
            var excelApp = new Microsoft.Office.Interop.Excel.Application();
            var workbook = excelApp.Workbooks.Open(filePath);
            workbook.PrintOut();
            workbook.Close(false);
            excelApp.Quit();
        }


        private List<string> adminUsers = new List<string> { "admin", "admin1", "admin3","admin4","admin5" }; // لیست ادمین‌ها، باید به نحو مناسبی مدیریت شود

        private void btn_SaveUserDailyReport_Click(object sender, EventArgs e)
        {
            string currentUser = txtUsername.Text.Trim();
            string reportText = tx_Dailyreport.Text.Trim();

            if (string.IsNullOrEmpty(currentUser))
            {
                MessageBox.Show("Username is not entered!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(reportText))
            {
                MessageBox.Show("Please enter your report text!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // مسیر فایل گزارش روزانه
                string reportFile = GlobalFileManager.DailyReportsFile;

                // اطمینان از وجود فایل (در صورت نبود، ساخته می‌شود)
                GlobalFileManager.EnsureFileExists(reportFile);

                SaveDailyReport(currentUser, reportText);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while saving report: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_Dailyreport_Click(object sender, EventArgs e)
        {
            string currentUser = txtUsername.Text.Trim();

            if (string.IsNullOrEmpty(currentUser))
            {
                MessageBox.Show("Please enter your username first!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // بررسی اینکه کاربر ادمین هست یا نه
            if (adminUsers.Contains(currentUser.ToLower()))
            {
                SwitchPanel(panelForDailyReportInput);
                LoadDailyReportsForAdmin();
            }
            else
            {
                SwitchPanel(panelForDailyReportInput);
            }
        }



        private void SaveDailyReport(string username, string reportText)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(reportText))
            {
                MessageBox.Show("Username or report text cannot be empty!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string filePath = GlobalFileManager.DailyReportsFile;

            using (var workbook = new XLWorkbook())
            {
                IXLWorksheet mainSheet;

                // Load existing workbook if exists
                if (File.Exists(filePath))
                {
                    using (var existingWorkbook = new XLWorkbook(filePath))
                    {
                        foreach (var ws in existingWorkbook.Worksheets)
                        {
                            workbook.AddWorksheet(ws);
                        }
                    }
                }

                // Master sheet for overview
                if (workbook.TryGetWorksheet("Daily Reports", out IXLWorksheet existingMainSheet))
                {
                    mainSheet = existingMainSheet;
                }
                else
                {
                    mainSheet = workbook.AddWorksheet("Daily Reports");
                    mainSheet.Cell(1, 1).Value = "Username";
                    mainSheet.Cell(1, 2).Value = "Last Report";
                    mainSheet.Cell(1, 3).Value = "Last Report Date";
                    mainSheet.Row(1).Style.Font.SetBold();
                    mainSheet.Column(1).Width = 25;
                    mainSheet.Column(2).Width = 60;
                    mainSheet.Column(3).Width = 30;
                }

                // User-specific sheet
                IXLWorksheet userSheet;
                if (workbook.TryGetWorksheet(username, out IXLWorksheet existingUserSheet))
                {
                    userSheet = existingUserSheet;
                }
                else
                {
                    userSheet = workbook.AddWorksheet(username);
                    userSheet.Cell(1, 1).Value = "Date";
                    userSheet.Cell(1, 2).Value = "Report Text";
                    userSheet.Row(1).Style.Font.SetBold();
                    userSheet.Column(1).Width = 25;
                    userSheet.Column(2).Width = 100;
                }

                // Add new report to user sheet
                int lastRowUser = userSheet.LastRowUsed()?.RowNumber() + 1 ?? 2;
                string dateNow = DateTime.Now.ToString("yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture);

                userSheet.Cell(lastRowUser, 1).Value = dateNow;
                userSheet.Cell(lastRowUser, 2).Value = reportText;
                userSheet.Cell(lastRowUser, 2).Style.Alignment.WrapText = true;

                // Update or insert into master sheet
                bool userFoundInMainSheet = false;
                int lastUsedRow = mainSheet.LastRowUsed()?.RowNumber() ?? 1;

                for (int row = 2; row <= lastUsedRow; row++)
                {
                    if (mainSheet.Cell(row, 1).GetString() == username)
                    {
                        mainSheet.Cell(row, 2).Value = reportText;
                        mainSheet.Cell(row, 3).Value = dateNow;
                        mainSheet.Cell(row, 1).SetHyperlink(new XLHyperlink($"'{username}'!A1", username));
                        userFoundInMainSheet = true;
                        break;
                    }
                }

                if (!userFoundInMainSheet)
                {
                    int lastRowMain = mainSheet.LastRowUsed()?.RowNumber() + 1 ?? 2;
                    mainSheet.Cell(lastRowMain, 1).Value = username;
                    mainSheet.Cell(lastRowMain, 2).Value = reportText;
                    mainSheet.Cell(lastRowMain, 3).Value = dateNow;
                    mainSheet.Cell(lastRowMain, 1).SetHyperlink(new XLHyperlink($"'{username}'!A1", username));
                }

                try
                {
                    workbook.SaveAs(filePath);
                    MessageBox.Show("Daily report saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    tx_Dailyreport.Text = "";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving Excel file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void LoadDailyReportsForAdmin()
        {
            flow_Dailyreport.Controls.Clear();

            string reportFile = GlobalFileManager.DailyReportsFile;
            GlobalFileManager.EnsureFileExists(reportFile);

            if (!File.Exists(reportFile))
            {
                Label lblNoReports = new Label
                {
                    Text = "No reports have been submitted yet.",
                    AutoSize = true,
                    ForeColor = Color.Gray
                };
                flow_Dailyreport.Controls.Add(lblNoReports);
                return;
            }

            using (var workbook = new XLWorkbook(reportFile))
            {
                if (workbook.TryGetWorksheet("Daily Reports", out IXLWorksheet mainSheet))
                {
                    var lastRow = mainSheet.LastRowUsed();
                    if (lastRow == null || lastRow.RowNumber() < 2)
                    {
                        Label lblNoReports = new Label
                        {
                            Text = "No reports have been submitted yet.",
                            AutoSize = true,
                            ForeColor = Color.Gray
                        };
                        flow_Dailyreport.Controls.Add(lblNoReports);
                        return;
                    }

                    for (int row = 2; row <= lastRow.RowNumber(); row++)
                    {
                        string username = mainSheet.Cell(row, 1).GetString();
                        string lastReport = mainSheet.Cell(row, 2).GetString();
                        string reportDate = mainSheet.Cell(row, 3).GetString();
                        // --- Panel ---
                        Panel userPanel = new Panel
                        {
                            Width = flow_Dailyreport.Width - 40,
                            Height = 90,
                            BackColor = Color.FromArgb(50, 50, 50),
                            Margin = new Padding(10),
                            Padding = new Padding(12),
                            BorderStyle = BorderStyle.None,
                            Tag = username
                        };

                        // لبه‌های گرد (اختیاری ولی زیبا)
                        userPanel.Paint += (s, e) =>
                        {
                            int radius = 12;
                            var path = new System.Drawing.Drawing2D.GraphicsPath();
                            path.AddArc(0, 0, radius, radius, 180, 90);
                            path.AddArc(userPanel.Width - radius, 0, radius, radius, 270, 90);
                            path.AddArc(userPanel.Width - radius, userPanel.Height - radius, radius, radius, 0, 90);
                            path.AddArc(0, userPanel.Height - radius, radius, radius, 90, 90);
                            path.CloseAllFigures();
                            userPanel.Region = new Region(path);
                        };

                        // --- Username ---
                        Label lblUsername = new Label
                        {
                            Text = username,
                            AutoSize = true,
                            Font = new Font("Segoe UI Semibold", 10.5f),
                            ForeColor = Color.FromArgb(255, 180, 0), // طلایی ملایم
                            Location = new Point(15, 10)
                        };
                        userPanel.Controls.Add(lblUsername);

                        // --- Summary ---
                        Label lblReportSummary = new Label
                        {
                            Text = $"🕓 Last Report ({reportDate})\n\t{(lastReport.Length > 80 ? lastReport.Substring(0, 80) + "..." : lastReport)}",
                            AutoSize = false,
                            Width = userPanel.Width - 110,
                            Height = 55,
                            Location = new Point(15, 30),
                            Font = new Font("Segoe UI", 9f),
                            ForeColor = Color.Gainsboro,
                            BackColor = Color.Transparent,
                            TextAlign = ContentAlignment.TopLeft
                        };
                        userPanel.Controls.Add(lblReportSummary);

                        // --- Expand Button ---
                        Button btnExpand = new Button
                        {
                            Text = "↗",
                            Width = 32,
                            Height = 32,
                            BackColor = Color.FromArgb(50, 50, 50),
                            ForeColor = Color.White,
                            FlatStyle = FlatStyle.Flat,
                            Font = new Font("Segoe UI", 14, FontStyle.Bold),
                            Cursor = Cursors.Hand,
                            Tag = username
                        };
                        btnExpand.FlatAppearance.BorderSize = 0;
                        btnExpand.Location = new Point(userPanel.Width - btnExpand.Width - 10, 10);
                        btnExpand.Click += BtnExpand_Click;

                        // سایه ملایم برای دکمه (نمای 3D)
                        btnExpand.Paint += (s, e) =>
                        {
                            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                            using (Pen pen = new Pen(Color.FromArgb(80, 255, 255, 255), 1))
                                e.Graphics.DrawEllipse(pen, 1, 1, btnExpand.Width - 3, btnExpand.Height - 3);
                        };

                        // اضافه به پنل
                        userPanel.Controls.Add(btnExpand);
                        btnExpand.BringToFront();

                        // اضافه به FlowLayoutPanel
                        flow_Dailyreport.Controls.Add(userPanel);

                    }
                }
                else
                {
                    Label lblNoReports = new Label
                    {
                        Text = "Worksheet 'Daily Reports' not found.",
                        AutoSize = true,
                        ForeColor = Color.Red
                    };
                    flow_Dailyreport.Controls.Add(lblNoReports);
                }
            }
        }

        private void BtnExpand_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.Tag is string username)
            {
                OpenUserReportSheet(username);
            }
        }
        private void OpenUserReportSheet(string username)
        {
            string reportFile = GlobalFileManager.DailyReportsFile;

            if (!File.Exists(reportFile))
            {
                MessageBox.Show("The daily reports file was not found!", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                Excel.Application excelApp = new Excel.Application();
                excelApp.Visible = true;
                Excel.Workbook workbook = excelApp.Workbooks.Open(reportFile);

                try
                {
                    Excel.Worksheet sheet = null;
                    foreach (Excel.Worksheet ws in workbook.Sheets)
                    {
                        if (ws.Name.Equals(username, StringComparison.OrdinalIgnoreCase))
                        {
                            sheet = ws;
                            break;
                        }
                    }

                    if (sheet != null)
                    {
                        sheet.Activate();
                    }
                    else
                    {
                        MessageBox.Show($"No worksheet found for user '{username}'.",
                                        "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error activating worksheet:\n{ex.Message}",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    workbook.Close(false);
                    excelApp.Quit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Excel file:\n{ex.Message}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
    public static class OnlineUsersManager
    {
        private static string OnlineFolder;

        static OnlineUsersManager()
        {
            try
            {
                // مسیر سرور از ServerConfig
                OnlineFolder = ServerConfig.GetNetworkPath("OnlineUsers");

                // تست دسترسی به مسیر شبکه
                if (!Directory.Exists(OnlineFolder))
                {
                    try
                    {
                        Directory.CreateDirectory(OnlineFolder);
                    }
                    catch (IOException)
                    {
                        // اگر شبکه قطع بود => مسیر محلی
                        UseLocalFallback();
                    }
                    catch (UnauthorizedAccessException)
                    {
                        UseLocalFallback();
                    }
                }
            }
            catch
            {
                UseLocalFallback();
            }
        }

        private static void UseLocalFallback()
        {
            MessageBox.Show("⚠️ Server unavailable — The application is running in local mode.");
            OnlineFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LocalUsers");
            if (!Directory.Exists(OnlineFolder))
                Directory.CreateDirectory(OnlineFolder);
        }

        public static void SetUserOnline(string username)
        {
            try
            {
                if (!Directory.Exists(OnlineFolder))
                    Directory.CreateDirectory(OnlineFolder);

                string userFile = Path.Combine(OnlineFolder, username + ".online");
                File.WriteAllText(userFile, "online");
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌Error setting online status:\n" + ex.Message);
            }
        }

        public static void SetUserOffline(string username)
        {
            try
            {
                string onlineFile = Path.Combine(OnlineFolder, username + ".online");
                if (File.Exists(onlineFile))
                    File.Delete(onlineFile);
            }
            catch { /* نادیده بگیر، مشکلی نیست */ }
        }

        public static List<string> GetOnlineUsers()
        {
            List<string> onlineUsers = new List<string>();

            try
            {
                if (!Directory.Exists(OnlineFolder))
                    return onlineUsers;

                var files = Directory.GetFiles(OnlineFolder, "*.online");
                foreach (var file in files)
                {
                    onlineUsers.Add(Path.GetFileNameWithoutExtension(file));
                }
            }
            catch
            {
                // حالت آفلاین: فقط خالی برگردون
            }

            return onlineUsers;
        }
    }
}