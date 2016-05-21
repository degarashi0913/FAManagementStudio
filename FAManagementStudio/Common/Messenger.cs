using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FAManagementStudio.Common
{
    public class Messenger
    {
        private static readonly Messenger _instance = new Messenger();
        public static Messenger Instance { get { return _instance; } }
        private Dictionary<Type, List<Delegate>> _actions = new Dictionary<Type, List<Delegate>>();
        public void Register<TMessage>(object recipient, Action<TMessage> action)
        {
            var type = typeof(TMessage);
            if (!_actions.ContainsKey(type))
            {
                _actions.Add(type, new List<Delegate>());
            }
            var list = _actions[type];

            list.Add(action);

        }
        public void Send<TMessage>(TMessage message)
        {
            var actions = _actions[typeof(TMessage)].ToList();
            foreach (Action<TMessage> item in actions)
            {
                item(message);
            }
        }

        public void Unregister<TMessage>(object recipient)
        {
            _actions[typeof(TMessage)].Clear();
        }
    }
}
