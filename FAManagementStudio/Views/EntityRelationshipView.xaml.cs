using FAManagementStudio.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FAManagementStudio.Views
{
    /// <summary>
    /// EntityRelationshipView.xaml の相互作用ロジック
    /// </summary>
    public partial class EntityRelationshipView : Window
    {
        public EntityRelationshipView()
        {
            InitializeComponent();
        }

        private void RelayoutButton_Click(object sender, RoutedEventArgs e)
        {
            Layout.Relayout();
        }

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is EntityRelationshipViewModel vm)
            {
                vm.Scale += 0.1;
            }

        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is EntityRelationshipViewModel vm)
            {
                vm.Scale -= 0.1;
            }
        }

        private void ExpandOpenButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var expander in FindVisualChild<Expander>(Layout))
            {
                expander.IsExpanded = true;
            }
        }

        private void ExpandCloseButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var expander in FindVisualChild<Expander>(Layout))
            {
                expander.IsExpanded = false;
            }
        }
        private IEnumerable<childItem> FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is childItem target)
                {
                    yield return target;
                }
                else
                {
                    foreach (var item in FindVisualChild<childItem>(child))
                    {
                        yield return item;
                    }
                }
            }
        }
    }
}
