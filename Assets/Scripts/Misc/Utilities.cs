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
    // Source: http://euanfreeman.co.uk/balanced-latin-squares/
    public static int[,] GetLatinSquare(int n)
    {
        // 1. Create initial square.
        int[,] latinSquare = new int[n, n];

        // 2. Initialise first row.
        latinSquare[0, 0] = 1;
        latinSquare[0, 1] = 2;

        for (int i = 2, j = 3, k = 0; i < n; i++)
        {
            if (i % 2 == 1)
                latinSquare[0, i] = j++;
            else
                latinSquare[0, i] = n - (k++);
        }

        // 3. Initialise first column.
        for (int i = 1; i <= n; i++)
        {
            latinSquare[i - 1, 0] = i;
        }

        // 4. Fill in the rest of the square.
        for (int row = 1; row < n; row++)
        {
            for (int col = 1; col < n; col++)
            {
                latinSquare[row, col] = (latinSquare[row - 1, col] + 1) % n;

                if (latinSquare[row, col] == 0)
                    latinSquare[row, col] = n;
            }
        }

        return latinSquare;
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