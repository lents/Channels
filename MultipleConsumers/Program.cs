using System;
using System.Threading.Channels;
using System.Threading.Tasks;

public class MultipleConsumersExample
{
    public static async Task Main()
    {
        Console.WriteLine("--- Multiple Consumers Example ---");

        var channel = Channel.CreateUnbounded<string>();

        var producer = Task.Run(async () =>
        {
            for (int i = 0; i < 20; i++)
            {
                var message = $"Message {i}";
                Console.WriteLine($"[Producer] Sending: {message}");
                await channel.Writer.WriteAsync(message);
                await Task.Delay(100);
            }
            channel.Writer.Complete();
            Console.WriteLine("[Producer] Finished.");
        });

        var consumers = new Task[3];
        for (int i = 0; i < consumers.Length; i++)
        {
            var consumerId = i + 1;
            consumers[i] = Task.Run(async () =>
            {
                await foreach (var message in channel.Reader.ReadAllAsync())
                {
                    Console.WriteLine($"  [Consumer {consumerId}] Processing: {message}");
                    await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(150, 300)));
                }
                Console.WriteLine($"  [Consumer {consumerId}] Finished.");
            });
        }

        await Task.WhenAll(producer);
        await Task.WhenAll(consumers);

        Console.WriteLine("--- All tasks finished ---");
    }
}