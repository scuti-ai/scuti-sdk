using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UI;

namespace Scuti.UI
{
	public class ColumnSystem : MonoBehaviour
	{
		[Tooltip("This is the width of the column without the space between them, only the content width")]

		[SerializeField] private RectTransform _columPref;
		[SerializeField] private RectTransform _container;
		[SerializeField] private ScrollRect _scrollRect;
		[Tooltip("How close to the end it's going to check for new offers")]
		[SerializeField] [Slider(0,1)] private float _scrollThreshold;
		[Tooltip("If you want to add a delay to the check in order to control the frecuency of new offers")]
		[SerializeField] private float _delay;
		private float _time;
		
		private List<RectTransform> _columns = new List<RectTransform>();
		private List<float> _heights = new List<float>();
		private List<GameObject> _children = new List<GameObject>();

		internal bool isInitialized = false;
		internal Action ScrollPass;

		internal void Init(float columnWidth)
		{

#if UNITY_EDITOR
			Debug.Assert(columnWidth > 0, "Column width not assigned");
#endif
			var transf = GetComponent<RectTransform>();
			var containerSize = GetComponent<RectTransform>().rect.width;
			var numberOfColumns = Math.Floor(containerSize / (columnWidth + 50));
			while (_columns.Count < numberOfColumns)
			{
				_columns.Add(Instantiate(_columPref, _container));
				_heights.Add(0);
			}
			isInitialized = true;

			_scrollRect.onValueChanged.AddListener(OnScroll);
		}

		private void OnScroll(Vector2 arg0)
		{
			if (_scrollRect.verticalNormalizedPosition < _scrollThreshold && _time > _delay)
			{
				ScrollPass?.Invoke();
				_time = 0;
			}
		}
		private void Update()
		{
			if (isInitialized)
			{
				if(_time <= _delay)
				{
					_time += Time.deltaTime;
				}
			}
		}

		public OfferSummaryPresenterUniversal InstantiateWidget(OfferSummaryPresenterUniversal obj)
		{
#if UNITY_EDITOR
			Debug.Assert(_columns != null, "Columns not assigned");
			Debug.Assert(_columns.Count > 0, "Columns not assigned");
#endif
			// 1. Select the shortest Column
			int minCol = ShortestColumn();
			// 2. Add widget to column
			if(_heights.Count > minCol)
			{
				_heights[minCol] += obj.IsTall ? 2 : 1;
			}
			var child = Instantiate(obj, _columns[minCol]);
			_children.Add(child.gameObject);
			return child;
		}

		private int ShortestColumn()
		{
			if(_columns.Count == 1)
			{
				return 0;
			}

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

			return minCol;
		}

		internal void Clear()
		{
			if (_columns == null) return;
			foreach (var col in _columns)
			{
				foreach (Transform child in col)
					if(child != col)
						Destroy(child.gameObject);
			}
		}

		public List<GameObject> GetChildren()
		{
			return _children;
		}
	}
}
