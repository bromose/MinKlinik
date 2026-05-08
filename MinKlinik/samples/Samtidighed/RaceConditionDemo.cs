using System;
using System.Threading.Tasks;

var counter = 0;
var tasks = new Task[100];

for (var i = 0; i < tasks.Length; i++)
{
    tasks[i] = Task.Run(() =>
    {
        for (var j = 0; j < 1000; j++)
        {
            counter++;
        }
    });
}

await Task.WhenAll(tasks);
Console.WriteLine($"Race condition resultat: {counter}");
