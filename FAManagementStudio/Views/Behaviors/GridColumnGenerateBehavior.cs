using System.Data;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace FAManagementStudio.Views.Behaviors
{
    /// <summary>
    /// Generate Columns, When ItemSource.Count is 0 
    /// </summary>
    public class GridColumnGenerateBehavior : Behavior<DataGrid>
    {
        public GridColumnGenerateBehavior() { }
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.ItemContainerGenerator.ItemsChanged += ItemContainerGenerator_ItemsChanged;
        }

        private void ItemContainerGenerator_ItemsChanged(object sender, ItemsChangedEventArgs e)
        {
            var itemsource = this.AssociatedObject?.ItemsSource as DataView;
            if (itemsource?.Count == 0)
            {
                AssociatedObject.Columns.Clear();
                foreach (DataColumn col in itemsource.Table.Columns)
                {
                    AssociatedObject.Columns.Add(new DataGridTextColumn { Header = col.Caption });
                }
            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject.ItemContainerGenerator.ItemsChanged -= ItemContainerGenerator_ItemsChanged;
            base.OnDetaching();
        }
    }
}
