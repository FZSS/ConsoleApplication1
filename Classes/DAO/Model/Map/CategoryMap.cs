using SneakerIcon.Classes.DAO.Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.DAO.Model.Map
{
    public class CategoryMap : BaseEntityMap<Category>
    {
        public CategoryMap()
        {
            Table("Category");
            Map(x => x.Name).Column("Name").Not.Nullable();
        }
    }
}
