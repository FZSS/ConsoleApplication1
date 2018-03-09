using System;
using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using NHibernate.Transform;

namespace SneakerIcon.Classes.DAO
{
    public class NHibernate
    {
        //http://www.dreamincode.net/forums/topic/161638-fluent-nhibernate-tutorial/
        //http://slynetblog.blogspot.ru/2009/10/nhibernate-1.html
        //https://github.com/jagregory/fluent-nhibernate/wiki/Database-configuration

        private ISessionFactory _sessionFactory;
        private ISession _session;

        public void OpenSession()
        {
            _session = SessionFactory.OpenSession();
        }

        public void CloseSession()
        {
            _session.Close();
        }

        public NHibernate()
        {
            BuildSessionFactory();
        }

        public T  SaveObject<T>(T obj) where T : BaseEntity
        {
            using (var transaction = _session.BeginTransaction())
            {
                _session.Save(obj);
                transaction.Commit();
                return !obj.IsDeleted ? obj : null;
            }
        }

        public IList<T> GetObjects<T>() where T : BaseEntity
        {
            var result = _session.CreateCriteria(typeof(T)).List<T>();
            return result.Where(x => !x.IsDeleted).ToList();
        }

        public IList<T> GetObjectsForHql<T>(string hqlQuery) where T : BaseEntity
        {
            var result = _session.CreateQuery(hqlQuery).List<T>();
            return result.Where(x => !x.IsDeleted).ToList();
        }

        public IList<T> GetObjectsForHqlManyToMany<T>(string hqlQuery, string s1) where T : BaseEntity
        {
            var result = _session.CreateQuery(hqlQuery)
                .SetString(0, s1)
                .SetResultTransformer(new DistinctRootEntityResultTransformer())
                .List<T>();
            return result.Where(x => !x.IsDeleted).ToList();
        }

        public IList<T> GetObjectsForHql<T>(string hqlQuery, IList<RequestParam> listParam) where T : BaseEntity
        {
            var query = _session.CreateQuery(hqlQuery);

            var i = 0;

            foreach (var param in listParam)
            {
                switch (param.TypeParam)
                {
                    case TypeParamEnum.Boolean:
                        query.SetBoolean(i, (bool)param.Param);
                        break;
                    case TypeParamEnum.DateTime:
                        query.SetDateTime(i, (DateTime)param.Param);
                        break;
                    case TypeParamEnum.Float:
                        query.SetDouble(i, (double)param.Param);
                        break;
                    case TypeParamEnum.Int:
                        query.SetInt32(i, (int)param.Param);
                        break;
                    case TypeParamEnum.Long:
                        query.SetInt64(i, (long)param.Param);
                        break;
                    case TypeParamEnum.String:
                        query.SetString(i, (string)param.Param);
                        break;
                }
                i++;
            }

            var result = query.List<T>();

            return result.Where(x => !x.IsDeleted).ToList();
        }

        public T GetObjects<T>(int id) where T : BaseEntity
        {
            var obj = _session.Get<T>(id);
            return !obj.IsDeleted ? obj : null;
        }

        public T SaveOrUpdateObject<T>(T obj) where T : BaseEntity
        {
            using (var transaction = _session.BeginTransaction())
            {
                //Если свойство изменено через интерфейс, оно всё-равно останется false
                obj.IsDeleted = false;
                _session.SaveOrUpdate(obj);
                transaction.Commit();
                return obj;
            }
        }

        public void DeleteObject<T>(T obj) where T : BaseEntity
        {
            using (var transaction = _session.BeginTransaction())
            {
                obj.IsDeleted = true;
                _session.SaveOrUpdate(obj);
                transaction.Commit();
            }
        }

        private void BuildSessionFactory()
        {
            //Если в условии стоит обновление базы
            if (ConfigManage.ResetDatabase)
            {
                _sessionFactory = Fluently.Configure()
                    .Database(MsSqlConfiguration.MsSql2012
                        .ConnectionString(c => c
                            .Server(ConfigManage.Server)
                            .Database(ConfigManage.Database)
                            .Username(ConfigManage.Username)
                            .Password(ConfigManage.Password)))
                    .Mappings(m => m.FluentMappings.AddFromAssemblyOf<NHibernate>())
                    .ExposeConfiguration(BuildSchema)		//Ахтунг!!!! Подумай о последствиях!!! Базу убьёшь!!!!!!
                    .BuildSessionFactory();
            }
            else
            {
                _sessionFactory = Fluently.Configure()
                    .Database(MsSqlConfiguration.MsSql2012
                        .ConnectionString(c => c
                            .Server(ConfigManage.Server)
                            .Database(ConfigManage.Database)
                            .Username(ConfigManage.Username)
                            .Password(ConfigManage.Password)))
                    .Mappings(m => m.FluentMappings.AddFromAssemblyOf<NHibernate>())
                    .BuildSessionFactory();
            }
        }

        //Метод для формирования базы. Быть крайне аккуратным
        private void BuildSchema(Configuration config)
        {
            //This method will create/recreate our database
            //This method should be called only once when
            //we want to create our database
            //TODO разобраться, что значат параметры
            new SchemaExport(config).Create(false, true);
        }

        private ISessionFactory SessionFactory
        {
            get
            {
                if (_sessionFactory == null)
                    BuildSessionFactory();

                return _sessionFactory;
            }
        }

    }
}
