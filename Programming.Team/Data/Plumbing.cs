﻿using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Programming.Team.Core;
using Programming.Team.Data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Programming.Team.Data
{
    public class ContextFactory : IContextFactory
    {
        protected DbContextOptions<ResumesContext> Config { get; }
        protected IServiceProvider ServiceProvider { get; }
        public ContextFactory(DbContextOptions<ResumesContext> config, IServiceProvider provider)
        {
            Config = config;
            ServiceProvider = provider;
        }

        public DbContext CreateContext()
        {
            return new ResumesContext(Config);
        }
        public async Task<ClaimsPrincipal?> GetPrincipal()
        {
            var authState = ServiceProvider.GetService<AuthenticationStateProvider>();
            bool isBlazor = authState != null;
            if (authState != null)
            {
                try
                {
                    var state = await authState.GetAuthenticationStateAsync();
                    return state.User;
                }
                catch
                {
                    isBlazor = false;
                }
            }
            if (!isBlazor)
            {
                var httpContext = ServiceProvider.GetService<IHttpContextAccessor>();
                if (httpContext != null)
                {
                    return httpContext.HttpContext.User;
                }
                else
                    return Thread.CurrentPrincipal as ClaimsPrincipal;
            }
            return null;
        }
        private const string ImpersonatedUserId = nameof(ImpersonatedUserId);
        public async Task<Guid?> GetImpersonatedUser()
        {
            var httpContext = ServiceProvider.GetService<IHttpContextAccessor>();
            var strUserId = httpContext?.HttpContext.Session.GetString(ImpersonatedUserId);

            if (Guid.TryParse(strUserId, out Guid userId))
                return userId;
            return null;
        }

        public Task SetImpersonatedUser(Guid? userId)
        {
            var httpContext = ServiceProvider.GetService<IHttpContextAccessor>();
            if (httpContext != null)
            {
                if (userId == null)
                    httpContext.HttpContext.Session.Remove(ImpersonatedUserId);
                else
                    httpContext.HttpContext.Session.SetString(ImpersonatedUserId, userId.ToString());

            }
            return Task.CompletedTask;
        }
    }
    public class UnitOfWork : IUnitOfWork
    {
        public DbContext Context { get; }
        internal ResumesContext ResumesContext => (ResumesContext)Context;
        //protected TransactionScope Transaction { get; }
        public UnitOfWork(IContextFactory contextFactory)
        {
            Context = contextFactory.CreateContext();
            /*Transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = TransactionManager.MaximumTimeout
            },
            TransactionScopeAsyncFlowOption.Enabled);*/

        }
        public async Task Commit(CancellationToken token = default)
        {
            await Context.SaveChangesAsync();
            //Transaction.Complete();
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            //Transaction.Dispose();
        }

        public async Task Rollback()
        {
            await DisposeAsync();
        }
    }
    public class Repository<TEntity, TKey> : IRepository<TEntity, TKey>
       where TKey : struct
       where TEntity : Entity<TKey>, new()
    {
        public virtual IUnitOfWork CreateUnitOfWork()
        {
            return new UnitOfWork(ContextFactory);
        }
        protected IContextFactory ContextFactory { get; private set; }
        protected IMemoryCache Cache { get; }
        public Repository(IContextFactory contextFactory, IMemoryCache cache)
        {
            ContextFactory = contextFactory;
            Cache = cache;
        }
        protected async Task Use(Func<UnitOfWork, CancellationToken, Task> worker,
            IUnitOfWork? work = null, CancellationToken token = default,
            bool saveChanges = false)
        {
            UnitOfWork? uow = work as UnitOfWork;
            bool hasWork = uow != null;
            uow ??= (UnitOfWork)CreateUnitOfWork();
            try
            {
                await worker(uow, token);
            }
            finally
            {
                if (!hasWork)
                {
                    if (saveChanges)
                        await uow.Commit(token);
                    await uow.DisposeAsync();
                }
            }
        }
        public virtual Task Delete(TKey key, IUnitOfWork? work = null, CancellationToken token = default)
        {
            return Use(async (w, t) =>
            {
                TEntity? entity = await w.Context.FindAsync<TEntity>(key, token);
                if (entity != null)
                {
                    await Delete(entity, w, t);
                }

            }, work, token, true);

        }
        protected async Task PopulateBaseAttributes(TEntity entity, UnitOfWork uow, CancellationToken token)
        {
            if (entity.CreatedByUserId == null)
            {
                var e = await GetByID(entity.Id, token: token);
                if (e != null)
                {
                    entity.CreatedByUserId = e.CreatedByUserId;
                    entity.IsDeleted = e.IsDeleted;
                    entity.CreateDate = e.CreateDate;
                }
            }
        }
        public async Task<Guid?> GetCurrentUserId(IUnitOfWork? uow = null, bool fetchTrueUserId = false, CancellationToken token = default)
        {
            var userId = await ContextFactory.GetImpersonatedUser();
            if (userId == null || fetchTrueUserId)
            {
                Guid? id = null;
                var user = await ContextFactory.GetPrincipal();
                var objectId = user?.GetUserId();
                if (objectId == null)
                    return null;
                if (!Cache.TryGetValue(objectId, out id))
                {
                    await Use(async (w, t) =>
                    {
                        var work = (UnitOfWork)w;
                        if (!Cache.TryGetValue(objectId, out id))
                        {
                            var u = await work.ResumesContext.Users.AsNoTracking().SingleOrDefaultAsync(u => u.ObjectId == objectId, token);
                            id = u?.Id;
                            Cache.Set(objectId, id);
                        }
                    }, uow, token, false);
                }
                return id;
            }
            else
                return userId;

        }
        public virtual Task Delete(TEntity entity, IUnitOfWork? work = null, CancellationToken token = default)
        {
            return Use(async (w, t) =>
            {
                entity.UpdateDate = DateTime.UtcNow;
                entity.UpdatedByUserId = await GetCurrentUserId(work, true, token: token);
                entity.IsDeleted = true;
            }, work, token, true);
        }
        protected virtual async Task HydrateResultsSet(RepositoryResultSet<TKey, TEntity> results,
            IQueryable<TEntity> query,
            IUnitOfWork w,
            CancellationToken t,
            Pager? page = null,
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? properites = null)
        {
            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (properites != null)
            {
                query = properites(query);
            }
            if (page != null)
            {
                int skip = page.Value.Size * (page.Value.Page - 1);
                int take = page.Value.Size;
                results.PageSize = page.Value.Size;
                results.Page = page.Value.Page;
                results.Count = await query.CountAsync(t);
                if (orderBy != null)
                    results.Entities = await orderBy(query).Skip(skip).Take(take).ToArrayAsync(t);
                else
                    results.Entities = await query.Skip(skip).Take(take).ToArrayAsync(t);
            }
            else if (orderBy != null)
                results.Entities = await orderBy(query).ToArrayAsync(t);
            else
                results.Entities = await query.ToArrayAsync(t);
        }
        public virtual async Task<RepositoryResultSet<TKey, TEntity>> Get(IUnitOfWork? work = null,
            Pager? page = null,
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? properites = null, CancellationToken token = default)
        {
            RepositoryResultSet<TKey, TEntity> results = new RepositoryResultSet<TKey, TEntity>();
            bool hasWork = work != null;
            work ??= CreateUnitOfWork();
            try
            {
                await Use(async (w, t) =>
                {
                    IQueryable<TEntity> query = w.Context.Set<TEntity>().AsNoTracking();
                    await HydrateResultsSet(results, query, w, t, page, filter, orderBy, properites);
                }, work, token);
            }
            finally
            {
                if (!hasWork)
                    await work.DisposeAsync();
            }
            return results;
        }

        public virtual async Task<TEntity?> GetByID(TKey key, IUnitOfWork? work = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? properites = null, CancellationToken token = default)
        {
            TEntity? entity = null;
            await Use(async (w, t) =>
            {
                var query = w.Context.Set<TEntity>().AsQueryable().AsNoTracking();
                if (properites != null)
                {
                    query = properites(query);
                }
                entity = await query.SingleOrDefaultAsync(q => q.Id.Equals(key));

            }, work, token);
            return entity;
        }
        public virtual Task Add(TEntity entity, IUnitOfWork? work = null, CancellationToken token = default)
        {
            return Use(async (w, t) =>
            {
                var userId = await GetCurrentUserId(w, true, token: token);
                entity.CreateDate = DateTime.UtcNow;
                entity.UpdateDate = DateTime.UtcNow;
                entity.CreatedByUserId = userId;
                entity.UpdatedByUserId = entity.CreatedByUserId;
                await w.Context.AddAsync(entity, t);
            }, work, token, true);
        }

        public async virtual Task<TEntity> Update(TEntity entity,

            IUnitOfWork? work = null, Func<IQueryable<TEntity>, IQueryable<TEntity>>? properites = null, CancellationToken token = default)
        {
            await Use(async (w, t) =>
            {
                await PopulateBaseAttributes(entity, w, t);
                var userId = await GetCurrentUserId(w, true, token: token);
                entity.UpdateDate = DateTime.UtcNow;
                entity.UpdatedByUserId = userId;
                w.Context.Update(entity);
            }, work, token, true);
            if (properites != null)
                return await GetByID(entity.Id, properites: properites, token: token) ?? throw new InvalidDataException();
            else
                return entity;
        }

        public virtual async Task<int> Count(IUnitOfWork? work = null,
            Expression<Func<TEntity, bool>>? filter = null,
            CancellationToken token = default)
        {
            int count = 0;
            await Use(async (w, t) =>
            {
                IQueryable<TEntity> query = w.Context.Set<TEntity>();
                if (filter != null)
                    query = query.Where(filter);

                count = await query.CountAsync(t);
            }, work, token);
            return count;
        }

    }
}
