using FAManagementStudio.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

        private void ResultView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is not DataGrid dataGrid) return;

            var scrollViewer = FindVisualChild<ScrollViewer>(dataGrid);
            if (scrollViewer == null) return;

            if (0 < e.Delta)
            {
                if (scrollViewer.VerticalOffset == 0)
                {
                    e.Handled = true;
                    RaiseMouseWheelEventToParent(dataGrid, e);
                }
            }
            else
            {
                if (scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight)
                {
                    e.Handled = true;
                    RaiseMouseWheelEventToParent(dataGrid, e);
                }
            }
        }

        private void RaiseMouseWheelEventToParent(UIElement sender, MouseWheelEventArgs e)
        {
            var e2 = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = MouseWheelEvent,
                Source = sender
            };
            if (VisualTreeHelper.GetParent(sender) is UIElement parent)
            {
                parent.RaiseEvent(e2);
            }
        }

        private T? FindVisualChild<T>(DependencyObject? obj) where T : DependencyObject
        {
            if (obj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is T childElement) return childElement;
                if (FindVisualChild<T>(child) is { } childOfChild) return childOfChild;
            }
            return null;
        }
    }
}
