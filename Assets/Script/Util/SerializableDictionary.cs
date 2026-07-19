using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableKeyValue<K,V> where K : class
{
    public K key;
    public V value;
}

[Serializable]
public class SerializableDictionary<K, V> : Dictionary<K, V>, ISerializationCallbackReceiver where K : class
{
    [SerializeField]
    List<SerializableKeyValue<K, V>> keyValueList = new List<SerializableKeyValue<K, V>>();

    public void OnBeforeSerialize()
    {
        if(this.Count < keyValueList.Count)
        {
            return;
        }

        keyValueList.Clear();

        foreach(var kv in this) //인터페이스 개신기하네
        {
            if(kv.Value.GetType() == typeof(object))
            {
                Debug.Log("옵젝");
            }
            keyValueList.Add(new SerializableKeyValue<K, V>()
            {
                key = kv.Key,
                value = kv.Value
            });
        }
    }


    public void OnAfterDeserialize()
    {
        this.Clear();

        foreach(var kv in keyValueList)
        {
            if(!this.TryAdd(kv.key, kv.value))
            {
                Debug.Log("실패");
            }
        }
    }

}
