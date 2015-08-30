﻿using Codartis.SoftVis.Rendering.Wpf.Viewport.Commands;
using System.Windows;
using System.Windows.Media.Animation;

namespace Codartis.SoftVis.Rendering.Wpf.Viewport.Gestures.Animated
{
    public class ZoomViewportCommandAnimator : ViewportCommandAnimatorBase
    {
        private readonly ZoomViewportCommand _originalCommand;

        public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register("Zoom",
            typeof(double), typeof(ZoomViewportCommandAnimator), new PropertyMetadata(OnZoomPropertyChanged));

        public ZoomViewportCommandAnimator(IViewportGesture originalGesture, Duration animationDuration,
            ZoomViewportCommand originalCommand)
            : base(originalGesture, animationDuration)
        {
            _originalCommand = originalCommand;
        }

        public override void BeginAnimation()
        {
            var animation = new DoubleAnimation(_originalGesture.Viewport.Zoom, 
                _originalCommand.NewZoom, _animationDuration)
            {
                EasingFunction = _easingFunction
            };
            BeginAnimation(ZoomProperty, animation);
        }

        private static void OnZoomPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var commandAnimator = obj as ZoomViewportCommandAnimator;
            commandAnimator.OnZoomPropertyChanged((double)args.NewValue);
        }

        private void OnZoomPropertyChanged(double newZoom)
        {
            SendCommand(new ZoomViewportCommand(_originalGesture, newZoom));
        }
    }
}