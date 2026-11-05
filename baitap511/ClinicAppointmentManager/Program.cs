using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace ClinicAppointmentManager;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}

public class Appointment
{
    public string AppointmentId { get; set; } = "";
    public string PatientName { get; set; } = "";
    public string Phone { get; set; } = "";
    public DateTime AppointmentDate { get; set; }
    public string Doctor { get; set; } = "";
    public string Reason { get; set; } = "";
    public string Status { get; set; } = "";
}

public class MainForm : Form
{
    private readonly BindingList<Appointment> appointments = new();
    private readonly BindingSource bindingSource = new();
    private readonly ErrorProvider errorProvider = new();

    private TextBox txtId = null!;
    private TextBox txtName = null!;
    private TextBox txtPhone = null!;
    private DateTimePicker dtpAppointment = null!;
    private ComboBox cboDoctor = null!;
    private TextBox txtReason = null!;
    private ComboBox cboStatus = null!;
    private TextBox txtSearch = null!;
    private CheckBox chkFilterDate = null!;
    private DateTimePicker dtpFilterDate = null!;
    private ComboBox cboFilterStatus = null!;
    private DataGridView dgvAppointments = null!;
    private ToolStripStatusLabel statusLabel = null!;

    public MainForm()
    {
        Text = "Clinic Appointment Manager - Quản lý lịch hẹn phòng khám";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1050, 680);
        Size = new Size(1250, 780);
        Font = new Font("Segoe UI", 9F);
        BuildInterface();

