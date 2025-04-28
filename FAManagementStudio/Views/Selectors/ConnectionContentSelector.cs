using FAManagementStudio.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace FAManagementStudio.Views.Selectors
{
    public class ConnectionContentSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var data = item as ConnectionSettingsDisplayModel;
            if (data == null) return null;

            if (data.PropertyType == typeof(bool))
            {
                return (DataTemplate)((FrameworkElement)container).FindResource("Boolean");
            }
            else
            {
                return (DataTemplate)((FrameworkElement)container).FindResource("Other");
            }
        }
    }
}
