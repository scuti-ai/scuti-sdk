using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using Scuti.GraphQL.Generated;

using System.Threading;
using Scuti.Net;
using UnityEngine.Events;

namespace Scuti.UI {
    public class OfferRecommendedPresenter : Presenter<OfferRecommendedPresenter.Model> {
        [Serializable]
        public class Model : Presenter.Model
        {
            public List<OfferSummaryPresenterBase.Model> Items = new List<OfferSummaryPresenterBase.Model>();
        }

        [Header("Header")]
        [SerializeField] int MaxRecommendations = 3;
        [SerializeField] float instantiationInterval = .5f;

        [Header("Instantiation")]
        [SerializeField] OfferSummaryPresenterBase widgetPrefab_Small;
        [SerializeField] Transform container;
        CancellationTokenSource _loadingSource;
        List<OfferSummaryPresenterBase> m_Instantiated = new List<OfferSummaryPresenterBase>();

        public UnityEvent OnPopulateFinished;

        public async void SearchForRecommendations(string shopName, string currentOfferId)
        {
            Clear();
            if (!string.IsNullOrEmpty(shopName))
            {
                var offers = await ScutiNetClient.Instance.Offer.GetOffers(new List<CampaignType> { CampaignType.Product, CampaignType.Product_Listing }, FILTER_TYPE.In, null, shopName, null, 0, MaxRecommendations+1);
                if (offers.Nodes.Count > 0)
                {
                    var tempData = new Model() { Items = new List<OfferSummaryPresenterBase.Model>() };
                    foreach (var offer in offers.Nodes)
                    {
                        if (offer.Id.ToString() != currentOfferId)
                        {
                            tempData.Items.Add(Mappers.GetOfferSummaryPresenterModel(offer, true));
                        }

                        // we requested 1 more than we needed in case they returned the item we are currently looking at
                        if (tempData.Items.Count == MaxRecommendations) break;
                    }
                    Data = tempData;
                    Open();
                }
                else
                {
                    Close();
                }
            } else
            {
                Close();
            }
        }

        protected override void OnSetState()
        {
#pragma warning disable 4014
            _loadingSource = new CancellationTokenSource();
            PopulateOffers(_loadingSource.Token);
#pragma warning restore 4014
        }

        public void Clear()
        {
            if (_loadingSource != null)
            {
                _loadingSource.Cancel();
            }
            foreach (var widget in m_Instantiated)
            {
                widget.OnLoaded -= OnWidgetLoaded;
            }
            m_Instantiated.Clear();
            foreach (Transform child in container)
                Destroy(child.gameObject);


            Resources.UnloadUnusedAssets();
        }

        async private Task PopulateOffers(CancellationToken cancelToken)
        {
            for (int i = 0; i < Data.Items.Count; i++)
            {
                if (cancelToken.IsCancellationRequested) return;

                // Based on the index, the template and container are chosen.
                // Currently, the first two offers are large, the other are small
                var index = i; 

                var widget = Instantiate(widgetPrefab_Small, container);
                m_Instantiated.Add(widget);
                widget.gameObject.hideFlags = HideFlags.DontSave;
                widget.SetStatic();
                //var colorData = GetColorInfo(index);
                //widget.SetColorData(colorData.Background, colorData.Glow);
                //widget.SetDuration(showDuration);

                await Task.Delay((int)(instantiationInterval * 1000));

                if (cancelToken.IsCancellationRequested) return;

               
                widget.Data = Data.Items[index];
                widget.Data.IsTall = false;
                widget.Data.LoadImage();
                widget.OnLoaded += OnWidgetLoaded;

                widget.OnClick += async () =>
                {
                    var id = widget.Data.ID;
                    var offer = await ScutiNetClient.Instance.Offer.GetOfferByID(id);
                    var panelModel = Mappers.GetOfferDetailsPresenterModel(offer);
                    UIManager.OfferDetails.SetData(panelModel);
                    UIManager.OfferDetails.SetIsVideo(!string.IsNullOrEmpty(widget.Data.VideoURL));
                    UIManager.Open(UIManager.OfferDetails);
                };
            }

            OnPopulateFinished?.Invoke();

        }

        private void OnWidgetLoaded(bool value, OfferSummaryPresenterBase widget)
        {
            widget.Show();
            widget.DisplayCurrentImage();
            Debug.LogError("TODO record unpaid impression");
            //widget.ResetTimer();
        }
    }
}
