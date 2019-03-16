using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FuncSharp;
using Monad;
using Try = FuncSharp.Try;

namespace CodeInsight.Library.Extensions
{
    public static class MonadExtension
    {
        // General
        
        public static B Pipe<A, B>(this A obj, Func<A, B> f) => 
            f(obj);
        
        // Enumerable
        
        public static IEnumerable<T> ToEnumerable<T>(this T obj)
        {
            yield return obj;
        }
        
        // Try
        
        public static ITry<A, E> ToSuccess<A, E>(this A obj) =>
            Try.Success<A, E>(obj);
        
        public static B MatchSingle<A, B>(this ITry<A> iTry, Func<A, B> ifSuccess, Func<Exception, B> ifError)
        {
            return iTry.Match(
                ifSuccess,
                e => ifError(e.Single())
            );
        }
        
        // Task
        
        public static Task<T> Async<T>(this T obj) =>
            Task.FromResult(obj);
        
        public static Task<B> Async<A, B>(this A obj) where A : B =>
            Task.FromResult((B)obj);
        
        public static Task<Unit> ToUnit<A>(this Task<A> task) =>
            task.Map(_ => Unit.Value);
        
        public static Task<Unit> ToUnit(this Task task) =>
            task.ContinueWith(_ => Unit.Value);
        
        public static Task<B> Map<A, B>(this Task<A> task, Func<A, B> mapper) =>
            task.ContinueWith(r => mapper(r.Result));
        
        public static Task<B> SafeMap<A, B>(this Task<A> task, Func<ITry<A>, B> mapper) =>
            task.ContinueWith(r => mapper(r.IsFaulted ? Try.Error<A>(r.Exception) : Try.Success(r.Result)));
        
        public static Task<B> Bind<A, B>(this Task<A> task, Func<A, Task<B>> binder) =>
            task.ContinueWith(t => binder(t.Result)).Unwrap();
        
        // Task Try transformer
        
        public static async Task<ITry<B>> Bind<A, B>(this Task<ITry<A>> task, Func<A, Task<ITry<B>>> binder)
        {
            var a = await task;
            return await a.Match(
                s => binder(s),
                e => Try.Error<B>(e).Async()
            );
        }
        
        public static async Task<ITry<B, E>> Bind<A, B, E>(this Task<ITry<A, E>> task, Func<A, Task<ITry<B, E>>> binder)
        {
            var a = await task;
            return await a.Match(
                s => binder(s),
                e => Try.Error<B, E>(e).Async()
            );
        }
        
        public static A Execute<E, A>(this Reader<E, A> reader, E env)
        {
            return reader(env);
        }
        
        // Reader
        
        public static Reader<E, A> ToReader<E, A>(this A obj) =>
            _ => obj;
        
        public static Reader<E, B> Bind<E, A, B>(this Reader<E, A> reader, Func<A, Reader<E, B>> binder)
        {
            return reader.SelectMany(binder, (a, b) => b);
        }
        
        // Reader Task transformer

        public static Reader<E, Task<B>> Bind<E, A, B>(this Reader<E, Task<A>> reader, Func<A, Reader<E, Task<B>>> binder)
        {
            return reader.SelectMany(a => binder(a), (r1, r2) => r2);
        }
        
        public static Reader<E, Task<B>> Bind<E, A, B>(this Reader<E, Task<A>> reader, Func<A, Task<B>> binder)
        {
            return reader.Bind(a => new Reader<E, Task<B>>(env => binder(a)));
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
        
        public static Reader<E, Task<B>> Map<E, A, B>(this Reader<E, Task<A>> reader, Func<A, B> project) =>
            reader.Select(project);
        
        // Reader Task Try transformer
        
        public static Reader<E, Task<ITry<B, TE>>> Bind<E, A, B, TE>(this Reader<E, Task<ITry<A, TE>>> reader, Func<A, Reader<E, Task<ITry<B, TE>>>> binder)
        {
            return env => reader
                .Execute(env)
                .Bind(t => binder(t).Execute(env));
        }
        
        // IO
        
        public static A Execute<A>(this IO<A> io) =>
            io();
        
        public static IO<B> Map<A, B>(this IO<A> io, Func<A, B> project) =>
            io.Select(project);
        
        // IO Task Transformer
        
        public static IO<Task<B>> Map<A, B>(this IO<Task<A>> ioT, Func<A, B> project) =>
            ioT.Map(t => t.Map(project));
        
        public static IO<Task<B>> Select<A, B>(this IO<Task<A>> ioT, Func<A, B> project) =>
            ioT.Map(project);
        
        public static IO<Task<B>> Bind<A, B>(this IO<Task<A>> ioT, Func<A, IO<Task<B>>> binder)
        {
            return () => ioT.Execute().Bind(t => binder(t).Execute());
        }
        
        public static IO<Task<C>> SelectMany<A, B, C>(
            this IO<Task<A>> ioT,
            Func<A, IO<Task<B>>> binder,
            Func<A, B, C> project)
        {
            return ioT.Bind(a => binder(a).Map(b => project(a, b)));
        }
        
        // IO Task Try Transformer
        
        public static IO<Task<ITry<B, E>>> Map<E, A, B>(this IO<Task<ITry<A, E>>> ioT, Func<A, B> project) =>
            ioT.Map(t => t.Map(tr => tr.Map(project)));
        
        public static IO<Task<ITry<B, E>>> Select<E, A, B>(this IO<Task<ITry<A, E>>> ioT, Func<A, B> project) =>
            ioT.Map(project);
        
        public static IO<Task<ITry<B, E>>> Bind<E, A, B>(this IO<Task<ITry<A, E>>> ioT, Func<A, IO<Task<ITry<B, E>>>> binder)
        {
            return () => ioT.Execute().Bind(t => t.Match(
                r => binder(r)(),
                e => Try.Error<B, E>(e).Async()
            ));
        }
        
        public static IO<Task<ITry<C, E>>> SelectMany<E, A, B, C>(
            this IO<Task<ITry<A, E>>> ioT,
            Func<A, IO<Task<ITry<B, E>>>> binder,
            Func<A, B, C> project)
        {
            return ioT.Bind(a => binder(a).Map(b => project(a, b)));
        }
    }
}