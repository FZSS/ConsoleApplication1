namespace SneakerIcon.Classes.DAO
{
    public abstract class BaseEntity
    {
        public virtual int Id { get; protected set; }
        public virtual bool IsDeleted { get; set; }
    }
}
