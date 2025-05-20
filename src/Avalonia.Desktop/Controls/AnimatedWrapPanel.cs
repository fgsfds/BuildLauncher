using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using System.Collections.Specialized;

namespace Avalonia.Desktop.Controls;

/// <summary>
/// WrapPanel with animated repositioning of elements between rows.
/// </summary>
public sealed class AnimatedWrapPanel : WrapPanel
{
    private const bool EnableAnimationsDefault = true;
    private const int AnimationDurationDefault = 300;

    // Everything runs in the UI thread, so no need for the thread safe dictionaries
    private readonly Dictionary<Visual, Rect> _previousBounds = [];
    private readonly Dictionary<Visual, CancellationTokenSource> _runningAnimationsCtss = [];
    private readonly Dictionary<Visual, Animation.Animation> _animationsCache = [];

    /// <summary>
    /// Whether to enable animations. Can be turned off for performance-critical scenarios.
    /// </summary>
    public static readonly AvaloniaProperty<bool> EnableAnimationsProperty = AvaloniaProperty.Register<AnimatedWrapPanel, bool>(nameof(EnableAnimations), defaultValue: EnableAnimationsDefault);

    public bool EnableAnimations
    {
        get => (bool?)GetValue(EnableAnimationsProperty) ?? EnableAnimationsDefault;
        set => SetValue(EnableAnimationsProperty, value);
    }

    /// <summary>
    /// Duration of the animations in milliseconds.
    /// </summary>
    public static readonly AvaloniaProperty<int> AnimationDurationProperty = AvaloniaProperty.Register<AnimatedWrapPanel, int>(nameof(AnimationDuration), defaultValue: AnimationDurationDefault);

    public int AnimationDuration
    {
        get => (int?)GetValue(AnimationDurationProperty) ?? AnimationDurationDefault;
        set => SetValue(AnimationDurationProperty, value);
    }


    public AnimatedWrapPanel()
    {
        DetachedFromVisualTree += OnDetachedFromVisualTree;
        Children.CollectionChanged += OnChildrenChanged;
    }


    protected override Size ArrangeOverride(Size finalSize)
    {
        var result = base.ArrangeOverride(finalSize);

        foreach (var child in Children)
        {
            if (!EnableAnimations)
            {
                continue;
            }

            if (!child.IsEffectivelyVisible)
            {
                continue;
            }

            var currentPosition = child.Bounds;

            if (!_previousBounds.TryGetValue(child, out var previousPosition))
            {
                _previousBounds[child] = currentPosition;
                continue;
            }

            var xDelta = previousPosition.X - currentPosition.X;
            var yDelta = previousPosition.Y - currentPosition.Y;

            if (Math.Abs(xDelta) < 1 && Math.Abs(yDelta) < 1)
            {
                continue;
            }


            if (child.RenderTransform is not TranslateTransform transform)
            {
                transform = new TranslateTransform();
                child.RenderTransform = transform;
                child.RenderTransformOrigin = RelativePoint.TopLeft;
            }

            transform.X = xDelta;
            transform.Y = yDelta;

            _ = Dispatcher.UIThread.Invoke(async () => await Animate(child, xDelta, yDelta).ConfigureAwait(true));
        }

        return result;
    }

    private async Task Animate(Control controlToAnimate, double fromX, double fromY)
    {
        if (_runningAnimationsCtss.TryGetValue(controlToAnimate, out var existingAnimationCts))
        {
            try
            {
                existingAnimationCts.Cancel();
                existingAnimationCts.Dispose();
            }
            catch
            {
                //nothing to do
            }
        }

        var currentAnimationCts = new CancellationTokenSource();
        _runningAnimationsCtss[controlToAnimate] = currentAnimationCts;

        try
        {
            if (!_animationsCache.TryGetValue(controlToAnimate, out var animation))
            {
                animation = new Animation.Animation
                {
                    Duration = TimeSpan.FromMilliseconds(AnimationDuration),
                    Easing = new SplineEasing(0.4, 0, 0.2, 1),
                    FillMode = FillMode.Both,
                    Children =
                    {
                        new KeyFrame
                        {
                            Cue = new Cue(0),
                            Setters =
                            {
                                new Setter(TranslateTransform.XProperty, fromX),
                                new Setter(TranslateTransform.YProperty, fromY),
                            }
                        },
                        new KeyFrame
                        {
                            Cue = new Cue(1),
                            Setters =
                            {
                                new Setter(TranslateTransform.XProperty, 0d),
                                new Setter(TranslateTransform.YProperty, 0d),
                            }
                        }
                    }
                };

                _animationsCache.Add(controlToAnimate, animation);
            }
            else
            {
                animation.Children[0].Setters[0] = new Setter(TranslateTransform.XProperty, fromX);
                animation.Children[0].Setters[1] = new Setter(TranslateTransform.YProperty, fromY);
            }

            await animation.RunAsync(controlToAnimate, currentAnimationCts.Token).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            //nothing to do
        }

        if (_runningAnimationsCtss.Remove(controlToAnimate, out var cts))
        {
            try
            {
                cts.Cancel();
                cts.Dispose();
            }
            catch
            {
                //nothing to do
            }
        }

        _previousBounds[controlToAnimate] = controlToAnimate.Bounds;
    }

    private void OnChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is null)
        {
            return;
        }

        foreach (Control control in e.OldItems)
        {
            _ = _previousBounds.Remove(control);
            _ = _animationsCache.Remove(control);

            if (_runningAnimationsCtss.Remove(control, out var cts))
            {
                try
                {
                    cts.Cancel();
                    cts.Dispose();
                }
                catch
                {
                    //nothing to do
                }
            }
        }
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        DetachedFromVisualTree -= OnDetachedFromVisualTree;
        Children.CollectionChanged -= OnChildrenChanged;

        foreach (var item in _runningAnimationsCtss)
        {
            item.Value.Dispose();
        }
    }
}
