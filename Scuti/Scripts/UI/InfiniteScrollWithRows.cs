using Scuti.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfiniteScrollWithRows : MonoBehaviour
{

	[SerializeField] private GameObject widgetPrefab_Large;
	[SerializeField] private GameObject widgetPrefab_small;
	[SerializeField] private GameObject _rowPrefab;
	[SerializeField] private Transform _contentTransform;

	private int _numberOfOffers;
	private List<OfferSummaryPresenterUniversal> _offers;
	private int _currentOffer;

	public void Init( int numberOfOffers)
    {
		// Create Rows

		_numberOfOffers = numberOfOffers;

		// 1. Determine the amount of rows
		var tempColumnWidth = widgetPrefab_Large.GetComponent<RectTransform>().rect.width;
		var containerSize = GetComponent<RectTransform>().rect.width;
		var columnSpacing = _contentTransform.GetComponent<VerticalLayoutGroup>().spacing;
		var numberOfColumns = Math.Max(1, Math.Floor(containerSize / (containerSize + columnSpacing)));


		// 2. Generate the Rows
		_offers = new List<OfferSummaryPresenterUniversal>();
		for (int i = 0; i < numberOfOffers; i++)
		{
			var tempRow = Instantiate(_rowPrefab, _contentTransform);
			for (int j = 0; j < numberOfColumns; j++)
			{
				_offers.Add(Instantiate(widgetPrefab_Large, tempRow.transform).GetComponent<OfferSummaryPresenterUniversal>());
				_offers.Add(Instantiate(widgetPrefab_small, tempRow.transform).GetComponent<OfferSummaryPresenterUniversal>());
			}
		}
		_currentOffer = 0;
	}
	private OfferSummaryPresenterUniversal GetNextWidget(bool isTall)
	{
		if(_offers[_currentOffer].IsTall == isTall)
		{
			return _offers[_currentOffer];
		}
		else
		{
			if(_currentOffer >= _offers.Count)
			{
				return null;
			}
			else
			{
				_currentOffer++;
				return GetNextWidget(isTall);
			}
		}

	}

	public OfferSummaryPresenterUniversal InstantiateWidget(OfferSummaryPresenterUniversal obj)
	{
		OfferSummaryPresenterUniversal tempWidget = GetNextWidget(obj.IsTall);
		tempWidget.SetData(obj.Data);
		return tempWidget;
	}


}
