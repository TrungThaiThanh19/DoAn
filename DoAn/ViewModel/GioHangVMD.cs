namespace DoAn.ViewModel
{
    public class GioHangVMD
    {
        public Guid GioHangId { get; set; }
        public List<ChiTietGioHangVMD> Items { get; set; } = new();
        public decimal Subtotal => Items.Sum(i => i.ThanhTien);
    }
}
