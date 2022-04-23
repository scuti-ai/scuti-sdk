using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;
using Image = UnityEngine.UI.Image;


using Scuti.GraphQL.Generated;

using System.Threading;
using Scuti.Net;
using UnityEngine.Events;
using System.Linq;
using UnityEngine.UI;

namespace Scuti.UI
{
    public class OffersPresenterUniversal : OffersPresenterBase
    {

        [Header("Instantiation")]
        public List<OfferSummaryRowContainer> RowContainerPrefabs;


        private List<OfferSummaryRowContainer> _allRows = new List<OfferSummaryRowContainer>();
        private List<OfferSummaryTallSmallContainer> _allColumns = new List<OfferSummaryTallSmallContainer>();
        private List<OfferSummaryPresenterBase> _allCells = new List<OfferSummaryPresenterBase>();

        public ScutiInfiniteScroll InfinityScroll;
        public Transform OfferContainer;


        private int _columns = 2;
        private int _rows = 6;

        protected override void Awake()
        {
            base.Awake();

			if (Landscape())
			{
				var defaultrow = RowContainerPrefabs[0]; // -> TODO Is there a better way to get these numbers? -je
				var columnWidth = defaultrow.Columns[0].GetComponent<RectTransform>().rect.width /3f;
				var canvasWidth = OfferContainer.GetComponentInParent<Canvas>().GetComponent<RectTransform>().rect.width;
				var screenWidth = Screen.width;

				var containerSize = screenWidth * (1920f / canvasWidth);
				var numberOfColumns = Math.Max(1, Mathf.FloorToInt(containerSize / (columnWidth)));
				if(numberOfColumns >= RowContainerPrefabs.Count)
				{
					numberOfColumns = RowContainerPrefabs.Count - 1;
				}
				_columns = numberOfColumns;

			}
			else
			{
				_columns = 1;
			}


			// figure out columns here
			var prefab = RowContainerPrefabs[_columns - 1];

            for (var r = 0; r < _rows; r++)
            {
                var row = Instantiate(prefab, OfferContainer);


				_allRows.Add(row);
                for (var c = 0; c < row.Columns.Count; c++)
                {
                    var col = row.Columns[c];
                    _allColumns.Add(col);
                    _allCells.Add(col.Tall);
                    _allCells.AddRange(col.Small.Presenters.ToArray());
                }
                offerDataToRequest = _allColumns.Count*2;
            }

		}

		public void ResizeScrollRect()
		{
			if (_columns == 1) return;
			// scale ->
			// 4, 1920 => 0.521976  (screenwidth/row.rect.width)/ scaleFactor
			// 4, 2778 => 0.64399    0.7716666/1.1889
			// 3, 859 => 0.71172  

			var tempScale = Screen.width / (_allRows[0].transform.GetComponent<RectTransform>().rect.width + 125);
			var canvas = GetComponentInParent<Canvas>();
			var canvasScale = canvas.scaleFactor;

			OfferContainer.localScale = Vector3.one * (tempScale / canvasScale);
		}

		private bool Landscape()
		{
				var _landscape = Screen.orientation == ScreenOrientation.Landscape || Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight;
#if UNITY_EDITOR
				_landscape = Camera.main.pixelWidth > Camera.main.pixelHeight;
#endif
				return _landscape;
			
		}


		private int colorCount = 0;
        // ================================================
        #region API

        public override void Clear()
        {

            base.Clear();
            foreach (var presenter in _allColumns)
            {
                presenter.Clear();
            }
        }



        #endregion

        // ================================================
        #region PRESENTATION
        // ================================================
//        override protected void OnSetState()
//        {
//            Clear();
//#pragma warning disable 4014
//            _loadingSource = new CancellationTokenSource();
//            PopulateOffers(_loadingSource.Token);
//#pragma warning restore 4014
//        }


        async protected override Task PopulateOffers(CancellationToken cancelToken)
        {
            OfferSummaryPresenterBase.Model offerData = null;
            foreach (var row in _allRows)
            {
                if (cancelToken.IsCancellationRequested) return;

                var columns = row.Columns;
                foreach (var col in columns)
                {
                    var mediaType = ScutiUtils.RollForType(true);
                    offerData = Data.RequestOffer(mediaType);

                    // Fallback to products
                    if (offerData == null && mediaType != OfferService.MediaType.Product)
                    {
                        mediaType = OfferService.MediaType.Product;
                        offerData = Data.RequestOffer(mediaType);
                    }

                    OfferSummaryPresenterBase.Model offerData2 = null;
                    if (mediaType == OfferService.MediaType.Product)
                    {
                        offerData2 = Data.RequestOffer(mediaType);
                    }

                    List<OfferSummaryPresenterBase> presenters = col.GetPresenters(mediaType);

                    // still needed?
                    await Task.Delay((int)(instantiationInterval * 1000));

                    if (cancelToken.IsCancellationRequested) return;

                    int count = 0;
                    foreach (var presenter in presenters)
                    {
                        var oData = offerData;
                        if (count > 0) oData = offerData2;

                        m_Instantiated.Add(presenter);

                        presenter.Inject(GetNext);
                        presenter.gameObject.hideFlags = HideFlags.DontSave;

                        if (oData == null)
                        {
                            continue;
                        }

                        presenter.Data = oData;
                        presenter.FirstLoad = true;
                        presenter.OnLoaded -= OnWidgetLoaded;
                        presenter.OnLoaded += OnWidgetLoaded;
                        presenter.Data.isSingle = presenter.Single;
                        presenter.Data.LoadImage();
                        presenter.OnClick -= OnPresenterClicked;
                        presenter.OnClick += OnPresenterClicked;
                        count++;
                    }
                }
            }

            await Task.Delay(250);
           

            OnPopulateFinished?.Invoke();
            m_ChangingCategories = false;
        }

        private async void OnPresenterClicked(OfferSummaryPresenterBase presenter)
        {
            if (presenter.Data != null && !presenter.Data.ID.IsNullOrEmpty())
            {
                UIManager.ShowLoading(false);
                var id = presenter.Data.ID;
                var offer = await ScutiNetClient.Instance.Offer.GetOfferByID(id);
                if (!ScutiUtils.TryOpenLink(offer))
                {
                    var panelModel = Mappers.GetOfferDetailsPresenterModel(offer);

                    try
                    {
                        UIManager.OfferDetails.SetData(panelModel);
                        UIManager.OfferDetails.SetIsVideo(!string.IsNullOrEmpty(presenter.Data.VideoURL));
                        UIManager.Open(UIManager.OfferDetails);
                    }
                    catch (Exception e)
                    {
                        ScutiLogger.LogException(e);
                        UIManager.Alert.SetHeader("Out of Stock").SetBody("This item is out of stock. Please try again later.").SetButtonText("OK").Show(() => { });
                        //UIManager.Open(UIManager.Offers);
                    }
                }
                UIManager.HideLoading(false);
            }
        }

        protected override void OnWidgetLoaded(bool initial, OfferSummaryPresenterBase widget)
        {
            widget.Show();
            widget.DisplayCurrentImage();
            widget.ResetTimer();
        }


#endregion
    }
}