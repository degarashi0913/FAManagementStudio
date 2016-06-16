using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace FAManagementStudio.Common
{
    public class Messenger
    {
        private static readonly Messenger _instance = new Messenger();
        public static Messenger Instance { get { return _instance; } }
        private Dictionary<Type, List<MessageAction>> _actions = new Dictionary<Type, List<MessageAction>>();
        public void Register<TMessage>(object recipient, Action<TMessage> action)
        {
            var type = typeof(TMessage);
            if (!_actions.ContainsKey(type))
            {
                _actions.Add(type, new List<MessageAction>());
            }
            var list = _actions[type];

            list.Add(new MessageAction(recipient, action));

        }
        public void Send<TMessage>(TMessage message)
        {
            var actions = _actions[typeof(TMessage)].ToList();
            foreach (var item in actions)
            {
                item.Execute(message);
            }
        }

        public void Unregister<TMessage>(object recipient)
        {
            var items = _actions[typeof(TMessage)].Where(x => x.Target == recipient);
            foreach (var item in items)
            {
                _actions[typeof(TMessage)].Remove(item);
            }

        }
    }

    public class MessageAction
    {
        private object _target;
        private Delegate _action;

        public object Target { get { return _target; } }
        public MessageAction(object target, Delegate action)
        {
            _target = target;
            _action = action;
        }
        public void Execute<T>(T param)
        {
            _action.Method.Invoke(_target, new object[] { param });
        }
    }
}