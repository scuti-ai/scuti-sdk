using System;
using UnityEngine;
using UnityEngine.UI;
 
namespace Scuti { 
    // Make code clearer
    public class ColorCarouselElement : MonoBehaviour {
        public event Action OnSelected;
        public event Action OnDeselected;

        [SerializeField] Vector2 m_DeselectedSize;
        [SerializeField] Vector2 m_SelectedSize;

        RectTransform m_RT;
        Image m_Image;

        void Awake() {
            m_RT = GetComponent<RectTransform>();
            m_Image = GetComponent<Image>();
        }

        public void SetColor(Color color) {
            m_Image.color = color;
        }

        public Color Value {
            get { return m_Image.color; }
        }

        public void Select() {
            m_RT.sizeDelta = m_SelectedSize;
            OnSelected?.Invoke();
        }

        public void Deselect() {
            m_RT.sizeDelta = m_DeselectedSize;
            OnDeselected?.Invoke();
        }
    }
}
