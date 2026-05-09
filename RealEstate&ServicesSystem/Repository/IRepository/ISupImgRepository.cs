using RealEstate_ServicesSystem.Models;

namespace RealEstate_ServicesSystem.Repository.IRepository
{
    public interface ISupImgRepository
    {
            void RemoveAll (IEnumerable<UnitSupImg>  unitSupImgs);
    }
}
