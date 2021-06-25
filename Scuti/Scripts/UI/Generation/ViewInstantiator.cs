﻿using Scuti.UI;
using System;
using UnityEngine;

namespace Scuti {
    [Serializable]
    public class ViewInstantiator {
        public string id = "New View Instantiator";
        public View view;
        public View view_portrait;
        public View view_pc;
        public Transform parent;

        [ReadOnly] [SerializeField] View instance;

        void Start() { }

        void OnValidate() {
            if (id.Contains("/"))
                id.Replace("/", "");
        }

        public View Instantiate() {

            float pixelsWide = Camera.main.pixelWidth;
            float pixelsHigh = Camera.main.pixelHeight;

            bool portrait = pixelsHigh > pixelsWide;
            if(ScutiConstants.FORCE_LANDSCAPE)
            {
                portrait = false;
            }

            var v = portrait && view_portrait != null ? view_portrait : (UIManager.IsLargeDisplay() && view_pc!=null) ? view_pc :  view;
            instance = MonoBehaviour.Instantiate(v, parent);
            instance.gameObject.hideFlags = HideFlags.DontSave;
            return instance;
        }

        internal void Destroy() {
            MonoBehaviour.Destroy(instance.gameObject);
        }
    }
}