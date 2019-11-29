using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.ExtensionClasses
{
    public static class ArrayExtensions
    {
        public static IEnumerable<IEnumerable<T>> Split<T>(this T[] self, int size)
        {
            for(int i = 0; i < self.Length / size; i++)
            {
                yield return self.Skip(i * size).Take(size);
            }
        }
    }
}
