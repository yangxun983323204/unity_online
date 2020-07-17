using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// 消息分发器
/// </summary>
public class MsgDispatcher<TKey,TValue>
{
    Dictionary<TKey, Action<TValue>> _listeners = new Dictionary<TKey, Action<TValue>>();

    public void AddListener(TKey key, Action<TValue> listener)
    {
        Debug.Assert(listener != null);
        if (_listeners.ContainsKey(key))
            _listeners[key] += listener;
        else
            _listeners.Add(key, listener);
    }

    public void RemoveListener(TKey key, Action<TValue> listener)
    {
        Debug.Assert(listener != null);
        if (_listeners.ContainsKey(key))
            _listeners[key] -= listener;

        if (_listeners[key] == null)
            _listeners.Remove(key);
    }

    public void Trigger(TKey key,TValue value)
    {
        if (_listeners.ContainsKey(key))
            _listeners[key](value);
    }
}
