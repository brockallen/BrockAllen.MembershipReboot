namespace BrockAllen.MembershipReboot.Nh.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using global::NHibernate;
    using global::NHibernate.Linq;

    public class NhRepository<TEntity> : IRepository<TEntity>
        where TEntity : class
    {
        private readonly ISession session;

        public NhRepository(ISession session)
        {
            this.session = session;
        }

        public TEntity Get<TId>(TId id)
        {
            return this.session.Get<TEntity>(id);
        }

        public TEntity FindBy(Expression<Func<TEntity, bool>> expression)
        {
            return this.FilterBy(expression).FirstOrDefault();
        }

        public IQueryable<TEntity> FilterBy(Expression<Func<TEntity, bool>> expression)
        {
            return this.FindAll().Where(expression);
        }

        public IQueryable<TEntity> FindAll()
        {
            return this.session.Query<TEntity>();
        }

        public bool Save(TEntity entity)
        {
            this.session.SaveOrUpdate(entity);
            return true;
        }

        public bool Save(IEnumerable<TEntity> entities)
        {
            var result = false;
            foreach (var entity in entities)
            {
                result = this.Save(entity);
            }

            return result;
        }

        public bool Update(TEntity entity)
        {
            return this.Save(entity);
        }

        public bool Delete(TEntity entity)
        {
            this.session.Delete(entity);
            return true;
        }

        public bool Delete(IEnumerable<TEntity> entities)
        {
            var result = false;
            foreach (var entity in entities)
            {
                result = this.Delete(entity);
            }

            return result;
        }
    }
}