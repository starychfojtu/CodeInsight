using System.Collections.Generic;
using CodeInsight.Library.Extensions;
using FuncSharp;

namespace CodeInsight.Domain.Common
{
    public interface IStorage<T>
    {
        Unit Add(IEnumerable<T> entities);
    }
    
    public static class IStorageExtensions
    {
        public static Unit Add<T>(this IStorage<T> storage, T entity) =>
            storage.Add(entity.ToEnumerable());
    }
}