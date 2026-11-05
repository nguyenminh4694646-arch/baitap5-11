using System.ComponentModel;
using System.Text;

namespace RepairServiceTracker;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}

public class RepairOrder
{
    public string OrderId { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string EquipmentType { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime ReceivedDate { get; set; }
    public decimal EstimatedCost { get; set; }
    public string Status { get; set; } = "Đang sửa";
    public string ImagePath { get; set; } = "";

    public string CostDisplay => EstimatedCost.ToString("N0");
}

public class MainForm : Form
{
    private readonly BindingList<RepairOrder> orders = new();
    private readonly BindingSource bindingSource = new();
    private readonly ErrorProvider errorProvider = new();

    private TextBox txtOrderId = null!;
    private TextBox txtCustomerName = null!;
    private TextBox txtPhone = null!;
    private ComboBox cboEquipment = null!;
    private TextBox txtDescription = null!;
    private DateTimePicker dtpReceivedDate = null!;
    private NumericUpDown numCost = null!;
    private ComboBox cboStatus = null!;
    private TextBox txtSearch = null!;
    private ComboBox cboFilterStatus = null!;
    private PictureBox pictureBox = null!;
    private Label lblImagePath = null!;
    private DataGridView dgvOrders = null!;
    private ToolStripStatusLabel statusLabel = null!;
    private string selectedImagePath = "";

    public MainForm()
    {
        Text = "Repair Service Tracker - Quản lý đơn sửa chữa";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1120, 720);
        Size = new Size(1380, 850);
        Font = new Font("Segoe UI", 9F);

        BuildInterface();

        bindingSource.DataSource = orders;
        dgvOrders.DataSource = bindingSource;
        orders.ListChanged += (_, _) => ApplyFilters();

        CreateNewOrderId();
        ApplyFilters();
    }

    private void BuildInterface()
    {
        var menu = new MenuStrip();
        var fileMenu = new ToolStripMenuItem("Tệp");
        var exportItem = new ToolStripMenuItem("Xuất danh sách CSV");
        exportItem.Click += (_, _) => ExportCsv();
        var exitItem = new ToolStripMenuItem("Thoát");
        exitItem.Click += (_, _) => Close();
        fileMenu.DropDownItems.Add(exportItem);
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add(exitItem);
        menu.Items.Add(fileMenu);
        MainMenuStrip = menu;
        Controls.Add(menu);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 290));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(root);

        var title = new Label
        {
            Text = "QUẢN LÝ ĐƠN ĐĂNG KÝ DỊCH VỤ SỬA CHỮA",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = Color.DarkBlue
        };
        root.Controls.Add(title, 0, 0);

        var inputGroup = new GroupBox
        {
            Text = "Thông tin đơn sửa chữa",
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };
        root.Controls.Add(inputGroup, 0, 1);

