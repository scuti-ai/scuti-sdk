using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.ComponentModel;

namespace Scuti{
	public class LabelledImageWidget : MonoBehaviour {
		public object data;

		[SerializeField] bool isSelectable;
		[SerializeField] Text stringDisplay;
		[SerializeField] Image spriteDisplay;
		[SerializeField] Image iconImage;
		[SerializeField] GameObject selectedDisplay;


		public Action<LabelledImageWidget> onInteract;
		[ShowIf("isSelectable")] public Action<string> onSelect;
		[ShowIf("isSelectable")] public Action<string> onDeselect;



		[ReadOnly] [SerializeField] bool m_IsSelected = false;
		public bool IsSelected{
			get{ return m_IsSelected;}
			private set{m_IsSelected = value;}
		}

        private void Awake()
        {
            Refresh();
        }

        private Enum _enumVal;

        public void SetIcon(Sprite sprite)
        {
            iconImage.sprite = sprite;
        }

        public void SetLabel(Enum enumVal)
        {
            _enumVal = enumVal;
            SetLabel(enumVal.GetAttributeOfType<DescriptionAttribute>().Description);
        }

        public Enum GetEnumValue()
        {
            return _enumVal;
        }

		public void SetLabel(string stringVal){
			stringDisplay.text = stringVal;
		}

		public LabelledImageWidget SetSprite(Sprite sprite){
			spriteDisplay.sprite = sprite;
			return this;
		}

		public void Interact(){
			if(isSelectable)
				ToggleSelection();

			onInteract?.Invoke(this);
		}

        void ToggleSelection() {
            Select(!IsSelected);
        }

        public void Select(bool value)
        {
            IsSelected = value;
			if(IsSelected){
				onSelect?.Invoke(stringDisplay.text);
			}
			else {
				onDeselect?.Invoke(stringDisplay.text);
			}
            Refresh();
        }

        void Refresh()
        {
            selectedDisplay.SetActive(IsSelected);
        }

    }
}
