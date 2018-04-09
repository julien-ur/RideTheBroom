using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities {

    public static float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    // Source: http://answers.unity.com/answers/893984/view.html
    public static List<T> FindComponentsInChildWithTag<T>(this GameObject parent, string tag) where T : Component
    {
        Transform t = parent.transform;
        List<T> trs = new List<T>();

        foreach (Transform tr in t)
        {
            if (tr.tag == tag)
            {
                trs.Add(tr.GetComponent<T>());
            }
        }
        return (trs.Count != 0) ? trs : null;
    }

    public static IEnumerable<T[]> Permutations<T>(T[] values, int fromInd = 0)
    {
        if (fromInd + 1 == values.Length)
            yield return values;
        else
        {
            foreach (var v in Permutations(values, fromInd + 1))
                yield return v;

            for (var i = fromInd + 1; i < values.Length; i++)
            {
                SwapValues(values, fromInd, i);
                foreach (var v in Permutations(values, fromInd + 1))
                    yield return v;
                SwapValues(values, fromInd, i);
            }
        }
    }

    private static void SwapValues<T>(T[] values, int pos1, int pos2)
    {
        if (pos1 != pos2)
        {
            T tmp = values[pos1];
            values[pos1] = values[pos2];
            values[pos2] = tmp;
        }
    }
}