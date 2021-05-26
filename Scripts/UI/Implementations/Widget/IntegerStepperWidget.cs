 
﻿using System;

using UnityEngine;
using UnityEngine.UI;

namespace Scuti {
    public class IntegerStepperWidget : MonoBehaviour {
        public event Action<int> OnValueChanged;

        int _value;
        public int Value {
            get {
                return _value;
            }
            set {
                _value = value;
                valueText.text = value.ToString();
                OnValueChanged?.Invoke(_value);
            }
        }

        [SerializeField] Text valueText;
        [SerializeField] int minValue = 0;

        public void StepUp() {
            Value++;
        }

        public void StepDown() {
            if(_value == minValue)
                return;
            Value--;
        }
    }
} 
