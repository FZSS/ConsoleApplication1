using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.DAO.Model.Entity
{
    public class Size : BaseEntity
    {
        public virtual Brand Brand { get; set; }
        public virtual string US { get; set; }
        public virtual string UK { get; set; }
        public virtual string EUR { get; set; }
        public virtual string CM { get; set; }
        public virtual string RUS { get; set; }
        /// <summary>
        /// man woman or kids
        /// </summary>
        public virtual Category Category { get; set; }
        
    }
}
