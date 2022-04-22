using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Scuti.Net;

namespace Scuti.UI
{
    public class OfferSummaryTallSmallContainer : View
    {
        public OfferSummaryPresenterContainer Small;
        public OfferSummaryPresenterBase Tall;

        internal void Clear()
        {
            Small.Clear();
            Tall.Clear();
            Tall.gameObject.SetActive(false);
            Small.gameObject.SetActive(false);
        }


        internal List<OfferSummaryPresenterBase> GetPresenters(OfferService.MediaType mediaType)
        {

            if (mediaType == OfferService.MediaType.Vertical)
            {
                Tall.gameObject.SetActive(true);
                Small.gameObject.SetActive(false);
                return new List<OfferSummaryPresenterBase>() { Tall };
            } else
            {
                Tall.gameObject.SetActive(false);
                Small.gameObject.SetActive(true);
                return Small.Presenters;
            }

        }
 
    }
}
