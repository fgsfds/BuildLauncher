using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Core.All;

public interface IChannelSubscriber<T>
{
    ChannelReader<T> Subscribe();
    void Unsubscribe(ChannelReader<T> reader);
}


public interface IChannelPublisher<T>
{
    ValueTask PublishAsync(T message);
}


public class ChannelBroadcaster<T> : IChannelSubscriber<T>, IChannelPublisher<T>
{
    private readonly Dictionary<ChannelReader<T>, ChannelWriter<T>> _readerChannelDict = [];
    private readonly Lock _locker = new();

    public ChannelReader<T> Subscribe()
    {
        using (_locker.EnterScope())
        {
            var channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions
            {
                SingleWriter = true
            });

            _readerChannelDict.Add(channel.Reader, channel.Writer);

            return channel.Reader;
        }
    }
    /// <inheritdoc />
    public void Unsubscribe(ChannelReader<T> reader)
    {
        using (_locker.EnterScope())
        {
            if (_readerChannelDict.Remove(reader, out var writer))
            {
                writer.Complete();
            }
        }
    }

    public async ValueTask PublishAsync(T message)
    {
        List<ChannelWriter<T>> targets;

        using (_locker.EnterScope())
        {
            targets = _readerChannelDict.Values.ToList();
        }

        foreach (var writer in targets)
        {
            await writer.WriteAsync(message);
        }
    }
}
