using FluentNHibernate.Mapping;

namespace SneakerIcon.Classes.DAO
{
    public abstract class BaseEntityMap<T> : ClassMap<T> where T : BaseEntity
    {
        protected BaseEntityMap()
        {
            Id(x => x.Id).GeneratedBy.Native().Not.Nullable();
            Map(x => x.IsDeleted).Column("IsDeleted").Not.Nullable().Default("0");
        }
    }
}
