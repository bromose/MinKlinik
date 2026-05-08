using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

var data = Enumerable.Range(1, 200_000).ToArray();

var sequentialSw = Stopwatch.StartNew();
var sequentialSum = data.Select(x => (decimal)Math.Sqrt(x) * 0.1m).Sum();
sequentialSw.Stop();

var parallelSw = Stopwatch.StartNew();
var bag = new ConcurrentBag<decimal>();
await Parallel.ForEachAsync(data, async (x, _) =>
{
    await Task.Yield();
    bag.Add((decimal)Math.Sqrt(x) * 0.1m);
});
var parallelSum = bag.Sum();
parallelSw.Stop();

Console.WriteLine($"Sekventiel: {sequentialSw.ElapsedMilliseconds} ms ({sequentialSum})");
Console.WriteLine($"Parallel:   {parallelSw.ElapsedMilliseconds} ms ({parallelSum})");
