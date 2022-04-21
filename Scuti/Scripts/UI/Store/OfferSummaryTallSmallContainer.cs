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

        public bool IsTall { get; private set; }
        public OfferService.MediaType MediaType { get; private set; }

        internal void Clear()
        {
            Small.Clear();
            Tall.Clear();
        }

        public virtual OfferService.MediaType RollForType(bool allowTall)
        {
            MediaType = OfferService.MediaType.Product;
            int rand;
            if(allowTall) rand = UnityEngine.Random.Range(0, 6);
            else rand = UnityEngine.Random.Range(0, 4);

            IsTall = false;
            if(rand>3) // 4, 5
            {
                IsTall = true;
                if(rand>4)
                {
                    MediaType = OfferService.MediaType.Vertical;
                }
            }
            else
            {
                if (rand == 0) MediaType = OfferService.MediaType.SmallTile;
            }
 
            return MediaType;
        }

        internal List<OfferSummaryPresenterBase> GetPresenters(OfferService.MediaType mediaType)
        {
            if (mediaType == OfferService.MediaType.Vertical)
            {
                return new List<OfferSummaryPresenterBase>() { Tall };
            } else
            {
                return Small.Presenters;
            }

        }
    }
}
