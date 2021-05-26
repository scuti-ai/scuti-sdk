using System;
using UnityEngine;

[Serializable]
public class GenericBinding<T> {
    public event Action<T> OnValueChanged;

    [SerializeField] T _value;
    public T Value {
        get { return _value; }
        set {
            _value = value;
            OnValueChanged?.Invoke(_value);
        }
    }

    public void Invoke(){
        OnValueChanged?.Invoke(_value);
    }

    public override string ToString() {
        return Value.ToString();
    }
}