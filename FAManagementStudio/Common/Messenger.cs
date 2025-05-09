using System;
using System.Collections.Generic;
using System.Linq;

namespace FAManagementStudio.Common;

public class Messenger
{
    private static readonly Messenger _instance = new ();
    public static Messenger Instance { get { return _instance; } }
    private readonly Dictionary<Type, List<MessageAction>> _actions = [];
    public void Register<TMessage>(object recipient, Action<TMessage> action)
    {
        var type = typeof(TMessage);
        if (!_actions.TryGetValue(type, out List<MessageAction>? list))
        {
            list = [];
            _actions.Add(type, list);
        }

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

public class MessageAction(object target, Delegate action)
{
    public object Target => target; 

    public void Execute<T>(T param)
    {
        action.Method.Invoke(target, [param]);
    }
}