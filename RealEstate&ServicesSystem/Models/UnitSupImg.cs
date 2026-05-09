namespace RealEstate_ServicesSystem.Models
{
    public class UnitSupImg
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int UnitId { get; set; }
        public Unit? Unit { get; set; }
    }
}