        bindingSource.DataSource = appointments;
        dgvAppointments.DataSource = bindingSource;
        appointments.ListChanged += (_, _) => ApplyFilters();
        CreateNewAppointmentId();
        ApplyFilters();
    }

    private void BuildInterface()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 230));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(root);

        var title = new Label
        {
            Text = "QUẢN LÝ LỊCH HẸN PHÒNG KHÁM",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = Color.DarkBlue
        };
        root.Controls.Add(title, 0, 0);

        var inputGroup = new GroupBox
        {
            Text = "Thông tin lịch hẹn",
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };
        root.Controls.Add(inputGroup, 0, 1);

        var inputTable = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 3,
            Padding = new Padding(4)
        };
        inputTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115));
        inputTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        inputTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115));
        inputTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (int i = 0; i < 3; i++)
            inputTable.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
        inputGroup.Controls.Add(inputTable);

        txtId = new TextBox { Dock = DockStyle.Fill };
        txtName = new TextBox { Dock = DockStyle.Fill };
        txtPhone = new TextBox { Dock = DockStyle.Fill };
        dtpAppointment = new DateTimePicker
        {
            Dock = DockStyle.Fill,
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "dd/MM/yyyy HH:mm",
            ShowUpDown = true,
            Value = DateTime.Now.AddHours(1)
        };
        cboDoctor = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cboDoctor.Items.AddRange(new object[] { "BS. Nguyễn Văn An", "BS. Trần Thị Bình", "BS. Lê Minh Cường", "BS. Phạm Thu Hà" });
        cboDoctor.SelectedIndex = 0;
        txtReason = new TextBox { Dock = DockStyle.Fill };
        cboStatus = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cboStatus.Items.AddRange(new object[] { "Chờ khám", "Đang khám", "Hoàn tất", "Đã hủy" });
        cboStatus.SelectedIndex = 0;

        AddField(inputTable, "Mã lịch hẹn:", txtId, 0, 0);
        AddField(inputTable, "Họ tên bệnh nhân:", txtName, 1, 0);
        AddField(inputTable, "Số điện thoại:", txtPhone, 0, 1);
        AddField(inputTable, "Ngày giờ hẹn:", dtpAppointment, 1, 1);
        AddField(inputTable, "Bác sĩ:", cboDoctor, 0, 2);
        AddField(inputTable, "Lý do khám:", txtReason, 1, 2);

        var rightStatusPanel = new Panel { Dock = DockStyle.Fill };
        var statusLabelInput = new Label
        {
            Text = "Trạng thái:",
            Dock = DockStyle.Top,
            Height = 25,
            TextAlign = ContentAlignment.MiddleLeft
        };
        rightStatusPanel.Controls.Add(cboStatus);
        rightStatusPanel.Controls.Add(statusLabelInput);
        inputTable.Controls.Add(rightStatusPanel, 2, 2);
        inputTable.Controls.Add(new Label { Text = "", Dock = DockStyle.Fill }, 3, 2);

        var filterGroup = new GroupBox
        {
            Text = "Tìm kiếm và lọc lịch hẹn",
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };
        root.Controls.Add(filterGroup, 0, 2);

        var filterTable = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 2
        };
        filterTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        filterTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        filterTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        filterTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 1));
        filterTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));
        filterTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        filterGroup.Controls.Add(filterTable);

        filterTable.Controls.Add(new Label { Text = "Tìm theo tên bệnh nhân hoặc số điện thoại", Dock = DockStyle.Fill }, 0, 0);
        filterTable.Controls.Add(new Label { Text = "Lọc theo ngày hẹn", Dock = DockStyle.Fill }, 1, 0);
        filterTable.Controls.Add(new Label { Text = "Lọc theo trạng thái", Dock = DockStyle.Fill }, 2, 0);

        txtSearch = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Nhập tên hoặc số điện thoại..." };
        chkFilterDate = new CheckBox { Text = "Bật lọc ngày", Dock = DockStyle.Left, Width = 105 };
        dtpFilterDate = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            Width = 125,
            Dock = DockStyle.Right,
            Enabled = false
        };
        var datePanel = new Panel { Dock = DockStyle.Fill };
        datePanel.Controls.Add(dtpFilterDate);
        datePanel.Controls.Add(chkFilterDate);

        cboFilterStatus = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cboFilterStatus.Items.AddRange(new object[] { "Tất cả trạng thái", "Chờ khám", "Đang khám", "Hoàn tất", "Đã hủy" });
        cboFilterStatus.SelectedIndex = 0;

        filterTable.Controls.Add(txtSearch, 0, 1);
        filterTable.Controls.Add(datePanel, 1, 1);
        filterTable.Controls.Add(cboFilterStatus, 2, 1);

        txtSearch.TextChanged += (_, _) => ApplyFilters();
        chkFilterDate.CheckedChanged += (_, _) =>
        {
            dtpFilterDate.Enabled = chkFilterDate.Checked;
            ApplyFilters();
        };
        dtpFilterDate.ValueChanged += (_, _) => ApplyFilters();
        cboFilterStatus.SelectedIndexChanged += (_, _) => ApplyFilters();

        var bottomPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        bottomPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        bottomPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(bottomPanel, 0, 3);

        var buttonFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 4, 0, 4)
        };
        bottomPanel.Controls.Add(buttonFlow, 0, 0);

        AddButton(buttonFlow, "Thêm mới", Color.SeaGreen, AddAppointment);
        AddButton(buttonFlow, "Cập nhật", Color.RoyalBlue, UpdateAppointment);
        AddButton(buttonFlow, "Xóa", Color.Firebrick, DeleteAppointment);
        AddButton(buttonFlow, "Làm mới", Color.DimGray, ClearInputs);
        AddButton(buttonFlow, "Xuất CSV", Color.DarkSlateBlue, ExportCsv);

        dgvAppointments = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoGenerateColumns = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowHeadersVisible = false,
            BackgroundColor = Color.White,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BorderStyle = BorderStyle.Fixed3D
        };
        dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Appointment.AppointmentId), HeaderText = "Mã lịch hẹn", FillWeight = 75 });
        dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Appointment.PatientName), HeaderText = "Bệnh nhân", FillWeight = 125 });
        dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Appointment.Phone), HeaderText = "Số điện thoại", FillWeight = 95 });
        dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Appointment.AppointmentDate), HeaderText = "Ngày giờ hẹn", FillWeight = 105, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" } });
        dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Appointment.Doctor), HeaderText = "Bác sĩ", FillWeight = 115 });
        dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Appointment.Reason), HeaderText = "Lý do khám", FillWeight = 115 });
        dgvAppointments.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Appointment.Status), HeaderText = "Trạng thái", FillWeight = 85 });
        dgvAppointments.SelectionChanged += (_, _) => LoadSelectedAppointment();
        bottomPanel.Controls.Add(dgvAppointments, 0, 1);

        var statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel("Số lịch hẹn: 0");
        statusStrip.Items.Add(statusLabel);
        Controls.Add(statusStrip);
        statusStrip.Dock = DockStyle.Bottom;

        errorProvider.ContainerControl = this;
        errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
    }

    private static void AddField(TableLayoutPanel table, string caption, Control control, int labelColumn, int row)
    {
        var label = new Label
        {
            Text = caption,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        table.Controls.Add(label, labelColumn, row);
        table.Controls.Add(control, labelColumn + 1, row);
    }

    private static void AddButton(FlowLayoutPanel panel, string text, Color color, Action action)
    {
        var button = new Button
        {
            Text = text,
            Width = 115,
            Height = 34,
            BackColor = color,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(0, 0, 10, 0)
        };
        button.FlatAppearance.BorderSize = 0;
        button.Click += (_, _) => action();
        panel.Controls.Add(button);
    }

    private void CreateNewAppointmentId()
    {
        int next = 1;
        while (appointments.Any(a => a.AppointmentId == $"LH{next:000}"))
            next++;
        txtId.Text = $"LH{next:000}";
    }

    private bool ValidateInputs(bool isUpdate)
    {
        errorProvider.Clear();
        bool valid = true;
        if (string.IsNullOrWhiteSpace(txtId.Text))
        {
            errorProvider.SetError(txtId, "Vui lòng nhập mã lịch hẹn.");
            valid = false;
        }
        else if (appointments.Any(a => a.AppointmentId.Equals(txtId.Text.Trim(), StringComparison.OrdinalIgnoreCase)
                                      && (!isUpdate || !ReferenceEquals(a, GetSelectedAppointment()))))
        {
            errorProvider.SetError(txtId, "Mã lịch hẹn đã tồn tại.");
            valid = false;
        }

        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            errorProvider.SetError(txtName, "Vui lòng nhập họ tên bệnh nhân.");
            valid = false;
        }

        string phone = txtPhone.Text.Trim();
        if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^(0|\+84)[0-9]{9,10}$"))
        {
            errorProvider.SetError(txtPhone, "Số điện thoại không hợp lệ (VD: 0912345678).");
            valid = false;
        }

        if (dtpAppointment.Value.Date < DateTime.Today)
        {
            errorProvider.SetError(dtpAppointment, "Không được chọn ngày trong quá khứ.");
            valid = false;
        }

        if (string.IsNullOrWhiteSpace(txtReason.Text))
        {
            errorProvider.SetError(txtReason, "Vui lòng nhập lý do khám.");
            valid = false;
        }

        if (cboDoctor.SelectedIndex < 0)
        {
            errorProvider.SetError(cboDoctor, "Vui lòng chọn bác sĩ.");
            valid = false;
        }

        if (cboStatus.SelectedIndex < 0)
        {
            errorProvider.SetError(cboStatus, "Vui lòng chọn trạng thái.");
            valid = false;
        }

        return valid;
    }

    private Appointment? GetSelectedAppointment()
    {
        return dgvAppointments.CurrentRow?.DataBoundItem as Appointment;
    }

    private void AddAppointment()
    {
        if (!ValidateInputs(false)) return;

        appointments.Add(ReadForm());
        ClearInputs();
        MessageBox.Show("Thêm lịch hẹn thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void UpdateAppointment()
    {
        Appointment? selected = GetSelectedAppointment();
        if (selected == null)
        {
            MessageBox.Show("Vui lòng chọn lịch hẹn cần cập nhật.");
            return;
        }
        if (!ValidateInputs(true)) return;

        Appointment updated = ReadForm();
        selected.AppointmentId = updated.AppointmentId;
        selected.PatientName = updated.PatientName;
        selected.Phone = updated.Phone;
        selected.AppointmentDate = updated.AppointmentDate;
        selected.Doctor = updated.Doctor;
        selected.Reason = updated.Reason;
        selected.Status = updated.Status;
        bindingSource.ResetBindings(false);
        ApplyFilters();
        MessageBox.Show("Cập nhật lịch hẹn thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void DeleteAppointment()
    {
        Appointment? selected = GetSelectedAppointment();
        if (selected == null)
        {
            MessageBox.Show("Vui lòng chọn lịch hẹn cần xóa.");
            return;
        }

        DialogResult result = MessageBox.Show(
            $"Bạn có chắc chắn muốn xóa lịch hẹn {selected.AppointmentId} của {selected.PatientName} không?",
            "Xác nhận xóa",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            appointments.Remove(selected);
            ClearInputs();
        }
    }

    private Appointment ReadForm()
    {
        return new Appointment
        {
            AppointmentId = txtId.Text.Trim(),
            PatientName = txtName.Text.Trim(),
            Phone = txtPhone.Text.Trim(),
            AppointmentDate = dtpAppointment.Value,
            Doctor = cboDoctor.Text,
            Reason = txtReason.Text.Trim(),
            Status = cboStatus.Text
        };
    }

    private void LoadSelectedAppointment()
    {
        Appointment? selected = GetSelectedAppointment();
        if (selected == null) return;

        txtId.Text = selected.AppointmentId;
        txtName.Text = selected.PatientName;
        txtPhone.Text = selected.Phone;
        dtpAppointment.Value = selected.AppointmentDate;
        cboDoctor.SelectedItem = selected.Doctor;
        txtReason.Text = selected.Reason;
        cboStatus.SelectedItem = selected.Status;
        errorProvider.Clear();
    }

    private void ClearInputs()
    {
        errorProvider.Clear();
        txtName.Clear();
        txtPhone.Clear();
        txtReason.Clear();
        dtpAppointment.Value = DateTime.Now.AddHours(1);
        cboDoctor.SelectedIndex = 0;
        cboStatus.SelectedIndex = 0;
        dgvAppointments.ClearSelection();
        CreateNewAppointmentId();
    }

    private void ApplyFilters()
    {
        if (dgvAppointments == null || txtSearch == null) return;

        string keyword = txtSearch.Text.Trim().ToLowerInvariant();
        string status = cboFilterStatus.SelectedItem?.ToString() ?? "Tất cả trạng thái";

        var filtered = appointments.Where(a =>
            (a.PatientName.ToLowerInvariant().Contains(keyword) || a.Phone.Contains(keyword)) &&
            (!chkFilterDate.Checked || a.AppointmentDate.Date == dtpFilterDate.Value.Date) &&
            (status == "Tất cả trạng thái" || a.Status == status))
            .OrderBy(a => a.AppointmentDate)
            .ToList();

        bindingSource.DataSource = new BindingList<Appointment>(filtered);
        dgvAppointments.DataSource = bindingSource;
        statusLabel.Text = $"Số lịch hẹn sau khi lọc: {filtered.Count}";
    }

    private void ExportCsv()
    {
        var rows = dgvAppointments.Rows
            .Cast<DataGridViewRow>()
            .Where(r => !r.IsNewRow)
            .Select(r => r.DataBoundItem as Appointment)
            .Where(a => a != null)
            .Cast<Appointment>()
            .ToList();

        if (rows.Count == 0)
        {
            MessageBox.Show("Không có dữ liệu để xuất.");
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Filter = "CSV (*.csv)|*.csv",
            FileName = "DanhSachLichHen.csv",
            Title = "Xuất danh sách lịch hẹn"
        };

        if (dialog.ShowDialog() != DialogResult.OK) return;

        var lines = new List<string>
        {
            "Ma lich hen,Ho ten benh nhan,So dien thoai,Ngay gio hen,Bac si,Ly do kham,Trang thai"
        };

        foreach (Appointment a in rows)
        {
            lines.Add(string.Join(",", new[]
            {
                Csv(a.AppointmentId), Csv(a.PatientName), Csv(a.Phone),
                Csv(a.AppointmentDate.ToString("dd/MM/yyyy HH:mm")),
                Csv(a.Doctor), Csv(a.Reason), Csv(a.Status)
            }));
        }

        File.WriteAllLines(dialog.FileName, lines, new UTF8Encoding(true));
        MessageBox.Show("Xuất file CSV thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static string Csv(string value)
    {
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
