using FAManagementStudio.Common;
using System.Windows;
using System.Windows.Interactivity;

namespace FAManagementStudio.Views.Behaviors
{
    public class MessageTrigger<TMessage> : TriggerBase<DependencyObject>
       where TMessage : MessageBase
    {
        public object Target
        {
            get { return (object)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }

        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register(nameof(Target), typeof(object), typeof(MessageTrigger<TMessage>), new PropertyMetadata(null));


        protected override void OnAttached()
        {
            base.OnAttached();
            Messenger.Instance.Register<TMessage>(this, this.Action);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            Messenger.Instance.Unregister<TMessage>(this);
        }

        private void Action(TMessage message)
        {
            if (this.Target == message.Target || (this.Target != null && this.Target.Equals(message.Target)))
            {
                this.InvokeActions(message);
            }
        }

    }
}
