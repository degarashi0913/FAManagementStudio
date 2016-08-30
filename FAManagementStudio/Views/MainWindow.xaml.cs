using FAManagementStudio.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FAManagementStudio.Views
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void TreeViewItem_MouseRightButtonDown(Object sender, MouseButtonEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null)
            {
                item.IsSelected = true;
                e.Handled = true;
            }
        }
        private void TabItem_MouseRightButtonDown(Object sender, MouseButtonEventArgs e)
        {
            TabItem item = sender as TabItem;
            if (item != null)
            {
                item.IsSelected = true;
                e.Handled = true;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            QueryTab.SelectedIndex = 0;
        }

        private void TextBox_PreviewDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }
        private void TabItem_Drag(object sender, MouseEventArgs e)
        {
            if (Mouse.PrimaryDevice.LeftButton != MouseButtonState.Pressed) return;
            var tabItem = (TabItem)e.Source;
            DragDrop.DoDragDrop(tabItem, tabItem, DragDropEffects.All);

        }
        private void TabItem_Drop(object sender, DragEventArgs e)
        {
            var obj = VisualTreeHelper.GetParent((DependencyObject)e.OriginalSource);
            while (obj.GetType() != typeof(TabItem))
            {
                obj = VisualTreeHelper.GetParent(obj);
            }
            var target = (TabItem)obj;
            var source = (TabItem)e.Data.GetData(typeof(TabItem));

            if (target.Equals(source)) return;

            var itemSource = (ObservableCollection<QueryTabViewModel>)QueryTab.ItemsSource;
            var sourceIdx = itemSource.IndexOf((QueryTabViewModel)source.Header);
            var targetIdx = itemSource.IndexOf((QueryTabViewModel)target.Header);

            if (itemSource.Count - 1 <= targetIdx) return;

            itemSource.Move(sourceIdx, targetIdx);
        }
    }
}
