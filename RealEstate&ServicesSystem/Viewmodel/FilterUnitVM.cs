using RealEstate_ServicesSystem.Models;

namespace RealEstate_ServicesSystem.Viewmodel
{
    public record FilterUnitVM
    (
        int? unitNumber, int? bedrooms, int? bathrooms, double? areaSize, int? floorNumber, decimal? price, UnitStatus? status, UnitPurpose? purpose, UnitType? type
    );

}
