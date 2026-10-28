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

    public bool TryGetFloat(string key, out float value)
    {
        if (lookUp.TryGetValue(key, out var variable) && variable is FloatVariable floatVariable)
        {
            value = floatVariable.value;
            return true;
        }

        value = 0f;
        return false;
    }

    public bool Compare(string key, CompareOperatorType op, float rightNumber, string rightText, bool rightBool)
    {
        if (!lookUp.TryGetValue(key, out var variable) || variable == null)
            return false;

        switch (variable)
        {
            case FloatVariable floatVariable:
                return CompareFloat(op, floatVariable.value, rightNumber);
            case IntVariable intVariable:
                return CompareFloat(op, intVariable.value, rightNumber);
            case StringVariable stringVariable:
                return CompareString(op, stringVariable.value, rightText);
            case BoolVariable boolVariable:
                return CompareBool(op, boolVariable.value, rightBool);
            default:
                return false;
        }
    }

    static bool CompareFloat(CompareOperatorType op, float left, float right)
    {
        switch (op)
        {
            case CompareOperatorType.Equal:
                return left == right;
            case CompareOperatorType.NotEqual:
                return left != right;
            case CompareOperatorType.Greater:
                return left > right;
            case CompareOperatorType.Less:
                return left < right;
            case CompareOperatorType.GreaterOrEqual:
                return left >= right;
            case CompareOperatorType.LessOrEqual:
                return left <= right;
            default:
                return false;
        }
    }

    static bool CompareString(CompareOperatorType op, string left, string right)
    {
        int cmp = string.CompareOrdinal(left, right);
        switch (op)
        {
            case CompareOperatorType.Equal:
                return cmp == 0;
            case CompareOperatorType.NotEqual:
                return cmp != 0;
            case CompareOperatorType.Greater:
                return cmp > 0;
            case CompareOperatorType.Less:
                return cmp < 0;
            case CompareOperatorType.GreaterOrEqual:
                return cmp >= 0;
            case CompareOperatorType.LessOrEqual:
                return cmp <= 0;
            default:
                return false;
        }
    }

    static bool CompareBool(CompareOperatorType op, bool left, bool right)
    {
        switch (op)
        {
            case CompareOperatorType.Equal:
                return left == right;
            case CompareOperatorType.NotEqual:
                return left != right;
            case CompareOperatorType.Greater:
                return left && !right;
            case CompareOperatorType.Less:
                return !left && right;
            case CompareOperatorType.GreaterOrEqual:
                return left == right || left;
            case CompareOperatorType.LessOrEqual:
                return left == right || !left;
            default:
                return false;
        }
    }

    //레거신가?
    public void Set(string key, object value, bool notify = true)
    {
        if (!lookUp.TryGetValue(key, out var v))
            throw new KeyNotFoundException();

        switch (v)
        {
            case FloatVariable floatVariable when value is float floatValue:
                floatVariable.value = floatValue;
                break;
            case BoolVariable boolVariable when value is bool boolValue:
                boolVariable.value = boolValue;
                break;
            case StringVariable stringVariable when value is string stringValue:
                stringVariable.value = stringValue;
                break;
            case StringVariable stringVariable when value == null:
                stringVariable.value = string.Empty;
                break;
            case GameObjectVariable gameObjectVariable:
                gameObjectVariable.value = value as GameObject;
                break;
            default:
                v.SetValue(value);
                break;
        }

        if (notify)
            OnChanged?.Invoke();
    }

    public void Set(string key, float value, bool notify = true)
    {
        if (!lookUp.TryGetValue(key, out var variable))
            throw new KeyNotFoundException();
        if (variable is not FloatVariable floatVariable)
            throw new InvalidCastException(variable.GetType().Name);

        floatVariable.value = value;
        if (notify)
            OnChanged?.Invoke();
    }

    public void Set(string key, int value, bool notify = true)
    {
        if (!lookUp.TryGetValue(key, out var variable))
            throw new KeyNotFoundException();
        if (variable is not IntVariable intVariable)
            throw new InvalidCastException(variable.GetType().Name);

        intVariable.value = value;
        if (notify)
            OnChanged?.Invoke();
    }

    public void Set(string key, bool value, bool notify = true)
    {
        if (!lookUp.TryGetValue(key, out var variable))
            throw new KeyNotFoundException();
        if (variable is not BoolVariable boolVariable)
            throw new InvalidCastException(variable.GetType().Name);

        boolVariable.value = value;
        if (notify)
            OnChanged?.Invoke();
    }

    public void Set(string key, string value, bool notify = true)
    {
        if (!lookUp.TryGetValue(key, out var variable))
            throw new KeyNotFoundException();
        if (variable is not StringVariable stringVariable)
            throw new InvalidCastException(variable.GetType().Name);

        stringVariable.value = value;
        if (notify)
            OnChanged?.Invoke();
    }

    //밸류 객체 Blackboard.CreateVariable로 생성할것 //
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

    //블랙보드 밸류 객체 생성
    public static BlackboardVariable CreateVariable(Type type, string key)
    {
        if (type == typeof(float))
            return new FloatVariable { key = key, value = 0f };
        if (type == typeof(int))
            return new IntVariable { key = key, value = 0 };
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
    public override void SetValue(object v) => value = v is float floatValue ? floatValue : Convert.ToSingle(v);
}

[Serializable]
public sealed class IntVariable : BlackboardVariable
{
    public int value;

    public override object GetValue() => value;
    public override void SetValue(object v) => value = (int)(v is int intValue ? intValue : Convert.ToSingle(v));
}

[Serializable]
public sealed class BoolVariable : BlackboardVariable
{
    public bool value;

    public override object GetValue() => value;
    public override void SetValue(object v) => value = v is bool boolValue ? boolValue : Convert.ToBoolean(v);
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