using NVIDIAGeForceNowEnabler.Models;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace NVIDIAGeForceNowEnabler.ViewModels
{
    public class GfnDatabaseBrowserViewModel : ObservableObject
    {
        private string searchString = string.Empty;
        public string SearchString
        {
            get { return searchString; }
            set
            {
                searchString = value.ToLower();
                OnPropertyChanged();
                variantsCollection.Refresh();
            }
        }

        private string storeSearchString = string.Empty;
        public string StoreSearchString
        {
            get { return storeSearchString; }
            set
            {
                storeSearchString = value.ToLower();
                OnPropertyChanged();
                variantsCollection.Refresh();
            }
        }

        private bool isVariantSelected { get; set; } = false;
        public bool IsVariantSelected
        {
            get => isVariantSelected;
            set
            {
                isVariantSelected = value;
                OnPropertyChanged();
            }
        }

        private List<GeforceNowItemVariant> variantsList;
        public List<GeforceNowItemVariant> VariantsList
        {
            get { return variantsList; }
            set
            {
                variantsList = value;
                OnPropertyChanged();
            }
        }

        private GeforceNowItemVariant selectedVariant;
        public GeforceNowItemVariant SelectedVariant
        {
            get { return selectedVariant; }
            set
            {
                selectedVariant = value;
                IsVariantSelected = selectedVariant != null;

                OnPropertyChanged();
            }
        }

        
        private IPlayniteAPI playniteApi;

        private readonly ICollectionView variantsCollection;
        public ICollectionView VariantsCollection
        {
            get => variantsCollection;
        }

        public GfnDatabaseBrowserViewModel(IPlayniteAPI playniteApi, List<GeforceNowItem> supportedList)
        {
            this.playniteApi = playniteApi;
            VariantsList = SetVariants(supportedList);
            variantsCollection = CollectionViewSource.GetDefaultView(VariantsList);
            variantsCollection.Filter = FilterVariantsCollection;
        }

        private List<GeforceNowItemVariant> SetVariants(List<GeforceNowItem> supportedList)
        {
            var variants = new List<GeforceNowItemVariant>();
            foreach (var supportedGame in supportedList)
            {
                if (supportedGame.Type != AppType.Game)
                {
                    continue;
                }
                
                foreach (var variant in supportedGame.Variants)
                {
                    if (variant.OsType != OsType.Windows)
                    {
                        continue;
                    }
                    
                    variants.Add(variant);
                }
            }

            variants.Sort((x, y) => x.Title.CompareTo(y.Title));
            return variants;
        }

        private bool FilterVariantsCollection(object item)
        {
            var variant = item as GeforceNowItemVariant;
            if (!SearchString.IsNullOrEmpty() && !variant.Title.ToLower().Contains(SearchString))
            {
                return false;
            }

            if (!StoreSearchString.IsNullOrEmpty() && !variant.AppStore.ToString().ToLower().Contains(StoreSearchString))
            {
                return false;
            }

            return true;
        }

        public RelayCommand CopyVariantTitleToClipboardCommand
        {
            get => new RelayCommand(() =>
            {
                if (SelectedVariant != null)
                {
                    Clipboard.SetText(SelectedVariant.Title);
                }
            });
        }

    }
}