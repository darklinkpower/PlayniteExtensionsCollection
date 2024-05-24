using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VndbApiDomain.CharacterAggregate;
using VndbApiDomain.SharedKernel;
using VndbApiDomain.TraitAggregate;

namespace VNDBNexus.Controls
{
    public class CharacterTraitControl : Control
    {
        static CharacterTraitControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CharacterTraitControl), new FrameworkPropertyMetadata(typeof(CharacterTraitControl)));
        }

        public static readonly DependencyProperty TraitGroupProperty =
            DependencyProperty.Register("TraitGroup", typeof(TraitGroupEnum), typeof(CharacterTraitControl), new PropertyMetadata(default(TraitGroupEnum), OnFilterChanged));

        public static readonly DependencyProperty MaxSpoilerLevelProperty =
            DependencyProperty.Register("MaxSpoilerLevel", typeof(SpoilerLevelEnum), typeof(CharacterTraitControl), new PropertyMetadata(SpoilerLevelEnum.None, OnFilterChanged));

        public static readonly DependencyProperty TraitsProperty =
            DependencyProperty.Register("Traits", typeof(IEnumerable<CharacterTrait>), typeof(CharacterTraitControl), new PropertyMetadata(default(IEnumerable<CharacterTrait>), OnFilterChanged));

        public static readonly DependencyProperty IncludeSexualTraitsProperty =
            DependencyProperty.Register("IncludeSexualTraits", typeof(bool), typeof(CharacterTraitControl), new PropertyMetadata(false, OnFilterChanged));

        public static readonly DependencyProperty FilteredTraitsProperty =
            DependencyProperty.Register("FilteredTraits", typeof(IEnumerable<CharacterTrait>), typeof(CharacterTraitControl), new PropertyMetadata(default(IEnumerable<CharacterTrait>)));

        public TraitGroupEnum TraitGroup
        {
            get => (TraitGroupEnum)GetValue(TraitGroupProperty);
            set => SetValue(TraitGroupProperty, value);
        }

        public SpoilerLevelEnum MaxSpoilerLevel
        {
            get => (SpoilerLevelEnum)GetValue(MaxSpoilerLevelProperty);
            set => SetValue(MaxSpoilerLevelProperty, value);
        }

        public IEnumerable<CharacterTrait> Traits
        {
            get => (IEnumerable<CharacterTrait>)GetValue(TraitsProperty);
            set => SetValue(TraitsProperty, value);
        }

        public bool IncludeSexualTraits
        {
            get => (bool)GetValue(IncludeSexualTraitsProperty);
            set => SetValue(IncludeSexualTraitsProperty, value);
        }

        public IEnumerable<CharacterTrait> FilteredTraits
        {
            get => (IEnumerable<CharacterTrait>)GetValue(FilteredTraitsProperty);
            private set => SetValue(FilteredTraitsProperty, value);
        }

        private static void OnFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CharacterTraitControl)d;
            control.UpdateFilteredTraits();
        }

        private void UpdateVisibility()
        {
            Visibility = FilteredTraits != null && FilteredTraits.Any() ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateFilteredTraits()
        {
            if (Traits is null || (!IncludeSexualTraits && TraitGroup == TraitGroupEnum.EngagesInSexual) || (!IncludeSexualTraits && TraitGroup == TraitGroupEnum.SubjectOfSexual))
            {
                FilteredTraits = Enumerable.Empty<CharacterTrait>();
            }
            else
            {
                FilteredTraits = Traits
                .Where(t => t.Group == TraitGroup &&
                (
                    t.SpoilerLevel == SpoilerLevelEnum.None ||
                    (t.SpoilerLevel == SpoilerLevelEnum.Minimum && MaxSpoilerLevel == SpoilerLevelEnum.Minimum) ||
                    MaxSpoilerLevel == SpoilerLevelEnum.Major
                )).OrderBy(x => x.Name);
            }

            UpdateVisibility();
        }
    }
}
