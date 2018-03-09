using System.Collections.Generic;

namespace SneakerIcon.Classes.DAO
{
    public class DAO
    {
        private static DAO _dao;
        private readonly NHibernate _hibernate;

        private DAO()
        {
            _hibernate = new NHibernate();
        }

        public static DAO GetDAO()
        {
            return _dao ?? (_dao = new DAO());
        }

        public T SaveObject<T>(T obj) where T : BaseEntity
        {
            lock (this)
            {
                _hibernate.OpenSession();
                var result = _hibernate.SaveObject(obj);
                _hibernate.CloseSession();
                return result;
            }
        }

        public IList<T> GetObjects<T>() where T : BaseEntity
        {
            lock (this)
            {
                _hibernate.OpenSession();
                var result = _hibernate.GetObjects<T>();
                _hibernate.CloseSession();
                return result;
            }
        }

        public T GetObjects<T>(int id) where T : BaseEntity
        {
            lock (this)
            {
                _hibernate.OpenSession();
                var result = _hibernate.GetObjects<T>(id);
                _hibernate.CloseSession();
                return result;
            }
        }

        public IList<T> GetObjectsForHql<T>(string hqlQuery) where T : BaseEntity
        {
            lock (this)
            {
                _hibernate.OpenSession();
                var result = _hibernate.GetObjectsForHql<T>(hqlQuery);
                _hibernate.CloseSession();
                return result;
            }
        }

        public IList<T> GetObjectsForHql<T>(string hqlQuery, IList<RequestParam> listParam) where T : BaseEntity
        {
            lock (this)
            {
                _hibernate.OpenSession();
                var result = _hibernate.GetObjectsForHql<T>(hqlQuery, listParam);
                _hibernate.CloseSession();
                return result;
            }
        }

        public IList<T> GetObjectsForHqlManyToMany<T>(string hqlQuery, string s1) where T : BaseEntity
        {
            lock (this)
            {
                _hibernate.OpenSession();
                var result = _hibernate.GetObjectsForHqlManyToMany<T>(hqlQuery, s1);
                _hibernate.CloseSession();
                return result;
            }
        }

        public T SaveOrUpdateObject<T>(T obj) where T : BaseEntity
        {
            lock (this)
            {
                _hibernate.OpenSession();
                var result = _hibernate.SaveOrUpdateObject(obj);
                _hibernate.CloseSession();
                return result;
            }
        }

        public void DeleteObject<T>(T obj) where T : BaseEntity
        {
            lock (this)
            {
                _hibernate.OpenSession();
                _hibernate.DeleteObject(obj);
                _hibernate.CloseSession();
            }

        }
    }
}
