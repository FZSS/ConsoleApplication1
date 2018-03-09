using SneakerIcon.Classes.DAO.Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.DAO.Model.Map
{
    public class SizeMap : BaseEntityMap<Size>
    {
        public SizeMap()
        {
            Table("Size");
            References(x => x.Brand).Column("Brand").Not.LazyLoad().Not.Nullable();
            Map(x => x.US).Column("US").Not.Nullable();
            Map(x => x.UK).Column("UK").Not.Nullable();
            Map(x => x.RUS).Column("RUS").Not.Nullable();
            Map(x => x.EUR).Column("EUR").Not.Nullable();
            Map(x => x.CM).Column("CM").Not.Nullable();
            References(x => x.Category).Column("Category").Not.LazyLoad().Not.Nullable();
        }
    }
}
