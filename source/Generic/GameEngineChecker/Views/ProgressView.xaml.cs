using GameEngineChecker.ViewModels;
using System.Windows.Controls;

namespace GameEngineChecker.Views
{
	/// <summary>
	/// Interaction logic for ProgressView.xaml
	/// </summary>
	public partial class ProgressView : UserControl
	{
		public ProgressView(ProgressViewModel progressViewModel)
		{
			InitializeComponent();
			DataContext = progressViewModel;
		}
	}
}