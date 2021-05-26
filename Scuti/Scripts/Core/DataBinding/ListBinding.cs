using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using Scuti;

[SerializeField]
public class ListBinding<T> : IEnumerable<T> {
    public event Action OnModify;
    
    public event Action OnListChanged;
    public event Action OnCleared;
    
    public event Action<IEnumerable<T>> OnAdded;
    public event Action<IEnumerable<T>> OnRemoved;

    public event Action<T> OnElementAdded;
    public event Action<IEnumerable<T>> OnRangeAdded;

    public event Action<T> OnElementRemoved;
    public event Action<IEnumerable<T>> OnRangeRemoved;

    [SerializeField] [ReorderableList] List<T> _list = new List<T>();

    public List<T> List {
        get {
            return _list.ToList();
        }
        set {
            _list = value;
            OnListChanged?.Invoke();
            OnModify?.Invoke();
        }
    }

    public int Count {
        get { return List.Count; }
    }

    public void Clear() {
        _list.Clear();
        OnCleared?.Invoke();
        OnModify?.Invoke();
    }

    public void Add(T element) {
        _list.Add(element);
        OnElementAdded?.Invoke(element);
        OnAdded?.Invoke(new List<T> { element });
        OnModify?.Invoke();
    }

    public void AddRange(IEnumerable<T> elements) {
        _list.AddRange(elements);
        OnRangeAdded(elements);
        OnAdded?.Invoke(elements);
        OnModify?.Invoke();
    }

    public void Remove(T element) {
        if (_list.Contains(element)) {
            _list.Remove(element);
            OnElementRemoved?.Invoke(element);
            OnRemoved?.Invoke(new List<T> { element });
            OnModify?.Invoke();
        }
        else
            throw new Exception($"{element} not found in list. Cannot be removed");
    }

    public void RemoveAt(int index) {
        var element = _list[index];
        _list.RemoveAt(index);
        OnElementRemoved?.Invoke(element);
        OnRemoved?.Invoke(new List<T> { element });
        OnModify?.Invoke();
    }

    public void RemoveRange(int index, int count) {
        var elements = _list.GetRange(index, count);
        _list.RemoveRange(index, count);
        OnRemoved?.Invoke(elements);
        OnRangeRemoved?.Invoke(elements);
        OnModify?.Invoke();
    }

    public IEnumerator<T> GetEnumerator() {
        foreach (var element in _list)
            yield return element;
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}