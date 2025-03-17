using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Programming.Team.Core;
using Programming.Team.Data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.Business.Core
{
    public interface IBusinessRepositoryFacade
    {
        IUnitOfWork CreateUnitOfWork();
        Task<Guid?> GetCurrentUserId(IUnitOfWork? uow = null, bool fetchTrueUserId = false, CancellationToken token = default);
    }
    public interface IIBusinessRepositoryFacade<in TEntity, TKey> : IBusinessRepositoryFacade
        where TEntity : Entity<TKey>, new()
        where TKey : struct
    {
        Task Delete(TKey id, IUnitOfWork? work = null, CancellationToken token = default);
        Task Delete(TEntity entity, IUnitOfWork? work = null, CancellationToken token = default);
        Task Add(TEntity entity, IUnitOfWork? work = null, CancellationToken token = default);

    }
    public interface IBusinessRepositoryFacade<TEntity, TKey> : IIBusinessRepositoryFacade<TEntity, TKey>
        where TEntity : Entity<TKey>, new()
        where TKey : struct
    {
        Task<TEntity> Update(TEntity entity,
            IUnitOfWork? work = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? properites = null, CancellationToken token = default);
        Task<RepositoryResultSet<TKey, TEntity>> Get(IUnitOfWork? work = null,
            Pager? page = null,
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? properites = null,
            CancellationToken token = default);
        Task<int> Count(IUnitOfWork? work = null,
            Expression<Func<TEntity, bool>>? filter = null,
            CancellationToken token = default);
        Task<TEntity?> GetByID(TKey key, IUnitOfWork? work = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? properites = null, CancellationToken token = default);

    }
}
