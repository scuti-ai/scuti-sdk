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

namespace Scuti.UI
{
    public class OffersPresenterPortrait : OffersPresenterBase
    {

        [Header("Instantiation")]
        //[SerializeField] OfferSummaryPresenterBase widgetPrefab_Single;
        //[SerializeField] OfferSummaryPresenterPortrait widgetPrefab_Double;
        //[SerializeField] Transform container;

        public List<OfferSummaryPresenterBase> SingleOffers;
        public List<OfferSummaryPresenterPortrait> DoubleOffers;
        private List<OfferSummaryPresenterBase> _allOffers;

        public ScutiInfiniteScroll InfinityScroll;


        protected override void Awake()
        {
            base.Awake();
            _allOffers = new List<OfferSummaryPresenterBase>();
            var singleCopy = SingleOffers.ToList();
            var doubleCopy = DoubleOffers.ToList();
            while(singleCopy.Count>0 || doubleCopy.Count>0)
            {
                if(singleCopy.Count>0)
                {
                    var presenter = singleCopy[0];
                    presenter.Single = true;
                    presenter.FirstColumn = true;
                    _allOffers.Add(presenter);
                    singleCopy.RemoveAt(0);
                }
                if (doubleCopy.Count > 0)
                {
                    var presenter = doubleCopy[0].Presenters[0];
                    presenter.Single = false;
                    presenter.FirstColumn = true;
                    _allOffers.Add(presenter);
                    presenter = doubleCopy[0].Presenters[1];
                    presenter.Single = false;
                    presenter.FirstColumn = false;
                    //presenter.titleText.cha
                    _allOffers.Add(presenter);
                    doubleCopy.RemoveAt(0);
                }
            }

            offerDataToRequest =   _allOffers.Count;
        }

        private int colorCount = 0;
        // ================================================
        #region API

        public override void Clear()
        {

            base.Clear();
            foreach (var presenter in _allOffers)
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
            OfferColorData colorData;
            foreach (var presenter in _allOffers)
            {
                if (cancelToken.IsCancellationRequested) return;


                var mediaType = presenter.RollForMediaType();
                offerData = Data.RequestOffer(mediaType);

                // Fallback to products
                if(offerData==null && mediaType != OfferService.MediaType.Product)
                {
                    mediaType = OfferService.MediaType.Product;
                    offerData = Data.RequestOffer(mediaType);

                }

                m_Instantiated.Add(presenter);
                presenter.Inject(GetNext);
                presenter.gameObject.hideFlags = HideFlags.DontSave; 
                colorData = GetColorInfo(colorCount++);
                presenter.SetColorData(colorData.Background, colorData.Glow);
                await Task.Delay((int)(instantiationInterval * 1000));
                if (cancelToken.IsCancellationRequested) return;

                if (offerData == null)
                {
                    continue;
                }

                presenter.Data = offerData;                
                presenter.FirstLoad = true;
                presenter.OnLoaded -= OnWidgetLoaded;
                presenter.OnLoaded += OnWidgetLoaded;
                presenter.Data.isSingle = presenter.Single;
                presenter.Data.LoadImage();
                presenter.OnClick -= OnPresenterClicked;
                presenter.OnClick += OnPresenterClicked;
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