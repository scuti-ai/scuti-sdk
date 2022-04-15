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
		[SerializeField] private int _gapeBetweenColumns;
		[Tooltip("How close to the end it's going to check for new offers")]
		[SerializeField] [Slider(0,1)] private float _scrollThreshold;
		[Tooltip("If you want to add a delay to the check in order to control the frecuency of new offers")]
		[SerializeField] private float _delay;
		private float _time;
		
		private List<RectTransform> _columns = new List<RectTransform>();
		private List<float> _heights = new List<float>();
		private List<Transform> _children = new List<Transform>();

		internal bool isInitialized = false;
		internal Action ScrollPass;

		private float columnSpacing;

		internal void Init(float columnWidth)
		{

#if UNITY_EDITOR
			Debug.Assert(columnWidth > 0, "Column width not assigned");
#endif
			var transf = GetComponent<RectTransform>();
			var containerSize = GetComponent<RectTransform>().rect.width;
			var numberOfColumns = Math.Max(1, Math.Floor(containerSize / (columnWidth + _gapeBetweenColumns)));
			while (_columns.Count < numberOfColumns)
			{
				_columns.Add(Instantiate(_columPref, _container));
				_heights.Add(0);
			}
			isInitialized = true;

			_scrollRect.onValueChanged.AddListener(OnScroll);
			columnSpacing = _columPref.GetComponent<VerticalLayoutGroup>().spacing;
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
			_children.Add(child.transform);
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
			foreach (var widget in _children)
			{
				RemoveElementFromItsColumn(widget);
				Destroy(widget.gameObject);
			}
			_children.Clear();
		}

		private int RemoveElementFromItsColumn (Transform widget)
		{
			var col = _columns.First(x => x.Find(widget.name));
			var index = _columns.IndexOf(col);
			_heights[index] -= widget.GetComponent<OfferSummaryPresenterUniversal>().IsTall ? 2 : 1;

			return index;
		}


		internal void InfiniteScroll()
		{
			if (_children.Count <= 0) return;
			// Get the frist element and add it to the end of the list.
			var selected = _children[0];
			_children.Remove(selected);
			var col = RemoveElementFromItsColumn(selected);

			_children.Add(selected);
			int minCol = ShortestColumn();
			selected.SetParent(_columns[minCol]); 
			selected.SetAsLastSibling();
			_heights[minCol] += selected.GetComponent<OfferSummaryPresenterUniversal>().IsTall ? 2 : 1;
			var tempPos = _scrollRect.content.anchoredPosition;
			tempPos.y -= selected.GetComponent<RectTransform>().sizeDelta.y - columnSpacing;
			_scrollRect.content.anchoredPosition = tempPos;
		}

		internal void Remove(OfferSummaryPresenterUniversal widget)
		{
			_children.Remove(widget.transform);
			RemoveElementFromItsColumn(widget.transform);
		}
	}
}