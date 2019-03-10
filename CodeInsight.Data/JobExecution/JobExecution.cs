using System;
using CodeInsight.Jobs;
using FuncSharp;
using Newtonsoft.Json;
using NodaTime.Extensions;

namespace CodeInsight.Data.JobExecution
{
    public sealed class JobExecution
    {
        public JobExecution(Guid id, DateTimeOffset createdAt, int progress, string result)
        {
            Id = id;
            CreatedAt = createdAt;
            Progress = progress;
            Result = result;
        }

        public Guid Id { get; private set; }
        
        public DateTimeOffset CreatedAt { get; private set; }
        
        public int Progress { get; private set; }
        
        public string Result { get; private set; }

        public static JobExecution FromDomain<T>(JobExecution<T> execution)
        {
            return new JobExecution(
                execution.Id,
                execution.CreatedAt.ToDateTimeOffset(),
                (int)execution.Progress,
                execution.Result.Map(r => JsonConvert.SerializeObject(r)).GetOrNull()
            );
        }
        
        public static JobExecution<T> ToDomain<T>(JobExecution execution)
        {
            return new JobExecution<T>(
                execution.Id,
                execution.CreatedAt.ToInstant(),
                (uint)execution.Progress,
                execution.Result.ToOption().Map(r => JsonConvert.DeserializeObject<T>(r))
            );
        }
    }
}