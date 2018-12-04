using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FuncSharp;
using Monad;
using Try = FuncSharp.Try;

namespace CodeInsight.Library
{
    public static class Reader
    {
        public static Task<T> Async<T>(this T obj) =>
            Task.FromResult(obj);
        
        public static B Pipe<A, B>(this A obj, Func<A, B> f) => 
            f(obj);
        
        public static IEnumerable<T> ToEnumerable<T>(this T obj)
        {
            yield return obj;
        }
        
        public static async Task<B> Map<A, B>(this Task<A> task, Func<A, B> mapper)
        {
            return mapper(await task);
        }
        
        public static async Task<B> Bind<A, B>(this Task<A> task, Func<A, Task<B>> binder)
        {
            return await binder(await task);
        }
        
        public static async Task<ITry<B, E>> BindTry<A, B, E>(this Task<ITry<A, E>> task, Func<A, Task<ITry<B, E>>> binder)
        {
            var a = await task;
            return await a.Match(
                s => binder(s),
                e => Try.Error<B, E>(e).Async()
            );
        }
        
        public static Task<Unit> ToUnit<A>(this Task<A> task)
        {
            return task.Map(_ => Unit.Value);
        }
        
        public static Task<Unit> ToUnit(this Task task)
        {
            return task.ContinueWith(_ => Unit.Value);
        }
        
        public static A Execute<E, A>(this Reader<E, A> reader, E env)
        {
            return reader(env);
        }
        
        public static Reader<E, B> Bind<E, A, B>(this Reader<E, A> reader, Func<A, Reader<E, B>> binder)
        {
            return reader.SelectMany(binder, (a, b) => b);
        }

        public static Reader<E, Task<B>> Bind<E, A, B>(this Reader<E, Task<A>> reader, Func<A, Reader<E, Task<B>>> binder)
        {
            return reader.SelectMany(a => binder(a), (r1, r2) => r2);
        }
        
        public static Reader<E, Task<B>> Bind<E, A, B>(this Reader<E, Task<A>> reader, Func<A, Task<B>> binder)
        {
            return reader.Bind(a => new Reader<E, Task<B>>(env => binder(a)));
        }
        
        public static Reader<E, Task<ITry<B, TE>>> Bind<E, A, B, TE>(this Reader<E, Task<ITry<A, TE>>> reader, Func<A, Reader<E, Task<ITry<B, TE>>>> binder)
        {
            return env => reader
                .Execute(env)
                .BindTry(t => binder(t).Execute(env));
        }

        public static Reader<E, Task<C>> SelectMany<E, A, B, C>(
            this Reader<E, Task<A>> reader,
            Func<A, Reader<E, Task<B>>> binder,
            Func<A, B, C> selector)
        {
            return env =>
            {
                var first = reader(env);
                var second = first.Bind(a => binder(a)(env));
                return first.Bind(a => second.Map(b => selector(a, b)));
            };
        }
        
        public static Reader<E, Task<B>> Select<E, A, B>(this Reader<E, Task<A>> reader, Func<A, B> selector)
        {
            return env => reader(env).Map(r => selector(r));
        }
        public static Reader<E, Task<B>> Map<E, A, B>(this Reader<E, Task<A>> reader, Func<A, B> project)
        {
            return reader.Select(project);
        }

//        public static Reader<E, Task<IEnumerable<A>>> Traverse<A, E>(this IEnumerable<Reader<E, Task<A>>> readerSeq)
//        {
//            return Select(new Reader<E, IEnumerable<Task<A>>>(env => readerSeq.Select(r => r.Execute(env))), tasks => Task.WhenAll(tasks).Map(results => results as IEnumerable<A>));
//        }
    }
}