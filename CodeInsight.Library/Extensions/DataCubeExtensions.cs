using System;
using FuncSharp;

namespace CodeInsight.Library.Extensions
{
    public static class DataCubeExtensions
    {
        public static DataCube1<P, B> Map<P, A, B>(this DataCube1<P, A> cube, Func<A, B> project)
        {
            var newCube = new DataCube1<P, B>();
            cube.ForEach((p, v) => newCube.Set(p, project(v)));
            return newCube;
        }
    }
}