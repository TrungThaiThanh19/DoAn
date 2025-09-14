using System;
using System.Collections.Generic;

namespace DoAn.ViewModel
{
    public class LichSuTraHangVM
    {
        public Guid ID_TraHang { get; set; }
        public string LyDo { get; set; }
        public string GhiChu { get; set; }
        public string NhanVienXuLy { get; set; }
        public DateTime NgayTao { get; set; }
        public int TrangThai { get; set; }
        public decimal TongTienHoan { get; set; }
        public Guid ID_HoaDon { get; set; }

        // Thông tin hóa đơn (cho trang tổng quan)
        public string MaHoaDon { get; set; }
        public string TenKhachHang { get; set; }

        public List<LichSuTraHangChiTietVM> ChiTietTraHangs { get; set; } = new();

        // Mapping trạng thái cho khớp với QuanLyTraHangController
        public string TrangThaiText => TrangThai switch
        {
            6 => "Yêu cầu hoàn hàng",   // ReturnStatus.YeuCau
            1 => "Đã duyệt",            // ReturnStatus.DaDuyet
            2 => "Đã nhận hàng",        // ReturnStatus.DaNhanHang
            3 => "Đã hoàn tiền",        // ReturnStatus.DaHoanTien
            9 => "Từ chối",             // ReturnStatus.TuChoi
            _ => "Không rõ"
        };
    }
    }