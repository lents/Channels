using System;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

public class MultipleProducersExample
{
    public static async Task Main()
    {
        Console.WriteLine("--- Multiple Producers Example ---");

        var channel = Channel.CreateUnbounded<string>();

        var consumer = Task.Run(async () =>
        {
            await foreach (var message in channel.Reader.ReadAllAsync())
            {
                Console.WriteLine($"  [Consumer] Processing: {message}");
                await Task.Delay(200); // Simulate work
            }
            Console.WriteLine("  [Consumer] Finished.");
        });

        var producers = new Task[3];
        for (int i = 0; i < producers.Length; i++)
        {
            var producerId = i + 1;
            producers[i] = Task.Run(async () =>
            {
                for (int j = 0; j < 5; j++)
                {
                    var message = $"Message {j} from producer {producerId}";
                    Console.WriteLine($"[Producer {producerId}] Sending: {message}");
                    await channel.Writer.WriteAsync(message);
                    await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(50, 150)));
                }
                Console.WriteLine($"[Producer {producerId}] Finished.");
            });
        }

        // Wait for all producers to finish their work
        await Task.WhenAll(producers);

        // Mark the channel as complete, no more items will be added
        channel.Writer.Complete();

        // Wait for the consumer to process all items
        await consumer;

        Console.WriteLine("--- All tasks finished ---");
    }
}