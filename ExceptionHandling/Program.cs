using System.Threading.Channels;

internal class Program
{
    static async Task Main()
    {
        var channel = Channel.CreateUnbounded<string>();

        var producerTask = FaultyProducer(channel.Writer);
        var consumerTask = ExceptionHandlingConsumer(channel.Reader);

        await Task.WhenAll(producerTask, consumerTask);
        Console.WriteLine("Done.");
    }

    static async Task FaultyProducer(ChannelWriter<string> writer)
    {
        try
        {
            for (int i = 0; i < 3; i++)
            {
                await writer.WriteAsync($"Data {i}");
                Console.WriteLine($"FaultyProducer: Wrote Data {i}");
            }

            // Simulate failure
            throw new InvalidOperationException("Something went wrong in the producer!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FaultyProducer: Caught exception: {ex.Message}. Completing channel with error.");
            writer.Complete(ex); // propagate error
        }
    }

    static async Task ExceptionHandlingConsumer(ChannelReader<string> reader)
    {
        try
        {
            await foreach (var item in reader.ReadAllAsync())
            {
                Console.WriteLine($"ExceptionHandlingConsumer: Read {item}");
            }

            // If we got here without an exception the channel completed cleanly.
            Console.WriteLine("ExceptionHandlingConsumer: All items processed successfully.");
        }
        catch (Exception ex)
        {
            // This will be the original InvalidOperationException from the producer.
            Console.WriteLine($"ExceptionHandlingConsumer: Channel completed with error: {ex.Message}");
        }
    }
}