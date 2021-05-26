using System;

using UnityEngine;
using UnityEngine.UI;
 
namespace Scuti {
    public class SelectionListOptionView : MonoBehaviour { 
        public event Action<string> OnClick;

        [SerializeField] Text displayText;

        public void SetText(string text) {
            displayText.text = text;
        }

        public void Click() { 
            OnClick?.Invoke(displayText.text);
        }
    }
}
