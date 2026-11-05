using System.ComponentModel;
using System.Text;

namespace LabEquipmentCheckout;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}

public class CheckoutRecord
{
    public string TicketId { get; set; } = "";
    public string StudentId { get; set; } = "";
    public string StudentName { get; set; } = "";
    public string Equipment { get; set; } = "";
    public int Quantity { get; set; }
    public DateTime BorrowDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ActualReturnDate { get; set; }
    public string Status { get; set; } = "Đang mượn";

    public string DisplayStatus
    {
        get
        {
            if (Status == "Đã trả") return "Đã trả";
            return DueDate.Date < DateTime.Today ? "Quá hạn" : "Đang mượn";
        }
    }
}

public class MainForm : Form
{
    private readonly BindingList<CheckoutRecord> records = new();
    private readonly BindingSource bindingSource = new();
    private readonly ErrorProvider errorProvider = new();

    private TextBox txtTicketId = null!;
    private TextBox txtStudentId = null!;
    private TextBox txtStudentName = null!;
    private ComboBox cboEquipment = null!;
    private NumericUpDown numQuantity = null!;
    private DateTimePicker dtpBorrowDate = null!;
    private DateTimePicker dtpDueDate = null!;
    private DateTimePicker dtpReturnDate = null!;
    private ComboBox cboStatus = null!;
    private TextBox txtSearch = null!;
    private ComboBox cboFilter = null!;
    private DataGridView dgvRecords = null!;
    private ToolStripStatusLabel statusLabel = null!;

    public MainForm()
    {
        Text = "Lab Equipment Checkout - Quản lý mượn trả thiết bị";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1050, 690);
        Size = new Size(1280, 790);
        Font = new Font("Segoe UI", 9F);

        BuildInterface();

        bindingSource.DataSource = records;
        dgvRecords.DataSource = bindingSource;
        records.ListChanged += (_, _) => ApplyFilters();