        var mainInput = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(4)
        };
        mainInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75));
        mainInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        inputGroup.Controls.Add(mainInput);

        var fields = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 4,
            Padding = new Padding(2)
        };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (int i = 0; i < 4; i++)
            fields.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
        mainInput.Controls.Add(fields, 0, 0);

        txtOrderId = new TextBox { Dock = DockStyle.Fill };
        txtCustomerName = new TextBox { Dock = DockStyle.Fill };
        txtPhone = new TextBox { Dock = DockStyle.Fill };
        cboEquipment = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDown
        };
        cboEquipment.Items.AddRange(new object[]
        {
            "Điện thoại", "Laptop", "Máy tính để bàn", "Máy tính bảng",
            "Máy in", "Màn hình", "Thiết bị khác"
        });
        txtDescription = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical
        };
        dtpReceivedDate = new DateTimePicker
        {
            Dock = DockStyle.Fill,
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "dd/MM/yyyy",
            Value = DateTime.Today
        };
        numCost = new NumericUpDown
        {
            Dock = DockStyle.Left,
            Width = 180,
            Minimum = 0,
            Maximum = 1000000000,
            ThousandsSeparator = true,
            DecimalPlaces = 0
        };
        cboStatus = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cboStatus.Items.AddRange(new object[]
        {
            "Đang sửa", "Chờ linh kiện", "Hoàn tất", "Đã hủy"
        });
        cboStatus.SelectedIndex = 0;

        AddField(fields, "Mã đơn:", txtOrderId, 0, 0);
        AddField(fields, "Tên khách hàng:", txtCustomerName, 1, 0);
        AddField(fields, "Số điện thoại:", txtPhone, 0, 1);
        AddField(fields, "Loại thiết bị:", cboEquipment, 1, 1);
        AddField(fields, "Mô tả lỗi:", txtDescription, 0, 2);
        AddField(fields, "Ngày tiếp nhận:", dtpReceivedDate, 1, 2);
        AddField(fields, "Chi phí dự kiến:", numCost, 0, 3);
        AddField(fields, "Trạng thái:", cboStatus, 1, 3);

        var imagePanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(8, 0, 0, 0)
        };
        imagePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        imagePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        imagePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        mainInput.Controls.Add(imagePanel, 1, 0);

        pictureBox = new PictureBox
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.WhiteSmoke,
            SizeMode = PictureBoxSizeMode.Zoom
        };
        imagePanel.Controls.Add(pictureBox, 0, 0);

        var chooseImageButton = new Button
        {
            Text = "Chọn ảnh thiết bị...",
            Dock = DockStyle.Fill,
            BackColor = Color.SteelBlue,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        chooseImageButton.FlatAppearance.BorderSize = 0;
        chooseImageButton.Click += (_, _) => ChooseImage();
        imagePanel.Controls.Add(chooseImageButton, 0, 1);

        lblImagePath = new Label
        {
            Text = "Chưa chọn ảnh",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoEllipsis = true
        };
        imagePanel.Controls.Add(lblImagePath, 0, 2);

        var filterGroup = new GroupBox
        {
            Text = "Tìm kiếm và lọc đơn sửa chữa",
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };
        root.Controls.Add(filterGroup, 0, 2);

        var filterTable = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2
        };
        filterTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
        filterTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        filterTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 23));
        filterTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        filterGroup.Controls.Add(filterTable);

        filterTable.Controls.Add(new Label
        {
            Text = "Tìm theo mã đơn, tên khách hàng hoặc số điện thoại",
            Dock = DockStyle.Fill
        }, 0, 0);
        filterTable.Controls.Add(new Label
        {
            Text = "Lọc theo trạng thái",
            Dock = DockStyle.Fill
        }, 1, 0);

        txtSearch = new TextBox
        {
            Dock = DockStyle.Fill,
            PlaceholderText = "Nhập mã đơn, tên hoặc số điện thoại..."
        };
        cboFilterStatus = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cboFilterStatus.Items.AddRange(new object[]
        {
            "Tất cả trạng thái", "Đang sửa", "Chờ linh kiện", "Hoàn tất", "Đã hủy"
        });
        cboFilterStatus.SelectedIndex = 0;
        filterTable.Controls.Add(txtSearch, 0, 1);
        filterTable.Controls.Add(cboFilterStatus, 1, 1);
        txtSearch.TextChanged += (_, _) => ApplyFilters();
        cboFilterStatus.SelectedIndexChanged += (_, _) => ApplyFilters();

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
            Padding = new Padding(0, 4, 0, 4),
            AutoScroll = true
        };
        bottom.Controls.Add(buttons, 0, 0);
        AddButton(buttons, "Thêm đơn", Color.SeaGreen, AddOrder);
        AddButton(buttons, "Cập nhật", Color.RoyalBlue, UpdateOrder);
        AddButton(buttons, "Xóa đơn", Color.Firebrick, DeleteOrder);
        AddButton(buttons, "Làm mới", Color.DimGray, ClearInputs);
        AddButton(buttons, "Xuất CSV", Color.DarkSlateBlue, ExportCsv);

        dgvOrders = new DataGridView
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
        dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(RepairOrder.OrderId), HeaderText = "Mã đơn", FillWeight = 65 });
        dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(RepairOrder.CustomerName), HeaderText = "Khách hàng", FillWeight = 105 });
        dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(RepairOrder.Phone), HeaderText = "Số điện thoại", FillWeight = 90 });
        dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(RepairOrder.EquipmentType), HeaderText = "Thiết bị", FillWeight = 90 });
        dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(RepairOrder.Description), HeaderText = "Mô tả lỗi", FillWeight = 125 });
        dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(RepairOrder.ReceivedDate), HeaderText = "Ngày tiếp nhận", FillWeight = 85, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" } });
        dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(RepairOrder.EstimatedCost), HeaderText = "Chi phí (VNĐ)", FillWeight = 95, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } });
        dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(RepairOrder.Status), HeaderText = "Trạng thái", FillWeight = 85 });
        dgvOrders.SelectionChanged += (_, _) => LoadSelectedOrder();
        bottom.Controls.Add(dgvOrders, 0, 1);

        var statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel("Số đơn: 0 | Tổng chi phí: 0 VNĐ | Chi phí chưa hoàn tất: 0 VNĐ");
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
            Width = 112,
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

    private void CreateNewOrderId()
    {
        int number = 1;
        while (orders.Any(o => o.OrderId == $"SC{number:000}"))
            number++;
        txtOrderId.Text = $"SC{number:000}";
    }

    private RepairOrder? GetSelectedOrder()
    {
        return dgvOrders.CurrentRow?.DataBoundItem as RepairOrder;
    }

    private bool ValidateInputs(bool updating)
    {
        errorProvider.Clear();
        bool valid = true;
        RepairOrder? selected = GetSelectedOrder();

        if (string.IsNullOrWhiteSpace(txtOrderId.Text))
        {
            errorProvider.SetError(txtOrderId, "Mã đơn không được để trống.");
            valid = false;
        }
        else if (orders.Any(o =>
            o.OrderId.Equals(txtOrderId.Text.Trim(), StringComparison.OrdinalIgnoreCase) &&
            (!updating || !ReferenceEquals(o, selected))))
        {
            errorProvider.SetError(txtOrderId, "Mã đơn đã tồn tại.");
            valid = false;
        }

        if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
        {
            errorProvider.SetError(txtCustomerName, "Tên khách hàng không được để trống.");
            valid = false;
        }

        if (string.IsNullOrWhiteSpace(txtPhone.Text))
        {
            errorProvider.SetError(txtPhone, "Số điện thoại không được để trống.");
            valid = false;
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(txtPhone.Text.Trim(), @"^(0|\+84)[0-9]{9,10}$"))
        {
            errorProvider.SetError(txtPhone, "Số điện thoại không đúng định dạng.");
            valid = false;
        }

        if (string.IsNullOrWhiteSpace(cboEquipment.Text))
        {
            errorProvider.SetError(cboEquipment, "Vui lòng chọn hoặc nhập loại thiết bị.");
            valid = false;
        }

        if (string.IsNullOrWhiteSpace(txtDescription.Text))
        {
            errorProvider.SetError(txtDescription, "Mô tả lỗi là bắt buộc.");
            valid = false;
        }

        if (numCost.Value < 0)
        {
            errorProvider.SetError(numCost, "Chi phí dự kiến không được âm.");
            valid = false;
        }

        if (cboStatus.SelectedIndex < 0)
        {
            errorProvider.SetError(cboStatus, "Vui lòng chọn trạng thái.");
            valid = false;
        }

        return valid;
    }

    private RepairOrder ReadForm()
    {
        return new RepairOrder
        {
            OrderId = txtOrderId.Text.Trim(),
            CustomerName = txtCustomerName.Text.Trim(),
            Phone = txtPhone.Text.Trim(),
            EquipmentType = cboEquipment.Text.Trim(),
            Description = txtDescription.Text.Trim(),
            ReceivedDate = dtpReceivedDate.Value.Date,
            EstimatedCost = numCost.Value,
            Status = cboStatus.Text,
            ImagePath = selectedImagePath
        };
    }

    private void AddOrder()
    {
        if (!ValidateInputs(false)) return;

        orders.Add(ReadForm());
        ClearInputs();
        MessageBox.Show("Thêm đơn sửa chữa thành công.", "Thông báo",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void UpdateOrder()
    {
        RepairOrder? selected = GetSelectedOrder();
        if (selected == null)
        {
            MessageBox.Show("Vui lòng chọn đơn cần cập nhật.");
            return;
        }
        if (!ValidateInputs(true)) return;

        RepairOrder updated = ReadForm();
        selected.OrderId = updated.OrderId;
        selected.CustomerName = updated.CustomerName;
        selected.Phone = updated.Phone;
        selected.EquipmentType = updated.EquipmentType;
        selected.Description = updated.Description;
        selected.ReceivedDate = updated.ReceivedDate;
        selected.EstimatedCost = updated.EstimatedCost;
        selected.Status = updated.Status;
        selected.ImagePath = updated.ImagePath;

        bindingSource.ResetBindings(false);
        ApplyFilters();
        MessageBox.Show("Cập nhật đơn sửa chữa thành công.", "Thông báo",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void DeleteOrder()
    {
        RepairOrder? selected = GetSelectedOrder();
        if (selected == null)
        {
            MessageBox.Show("Vui lòng chọn đơn cần xóa.");
            return;
        }

        DialogResult answer = MessageBox.Show(
            $"Bạn có chắc chắn muốn xóa đơn {selected.OrderId} của {selected.CustomerName} không?",
            "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (answer == DialogResult.Yes)
        {
            orders.Remove(selected);
            ClearInputs();
        }
    }

    private void ChooseImage()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Chọn ảnh thiết bị",
            Filter = "Tệp hình ảnh|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
        };

        if (dialog.ShowDialog() != DialogResult.OK) return;

        try
        {
            SetPicture(dialog.FileName);
            selectedImagePath = dialog.FileName;
            lblImagePath.Text = Path.GetFileName(dialog.FileName);
            errorProvider.SetError(pictureBox, "");
        }
        catch
        {
            MessageBox.Show("Không thể mở ảnh này. Vui lòng chọn tệp hình ảnh khác.",
                "Lỗi ảnh", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void SetPicture(string path)
    {
        if (!File.Exists(path))
        {
            pictureBox.Image = null;
            return;
        }

        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var source = Image.FromStream(stream);
        var copy = new Bitmap(source);
        Image? oldImage = pictureBox.Image;
        pictureBox.Image = copy;
        oldImage?.Dispose();
    }

    private void LoadSelectedOrder()
    {
        RepairOrder? selected = GetSelectedOrder();
        if (selected == null) return;

        txtOrderId.Text = selected.OrderId;
        txtCustomerName.Text = selected.CustomerName;
        txtPhone.Text = selected.Phone;
        cboEquipment.Text = selected.EquipmentType;
        txtDescription.Text = selected.Description;
        dtpReceivedDate.Value = selected.ReceivedDate;
        numCost.Value = Math.Min(numCost.Maximum, Math.Max(numCost.Minimum, selected.EstimatedCost));
        cboStatus.SelectedItem = selected.Status;
        selectedImagePath = selected.ImagePath;
        lblImagePath.Text = string.IsNullOrWhiteSpace(selected.ImagePath)
            ? "Chưa chọn ảnh"
            : Path.GetFileName(selected.ImagePath);

        try
        {
            if (!string.IsNullOrWhiteSpace(selected.ImagePath) && File.Exists(selected.ImagePath))
                SetPicture(selected.ImagePath);
            else
                SetPicture("");
        }
        catch
        {
            SetPicture("");
        }

        errorProvider.Clear();
    }

    private void ClearInputs()
    {
        errorProvider.Clear();
        txtCustomerName.Clear();
        txtPhone.Clear();
        cboEquipment.Text = "";
        txtDescription.Clear();
        dtpReceivedDate.Value = DateTime.Today;
        numCost.Value = 0;
        cboStatus.SelectedIndex = 0;
        selectedImagePath = "";
        lblImagePath.Text = "Chưa chọn ảnh";
        SetPicture("");
        dgvOrders.ClearSelection();
        CreateNewOrderId();
    }

    private void ApplyFilters()
    {
        if (dgvOrders == null || txtSearch == null) return;

        string keyword = txtSearch.Text.Trim();
        string status = cboFilterStatus.SelectedItem?.ToString() ?? "Tất cả trạng thái";

        var filtered = orders.Where(o =>
        {
            bool matchesKeyword =
                o.OrderId.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                o.CustomerName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                o.Phone.Contains(keyword, StringComparison.OrdinalIgnoreCase);

            bool matchesStatus = status == "Tất cả trạng thái" || o.Status == status;
            return matchesKeyword && matchesStatus;
        })
        .OrderByDescending(o => o.ReceivedDate)
        .ToList();

        bindingSource.DataSource = new BindingList<RepairOrder>(filtered);
        dgvOrders.DataSource = bindingSource;

        decimal totalCost = filtered.Sum(o => o.EstimatedCost);
        decimal unfinishedCost = filtered
            .Where(o => o.Status != "Hoàn tất" && o.Status != "Đã hủy")
            .Sum(o => o.EstimatedCost);

        statusLabel.Text =
            $"Số đơn theo bộ lọc: {filtered.Count} | Tổng chi phí: {totalCost:N0} VNĐ | Chi phí chưa hoàn tất: {unfinishedCost:N0} VNĐ";
    }

    private void ExportCsv()
    {
        var visibleOrders = dgvOrders.Rows
            .Cast<DataGridViewRow>()
            .Where(row => !row.IsNewRow)
            .Select(row => row.DataBoundItem as RepairOrder)
            .Where(order => order != null)
            .Cast<RepairOrder>()
            .ToList();

        if (visibleOrders.Count == 0)
        {
            MessageBox.Show("Không có đơn sửa chữa để xuất.");
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Filter = "CSV (*.csv)|*.csv",
            FileName = "DanhSachDonSuaChua.csv",
            Title = "Xuất danh sách đơn sửa chữa"
        };

        if (dialog.ShowDialog() != DialogResult.OK) return;

        var lines = new List<string>
        {
            "Ma don,Ten khach hang,So dien thoai,Loai thiet bi,Mo ta loi,Ngay tiep nhan,Chi phi du kien,Trang thai,Duong dan anh"
        };

        foreach (RepairOrder o in visibleOrders)
        {
            lines.Add(string.Join(",", new[]
            {
                Csv(o.OrderId),
                Csv(o.CustomerName),
                Csv(o.Phone),
                Csv(o.EquipmentType),
                Csv(o.Description),
                Csv(o.ReceivedDate.ToString("dd/MM/yyyy")),
                Csv(o.EstimatedCost.ToString("0")),
                Csv(o.Status),
                Csv(o.ImagePath)
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

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        pictureBox?.Image?.Dispose();
        base.OnFormClosed(e);
    }
}
