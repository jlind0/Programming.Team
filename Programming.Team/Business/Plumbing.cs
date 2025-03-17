using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using Programming.Team.Data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.Business
{
    public abstract class BusinessRepositoryFacade : IBusinessRepositoryFacade
    {
        protected IRepository RepositoryRaw { get; }
        protected ILogger Logger { get; }
        public string? AccessCode { get; set; }

        protected BusinessRepositoryFacade(IRepository repository, ILogger logger)
        {
            RepositoryRaw = repository;
            Logger = logger;
        }

        public virtual IUnitOfWork CreateUnitOfWork()
        {
            return RepositoryRaw.CreateUnitOfWork();
        }

        public virtual Task<Guid?> GetCurrentUserId(IUnitOfWork? uow = null, bool fetchTrueUserId = false, CancellationToken token = default)
        {
            return RepositoryRaw.GetCurrentUserId(uow, fetchTrueUserId, token: token);
        }
    }
    public abstract class BusinessRepositoryFacade<TEntity, TKey> : BusinessRepositoryFacade,
        IBusinessRepositoryFacade<TEntity, TKey>
        where TEntity : Entity<TKey>, new()
        where TKey: struct
    {
        protected IRepository<TEntity, TKey> RepositoryDefault
        {
            get => (IRepository<TEntity, TKey>)RepositoryRaw;
        }
        protected BusinessRepositoryFacade(IRepository<TEntity, TKey> repository,
            ILogger<TEntity> logger)
            : base(repository, logger)
        {

        }


        public virtual async Task Delete(TKey id, IUnitOfWork? work = null, CancellationToken token = default)
        {
            try
            {
                await this.RepositoryDefault.Delete(id, work, token);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
        }

        public virtual async Task Delete(TEntity entity, IUnitOfWork? work = null, CancellationToken token = default)
        {
            try
            {
                await this.RepositoryDefault.Delete(entity, work, token);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
        }

        public virtual async Task Add(TEntity entity, IUnitOfWork? work = null, CancellationToken token = default)
        {
            try
            {
                await RepositoryDefault.Add(entity, work, token);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
        }
        public virtual Task<int> Count(IUnitOfWork? work = null, Expression<Func<TEntity, bool>>? filter = null, CancellationToken token = default)
        {
            return RepositoryDefault.Count(work, filter, token);
        }

        public virtual Task<RepositoryResultSet<TKey, TEntity>> Get(IUnitOfWork? work = null, Pager? page = null,
            Expression<Func<TEntity, bool>>? filter = null, Func<IQueryable<TEntity>,
            IOrderedQueryable<TEntity>>? orderBy = null, Func<IQueryable<TEntity>, IQueryable<TEntity>>? properites = null, CancellationToken token = default)
        {
            return RepositoryDefault.Get(work, page, filter, orderBy, properites, token);
        }

        public virtual Task<TEntity?> GetByID(TKey key, IUnitOfWork? work = null, Func<IQueryable<TEntity>, IQueryable<TEntity>>? properites = null, CancellationToken token = default)
        {
            return RepositoryDefault.GetByID(key, work, properites, token);
        }
        public virtual async Task<TEntity> Update(TEntity entity,
            IUnitOfWork? work = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? properites = null, CancellationToken token = default)
        {
            try
            {
                return await this.RepositoryDefault.Update(entity, work, properites, token);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
    public class BusinessRepositoryFacade<TEntity, TKey, TRepository> : BusinessRepositoryFacade<TEntity, TKey>, IBusinessRepositoryFacade<TEntity, TKey>
        where TEntity : Entity<TKey>, new()
        where TKey: struct
        where TRepository : IRepository<TEntity, TKey>
    {
        protected TRepository Repository { get => (TRepository)RepositoryRaw; }
        public BusinessRepositoryFacade(TRepository repository, ILogger<TEntity> logger) :
            base(repository, logger)
        {
        }

       
    }
}
