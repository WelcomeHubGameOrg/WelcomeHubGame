using System.Collections.Generic;
using System.Linq;

namespace HikingMinigame.Editor
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T> source) => source ?? Enumerable.Empty<T>();
    }
}
