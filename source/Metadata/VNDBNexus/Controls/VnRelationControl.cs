using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VndbApiDomain.VisualNovelAggregate;

namespace VNDBNexus.Controls
{
    public class VnRelationControl : Control
    {
        static VnRelationControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VnRelationControl), new FrameworkPropertyMetadata(typeof(VnRelationControl)));
        }

        public static readonly DependencyProperty RelationTypeProperty =
            DependencyProperty.Register("RelationType", typeof(VnRelationTypeEnum), typeof(VnRelationControl), new PropertyMetadata(default(VnRelationTypeEnum), OnRelationTypeChanged));

        public static readonly DependencyProperty IncludeUnofficialProperty =
            DependencyProperty.Register("IncludeUnofficial", typeof(bool), typeof(VnRelationControl), new PropertyMetadata(true, OnRelationTypeChanged));

        public static readonly DependencyProperty RelationsProperty =
            DependencyProperty.Register("Relations", typeof(IEnumerable<VisualNovelRelation>), typeof(VnRelationControl), new PropertyMetadata(default(IEnumerable<VisualNovelRelation>), OnRelationsChanged));

        public static readonly DependencyProperty RelationsSourceProperty =
            DependencyProperty.Register("RelationsSource", typeof(IEnumerable<VisualNovelRelation>), typeof(VnRelationControl), new PropertyMetadata(default(IEnumerable<VisualNovelRelation>), OnAllRelationsChanged));

        public VnRelationTypeEnum RelationType
        {
            get => (VnRelationTypeEnum)GetValue(RelationTypeProperty);
            set => SetValue(RelationTypeProperty, value);
        }

        public bool IncludeUnofficial
        {
            get => (bool)GetValue(IncludeUnofficialProperty);
            set => SetValue(IncludeUnofficialProperty, value);
        }

        public IEnumerable<VisualNovelRelation> Relations
        {
            get => (IEnumerable<VisualNovelRelation>)GetValue(RelationsProperty);
            set => SetValue(RelationsProperty, value);
        }

        public IEnumerable<VisualNovelRelation> RelationsSource
        {
            get => (IEnumerable<VisualNovelRelation>)GetValue(RelationsSourceProperty);
            set => SetValue(RelationsSourceProperty, value);
        }

        private static void OnRelationTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (VnRelationControl)d;
            control.UpdateRelations();
        }

        private static void OnAllRelationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (VnRelationControl)d;
            control.UpdateRelations();
        }

        private static void OnRelationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (VnRelationControl)d;
            control.UpdateVisibility();
        }

        private void UpdateRelations()
        {
            if (RelationsSource != null)
            {
                Relations = RelationsSource
                    .Where(r => r.Relation == RelationType && (IncludeUnofficial || r.RelationOfficial))
                    .OrderBy(x => !x.RelationOfficial)
                    .ThenBy(x => x.ReleaseDate.Year);
            }
        }

        private void UpdateVisibility()
        {
            Visibility = Relations != null && Relations.Any() ? Visibility.Visible : Visibility.Collapsed;
        }

        public VnRelationControl()
        {
            DataContextChanged += (s, e) => UpdateRelations();
        }
    }
}
