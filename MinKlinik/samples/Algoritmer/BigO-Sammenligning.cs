using System;
using System.Diagnostics;
using System.Linq;

var rnd = new Random(42);
var numbers = Enumerable.Range(0, 30_000).Select(_ => rnd.Next()).ToArray();

var n2 = (int[])numbers.Clone();
var nLogN = (int[])numbers.Clone();

var sw1 = Stopwatch.StartNew();
BubbleSort(n2);
sw1.Stop();

var sw2 = Stopwatch.StartNew();
Array.Sort(nLogN);
sw2.Stop();

Console.WriteLine($"O(n^2) bubble sort: {sw1.ElapsedMilliseconds} ms");
Console.WriteLine($"O(n log n) Array.Sort: {sw2.ElapsedMilliseconds} ms");

static void BubbleSort(int[] arr)
{
    for (var i = 0; i < arr.Length - 1; i++)
    {
        for (var j = 0; j < arr.Length - i - 1; j++)
        {
            if (arr[j] <= arr[j + 1]) continue;
            (arr[j], arr[j + 1]) = (arr[j + 1], arr[j]);
        }
    }
}
