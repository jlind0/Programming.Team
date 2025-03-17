using Microsoft.AspNetCore.Components;
using MudBlazor;
using Programming.Team.Core;
using Programming.Team.ViewModels;
using ReactiveUI;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Windows.Input;

namespace Programming.Team.Web.Helpers
{
    public static class Helpers
    {
        private static Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> BuildSort<TEntity>(this ICollection<SortDefinition<TEntity>> sorts)
        {
            if (sorts == null || sorts.Count == 0)
            {
                throw new ArgumentException("No sorting definitions provided.");
            }

            return query =>
            {
                IOrderedQueryable<TEntity>? orderedQuery = null;

                foreach (var sort in sorts)
                {
                    var parameter = Expression.Parameter(typeof(TEntity), "entity");
                    var property = Expression.Property(parameter, sort.SortBy);
                    var lambda = Expression.Lambda(property, parameter);

                    var methodName = orderedQuery == null
                        ? (sort.Descending ? "OrderByDescending" : "OrderBy")
                        : (sort.Descending ? "ThenByDescending" : "ThenBy");

                    var method = typeof(Queryable).GetMethods()
                        .First(m => m.Name == methodName && m.GetParameters().Length == 2);

                    var genericMethod = method.MakeGenericMethod(typeof(TEntity), property.Type);
                    if (genericMethod == null)
                        continue;
                    orderedQuery = genericMethod.Invoke(null, new object[] { orderedQuery ?? query, lambda }) as IOrderedQueryable<TEntity>;
                }

                return orderedQuery!;
            };
        }
        private static Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> BuildSort<TKey, TEntity, TViewModel>(this ICollection<SortDefinition<TViewModel>> sorts)
            where TKey : struct
            where TEntity : Entity<TKey>, new()
            where TViewModel : IEntityViewModel<TKey, TEntity>
        {
            if (sorts == null || sorts.Count == 0)
            {
                throw new ArgumentException("No sorting definitions provided.");
            }

            return query =>
            {
                IOrderedQueryable<TEntity>? orderedQuery = null;

                foreach (var sort in sorts)
                {
                    var parameter = Expression.Parameter(typeof(TEntity), "entity");
                    var property = Expression.Property(parameter, sort.SortBy);
                    var lambda = Expression.Lambda(property, parameter);

                    var methodName = orderedQuery == null
                        ? (sort.Descending ? "OrderByDescending" : "OrderBy")
                        : (sort.Descending ? "ThenByDescending" : "ThenBy");

                    var method = typeof(Queryable).GetMethods()
                        .First(m => m.Name == methodName && m.GetParameters().Length == 2);

                    var genericMethod = method.MakeGenericMethod(typeof(TEntity), property.Type);
                    if (genericMethod == null)
                        continue;
                    orderedQuery = genericMethod.Invoke(null, new object[] { orderedQuery ?? query, lambda }) as IOrderedQueryable<TEntity>;
                }

                return orderedQuery!;
            };
        }
        private static Expression<Func<TEntity, bool>>? BuildFilterExpression<TEntity>(this ICollection<IFilterDefinition<TEntity>> filters)
        {
            if (filters == null || filters.Count == 0)
                return null; // No filters, return always true

            // Parameter expression for the entity
            var parameter = Expression.Parameter(typeof(TEntity), "entity");

            // Start with an empty expression
            Expression? finalExpression = null;

            // Iterate over each filter to build individual expressions
            foreach (var filter in filters)
            {
                if (filter.Column == null)
                    continue;
                var property = Expression.Property(parameter, filter.Column.PropertyName);
                var constant = Expression.Constant(filter.Value);

                // Create the binary expression based on the filter operator
                Expression? filterExpression = filter.Operator switch
                {
                    FilterOperator.String.Equal or
                    FilterOperator.Number.Equal or
                    FilterOperator.DateTime.Is or
                    FilterOperator.Enum.Is or
                    FilterOperator.Boolean.Is
                        => Expression.Equal(property, constant),

                    FilterOperator.String.NotEqual or
                    FilterOperator.Number.NotEqual or
                    FilterOperator.DateTime.IsNot or
                    FilterOperator.Enum.IsNot
                        => Expression.NotEqual(property, constant),

                    FilterOperator.Number.GreaterThan or
                    FilterOperator.DateTime.After
                        => Expression.GreaterThan(property, constant),

                    FilterOperator.Number.GreaterThanOrEqual or
                    FilterOperator.DateTime.OnOrAfter
                        => Expression.GreaterThanOrEqual(property, constant),

                    FilterOperator.Number.LessThan or
                    FilterOperator.DateTime.Before
                        => Expression.LessThan(property, constant),

                    FilterOperator.Number.LessThanOrEqual or
                    FilterOperator.DateTime.OnOrBefore
                        => Expression.LessThanOrEqual(property, constant),

                    FilterOperator.String.Contains
                        => BuildContainsExpression(property, filter.Value),

                    FilterOperator.String.StartsWith
                        => BuildStartsWithExpression(property, filter.Value),

                    FilterOperator.String.EndsWith
                        => BuildEndsWithExpression(property, filter.Value),

                    FilterOperator.String.Empty or
                    FilterOperator.Number.Empty or
                    FilterOperator.DateTime.Empty
                        => Expression.Equal(property, Expression.Constant(null)),

                    FilterOperator.String.NotEmpty or
                    FilterOperator.Number.NotEmpty or
                    FilterOperator.DateTime.NotEmpty
                        => Expression.NotEqual(property, Expression.Constant(null)),

                    _ => throw new NotSupportedException($"Operator '{filter.Operator}' is not supported")
                };


                if (finalExpression == null)
                {
                    finalExpression = filterExpression;
                }
                else
                {
                    finalExpression = Expression.AndAlso(finalExpression, filterExpression);
                }
            }

            // Convert the final expression into a lambda expression
            return Expression.Lambda<Func<TEntity, bool>>(finalExpression ?? Expression.Constant(true), parameter);
        }
        private static Expression<Func<TEntity, bool>>? BuildFilterExpression<TKey, TEntity, TViewModel>(this ICollection<IFilterDefinition<TViewModel>> filters)
            where TKey : struct
            where TEntity : Entity<TKey>, new()
            where TViewModel : IEntityViewModel<TKey, TEntity>
        {
            if (filters == null || filters.Count == 0)
                return null; // No filters, return always true

            // Parameter expression for the entity
            var parameter = Expression.Parameter(typeof(TEntity), "entity");

            // Start with an empty expression
            Expression? finalExpression = null;

            // Iterate over each filter to build individual expressions
            foreach (var filter in filters)
            {
                if (filter.Column == null)
                    continue;
                var property = Expression.Property(parameter, filter.Column.PropertyName);
                var constant = Expression.Constant(filter.Value);

                // Create the binary expression based on the filter operator
                Expression? filterExpression = filter.Operator switch
                {
                    FilterOperator.String.Equal or
                    FilterOperator.Number.Equal or
                    FilterOperator.DateTime.Is or
                    FilterOperator.Enum.Is or
                    FilterOperator.Boolean.Is
                        => Expression.Equal(property, constant),

                    FilterOperator.String.NotEqual or
                    FilterOperator.Number.NotEqual or
                    FilterOperator.DateTime.IsNot or
                    FilterOperator.Enum.IsNot
                        => Expression.NotEqual(property, constant),

                    FilterOperator.Number.GreaterThan or
                    FilterOperator.DateTime.After
                        => Expression.GreaterThan(property, constant),

                    FilterOperator.Number.GreaterThanOrEqual or
                    FilterOperator.DateTime.OnOrAfter
                        => Expression.GreaterThanOrEqual(property, constant),

                    FilterOperator.Number.LessThan or
                    FilterOperator.DateTime.Before
                        => Expression.LessThan(property, constant),

                    FilterOperator.Number.LessThanOrEqual or
                    FilterOperator.DateTime.OnOrBefore
                        => Expression.LessThanOrEqual(property, constant),

                    FilterOperator.String.Contains
                        => BuildContainsExpression(property, filter.Value),

                    FilterOperator.String.StartsWith
                        => BuildStartsWithExpression(property, filter.Value),

                    FilterOperator.String.EndsWith
                        => BuildEndsWithExpression(property, filter.Value),

                    FilterOperator.String.Empty or
                    FilterOperator.Number.Empty or
                    FilterOperator.DateTime.Empty
                        => Expression.Equal(property, Expression.Constant(null)),

                    FilterOperator.String.NotEmpty or
                    FilterOperator.Number.NotEmpty or
                    FilterOperator.DateTime.NotEmpty
                        => Expression.NotEqual(property, Expression.Constant(null)),

                    _ => throw new NotSupportedException($"Operator '{filter.Operator}' is not supported")
                };


                if (finalExpression == null)
                {
                    finalExpression = filterExpression;
                }
                else
                {
                    finalExpression = Expression.AndAlso(finalExpression, filterExpression);
                }
            }

            // Convert the final expression into a lambda expression
            return Expression.Lambda<Func<TEntity, bool>>(finalExpression ?? Expression.Constant(true), parameter);
        }
        private static Expression BuildStartsWithExpression(Expression property, object? value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (property.Type != typeof(string))
                throw new NotSupportedException("StartsWith operator is only supported for string properties.");

            var method = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
            return Expression.Call(property, method!, Expression.Constant(value));
        }

