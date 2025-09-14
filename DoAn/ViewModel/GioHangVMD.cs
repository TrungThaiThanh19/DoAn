using System.Collections.Generic;
using System.Linq;

namespace DoAn.ViewModel
{
    public class GioHangVMD
    {
        public List<GioHangItemVMD> Items { get; set; } = new();
        public decimal Subtotal => Items?.Sum(i => i.ThanhTien) ?? 0m;
    }
}