REPAIR SERVICE TRACKER - QUAN LY DON SUA CHUA

Yeu cau:
- Windows 10/11
- Visual Studio 2022
- Workload .NET desktop development
- .NET 8 SDK

Cach mo va chay:
1. Giai nen file ZIP.
2. Mo RepairServiceTracker.csproj bang Visual Studio 2022.
3. Mo View > Terminal va chay:
   dotnet restore
   dotnet build
4. Neu hien Build succeeded, nhan Ctrl + F5.

Chuc nang:
- Form responsive su dung TableLayoutPanel, FlowLayoutPanel va Dock.
- Nhap ma don, khach hang, so dien thoai, loai thiet bi, mo ta loi,
  ngay tiep nhan, chi phi du kien va trang thai.
- Chon anh thiet bi bang OpenFileDialog; PictureBox Zoom de hien thi.
- DataGridView cot dinh nghia, AutoGenerateColumns=false.
- BindingList va BindingSource.
- Them, cap nhat, xoa co xac nhan.
- Tim kiem theo ma don, ten khach hang, so dien thoai.
- Loc theo trang thai.
- ErrorProvider kiem tra ma don trung, ten, dien thoai, loai thiet bi,
  mo ta loi va chi phi khong am.
- MenuStrip xuat CSV va thoat ung dung.
- StatusStrip hien so don theo bo loc, tong chi phi va chi phi chua hoan tat.

Luu y:
- Du lieu luu trong bo nho khi chuong trinh dang chay; dong ung dung se mat du lieu.
- Anh duoc tham chieu bang duong dan tren may, khong duoc nhung vao file CSV.