        private static Expression BuildEndsWithExpression(Expression property, object? value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (property.Type != typeof(string))
                throw new NotSupportedException("EndsWith operator is only supported for string properties.");

            var method = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
            return Expression.Call(property, method!, Expression.Constant(value));
        }
        private static Expression BuildContainsExpression(Expression property, object? value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            return Expression.Call(property, method!, Expression.Constant(value));
        }
        public static Func<GridState<TViewModel>, Task<GridData<TViewModel>>> BindServerDataCommand<TKey, TEntity, TViewModel>
     (this ReactiveCommand<DataGridRequest<TKey, TEntity>, ViewModelResult<TKey, TEntity, TViewModel>?> command)
     where TKey : struct
     where TEntity : Entity<TKey>, new()
     where TViewModel : IEntityViewModel<TKey, TEntity>
        {
            return async state =>
            {
                var request = CreateDataGridRequest<TKey, TEntity, TViewModel>(state);

                var response = await command.Execute(request).GetAwaiter();
                if (response is null)
                    return new GridData<TViewModel>();

                return new GridData<TViewModel>
                {
                    Items = response.Entities.ToArray(),
                    TotalItems = response.Count ?? 0
                };
            };
        }

        public static Func<GridState<TEntity>, Task<GridData<TEntity>>> BindServerDataCommand<TKey, TEntity>
            (this ReactiveCommand<DataGridRequest<TKey, TEntity>, RepositoryResultSet<TKey, TEntity>?> command)
            where TKey : struct
            where TEntity : Entity<TKey>, new()
        {
            return async state =>
            {
                var request = CreateDataGridRequest<TKey, TEntity>(state);

                var response = await command.Execute(request).GetAwaiter();
                if (response is null)
                    return new GridData<TEntity>();

                return new GridData<TEntity>
                {
                    Items = response.Entities.ToArray(),
                    TotalItems = response.Count ?? 0
                };
            };
        }

