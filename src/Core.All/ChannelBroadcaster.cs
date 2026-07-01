using System.Threading.Channels;

namespace Core.All;

/// <summary>
///     Defines a subscriber that can receive messages from a channel.
/// </summary>
/// <typeparam name="T">Type of messages.</typeparam>
public interface IChannelSubscriber<T>
{
    /// <summary>
    ///     Subscribes to the channel and returns a reader for receiving messages.
    /// </summary>
    ChannelReader<T> Subscribe();

    /// <summary>
    ///     Unsubscribes the specified reader from the channel.
    /// </summary>
    void Unsubscribe(ChannelReader<T> reader);
}


/// <summary>
///     Defines a publisher that can send messages to a channel.
/// </summary>
/// <typeparam name="T">Type of messages.</typeparam>
public interface IChannelPublisher<T>
{
    /// <summary>
    ///     Publishes a message to all subscribers.
    /// </summary>
    ValueTask PublishAsync(T message);
}


/// <summary>
///     Broadcasts messages to multiple subscribers using channels.
/// </summary>
/// <typeparam name="T">Type of messages.</typeparam>
public class ChannelBroadcaster<T> : IChannelSubscriber<T>, IChannelPublisher<T>
{
    /// <summary>
    ///     Synchronization lock for thread-safe access.
    /// </summary>
    private readonly Lock _locker = new();

    /// <summary>
    ///     Dictionary mapping channel readers to their corresponding writers.
    /// </summary>
    private readonly Dictionary<ChannelReader<T>, ChannelWriter<T>> _readerChannelDict = [];

    /// <inheritdoc />
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

    /// <inheritdoc />
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
}
