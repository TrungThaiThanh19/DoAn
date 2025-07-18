namespace DoAn.Models
{
    public class QuocGia
    {
        public Guid ID_QuocGia { get; set; }
        public string Ten_QuocGia { get; set; }
        
        public ICollection<SanPham> SanPhams { get; set; }
    }
}
