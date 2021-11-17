using System;
using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors
{
    public class TimedDictionary<TKey, TValue>
    {
        private Dictionary<TKey, (DateTime, TValue)> _dictionary;

        public  Dictionary<TKey, (DateTime, TValue)>.KeyCollection Keys => _dictionary.Keys;

        public TimedDictionary()
        {
            _dictionary = new Dictionary<TKey, (DateTime, TValue)>();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if(_dictionary.TryGetValue(key, out (DateTime, TValue) v))
            {
                value = v.Item2;
                return true;
            }
            value = default(TValue);
            return false;
        }

        public void SetValue(TKey key, TValue value)
        {
            _dictionary[key] = (DateTime.Now, value);
        }

        public void SetValue(TKey key, TValue value, DateTime date)
        {
            _dictionary[key] = (date, value);
        }

        public void ClearOld()
        {
            var keys = _dictionary.Keys;
            var oldestDate = DateTime.Now.AddDays(-30);
            foreach(var key in keys)
            {
                if(_dictionary[key].Item1 < oldestDate)
                    _dictionary.Remove(key);
            }
        }

        public List<(TKey, TValue, DateTime)> GetSerializedDictionary()
        {
            var serializedDictionary = new List<(TKey, TValue, DateTime)>();
            var keys = _dictionary.Keys;
            foreach(var key in keys)
                serializedDictionary.Add((key, _dictionary[key].Item2, _dictionary[key].Item1));
            return serializedDictionary;
        }
    }
}