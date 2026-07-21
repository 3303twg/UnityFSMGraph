using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[Serializable]
public class Blackboard : ISerializationCallbackReceiver
{
    [SerializeReference]
    List<BlackboardVariable> variables = new List<BlackboardVariable>();
    [NonSerialized]
    public Dictionary<string, BlackboardVariable> lookUp = new Dictionary<string, BlackboardVariable>();


    public event Action OnChanged; //이런게 있어야 UI를 갱신하든 하것지

    public object Get(string key)
    {
        return lookUp.TryGetValue(key, out var v) ? v.GetValue() : null;
    }

    public bool TryGet<T>(string key, out T value)
    {
        if(lookUp.TryGetValue(key, out var v) && v.GetValue() is T typedValue)
        {
            value = typedValue;
            return true;
        }
        value = default;
        return false;
    }

    public void Set(string key, object value)
    {
        if (!lookUp.TryGetValue(key, out var v))
            throw new KeyNotFoundException();

        v.SetValue(value);
        OnChanged?.Invoke();
    }


    public void Add(BlackboardVariable variable)
    {
        if (lookUp.ContainsKey(variable.key))
            throw new ArgumentException($"중복 키: {variable.key}");
        variables.Add(variable);
        lookUp.Add(variable.key, variable);
    }

    public void Add<T>(BlackboardVariable<T> variable)
    {
        if (lookUp.ContainsKey(variable.key))
            throw new ArgumentException($"중복 키: {variable.key}");
        variables.Add(variable);
        lookUp.Add(variable.key, variable);
    }

    public void Remove(string key)
    {
        if (!lookUp.Remove(key, out var variable))
            return;
        variables.Remove(variable);
    }

    public void Rename(string oldKey, string newKey)
    {
        if (!lookUp.Remove(oldKey, out var v))
            return;
        if (lookUp.ContainsKey(newKey))
            throw new ArgumentException($"중복 키: {newKey}");
        v.key = newKey;
        lookUp.Add(newKey, v);
    }

    public void OnBeforeSerialize()
    {
    }

    public void OnAfterDeserialize()
    {
        lookUp = new Dictionary<string, BlackboardVariable>();

        foreach(var v in variables)
        {
            if (v != null && !string.IsNullOrWhiteSpace(v.key))
                lookUp[v.key] = v;
        }

        OnChanged?.Invoke();
    }

    public static BlackboardVariable CreateVariable(Type type, string key)
    {
        if (type == typeof(float))
            return new FloatVariable { key = key, value = 0f };
        if (type == typeof(bool))
            return new BoolVariable { key = key, value = false };
        if (type == typeof(string))
            return new StringVariable { key = key, value = string.Empty };
        if (typeof(UnityEngine.GameObject).IsAssignableFrom(type))
            return new GameObjectVariable { key = key, value = null };
        throw new NotSupportedException($"Unsupported blackboard type: {type}");
    }

    public Blackboard Clone()
    {
        var clone = new Blackboard();

        foreach(var variable in variables)
        {
            clone.Add(variable switch
            {
                FloatVariable v => new FloatVariable
                {
                    key = v.key,
                    value = v.value
                },

                BoolVariable v => new BoolVariable
                {
                    key = v.key,
                    value = v.value
                },

                StringVariable v => new StringVariable
                {
                    key = v.key,
                    value = v.value
                },

                GameObjectVariable v => new GameObjectVariable
                {
                    key = v.key,
                    value = v.value
                },
                _ => throw new NotSupportedException(variable.GetType().Name)
            });
        }

        return clone;
    }
}


[Serializable]
public abstract class BlackboardVariable
{
    public string key;
    public abstract object GetValue();
    public abstract void SetValue(object value);
}

[Serializable]
public class BlackboardVariable<T> : BlackboardVariable
{
    public T value;
    public override object GetValue() => value;
    public override void SetValue(object v) => value = (T)v;
}


[Serializable]
public sealed class FloatVariable : BlackboardVariable
{
    public float value;

    public override object GetValue() => value;
    public override void SetValue(object v) => value = Convert.ToSingle(v);
}

[Serializable]
public sealed class BoolVariable : BlackboardVariable
{
    public bool value;

    public override object GetValue() => value;
    public override void SetValue(object v) => value = Convert.ToBoolean(v);
}

[Serializable]
public sealed class StringVariable : BlackboardVariable
{
    public string value;

    public override object GetValue() => value;
    public override void SetValue(object v) => value = Convert.ToString(v);
}

[Serializable]
public sealed class GameObjectVariable : BlackboardVariable
{
    public UnityEngine.GameObject value;

    public override object GetValue() => value;
    public override void SetValue(object v) => value = v as UnityEngine.GameObject;
}