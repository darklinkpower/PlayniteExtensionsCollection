using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace PluginsCommon.Behaviors
{
    public class HoverPopupBehavior : Behavior<FrameworkElement>
    {
        private bool _isMouseOverTrigger = false;
        private bool _isMouseOverPopup = false;
        private DispatcherTimer _closeTimer;


        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(HoverPopupBehavior),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty PopupProperty =
            DependencyProperty.Register(
                nameof(Popup),
                typeof(Popup),
                typeof(HoverPopupBehavior),
                new PropertyMetadata(null, OnPopupChanged));

        public static readonly DependencyProperty DelayProperty =
            DependencyProperty.Register(
                nameof(Delay),
                typeof(TimeSpan),
                typeof(HoverPopupBehavior),
                new PropertyMetadata(TimeSpan.FromMilliseconds(150), OnDelayChanged));

        public Popup Popup
        {
            get => (Popup)GetValue(PopupProperty);
            set => SetValue(PopupProperty, value);
        }

        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        public TimeSpan Delay
        {
            get => (TimeSpan)GetValue(DelayProperty);
            set => SetValue(DelayProperty, value);
        }

        protected override void OnAttached()
        {
            AssociatedObject.MouseEnter += Trigger_MouseEnter;
            AssociatedObject.MouseLeave += Trigger_MouseLeave;

            _closeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(150)
            };
            _closeTimer.Tick += (_, __) =>
            {
                _closeTimer.Stop();
                if (!_isMouseOverTrigger && !_isMouseOverPopup)
                {
                    IsOpen = false;
                }
            };
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseEnter -= Trigger_MouseEnter;
            AssociatedObject.MouseLeave -= Trigger_MouseLeave;

            if (Popup != null)
            {
                Popup.MouseEnter -= Popup_MouseEnter;
                Popup.MouseLeave -= Popup_MouseLeave;
            }

            _closeTimer?.Stop();
        }

        private static void OnDelayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HoverPopupBehavior behavior && behavior._closeTimer != null)
            {
                behavior._closeTimer.Interval = (TimeSpan)e.NewValue;
            }
        }

        private static void OnPopupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (HoverPopupBehavior)d;
            if (e.OldValue is Popup oldPopup)
            {
                oldPopup.MouseEnter -= behavior.Popup_MouseEnter;
                oldPopup.MouseLeave -= behavior.Popup_MouseLeave;
            }

            if (e.NewValue is Popup newPopup)
            {
                newPopup.MouseEnter += behavior.Popup_MouseEnter;
                newPopup.MouseLeave += behavior.Popup_MouseLeave;
            }
        }

        private void Trigger_MouseEnter(object sender, MouseEventArgs e)
        {
            _isMouseOverTrigger = true;
            IsOpen = true;
            _closeTimer.Stop();
        }

        private void Trigger_MouseLeave(object sender, MouseEventArgs e)
        {
            _isMouseOverTrigger = false;
            _closeTimer.Start();
        }

        private void Popup_MouseEnter(object sender, MouseEventArgs e)
        {
            _isMouseOverPopup = true;
            _closeTimer.Stop();
        }

        private void Popup_MouseLeave(object sender, MouseEventArgs e)
        {
            _isMouseOverPopup = false;
            _closeTimer.Start();
        }
    }
}
