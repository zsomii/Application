#region

using System.Collections.Generic;
using System.Linq;
using Application.Data.Entity;
using Microsoft.EntityFrameworkCore;

#endregion

namespace Application.Data.Repository
{
    public interface IBaseRepository<T> where T : BaseEntity
    {
        IQueryable<T> GetList();
        T CreateOrUpdateEntity(T entity);
        List<T> CreateOrUpdateEntities(List<T> entities);
        void DeleteEntity(string id);
        void DetachEntity<E>(BaseEntity entity) where E : BaseEntity;
        void DetachEntities<E>(IEnumerable<BaseEntity> entities) where E : BaseEntity;
        void DetachEntities(T parentEntity);
    }

    public abstract class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
    {
        protected readonly AppDbContext Context;

        protected BaseRepository(AppDbContext context)
        {
            Context = context;
        }

        public virtual IQueryable<T> GetList()
        {
            return GetEntityTable();
        }

        public virtual T CreateOrUpdateEntity(T entity)
        {
            SaveAnnotatedGraph(entity);
            var _ = Context.SaveChangesAsync().Result;
            return entity;
        }

        public virtual List<T> CreateOrUpdateEntities(List<T> entities)
        {
            foreach (var item in entities)
            {
                SaveAnnotatedGraph(item);
            }

            var _ = Context.SaveChangesAsync().Result;
            return entities;
        }

        public virtual void DeleteEntity(string id)
        {
            var entity = GetEntityTable().FirstOrDefault(x => x.Id == id);
            if (entity == null)
            {
                return;
            }

            Context.Entry(entity).State = EntityState.Deleted;
            var _ = Context.SaveChangesAsync().Result;
        }

        public virtual void DetachEntities(T parentEntity)
        {
        }

        public virtual void DetachEntities<E>(IEnumerable<BaseEntity> entities) where E : BaseEntity
        {
            if (entities == null)
            {
                return;
            }

            foreach (var entry in entities.ToList())
            {
                DetachEntity<E>(entry);
            }
        }

        public virtual void DetachEntity<E>(BaseEntity entity) where E : BaseEntity
        {
            if (entity != null && entity.Id != null)
            {
                var _entity = Context.Set<E>().FirstOrDefault(it => it.Id == entity.Id);
                Detach(_entity);
            }
        }

        private void Detach(BaseEntity entity)
        {
            if (entity == null)
            {
                return;
            }

            Context.Entry(entity).State = EntityState.Detached;
        }

        protected void SaveAnnotatedGraph(object rootEntity)
        {
            Context.ChangeTracker.TrackGraph(rootEntity,
                n => { n.Entry.State = n.Entry.IsKeySet ? EntityState.Modified : EntityState.Added; });
        }

        protected abstract DbSet<T> GetEntityTable();
    }
}