using System.Collections.Generic;

namespace CodeInsight.Github
{
    public sealed class ResponsePage<T>
    {
        public ResponsePage(bool hasNextPage, string endCursor, IReadOnlyList<T> items)
        {
            HasNextPage = hasNextPage;
            EndCursor = endCursor;
            Items = items;
        }

        public bool HasNextPage { get; }
        
        public string EndCursor { get; }
        
        public IReadOnlyList<T> Items { get; }
    }
}