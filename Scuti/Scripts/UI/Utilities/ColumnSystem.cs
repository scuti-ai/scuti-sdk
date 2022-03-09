using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace Scuti.UI
{
	public class ColumnSystem : MonoBehaviour
	{
		[SerializeField] private List<RectTransform> _columns;
		public OfferSummaryPresenterUniversal InstantiateWidget(OfferSummaryPresenterUniversal obj)
		{
#if UNITY_EDITOR
			Debug.Assert(_columns != null, "Columns not assigned");
#endif
			// 1. Select the shortest Column
			float height = float.MaxValue;
			RectTransform minCol = null;
			foreach (var col in _columns)
			{
				if(col.sizeDelta.y < height)
				{
					height = col.sizeDelta.y;
					minCol = col;
				}
			}
			if (minCol == null) return null;

			// 2. Add widget to column
			return Instantiate(obj, minCol);
		}

		internal void Clear()
		{
			foreach (var col in _columns)
			{
				foreach (Transform child in col)
					if(child != col)
						Destroy(child.gameObject);
			}
		}
	}
}
