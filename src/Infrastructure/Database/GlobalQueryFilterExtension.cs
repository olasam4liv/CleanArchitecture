using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Query;

namespace Infrastructure.Database;

internal static class GlobalQueryFilterExtension
{
    public static ModelBuilder AppendGlobalQueryFilter<TInterface>(this ModelBuilder modelBuilder, Expression<Func<TInterface, bool>> filter)
    {
        // get a list of entities without a baseType that implement the interface TInterface
        IEnumerable<Type> entities = modelBuilder.Model.GetEntityTypes()
            .Where(e => e.BaseType is null && e.ClrType.GetInterface(typeof(TInterface).Name) is not null)
            .Select(e => e.ClrType);

        foreach (Type entity in entities)
        {
            EntityTypeBuilder entityTypeBuilder = modelBuilder.Entity(entity);
            ParameterExpression parameterType = Expression.Parameter(entityTypeBuilder.Metadata.ClrType);
            Expression filterBody = ReplacingExpressionVisitor.Replace(filter.Parameters.Single(), parameterType, filter.Body);

            // Use the obsolete GetQueryFilter() method as it's the simplest way to access the filter expression
            // The new GetDeclaredQueryFilters() returns IQueryFilter which doesn't expose the expression publicly
#pragma warning disable CS0618 // Type or member is obsolete
            LambdaExpression? existingFilter = entityTypeBuilder.Metadata.GetQueryFilter();
#pragma warning restore CS0618 // Type or member is obsolete
            
            if (existingFilter is not null)
            {
                Expression existingFilterBody = ReplacingExpressionVisitor.Replace(
                    existingFilter.Parameters.Single(), 
                    parameterType, 
                    existingFilter.Body);

                // combine the existing query filter with the new query filter
                filterBody = Expression.AndAlso(existingFilterBody, filterBody);
            }

            // apply the new query filter
            entityTypeBuilder.HasQueryFilter(Expression.Lambda(filterBody, parameterType));
        }

        return modelBuilder;
    }
}
