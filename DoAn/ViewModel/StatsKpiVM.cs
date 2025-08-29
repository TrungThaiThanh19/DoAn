using System;
using System.ComponentModel;

namespace DoAn.ViewModels
{
    public class StatsKpiVM
    {
        public decimal DoanhThu { get; set; }
        public decimal TongGiaNhap { get; set; }
        public decimal ChiPhiVanChuyen { get; set; }
        public decimal HoanTienTraHang { get; set; }

        public decimal LoiNhuan => DoanhThu - TongGiaNhap - ChiPhiVanChuyen - HoanTienTraHang;

        public int DonHoanTat { get; set; }
        public int KhachHangMoi { get; set; }
        public decimal GiamGiaKM_Voucher { get; set; }
        public decimal PhuThu { get; set; }
    }
}