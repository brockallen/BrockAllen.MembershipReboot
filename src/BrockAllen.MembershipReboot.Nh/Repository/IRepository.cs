namespace BrockAllen.MembershipReboot.Nh.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public interface IRepository<TEntity>
        where TEntity : class
    {
        TEntity Get<TId>(TId id);

        TEntity FindBy(Expression<Func<TEntity, bool>> expression);

        IQueryable<TEntity> FilterBy(Expression<Func<TEntity, bool>> expression);

        IQueryable<TEntity> FindAll();

        bool Save(TEntity entity);

        bool Save(IEnumerable<TEntity> entities);

        bool Update(TEntity entity);

        bool Delete(TEntity entity);

        bool Delete(IEnumerable<TEntity> entities);
    }
}