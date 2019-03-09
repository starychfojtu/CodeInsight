using System.Collections.Generic;

namespace CodeInsight.Domain.Common
{
    public interface IStorage<T>
    {
        void Add(IEnumerable<T> pullRequests);
    }
}