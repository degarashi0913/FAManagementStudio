using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FAManagementStudio.Common
{
    public class MessageBase
    {
        public MessageBase() { }
        public MessageBase(object sender, object target)
        {
            Sender = sender;
            Target = target;

        }
        public object Sender { get; private set; }
        public object Target { get; private set; }
    }
}
