using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace UraniumCompute.Utils;

internal sealed class LRUCache<TKey, TValue> : IDictionary<TKey, TValue>
    where TKey : notnull
{
    public ICollection<TKey> Keys => cacheMap.Keys;

    public ICollection<TValue> Values =>
        throw new NotSupportedException($"Cannot get value collection of {nameof(LRUCache<TKey, TValue>)}");

    public int Count => items.Count;
    public bool IsReadOnly => false;

    public TValue this[TKey key]
    {
        get
        {
            if (cacheMap.TryGetValue(key, out var node))
            {
                var value = node.Value.Value;
                items.Remove(node);
                items.AddLast(node);
                return value;
            }

            throw new KeyNotFoundException($"Key {key} was not in the cache");
        }
        set => cacheMap[key].ValueRef = new KeyValuePair<TKey, TValue>(key, value);
    }

    private readonly int capacity;
    private readonly Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> cacheMap = new();
    private readonly LinkedList<KeyValuePair<TKey, TValue>> items = new();

    public LRUCache(int capacity)
    {
        this.capacity = capacity;
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (cacheMap.TryGetValue(key, out var node))
        {
            value = node.Value.Value;
            return true;
        }

        value = default;
        return false;
    }

    public void Add(TKey key, TValue val)
    {
        if (cacheMap.TryGetValue(key, out var existingNode))
        {
            items.Remove(existingNode);
        }
        else if (cacheMap.Count >= capacity)
        {
            RemoveFirst();
        }

        var cacheItem = new KeyValuePair<TKey, TValue>(key, val);
        var node = new LinkedListNode<KeyValuePair<TKey, TValue>>(cacheItem);
        items.AddLast(node);
        cacheMap[key] = node;
    }

    public bool ContainsKey(TKey key)
    {
        return cacheMap.ContainsKey(key);
    }

    public bool Remove(TKey key)
    {
        if (!ContainsKey(key))
        {
            return false;
        }

        var node = cacheMap[key];
        items.Remove(node);
        cacheMap.Remove(key);
        return true;
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return items.GetEnumerator();
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        items.Clear();
        cacheMap.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return cacheMap.TryGetValue(item.Key, out var value)
               && Equals(value.Value.Value, item.Value);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        items.CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        if (!Contains(item))
        {
            return false;
        }

        Remove(item.Key);
        return true;
    }

    private void RemoveFirst()
    {
        var node = items.First!;
        items.RemoveFirst();
        cacheMap.Remove(node.Value.Key);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
