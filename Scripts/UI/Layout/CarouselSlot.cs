 
﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Scuti {
    public class CarouselSlot : MonoBehaviour {
        public void Add(GameObject go) {
            go.transform.SetParent(transform, false);
            gameObject.SetActive(false);
            gameObject.SetActive(true);
        }

        public bool IsFull() {
            float maxHeight = GetComponentInParent<ScrollRect>().GetComponent<RectTransform>().rect.height;
            var group = LayoutGroup;

            float totalPadding =
                group.padding.top +
                group.padding.bottom +
                group.padding.left +
                group.padding.right;

            float totalSpacing = group.spacing * Mathf.Clamp(GetChildRects().Count - 1, 0, Mathf.Infinity);
            float totalElementHeight = GetChildRects().Sum(x => x.sizeDelta.y);

            float totalHeight = totalPadding + totalSpacing + totalElementHeight;
            return totalHeight > maxHeight;
        }

        public List<RectTransform> GetChildRects() {
            var result = new List<RectTransform>();
            for (int i = 0; i < transform.childCount; i++) {
                var rect = transform.GetChild(i).GetComponent<RectTransform>();
                result.Add(rect);
            }
            return result;
        }

        public HorizontalOrVerticalLayoutGroup LayoutGroup {
            get { return GetComponent<HorizontalOrVerticalLayoutGroup>(); }
        }
    }
} 