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
    /// <summary>
    /// Facade for business repository
    /// </summary>
    public interface IBusinessRepositoryFacade
    {
        /// <summary>
        /// Create a new unit of work
        /// </summary>
        /// <returns>Unit of Work for the given repository</returns>
        IUnitOfWork CreateUnitOfWork();
        /// <summary>
        /// Get the current user ID
        /// </summary>
        /// <param name="uow">Optional Unit of Work</param>
        /// <param name="fetchTrueUserId">If true return actual and not impersonated User.Id</param>
        /// <param name="token">Optional Cancellation Token</param>
        /// <returns>The current userId, if found</returns>
        Task<Guid?> GetCurrentUserId(IUnitOfWork? uow = null, bool fetchTrueUserId = false, CancellationToken token = default);
    }
    /// <summary>
    /// Contravariant Facade for working with storage for entities
    /// </summary>
    /// <typeparam name="TEntity">Target Entity</typeparam>
    /// <typeparam name="TKey">Target Key for an Entity</typeparam>
    public interface IIBusinessRepositoryFacade<in TEntity, TKey> : IBusinessRepositoryFacade
        where TEntity : Entity<TKey>, new()
        where TKey : struct
    {
        /// <summary>
        /// Delete an entity by key
        /// </summary>
        /// <param name="id">Target Entity.Id</param>
        /// <param name="work">Optional Unit of Work</param>
        /// <param name="token">Optional Cancellation Token</param>
        /// <returns>Task representing the work of deleting</returns>
        Task Delete(TKey id, IUnitOfWork? work = null, CancellationToken token = default);
        /// <summary>
        /// Delete an entity
        /// </summary>
        /// <param name="entity">Target Entity</param>
        /// <param name="work">Optional Unit of Work</param>
        /// <param name="token">Optional Cancellation Token</param>
        /// <returns>Task representing the work of deleting</returns>
        Task Delete(TEntity entity, IUnitOfWork? work = null, CancellationToken token = default);
        /// <summary>
        /// Add an entity
        /// </summary>
        /// <param name="entity">Entity to add</param>
        /// <param name="work">Optional unit of work</param>
        /// <param name="token">Optional Cancellation Token</param>
        /// <returns>Task representing the work of adding</returns>
        Task Add(TEntity entity, IUnitOfWork? work = null, CancellationToken token = default);

    }
    /// <summary>
    /// Facade for working with entities
    /// </summary>
    /// <typeparam name="TEntity">Target Entity</typeparam>
    /// <typeparam name="TKey">Target Entity.Id type</typeparam>
    public interface IBusinessRepositoryFacade<TEntity, TKey> : IIBusinessRepositoryFacade<TEntity, TKey>
        where TEntity : Entity<TKey>, new()
        where TKey : struct
    {
        /// <summary>
        /// Update an entity
        /// </summary>
        /// <param name="entity">Entity to Update</param>
        /// <param name="work">Optional Unit of Work</param>
        /// <param name="properites">Optional properties to include in return</param>
        /// <param name="token">Optional Cancellation Token</param>
        /// <returns>The updated Entity</returns>
        Task<TEntity> Update(TEntity entity,
            IUnitOfWork? work = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? properites = null, CancellationToken token = default);
        /// <summary>
        /// Get a set of entities
        /// </summary>
        /// <param name="work">Optional Unit of Work</param>
        /// <param name="page">Optional Paging Information</param>
        /// <param name="filter">Optional Filter Expression</param>
        /// <param name="orderBy">Optional Order By</param>
        /// <param name="properites">Optional Property includes</param>
        /// <param name="token">Optional Cancellation Token</param>
        /// <returns>A RepositoryResultSet for the query</returns>
        Task<RepositoryResultSet<TKey, TEntity>> Get(IUnitOfWork? work = null,
            Pager? page = null,
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? properites = null,
            CancellationToken token = default);
        /// <summary>
        /// Count the number of entities
        /// </summary>
        /// <param name="work">Optional Unit of Work</param>
        /// <param name="filter">Optional Filter Expression</param>
        /// <param name="token">Optional Cancellation Token</param>
        /// <returns>The count of the query</returns>
        Task<int> Count(IUnitOfWork? work = null,
            Expression<Func<TEntity, bool>>? filter = null,
            CancellationToken token = default);
        /// <summary>
        /// Get an entity by key
        /// </summary>
        /// <param name="key">Target Entity.Id</param>
        /// <param name="work">Optional Unit of Work</param>
        /// <param name="properites">Optional properties to include</param>
        /// <param name="token">Optional Cancellation Token</param>
        /// <returns>The entity, if found</returns>
        Task<TEntity?> GetByID(TKey key, IUnitOfWork? work = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? properites = null, CancellationToken token = default);

    }
}
