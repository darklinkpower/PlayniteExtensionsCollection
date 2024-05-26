using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VndbApiDomain.CharacterAggregate;
using VndbApiDomain.ImageAggregate;
using VndbApiDomain.SharedKernel;
using VndbApiDomain.TraitAggregate;

namespace VNDBNexus.Controls
{
    public class VndbImageControl : Control
    {
        static VndbImageControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VndbImageControl), new FrameworkPropertyMetadata(typeof(CharacterTraitControl)));
        }

        public static readonly DependencyProperty MaximumViolenceProperty =
            DependencyProperty.Register("MaximumViolence", typeof(ImageViolenceLevelEnum), typeof(VndbImageControl), new PropertyMetadata(default(ImageViolenceLevelEnum), OnFilterChanged));

        public static readonly DependencyProperty MaximumSexualityProperty =
            DependencyProperty.Register("MaximumSexuality", typeof(ImageSexualityLevelEnum), typeof(VndbImageControl), new PropertyMetadata(default(ImageSexualityLevelEnum), OnFilterChanged));

        public static readonly DependencyProperty ImagesSourceProperty =
            DependencyProperty.Register("ImagesSource", typeof(IEnumerable<VndbImage>), typeof(VndbImageControl), new PropertyMetadata(default(IEnumerable<VndbImage>), OnFilterChanged));

        public static readonly DependencyProperty FilteredImagesProperty =
            DependencyProperty.Register("FilteredImages", typeof(IEnumerable<VndbImage>), typeof(VndbImageControl), new PropertyMetadata(default(IEnumerable<VndbImage>)));

        public ImageViolenceLevelEnum MaximumViolence
        {
            get => (ImageViolenceLevelEnum)GetValue(MaximumViolenceProperty);
            set => SetValue(MaximumSexualityProperty, value);
        }

        public ImageSexualityLevelEnum MaximumSexuality
        {
            get => (ImageSexualityLevelEnum)GetValue(MaximumSexualityProperty);
            set => SetValue(MaximumSexualityProperty, value);
        }

        public IEnumerable<VndbImage> ImagesSource
        {
            get => (IEnumerable<VndbImage>)GetValue(ImagesSourceProperty);
            set => SetValue(ImagesSourceProperty, value);
        }

        public IEnumerable<VndbImage> FilteredImages
        {
            get => (IEnumerable<VndbImage>)GetValue(FilteredImagesProperty);
            private set => SetValue(FilteredImagesProperty, value);
        }

        private static void OnFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (VndbImageControl)d;
            control.UpdateFilteredImages();
        }

        private void UpdateVisibility()
        {
            Visibility = FilteredImages != null && FilteredImages.Any() ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateFilteredImages()
        {
            if (ImagesSource is null || !ImagesSource.Any())
            {
                FilteredImages = Enumerable.Empty<VndbImage>();
            }
            else
            {
                FilteredImages = ImagesSource.Where(x => IsViolenceAllowed(x) && IsSeualityAllowed(x));
            }

            UpdateVisibility();
        }

        private bool IsViolenceAllowed(VndbImage image)
        {
            if (image.ViolenceLevel == ImageViolenceLevelEnum.Tame || MaximumViolence == ImageViolenceLevelEnum.Brutal)
            {
                return true;
            }
            else if (image.ViolenceLevel == ImageViolenceLevelEnum.Violent && MaximumViolence == ImageViolenceLevelEnum.Violent)
            {
                return true;
            }

            return false;
        }

        private bool IsSeualityAllowed(VndbImage image)
        {
            if (image.SexualityLevel == ImageSexualityLevelEnum.Safe || MaximumSexuality == ImageSexualityLevelEnum.Explicit)
            {
                return true;
            }
            else if (image.SexualityLevel == ImageSexualityLevelEnum.Suggestive && MaximumSexuality == ImageSexualityLevelEnum.Suggestive)
            {
                return true;
            }

            return false;
        }

    }
}