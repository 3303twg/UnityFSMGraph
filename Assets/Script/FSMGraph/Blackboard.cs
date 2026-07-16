using System;
using System.Collections;
using System.Collections.Generic;

public class Blackboard
{
    public Dictionary<string, object> blackboardDic = new Dictionary<string, object>();
    public event Action OnChanged; //이런게 있어야 UI를 갱신하든 하것지


    public object Get(string key)
    {
        var value = blackboardDic[key];

        return (object)value;
    }


    /*
    public T Get<T>(string key)
    {
        var value = blackboardDic[key];

        return (T)value;
    }
    */

    public bool TryGet<T>(string key, out T value)
    {
        if(blackboardDic.TryGetValue(key, out object dicValue))
        {
            value = (T)dicValue;
            return true;
        }
        value = default;
        return false;
    }

    public void Set<T>(string key, T value)
    {
        blackboardDic[key] = value;
        OnChanged?.Invoke();
    }

}
