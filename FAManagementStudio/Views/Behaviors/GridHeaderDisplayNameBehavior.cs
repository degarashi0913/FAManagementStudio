using System.Data;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace FAManagementStudio.Views.Behaviors
{
    public class GridHeaderDisplayNameBehavior : Behavior<DataGrid>
    {
        public GridHeaderDisplayNameBehavior() { }
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.AutoGeneratingColumn += AssociatedObject_AutoGeneratingColumn;
        }

        private void AssociatedObject_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.Header = (AssociatedObject.ItemsSource as DataView).Table.Columns[int.Parse(e.Column.Header.ToString())].Caption;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.AutoGeneratingColumn -= AssociatedObject_AutoGeneratingColumn;
            base.OnDetaching();
        }
    }
}
