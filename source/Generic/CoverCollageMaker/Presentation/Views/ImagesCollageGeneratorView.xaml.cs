using CoverCollageMaker.Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CoverCollageMaker.Presentation.Views
{
    /// <summary>
    /// Interaction logic for ImagesCollageGeneratorView.xaml
    /// </summary>
    public partial class ImagesCollageGeneratorView : UserControl
    {
        public ImagesCollageGeneratorView()
        {
            InitializeComponent();
        }

        private void OnGridMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DataContext is ImagesCollageGeneratorViewModel viewModel)
            {
                viewModel.OnMouseWheel(sender, e);
            }
        }
    }

}
