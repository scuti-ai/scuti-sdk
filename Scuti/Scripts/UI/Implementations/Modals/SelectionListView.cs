 
﻿using System;

using UnityEngine;
using UnityEngine.UI;

namespace Scuti {
    public class SelectionListView : View {
        public event Action<string> OnClickOption;

        [SerializeField] Text titleText;
        [SerializeField] SelectionListOptionView optionTemplate;
        [SerializeField] Transform container;
        [SerializeField] string[] options;

        protected override void Awake() {
            base.Awake();
            Clear();
            Add(options);
        }

        public SelectionListView SetTitle(string title) {
            titleText.text = title;
            return this;
        }

        public SelectionListView Clear() {
            while (container.childCount > 1)
                Destroy(container.GetChild(1).gameObject);
            return this;
        }

        public SelectionListView Add(string[] options) {
            this.options = options;
            foreach (var option in options) {
                var instance = Instantiate(optionTemplate, container);
                instance.gameObject.SetActive(true);
                instance.gameObject.name = option;
                instance.SetText(option);
                instance.OnClick += clickedOption => {
                    OnClickOption?.Invoke(clickedOption);
                };
            }
            return this;
        }
    }
} 
