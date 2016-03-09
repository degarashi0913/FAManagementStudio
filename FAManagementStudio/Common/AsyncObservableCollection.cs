using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace FAManagementStudio.Common
{
    public class AsyncObservableCollection<T> : ObservableCollection<T>
    {
        Dispatcher _dispatcher = Application.Current.Dispatcher;
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_dispatcher.CheckAccess())
            {
                base.OnCollectionChanged(e);
            }
            else {
                _dispatcher.Invoke(new Action(() => base.OnCollectionChanged(e)));
            }
        }
    }
}
