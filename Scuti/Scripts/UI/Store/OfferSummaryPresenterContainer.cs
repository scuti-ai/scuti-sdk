using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Scuti.Net;

namespace Scuti.UI
{
    public class OfferSummaryPresenterContainer : View
    {
        public List<OfferSummaryPresenterBase> Presenters;

        internal void Clear()
        {
            foreach(var pres in Presenters)
            {
                pres.Clear();
            }
        }
    }
}
