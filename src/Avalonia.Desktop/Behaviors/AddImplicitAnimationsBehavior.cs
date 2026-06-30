using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Desktop.Helpers;
using Avalonia.Rendering.Composition;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;

namespace Avalonia.Desktop.Behaviors;

public class AddImplicitAnimationsBehavior : Behavior<ListBox>
{
    /// <inheritdoc/>
    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is null)
        {
            return;
        }

        AssociatedObject.ContainerPrepared += OnContainerPrepared;
        AssociatedObject.ContainerClearing += OnContainerClearing;

        foreach (var container in AssociatedObject.GetRealizedContainers())
        {
            if (container is ListBoxItem item)
            {
                AttachToItem(item);
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnDetaching()
    {
        if (AssociatedObject is not null)
        {
            AssociatedObject.ContainerPrepared -= OnContainerPrepared;
            AssociatedObject.ContainerClearing -= OnContainerClearing;

            foreach (var container in AssociatedObject.GetRealizedContainers())
            {
                if (container is ListBoxItem item)
                {
                    DetachFromItem(item);
                }
            }
        }

        base.OnDetaching();
    }

    private void OnContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        if (e.Container is ListBoxItem item)
        {
            AttachToItem(item);
        }
    }

    private void OnContainerClearing(object? sender, ContainerClearingEventArgs e)
    {
        if (e.Container is ListBoxItem item)
        {
            DetachFromItem(item);
        }
    }

    private void AttachToItem(ListBoxItem item)
    {
        if (item.Content is SeparatorItem)
        {
            item.IsHitTestVisible = false;
            item.Focusable = false;
            return;
        }

        // Ensure we don't double-subscribe
        item.AttachedToVisualTree -= OnItemAttachedToVisualTree;
        item.AttachedToVisualTree += OnItemAttachedToVisualTree;

        if (item.IsAttachedToVisualTree())
        {
            SetupImplicitAnimations(item);
        }
    }

    private void DetachFromItem(ListBoxItem item)
    {
        item.AttachedToVisualTree -= OnItemAttachedToVisualTree;
    }

    private void OnItemAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is ListBoxItem item)
        {
            SetupImplicitAnimations(item);
        }
    }

    private async void SetupImplicitAnimations(ListBoxItem item)
    {
        try
        {
            // wait for layout to finish before applying animations, 
            // preventing items from flying in from 0,0 on tab switches.
            await Task.Delay(500).ConfigureAwait(true);

            // abort if the tab was switched again during the delay
            if (!item.IsAttachedToVisualTree())
            {
                return;
            }

            var visual = ElementComposition.GetElementVisual(item);
            var compositor = visual?.Compositor;

            if (visual is null || compositor is null)
            {
                return;
            }

            var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.Target = "Offset";
            offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(400);

            var animationGroup = compositor.CreateAnimationGroup();
            animationGroup.Add(offsetAnimation);

            var implicitAnimations = compositor.CreateImplicitAnimationCollection();
            implicitAnimations["Offset"] = animationGroup;

            visual.ImplicitAnimations = implicitAnimations;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in SetupImplicitAnimations: {ex.Message}");
        }
    }
}
