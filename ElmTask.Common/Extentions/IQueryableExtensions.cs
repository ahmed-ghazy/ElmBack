﻿using ElmTask.Common.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


namespace ElmTask.Common.Extentions
{
    public static class IQueryableExtensions
    {
        public static async Task<QueryResult<T>> ToQueryResult<T>(this IQueryable<T> dbQuery, int pageNumber = 1, int pageSize = 10, bool isCountOnly = false, string sort = "", bool descending = true) where T : class
        {
            if (dbQuery == null)
                throw new ArgumentNullException("dbQuery");
            int count = await dbQuery.CountAsync();

        

            if (sort != "")
                dbQuery = dbQuery.SortBy(sort, descending).Page(pageNumber, pageSize);
            else
                dbQuery = dbQuery.Page(pageNumber, pageSize);
            var data = isCountOnly ? null : await dbQuery.ToListAsync();
            pageNumber = (int)Math.Ceiling((decimal)count / (decimal)pageSize);

            return new QueryResult<T>(data, count, pageNumber, pageSize);
        }
        public static IQueryable<TSource> Page<TSource>(this IQueryable<TSource> source, int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1)
                pageNumber = 1;
            if (pageSize <= 0)
                return source;
            return source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }
        public static IQueryable<T> SortBy<T>(this IQueryable<T> source, string propertyName, bool descending)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                return source;
            var type = typeof(T);
            var property = type.GetProperty(propertyName);
            if (property == null)
                throw new ArgumentException("Sort property not valid.");
            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExp = Expression.Lambda(propertyAccess, parameter);
            var methodName = "OrderBy";
            if (descending)
                methodName = "OrderByDescending";
            var resultExp = Expression.Call(typeof(Queryable), methodName, new Type[] { type, property.PropertyType }, source.Expression, Expression.Quote(orderByExp));
            return source.Provider.CreateQuery<T>(resultExp);
        }
        public static IQueryable<TSource> WhereIf<TSource>(this IQueryable<TSource> source, bool condition, Expression<Func<TSource, bool>> predicate)
        {
            if (condition)
                return source.Where(predicate);
            else
                return source;
        }
        public static IQueryable<TSource> WhereIfAndInculde<TSource>(this IQueryable<TSource> source, bool condition,
            Expression<Func<TSource, bool>> predicate, Expression<Func<TSource, object>> include)
            where TSource : class
        {
            if (condition)
                return source.Where(predicate).Include(include); 
            else
                return source;
        }

    }
}
