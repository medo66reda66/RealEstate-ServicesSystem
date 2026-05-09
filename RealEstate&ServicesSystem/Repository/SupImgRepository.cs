using RealEstate_ServicesSystem.DATABS;
using RealEstate_ServicesSystem.Models;
using RealEstate_ServicesSystem.Repository.IRepository;

namespace RealEstate_ServicesSystem.Repository
{
    public class SupImgRepository:Repository<UnitSupImg>, ISupImgRepository
    {
        private readonly ApplicationDBcontext dBcontext;
        public SupImgRepository(ApplicationDBcontext dBcontext):base(dBcontext)
        {
            this.dBcontext = dBcontext;
        }
        public void RemoveAll (IEnumerable<UnitSupImg>  unitSupImgs)
        {
            dBcontext.RemoveRange(unitSupImgs);
        }
    }
}
