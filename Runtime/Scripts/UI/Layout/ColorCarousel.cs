 
﻿using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Scuti {
    // Make code clearer
    public class ColorCarousel : MonoBehaviour {
        public event Action<Color> OnValueChanged;

        [SerializeField] ColorCarouselElement m_Template;
        [SerializeField] Transform m_Container;

        List<ColorCarouselElement> m_Instances = new List<ColorCarouselElement>();

        public int m_SelectedIndex;

        public Color Value {
            get { return m_Instances[m_SelectedIndex].Value; }
        }

        public void AddColors(Color[] colors) {
            if (colors == null || colors.Length == 0) return;

            foreach (var color in colors) {
                var instance = Instantiate(m_Template, m_Container);
                instance.gameObject.hideFlags = HideFlags.DontSave;
                instance.gameObject.SetActive(true);
                m_Instances.Add(instance);

                instance.SetColor(color);
                instance.Deselect();

                // When an instance is clicked, deselect all the others
                // then update the index and fire events
                instance.OnSelected += () => {
                    m_Instances.Where(x => x != instance)
                        .ToList()
                        .ForEach(x => x.Deselect());

                    m_SelectedIndex = m_Instances.IndexOf(instance);
                    OnValueChanged?.Invoke(color);
                };
            }
            if (m_Instances.Count > 0)
                m_Instances[0].Select();
        }

        public void Clear() {
            foreach (var instance in m_Instances)
                Destroy(instance.gameObject);
            m_Instances.Clear();
        }
    }
} 
