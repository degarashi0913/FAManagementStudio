using FAManagementStudio.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FAManagementStudio.ViewModels
{
    public class ViewModelBase : BindableBase
    {
        protected Messenger MessengerInstance { get; set; } = Messenger.Instance;
    }
}
