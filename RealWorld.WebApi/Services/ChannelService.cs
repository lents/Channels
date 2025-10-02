using System.Threading.Channels;

namespace RealWorld.WebApi.Services;

public class ChannelService
{
    private readonly Channel<Job> _channel;

    public ChannelService()
    {
        // Create an unbounded channel for simplicity.
        // For production scenarios, a bounded channel is often a better choice
        // to prevent uncontrolled memory growth.
        _channel = Channel.CreateUnbounded<Job>();
    }

    public ChannelReader<Job> Reader => _channel.Reader;
    public ChannelWriter<Job> Writer => _channel.Writer;
}