using Playnite.SDK;
using Playnite.SDK.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PluginsCommon
{
    /// <summary>
    /// Base class for plugin user controls that implements INotifyPropertyChanged and utility helpers.
    /// </summary>
    public abstract class PluginUserControlBase : PluginUserControl, INotifyPropertyChanged
    {
        private readonly Dispatcher _uiDispatcher = Application.Current.Dispatcher;
        private readonly DispatcherTimer _updateDebounceTimer;
        private readonly object _ctsLock = new object();
        private CancellationTokenSource _cts;
        protected IPlayniteAPI PlayniteApi { get; }
        /// <summary>
        /// Raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        protected PluginUserControlBase(IPlayniteAPI playniteApi)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            PlayniteApi = playniteApi ?? throw new ArgumentNullException(nameof(playniteApi));
            SetControlTextBlockStyle();
            _updateDebounceTimer = new DispatcherTimer
            {
                Interval = UpdateDebounceInterval
            };
            _updateDebounceTimer.Tick += OnDebounceTimerTick; 
        }

        private void SetControlTextBlockStyle()
        {
            // Desktop mode uses BaseTextBlockStyle and Fullscreen Mode uses TextBlockBaseStyle
            var baseStyleName = PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Desktop ? "BaseTextBlockStyle" : "TextBlockBaseStyle";
            if (ResourceProvider.GetResource(baseStyleName) is Style baseStyle &&
                baseStyle.TargetType == typeof(TextBlock))
            {
                var implicitStyle = new Style(typeof(TextBlock), baseStyle);
                Resources.Add(typeof(TextBlock), implicitStyle);
            }
        }

        /// <summary>
        /// Override to define how long to wait after last trigger before updating.
        /// </summary>
        protected virtual TimeSpan UpdateDebounceInterval => TimeSpan.FromMilliseconds(220);

        /// <summary>
        /// Indicates if an update is currently scheduled.
        /// </summary>
        public bool IsUpdateScheduled => _updateDebounceTimer.IsEnabled;

        /// <summary>
        /// Schedules a debounced update. Resets timer if already running.
        /// Cancels any pending operation via token (if used).
        /// </summary>
        protected void ScheduleUpdate()
        {
            lock (_ctsLock)
            {
                _updateDebounceTimer.Stop();
                _updateDebounceTimer.Interval = UpdateDebounceInterval;
                _updateDebounceTimer.Start();
            }
        }

        /// <summary>
        /// Cancels a scheduled update if one is pending.
        /// </summary>
        protected void CancelScheduledUpdate()
        {
            lock (_ctsLock)
            {
                _updateDebounceTimer.Stop();
            }
        }

        /// <summary>
        /// Cancels any ongoing update that is currently executing by triggering the cancellation token.
        /// Does not affect scheduled updates waiting for the debounce timer.
        /// </summary>
        protected void CancelOngoingUpdate()
        {
            lock (_ctsLock)
            {
                if (_cts != null && !_cts.IsCancellationRequested)
                {
                    try
                    {
                        _cts.Cancel();
                    }
                    catch (Exception)
                    {
                        
                    }
                }
            }
        }

        /// <summary>
        /// Called when the debounce interval elapses with no further ScheduleUpdate calls.
        /// </summary>
        protected virtual async Task OnDebouncedUpdateAsync(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        private async void OnDebounceTimerTick(object sender, EventArgs e)
        {
            _updateDebounceTimer.Stop();
            CancellationTokenSource ctsLocal;
            lock (_ctsLock)
            {
                _cts = new CancellationTokenSource();
                ctsLocal = _cts;
            }

            try
            {
                await OnDebouncedUpdateAsync(ctsLocal.Token);
            }
            catch (OperationCanceledException)
            {
                
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                lock (_ctsLock)
                {
                    // Dispose only if this is still the active CTS
                    if (_cts.Equals(ctsLocal))
                    {
                        _cts.Dispose();
                        _cts = null;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the backing field and raises <see cref="PropertyChanged"/> if the value changed.
        /// </summary>
        /// <typeparam name="T">Type of the property value.</typeparam>
        /// <param name="field">Reference to the backing field.</param>
        /// <param name="value">New value to assign.</param>
        /// <param name="propertyName">Property name (automatically set by the compiler).</param>
        /// <returns>True if the value was changed, false otherwise.</returns>
        protected bool SetValue<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for a specific property.
        /// </summary>
        /// <param name="propertyName">The name of the property (automatically set by the compiler).</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for multiple properties.
        /// </summary>
        /// <param name="propertyNames">The names of the properties to notify.</param>
        protected void OnPropertiesChanged(params string[] propertyNames)
        {
            foreach (var name in propertyNames)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        /// <summary>
        /// Indicates whether the control is running in the Visual Studio designer.
        /// </summary>
        public bool IsInDesignMode
        {
            get
            {
                return DesignerProperties.GetIsInDesignMode(new DependencyObject());
            }
        }

        /// <summary>
        /// Executes the specified action on the UI thread.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        protected void RunOnUI(Action action)
        {
            if (_uiDispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                _uiDispatcher.Invoke(action);
            }
        }

        /// <summary>
        /// Virtual method for handling cleanup or deactivation logic when the control is unloaded.
        /// </summary>
        public virtual void OnUnload()
        {
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event using a lambda expression for the property name.
        /// </summary>
        /// <typeparam name="T">Type of the property value.</typeparam>
        /// <param name="property">Lambda expression representing the property.</param>
        protected void OnPropertyChanged<T>(Expression<Func<T>> property)
        {
            if (property.Body is MemberExpression memberExpr)
            {
                OnPropertyChanged(memberExpr.Member.Name);
            }
        }
    }
}
