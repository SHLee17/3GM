using System.Collections.Generic;


public class CMultyDictionary<TKey, TValue>
{
    Dictionary<TKey, List<TValue>> dictionary;

    public CMultyDictionary()
    {
        dictionary = new Dictionary<TKey, List<TValue>>();
    }

    public void Add(TKey _key, TValue _value)
    {
        List<TValue> list;

        if (dictionary.TryGetValue(_key, out list))
        {
            list.Add(_value);
        }
        else
        {
            list = new List<TValue>();
            list.Add(_value);
            dictionary.Add(_key, list);
        }

    }
    public bool DuplicateValue(TValue _value)
    {
        if (dictionary.Count > 0)
        {
            foreach (var key in dictionary.Keys)
            {
                foreach (var value in dictionary[key])
                {
                    if (value.Equals(_value))
                    {
                        return true;
                    }

                }
            }
        }

        return false;
    }
    public int Count
    {
        get
        {
            return dictionary.Count;
        }
    }
    public void Remove(TKey key)
    {
        //dictionary[key].Clear();
        dictionary.Remove(key);
    }
    public void Clear()
    {
        dictionary.Clear();
    }
    public IEnumerable<TKey> Keys
    {
        get
        {
            return dictionary.Keys;
        }
    }
    public void RemoveValue(TKey _key ,TValue _value)
    {
        dictionary[_key].Remove(_value);
    }
    public List<TValue> this[TKey key]
    {
        get
        {
            List<TValue> list;
            if (dictionary.TryGetValue(key, out list))
            {
                return list;
            }
            else
            {
                return new List<TValue>();
            }
        }
    }
}
