using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Application.Services.UserData
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<TKey> _keys = new();
        [SerializeField] private List<TValue> _values = new();

        public void OnBeforeSerialize()
        {
            _keys.Clear();
            _values.Clear();

            foreach (var pair in this)
            {
                _keys.Add(pair.Key);
                _values.Add(pair.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();

            if (_keys.Count != _values.Count)
                throw new Exception(
                    $"Mismatch in number of keys ({_keys.Count}) and values ({_values.Count}). Ensure key and value types are serializable.");

            for (var i = 0; i < _keys.Count; i++)
            {
                if (_keys != null && _values != null && _keys[i] != null && _values[i] != null)
                {
                    Add(_keys[i], _values[i]);
                }
            }
        }
    }
}