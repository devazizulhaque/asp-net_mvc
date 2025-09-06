using System.Linq.Expressions;

public static class IQueryableExtensions
{
    public static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> source, string propertyName, bool ascending)
    {
        if (string.IsNullOrEmpty(propertyName))
            return source;

        var parameter = Expression.Parameter(typeof(T), "x");
        Expression property = propertyName.Split('.')
            .Aggregate((Expression)parameter, Expression.PropertyOrField);

        var lambda = Expression.Lambda(property, parameter);

        string methodName = ascending ? "OrderBy" : "OrderByDescending";

        var result = typeof(Queryable).GetMethods()
            .Single(
                m => m.Name == methodName
                     && m.IsGenericMethodDefinition
                     && m.GetGenericArguments().Length == 2
                     && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), property.Type)
            .Invoke(null, new object[] { source, lambda });

        return result is not null ? (IQueryable<T>)result : source;
    }
}
