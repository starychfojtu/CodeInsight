using System;
using FuncSharp;
using NodaTime;
using static CodeInsight.Library.Prelude;

namespace CodeInsight.Jobs
{
    public sealed class JobExecution<T>
    {
        public JobExecution(Guid id, Instant createdAt, uint progress, IOption<T> result)
        {
            Id = id;
            CreatedAt = createdAt;
            Progress = progress;
            Result = result;
        }

        public Guid Id { get; }
        
        public Instant CreatedAt { get; }
        
        public uint Progress { get; }

        public bool IsFinished => Progress == 100;
        
        public IOption<T> Result { get; }
        
        public JobExecution<T> With(uint? progress = null, IOption<T> result = null) =>
            new JobExecution<T>(
                Id,
                CreatedAt,
                progress ?? Progress,
                result ?? Result
            );

        public static JobExecution<T> CreateNew() =>
            new JobExecution<T>(
                Guid.NewGuid(), 
                SystemClock.Instance.GetCurrentInstant(),
                0,
                None<T>()
            );
    }
}