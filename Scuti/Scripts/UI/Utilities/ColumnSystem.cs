using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace Scuti.UI
{
	public class ColumnSystem : MonoBehaviour
	{
		[Tooltip("This is the width of the column without the space between them, only the content width")]
		public float ColumnWidth;

		[SerializeField] private RectTransform _columPref;
		[SerializeField] private RectTransform _container;
		
		private List<RectTransform> _columns;
		private List<float> _heights = new List<float>();

		internal void Init()
		{
#if UNITY_EDITOR
			Debug.Assert(ColumnWidth > 0, "Column width not assigned");
#endif

			var transf = GetComponent<RectTransform>();
			var containerSize = GetComponent<RectTransform>().rect.width;
			var numberOfColumns = Math.Floor(containerSize / (ColumnWidth + 50));
			while (_columns.Count < numberOfColumns)
			{
				_columns.Add(Instantiate(_columPref, _container));
				_heights.Add(0);
			}
		}

		public OfferSummaryPresenterUniversal InstantiateWidget(OfferSummaryPresenterUniversal obj)
		{
#if UNITY_EDITOR
			Debug.Assert(_columns != null, "Columns not assigned");
#endif
			// 1. Select the shortest Column
			float height = float.MaxValue;
			int minCol = 0;
			for (int i = 0; i < _heights.Count; i++)
			{
				float col = _heights[i];
				if (col < height)
				{
					height = col;
					minCol = i;
				}
			}
			// 2. Add widget to column
			_heights[minCol] += obj.IsTall ? 2 : 1;
			return Instantiate(obj, _columns[minCol]);
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
