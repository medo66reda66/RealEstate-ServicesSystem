using RealEstate_ServicesSystem.Models;

namespace RealEstate_ServicesSystem.Viewmodel
{
    public record FilterHome(
        
        string? city,
        string? Location,
        decimal? MinPrice,
        decimal? MaxPrice,
        double? MinArea,
        double? MaxArea,
        int? Bedrooms,
        int? Bathrooms,
        UnitPurpose? UnitPurpose
    );


}