        CreateNewTicketId();
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
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 245));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(root);

        var title = new Label
        {
            Text = "QUẢN LÝ MƯỢN TRẢ THIẾT BỊ PHÒNG THỰC HÀNH",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 15F, FontStyle.Bold),
            ForeColor = Color.DarkBlue
        };
        root.Controls.Add(title, 0, 0);

        var groupInput = new GroupBox
        {
            Text = "Thông tin phiếu mượn",
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };
        root.Controls.Add(groupInput, 0, 1);

        var input = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 4,
            Padding = new Padding(4)
        };
        input.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125));
        input.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        input.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125));
        input.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (int i = 0; i < 4; i++)
            input.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
        groupInput.Controls.Add(input);

        txtTicketId = new TextBox { Dock = DockStyle.Fill };
        txtStudentId = new TextBox { Dock = DockStyle.Fill };
        txtStudentName = new TextBox { Dock = DockStyle.Fill };
        cboEquipment = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDown };
        cboEquipment.Items.AddRange(new object[]
        {
            "Laptop", "Máy tính để bàn", "Chuột", "Bàn phím", "Màn hình",
            "Máy chiếu", "Arduino", "Kit Raspberry Pi", "Cáp mạng", "Thiết bị khác"
        });
        numQuantity = new NumericUpDown
        {
            Dock = DockStyle.Left,
            Width = 140,
            Minimum = 1,
            Maximum = 10000,
            Value = 1
        };
        dtpBorrowDate = new DateTimePicker
        {
            Dock = DockStyle.Fill,
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "dd/MM/yyyy"
        };
        dtpDueDate = new DateTimePicker
        {
            Dock = DockStyle.Fill,
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "dd/MM/yyyy",
            Value = DateTime.Today.AddDays(7)
        };
        dtpReturnDate = new DateTimePicker
        {
            Dock = DockStyle.Fill,
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "dd/MM/yyyy",
            ShowCheckBox = true,
            Checked = false
        };
        cboStatus = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cboStatus.Items.AddRange(new object[] { "Đang mượn", "Đã trả" });
        cboStatus.SelectedIndex = 0;

        AddField(input, "Mã phiếu:", txtTicketId, 0, 0);
        AddField(input, "Mã sinh viên:", txtStudentId, 1, 0);
        AddField(input, "Họ tên sinh viên:", txtStudentName, 0, 1);
        AddField(input, "Thiết bị:", cboEquipment, 1, 1);
        AddField(input, "Số lượng:", numQuantity, 0, 2);
        AddField(input, "Ngày mượn:", dtpBorrowDate, 1, 2);
        AddField(input, "Hạn trả:", dtpDueDate, 0, 3);
        AddField(input, "Ngày trả thực tế:", dtpReturnDate, 1, 3);

        var statusPanel = new Panel { Dock = DockStyle.Fill };
        statusPanel.Controls.Add(cboStatus);
        statusPanel.Controls.Add(new Label
        {
            Text = "Tình trạng:",
            Dock = DockStyle.Top,
            Height = 25,
            TextAlign = ContentAlignment.MiddleLeft
        });
        input.Controls.Add(statusPanel, 2, 3);
        input.Controls.Add(new Label { Text = "", Dock = DockStyle.Fill }, 3, 3);

        cboStatus.SelectedIndexChanged += (_, _) =>
        {
            if (cboStatus.Text == "Đã trả")
            {
                dtpReturnDate.Checked = true;
                if (dtpReturnDate.Value.Date < dtpBorrowDate.Value.Date)
                    dtpReturnDate.Value = DateTime.Today;
            }
            else
            {
                dtpReturnDate.Checked = false;
            }
        };

        var filterGroup = new GroupBox
        {
            Text = "Tìm kiếm và lọc phiếu mượn",
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };
        root.Controls.Add(filterGroup, 0, 2);

        var filter = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2
        };
        filter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
        filter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        filter.RowStyles.Add(new RowStyle(SizeType.Absolute, 23));
        filter.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        filterGroup.Controls.Add(filter);

        filter.Controls.Add(new Label
        {
            Text = "Tìm theo mã sinh viên hoặc tên thiết bị",
            Dock = DockStyle.Fill
        }, 0, 0);
        filter.Controls.Add(new Label
        {
            Text = "Lọc tình trạng phiếu",
            Dock = DockStyle.Fill
        }, 1, 0);

        txtSearch = new TextBox
        {
            Dock = DockStyle.Fill,
            PlaceholderText = "Nhập mã sinh viên hoặc tên thiết bị..."
        };
        cboFilter = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cboFilter.Items.AddRange(new object[]
        {
            "Tất cả phiếu", "Đang mượn", "Quá hạn", "Đã trả"
        });
        cboFilter.SelectedIndex = 0;
        filter.Controls.Add(txtSearch, 0, 1);
        filter.Controls.Add(cboFilter, 1, 1);
        txtSearch.TextChanged += (_, _) => ApplyFilters();
        cboFilter.SelectedIndexChanged += (_, _) => ApplyFilters();

        var bottom = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        bottom.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        bottom.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(bottom, 0, 3);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 4, 0, 4)
        };
        bottom.Controls.Add(buttons, 0, 0);
        AddButton(buttons, "Thêm phiếu", Color.SeaGreen, AddRecord);
        AddButton(buttons, "Cập nhật", Color.RoyalBlue, UpdateRecord);
        AddButton(buttons, "Xóa phiếu", Color.Firebrick, DeleteRecord);
        AddButton(buttons, "Đánh dấu đã trả", Color.Teal, MarkAsReturned);
        AddButton(buttons, "Làm mới", Color.DimGray, ClearInputs);
        AddButton(buttons, "Xuất CSV", Color.DarkSlateBlue, ExportCsv);

        dgvRecords = new DataGridView
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
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        dgvRecords.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CheckoutRecord.TicketId), HeaderText = "Mã phiếu", FillWeight = 70 });
        dgvRecords.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CheckoutRecord.StudentId), HeaderText = "Mã sinh viên", FillWeight = 85 });
        dgvRecords.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CheckoutRecord.StudentName), HeaderText = "Họ tên sinh viên", FillWeight = 115 });
        dgvRecords.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CheckoutRecord.Equipment), HeaderText = "Thiết bị", FillWeight = 100 });
        dgvRecords.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CheckoutRecord.Quantity), HeaderText = "Số lượng", FillWeight = 60 });
        dgvRecords.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CheckoutRecord.BorrowDate), HeaderText = "Ngày mượn", FillWeight = 80, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" } });
        dgvRecords.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CheckoutRecord.DueDate), HeaderText = "Hạn trả", FillWeight = 80, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" } });
        dgvRecords.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CheckoutRecord.ActualReturnDate), HeaderText = "Ngày trả thực tế", FillWeight = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" } });
        dgvRecords.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(CheckoutRecord.DisplayStatus), HeaderText = "Tình trạng", FillWeight = 80 });
        dgvRecords.SelectionChanged += (_, _) => LoadSelectedRecord();
        bottom.Controls.Add(dgvRecords, 0, 1);

        var statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel("Tổng số phiếu: 0 | Phiếu quá hạn: 0");
        statusStrip.Items.Add(statusLabel);
        Controls.Add(statusStrip);
        statusStrip.Dock = DockStyle.Bottom;

        errorProvider.ContainerControl = this;
        errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
    }

    private static void AddField(TableLayoutPanel table, string caption, Control control, int labelColumn, int row)
    {
        table.Controls.Add(new Label
        {
            Text = caption,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        }, labelColumn, row);
        table.Controls.Add(control, labelColumn + 1, row);
    }

    private static void AddButton(FlowLayoutPanel panel, string caption, Color color, Action action)
    {
        var button = new Button
        {
            Text = caption,
            Width = 125,
            Height = 34,
            BackColor = color,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(0, 0, 8, 0)
        };
        button.FlatAppearance.BorderSize = 0;
        button.Click += (_, _) => action();
        panel.Controls.Add(button);
    }

    private void CreateNewTicketId()
    {
        int number = 1;
        while (records.Any(r => r.TicketId == $"PM{number:000}"))
            number++;
        txtTicketId.Text = $"PM{number:000}";
    }

    private CheckoutRecord? GetSelectedRecord()
    {
        return dgvRecords.CurrentRow?.DataBoundItem as CheckoutRecord;
    }

    private bool ValidateInputs(bool updating)
    {
        errorProvider.Clear();
        bool valid = true;
        CheckoutRecord? selected = GetSelectedRecord();

        if (string.IsNullOrWhiteSpace(txtTicketId.Text))
        {
            errorProvider.SetError(txtTicketId, "Mã phiếu không được để trống.");
            valid = false;
        }
        else if (records.Any(r =>
            r.TicketId.Equals(txtTicketId.Text.Trim(), StringComparison.OrdinalIgnoreCase) &&
            (!updating || !ReferenceEquals(r, selected))))
        {
            errorProvider.SetError(txtTicketId, "Mã phiếu đã tồn tại.");
            valid = false;
        }

        if (string.IsNullOrWhiteSpace(txtStudentId.Text))
        {
            errorProvider.SetError(txtStudentId, "Mã sinh viên không được để trống.");
            valid = false;
        }

        if (string.IsNullOrWhiteSpace(txtStudentName.Text))
        {
            errorProvider.SetError(txtStudentName, "Họ tên sinh viên không được để trống.");
            valid = false;
        }

        if (string.IsNullOrWhiteSpace(cboEquipment.Text))
        {
            errorProvider.SetError(cboEquipment, "Vui lòng nhập hoặc chọn thiết bị.");
            valid = false;
        }

        if (numQuantity.Value <= 0)
        {
            errorProvider.SetError(numQuantity, "Số lượng phải lớn hơn 0.");
            valid = false;
        }

        if (dtpDueDate.Value.Date < dtpBorrowDate.Value.Date)
        {
            errorProvider.SetError(dtpDueDate, "Hạn trả không được trước ngày mượn.");
            valid = false;
        }

        if (cboStatus.Text == "Đã trả" && !dtpReturnDate.Checked)
        {
            errorProvider.SetError(dtpReturnDate, "Phiếu đã trả phải có ngày trả thực tế.");
            valid = false;
        }

        if (dtpReturnDate.Checked && dtpReturnDate.Value.Date < dtpBorrowDate.Value.Date)
        {
            errorProvider.SetError(dtpReturnDate, "Ngày trả thực tế không được trước ngày mượn.");
            valid = false;
        }

        if (dtpReturnDate.Checked && dtpReturnDate.Value.Date > DateTime.Today)
        {
            errorProvider.SetError(dtpReturnDate, "Ngày trả thực tế không được ở tương lai.");
            valid = false;
        }

        return valid;
    }

    private CheckoutRecord ReadForm()
    {
        return new CheckoutRecord
        {
            TicketId = txtTicketId.Text.Trim(),
            StudentId = txtStudentId.Text.Trim(),
            StudentName = txtStudentName.Text.Trim(),
            Equipment = cboEquipment.Text.Trim(),
            Quantity = (int)numQuantity.Value,
            BorrowDate = dtpBorrowDate.Value.Date,
            DueDate = dtpDueDate.Value.Date,
            ActualReturnDate = dtpReturnDate.Checked ? dtpReturnDate.Value.Date : null,
            Status = cboStatus.Text
        };
    }

    private void AddRecord()
    {
        if (!ValidateInputs(false)) return;
        records.Add(ReadForm());
        ClearInputs();
        MessageBox.Show("Thêm phiếu mượn thành công.", "Thông báo",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void UpdateRecord()
    {
        CheckoutRecord? selected = GetSelectedRecord();
        if (selected == null)
        {
            MessageBox.Show("Vui lòng chọn phiếu cần cập nhật.");
            return;
        }
        if (!ValidateInputs(true)) return;

        CheckoutRecord updated = ReadForm();
        selected.TicketId = updated.TicketId;
        selected.StudentId = updated.StudentId;
        selected.StudentName = updated.StudentName;
        selected.Equipment = updated.Equipment;
        selected.Quantity = updated.Quantity;
        selected.BorrowDate = updated.BorrowDate;
        selected.DueDate = updated.DueDate;
        selected.ActualReturnDate = updated.ActualReturnDate;
        selected.Status = updated.Status;

        bindingSource.ResetBindings(false);
        ApplyFilters();
        MessageBox.Show("Cập nhật phiếu thành công.", "Thông báo",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void DeleteRecord()
    {
        CheckoutRecord? selected = GetSelectedRecord();
        if (selected == null)
        {
            MessageBox.Show("Vui lòng chọn phiếu cần xóa.");
            return;
        }

        DialogResult answer = MessageBox.Show(
            $"Bạn có chắc chắn muốn xóa phiếu {selected.TicketId} không?",
            "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (answer == DialogResult.Yes)
        {
            records.Remove(selected);
            ClearInputs();
        }
    }

    private void MarkAsReturned()
    {
        CheckoutRecord? selected = GetSelectedRecord();
        if (selected == null)
        {
            MessageBox.Show("Vui lòng chọn phiếu cần đánh dấu đã trả.");
            return;
        }

        if (selected.Status == "Đã trả")
        {
            MessageBox.Show("Phiếu này đã được trả trước đó.");
            return;
        }

        DialogResult answer = MessageBox.Show(
            $"Xác nhận sinh viên đã trả thiết bị trong phiếu {selected.TicketId}?",
            "Xác nhận trả thiết bị", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (answer == DialogResult.Yes)
        {
            selected.Status = "Đã trả";
            selected.ActualReturnDate = DateTime.Today;
            bindingSource.ResetBindings(false);
            ApplyFilters();
            LoadRecordToForm(selected);
            MessageBox.Show("Đã cập nhật tình trạng và ngày trả thực tế.");
        }
    }

    private void LoadSelectedRecord()
    {
        CheckoutRecord? selected = GetSelectedRecord();
        if (selected != null)
            LoadRecordToForm(selected);
    }

    private void LoadRecordToForm(CheckoutRecord record)
    {
        txtTicketId.Text = record.TicketId;
        txtStudentId.Text = record.StudentId;
        txtStudentName.Text = record.StudentName;
        cboEquipment.Text = record.Equipment;
        numQuantity.Value = Math.Max(numQuantity.Minimum, Math.Min(numQuantity.Maximum, record.Quantity));
        dtpBorrowDate.Value = record.BorrowDate;
        dtpDueDate.Value = record.DueDate;
        cboStatus.SelectedItem = record.Status;
        if (record.ActualReturnDate.HasValue)
        {
            dtpReturnDate.Value = record.ActualReturnDate.Value;
            dtpReturnDate.Checked = true;
        }
        else
        {
            dtpReturnDate.Value = DateTime.Today;
            dtpReturnDate.Checked = false;
        }
        errorProvider.Clear();
    }

    private void ClearInputs()
    {
        errorProvider.Clear();
        txtStudentId.Clear();
        txtStudentName.Clear();
        cboEquipment.Text = "";
        numQuantity.Value = 1;
        dtpBorrowDate.Value = DateTime.Today;
        dtpDueDate.Value = DateTime.Today.AddDays(7);
        dtpReturnDate.Value = DateTime.Today;
        dtpReturnDate.Checked = false;
        cboStatus.SelectedIndex = 0;
        dgvRecords.ClearSelection();
        CreateNewTicketId();
    }

    private void ApplyFilters()
    {
        if (dgvRecords == null || txtSearch == null) return;

        string keyword = txtSearch.Text.Trim();
        string filter = cboFilter.SelectedItem?.ToString() ?? "Tất cả phiếu";

        var filtered = records.Where(r =>
        {
            bool matchesSearch =
                r.StudentId.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                r.Equipment.Contains(keyword, StringComparison.OrdinalIgnoreCase);

            bool matchesFilter = filter switch
            {
                "Đang mượn" => r.Status != "Đã trả" && r.DueDate.Date >= DateTime.Today,
                "Quá hạn" => r.Status != "Đã trả" && r.DueDate.Date < DateTime.Today,
                "Đã trả" => r.Status == "Đã trả",
                _ => true
            };

            return matchesSearch && matchesFilter;
        })
        .OrderBy(r => r.DueDate)
        .ToList();

        bindingSource.DataSource = new BindingList<CheckoutRecord>(filtered);
        dgvRecords.DataSource = bindingSource;

        int overdueCount = records.Count(r => r.Status != "Đã trả" && r.DueDate.Date < DateTime.Today);
        statusLabel.Text = $"Tổng số phiếu: {records.Count} | Phiếu quá hạn: {overdueCount} | Đang hiển thị: {filtered.Count}";
    }

    private void ExportCsv()
    {
        var visibleRecords = dgvRecords.Rows
            .Cast<DataGridViewRow>()
            .Where(row => !row.IsNewRow)
            .Select(row => row.DataBoundItem as CheckoutRecord)
            .Where(record => record != null)
            .Cast<CheckoutRecord>()
            .ToList();

        if (visibleRecords.Count == 0)
        {
            MessageBox.Show("Không có phiếu mượn nào để xuất.");
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Filter = "CSV (*.csv)|*.csv",
            FileName = "DanhSachPhieuMuon.csv",
            Title = "Xuất danh sách phiếu mượn"
        };

        if (dialog.ShowDialog() != DialogResult.OK) return;

        var lines = new List<string>
        {
            "Ma phieu,Ma sinh vien,Ho ten sinh vien,Thiet bi,So luong,Ngay muon,Han tra,Ngay tra thuc te,Tinh trang"
        };

        foreach (CheckoutRecord r in visibleRecords)
        {
            lines.Add(string.Join(",", new[]
            {
                Csv(r.TicketId), Csv(r.StudentId), Csv(r.StudentName),
                Csv(r.Equipment), Csv(r.Quantity.ToString()),
                Csv(r.BorrowDate.ToString("dd/MM/yyyy")),
                Csv(r.DueDate.ToString("dd/MM/yyyy")),
                Csv(r.ActualReturnDate?.ToString("dd/MM/yyyy") ?? ""),
                Csv(r.DisplayStatus)
            }));
        }

        File.WriteAllLines(dialog.FileName, lines, new UTF8Encoding(true));
        MessageBox.Show("Xuất CSV thành công.", "Thông báo",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static string Csv(string value)
    {
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
