using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace GameRelations
{
    public partial class GameRelationsSettingsView : UserControl
    {
        public GameRelationsSettingsView()
        {
            InitializeComponent();
        }

        private void SimilarGamesNotExcludeTagsLb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listbox && DataContext is GameRelationsSettingsViewModel viewModel)
            {
                ClearAndAddItems(listbox, viewModel.SgNotExcludeTagsSelectedItems);
                viewModel.NotifyCommandsPropertyChanged();
            }
        }

        private void SgExcludeTagsSelectedItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listbox && DataContext is GameRelationsSettingsViewModel viewModel)
            {
                ClearAndAddItems(listbox, viewModel.SgExcludeTagsSelectedItems);
                viewModel.NotifyCommandsPropertyChanged();
            }
        }

        private void SimilarGamesNotExcludeGenresLb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listbox && DataContext is GameRelationsSettingsViewModel viewModel)
            {
                ClearAndAddItems(listbox, viewModel.SgNotExcludeGenresSelectedItems);
                viewModel.NotifyCommandsPropertyChanged();
            }
        }

        private void SgExcludeGenresSelectedItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listbox && DataContext is GameRelationsSettingsViewModel viewModel)
            {
                ClearAndAddItems(listbox, viewModel.SgExcludeGenresSelectedItems);
                viewModel.NotifyCommandsPropertyChanged();
            }
        }

        private void SimilarGamesNotExcludeCategoriesLb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listbox && DataContext is GameRelationsSettingsViewModel viewModel)
            {
                ClearAndAddItems(listbox, viewModel.SgNotExcludeCategoriesSelectedItems);
                viewModel.NotifyCommandsPropertyChanged();
            }
        }

        private void SgExcludeCategoriesSelectedItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listbox && DataContext is GameRelationsSettingsViewModel viewModel)
            {
                ClearAndAddItems(listbox, viewModel.SgExcludeCategoriesSelectedItems);
                viewModel.NotifyCommandsPropertyChanged();
            }
        }

        private void ClearAndAddItems<T>(ListBox listbox, ObservableCollection<T> collection) where T : class
        {
            collection.Clear();
            foreach (var selectedItem in listbox.SelectedItems)
            {
                if (selectedItem is T item)
                {
                    collection.Add(item);
                }
            }
        }

    }
}