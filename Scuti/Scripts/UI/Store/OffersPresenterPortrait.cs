using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;
using Image = UnityEngine.UI.Image;


using Scuti.GraphQL.Generated;

using System.Threading;
using Scuti.Net;
using UnityEngine.Events;

namespace Scuti.UI
{
    public class OffersPresenterPortrait : OffersPresenterBase
    {
        [Header("Settings")]
        [SerializeField] int maxDoubleOffers = 4;
        [SerializeField] int maxSingleOffers = 4;

        [Header("Instantiation")]
        [SerializeField] OfferSummaryPresenterBase widgetPrefab_Single;
        [SerializeField] OfferSummaryPresenterPortrait widgetPrefab_Double;
        [SerializeField] Transform container;


        public ScutiInfiniteScroll InfinityScroll;

        private Vector3 _largeContainerDefaultPosition;

      



        // ================================================
#region API
        
        public override void Clear()
        {
            base.Clear();
            foreach (Transform child in container)
                Destroy(child.gameObject);
        }

        private void ResetTimeout()
        {
            m_Idle = false;
            TimeoutTimer.SoftReset();
        }


#endregion

        // ================================================
#region PRESENTATION
        // ================================================
 
    

        async protected override Task PopulateOffers(CancellationToken cancelToken)
        {
            var max = Math.Min(1111111111, Data.NewItemsCount);
            int colorCount = 0;
            int adTakeOverCount = 0;
            int productCount = 0;
            for (int i = 0; i < max; i++)
            {

                var widgetData = Data.NewItems[i];

                if (widgetData.DisplayAd)
                {
                    adTakeOverCount++;
                } else
                {
                    productCount++;
                }
            }

            // Sometimes in the 2 column ads we have to jump ahead. This stores which ones we have already shown so we can skip    
            HashSet<int> skipIds = new HashSet<int>();

            MonoBehaviour template;
            for (int i = 0; i < max; i++)
            {
                Debug.LogError("THIS SKIP WON'T WORK EITHER DAMN IT");
                if (skipIds.Contains(i)) continue;

                if (cancelToken.IsCancellationRequested) return;

                var widgetData = Data.UseItem();
                if(widgetData.DisplayAd)
                {
                    template = widgetPrefab_Single;
                    adTakeOverCount--;
                } else
                {
                    productCount--;
                    if(productCount<1)
                    {
                        template = widgetPrefab_Double;
                    } else
                    {
                        template = widgetPrefab_Double;
                    }
                }
                 var mono = Instantiate(template, container);

                List<OfferSummaryPresenterBase> offers = new List<OfferSummaryPresenterBase>();
                List<OfferSummaryPresenterBase.Model> datas = new List<OfferSummaryPresenterBase.Model>();
                if (template == widgetPrefab_Double)
                {
                    var multi = mono as OfferSummaryPresenterPortrait;
                    offers.AddRange(multi.Presenters);
                    datas.Add(widgetData);

                    for(int n = i+1; n<max; n++)
                    {
                        Debug.LogError("THIS WILL FAIL");
                        var secondWidget = Data.NewItems[n];
                        if (secondWidget.DisplayAd) continue;
                        datas.Add(secondWidget);
                        skipIds.Add(n);
                        productCount--;
                        break;
                    }
                }
                else
                {
                    offers.Add(mono as OfferSummaryPresenterBase);
                    datas.Add(widgetData);
                }

                for (var w = 0; w < offers.Count; w++)
                {
                    var widget = offers[w];
                    widgetData = null;
                    if (w < datas.Count)
                    {
                        widgetData = datas[w];
                    }

                    m_Instantiated.Add(widget);
                    widget.gameObject.hideFlags = HideFlags.DontSave;
                    var colorData = GetColorInfo(colorCount++);
                    widget.SetColorData(colorData.Background, colorData.Glow);
               

                    await Task.Delay((int)(instantiationInterval * 1000));

                    if (cancelToken.IsCancellationRequested) return;

                    widgetData.Index = w+i;
                    widget.Data = widgetData;
                    widget.Data.IsTall = false;
                    widget.Data.LoadImage();
                    widget.OnLoaded += OnWidgetLoaded;

                    widget.OnClick += async () =>
                    {
                        UIManager.ShowLoading(false);
                        var id = widget.Data.ID;
                        var offer = await ScutiNetClient.Instance.Offer.GetOfferByID(id);
                        var panelModel = Mappers.GetOfferDetailsPresenterModel(offer);

                        try
                        {
                            UIManager.OfferDetails.SetData(panelModel);
                            UIManager.OfferDetails.SetIsVideo(!string.IsNullOrEmpty(widget.Data.VideoURL));
                            UIManager.Open(UIManager.OfferDetails);
                        }
                        catch (Exception e)
                        {
                            ScutiLogger.LogException(e);
                            UIManager.Alert.SetHeader("Out of Stock").SetBody("This item is out of stock. Please try again later.").SetButtonText("OK").Show(() => { });
                            //UIManager.Open(UIManager.Offers);
                        }

                        UIManager.HideLoading(false);
                    };
                }
            }


            await Task.Delay(250);
            //Debug.LogWarning(container_Large.childCount+"   ++++++++++++++    "+ container_Small.childCount);
            Debug.LogError("CHECK OUBND");
            //InfinityScroll.CheckBounds();

            OnPopulateFinished?.Invoke();
            m_ChangingCategories = false;
        }

         
#endregion
    }
}