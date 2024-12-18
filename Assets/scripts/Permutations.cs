using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Permutations
{
    public static IEnumerable<IEnumerable<T>> GetPermutationsWithRept<T>(IEnumerable<T> list, int length)
    {
        if (length == 1) return list.Select(t => new T[] { t });
        return GetPermutationsWithRept(list, length - 1)
            .SelectMany(t => list,
                (t1, t2) => t1.Concat(new T[] { t2 }));
    }
}
