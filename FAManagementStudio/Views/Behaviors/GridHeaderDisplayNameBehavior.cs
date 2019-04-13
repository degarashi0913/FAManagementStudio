using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Xaml.Behaviors;

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

        private const string NullString = "(DB_Null)";

        private void AssociatedObject_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var propertyName = e.Column.Header.ToString();
            e.Column.Header = (AssociatedObject.ItemsSource as DataView).Table.Columns[int.Parse(propertyName)].Caption;
            ((DataGridBoundColumn)e.Column).Binding.TargetNullValue = NullString;

            //ex.)boolean
            if (e.Column is DataGridTextColumn column)
            {
                //Null Value
                var style = new Style(typeof(TextBlock));
                var trigger = new DataTrigger { Binding = new Binding(propertyName), Value = null };
                trigger.Setters.Add(new Setter(TextBlock.ForegroundProperty, new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightGray)));
                style.Triggers.Add(trigger);
                column.ElementStyle = style;
            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject.AutoGeneratingColumn -= AssociatedObject_AutoGeneratingColumn;
            base.OnDetaching();
        }
    }
}
