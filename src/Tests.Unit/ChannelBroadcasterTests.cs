using System.Collections.Concurrent;
using System.Threading.Channels;
using Core.All;

namespace Tests.Unit;

/// <summary>
///     Tests for the <see cref="ChannelBroadcaster{T}" /> class.
/// </summary>
public sealed class ChannelBroadcasterTests
{
    /// <summary>
    ///     Tests that a subscriber can read a published message.
    /// </summary>
    [Fact]
    public async Task Subscribe_ReturnsReader_ThatCanReadPublishedMessage()
    {
        var bc = new ChannelBroadcaster<string>();
        var reader = bc.Subscribe();

        await bc.PublishAsync("hello");

        Assert.True(await reader.WaitToReadAsync());
        Assert.True(reader.TryRead(out var msg));
        Assert.Equal("hello", msg);
    }

    /// <summary>
    ///     Tests that publishing delivers to multiple subscribers.
    /// </summary>
    [Fact]
    public async Task PublishAsync_DeliversToMultipleSubscribers()
    {
        var bc = new ChannelBroadcaster<int>();
        var r1 = bc.Subscribe();
        var r2 = bc.Subscribe();
        var r3 = bc.Subscribe();

        await bc.PublishAsync(42);

        Assert.True(r1.TryRead(out var m1));
        Assert.True(r2.TryRead(out var m2));
        Assert.True(r3.TryRead(out var m3));
        Assert.Equal(42, m1);
        Assert.Equal(42, m2);
        Assert.Equal(42, m3);
    }

    /// <summary>
    ///     Tests that unsubscribing prevents delivery to that subscriber.
    /// </summary>
    [Fact]
    public async Task Unsubscribe_RemovesSubscriber_PublishDoesNotDeliver()
    {
        var bc = new ChannelBroadcaster<string>();
        var reader = bc.Subscribe();

        bc.Unsubscribe(reader);

        await bc.PublishAsync("should not arrive");

        Assert.False(await reader.WaitToReadAsync().AsTask().WaitAsync(TimeSpan.FromMilliseconds(100)));
    }

    /// <summary>
    ///     Tests that unsubscribing completes the channel.
    /// </summary>
    [Fact]
    public async Task Unsubscribe_CompletesChannel_ReadAllAsyncExits()
    {
        var bc = new ChannelBroadcaster<string>();
        var reader = bc.Subscribe();

        bc.Unsubscribe(reader);

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        var items = new List<string>();

        await foreach (var item in reader.ReadAllAsync(cts.Token))
        {
            items.Add(item);
        }

        Assert.Empty(items);
    }

    /// <summary>
    ///     Tests that subscribers receive messages in FIFO order.
    /// </summary>
    [Fact]
    public async Task SubscriberReceivesMessages_InOrder()
    {
        var bc = new ChannelBroadcaster<int>();
        var reader = bc.Subscribe();

        await bc.PublishAsync(1);
        await bc.PublishAsync(2);
        await bc.PublishAsync(3);

        Assert.True(reader.TryRead(out var a));
        Assert.True(reader.TryRead(out var b));
        Assert.True(reader.TryRead(out var c));
        Assert.Equal(1, a);
        Assert.Equal(2, b);
        Assert.Equal(3, c);
    }

    /// <summary>
    ///     Tests that subscribing is thread-safe.
    /// </summary>
    [Fact]
    public void Subscribe_IsThreadSafe()
    {
        var bc = new ChannelBroadcaster<int>();
        var readers = new ConcurrentBag<ChannelReader<int>>();
        var exceptions = new ConcurrentBag<Exception>();

        Parallel.For(0, 100, _ =>
        {
            try
            {
                readers.Add(bc.Subscribe());
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        });

        Assert.Empty(exceptions);
        Assert.Equal(100, readers.Count);
    }

    /// <summary>
    ///     Tests that publishing with no subscribers does not throw.
    /// </summary>
    [Fact]
    public async Task PublishAsync_EmptySubscriberList_DoesNotThrow()
    {
        var bc = new ChannelBroadcaster<string>();

        await bc.PublishAsync("no one listening");
    }

    /// <summary>
    ///     Tests that unsubscribing removes only the specified subscriber.
    /// </summary>
    [Fact]
    public async Task Unsubscribe_RemovesOnlySpecifiedSubscriber()
    {
        var bc = new ChannelBroadcaster<string>();
        var keep = bc.Subscribe();
        var remove = bc.Subscribe();

        bc.Unsubscribe(remove);

        await bc.PublishAsync("test");

        Assert.True(await keep.WaitToReadAsync());
        Assert.False(await remove.WaitToReadAsync().AsTask().WaitAsync(TimeSpan.FromMilliseconds(100)));
    }
}
