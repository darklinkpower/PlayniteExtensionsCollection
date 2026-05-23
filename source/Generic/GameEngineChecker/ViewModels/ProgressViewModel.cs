using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace GameEngineChecker.ViewModels
{
	public class ProgressViewModel : ObservableObject, IDisposable
	{
		private readonly IPlayniteAPI _api;
		private readonly CancellationTokenSource _cts;
		private float _progressValue;
		private Window _window;

		public ProgressViewModel(IPlayniteAPI api, CancellationTokenSource cts)
		{
			_api = api;
			_cts = cts;
		}

		public void SetWindow(Window window)
		{
			_window = window;
		}

		public float ProgressValue
		{
			get => _progressValue;
			set => SetValue(ref _progressValue, value);
		}

		public ICommand Hide => new RelayCommand(CloseWindow);

		public ICommand Cancel => new RelayCommand(() => _cts.Cancel());

		public void Dispose()
		{
			CloseWindow();
			_cts.Dispose();
		}

		private void CloseWindow()
		{
			_api.MainView.UIDispatcher.Invoke(() => _window?.Close());
		}
	}
}