        private static DataGridRequest<TKey, TEntity> CreateDataGridRequest<TKey, TEntity>(GridState<TEntity> state)
            where TKey : struct
            where TEntity : Entity<TKey>, new()
        {
            var request = new DataGridRequest<TKey, TEntity>();

            if (state.FilterDefinitions.Any())
            {
                request.Filter = state.FilterDefinitions.BuildFilterExpression();
            }

            if (state.SortDefinitions.Any())
            {
                request.OrderBy = state.SortDefinitions.BuildSort();
            }

            if (state.PageSize > 0)
            {
                request.Pager = new Pager { Page = state.Page + 1, Size = state.PageSize };
            }

            return request;
        }
        private static DataGridRequest<TKey, TEntity> CreateDataGridRequest<TKey, TEntity, TViewModel>(GridState<TViewModel> state)
            where TKey : struct
            where TEntity : Entity<TKey>, new()
            where TViewModel : IEntityViewModel<TKey, TEntity>
        {
            var request = new DataGridRequest<TKey, TEntity>();

            if (state.FilterDefinitions.Any())
            {
                request.Filter = state.FilterDefinitions.BuildFilterExpression<TKey, TEntity, TViewModel>();
            }

            if (state.SortDefinitions.Any())
            {
                request.OrderBy = state.SortDefinitions.BuildSort<TKey, TEntity, TViewModel>();
            }

            if (state.PageSize > 0)
            {
                request.Pager = new Pager { Page = state.Page + 1, Size = state.PageSize };
            }

            return request;
        }
        public static Func<string, CancellationToken, Task<IEnumerable<T>>> BindCommand<T>(this ReactiveCommand<string?, IEnumerable<T>> command)
        {
            return async (str, token) => await command.Execute(str).GetAwaiter();
        }
        public static EventCallback<T> BindCommand<T>(this ICommand command, object? parameter = null)
        {
            MulticastDelegate m1 = () => command.Execute(parameter);
            return new EventCallback<T>(null, m1);
        }
    }

}