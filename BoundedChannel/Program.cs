using System;
using System.Threading.Channels;
using System.Threading.Tasks;

public class BoundedChannelExample
{
    public static async Task Main()
    {
        Console.WriteLine("--- Bounded Channel Examples ---");

        await RunExample(BoundedChannelFullMode.Wait, "Wait");
        await RunExample(BoundedChannelFullMode.DropNewest, "DropNewest");
        await RunExample(BoundedChannelFullMode.DropOldest, "DropOldest");
        await RunExample(BoundedChannelFullMode.DropWrite, "DropWrite");

        Console.WriteLine("--- All examples finished ---");
    }

    private static async Task RunExample(BoundedChannelFullMode mode, string modeName)
    {
        Console.WriteLine($"\n--- Running Example: {modeName} ---");

        var options = new BoundedChannelOptions(5) // Channel can hold a maximum of 5 items
        {
            FullMode = mode
        };

        var channel = Channel.CreateBounded<int>(options);
        var writer = channel.Writer;
        var reader = channel.Reader;

        var producer = Task.Run(async () =>
        {
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"[Producer] Attempting to write {i}");
                if (await writer.WaitToWriteAsync()) // Wait for space if channel is full (for Wait mode)
                {
                    if (writer.TryWrite(i))
                    {
                        Console.WriteLine($"[Producer] Successfully wrote {i}");
                    }
                    else
                    {
                         Console.WriteLine($"[Producer] Failed to write {i}");
                    }
                }
                else
                {
                    Console.WriteLine($"[Producer] Write operation denied for {i}");
                }
                await Task.Delay(50); // Producer is faster than consumer
            }
            writer.Complete();
            Console.WriteLine("[Producer] Finished.");
        });

        var consumer = Task.Run(async () =>
        {
            await Task.Delay(500); // Let the producer fill the channel first
            await foreach (var item in reader.ReadAllAsync())
            {
                Console.WriteLine($"  [Consumer] Read: {item}");
                await Task.Delay(200); // Consumer is slower
            }
            Console.WriteLine("  [Consumer] Finished.");
        });

        await Task.WhenAll(producer, consumer);
    }
}