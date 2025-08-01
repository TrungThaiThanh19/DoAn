﻿using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class TrangThaiDonHang
    {
        [Key]
        public Guid ID_TrangThaiDonHang { get; set; }
        public int TrangThai { get; set; }
        public DateTime NgayChuyen { get; set; }
        public string NhanVienDoi { get; set; }// Nhân viên đổi
        public string NoiDungDoi { get; set; }//Nội dung đổi

        public Guid ID_HoaDon { get; set; }
        public HoaDon HoaDon { get; set; }
    }
}
