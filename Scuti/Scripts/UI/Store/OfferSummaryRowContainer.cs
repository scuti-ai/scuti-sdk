using System;
using System.Collections.Generic;

namespace Scuti.UI
{
    public class OfferSummaryRowContainer : View
    {
        public List<OfferSummaryTallSmallContainer> Columns;

        //public delegate Task<Model> GetNext(OfferSummaryRowContainer presenter);
        //protected GetNext m_NextRequest;
        //public void Inject(GetNext getNextMethod)
        //{
            //m_NextRequest = getNextMethod;
        //}

        internal void Clear()
        {
            foreach(var col in Columns)
            {
                col.Clear();
            }
        }
    }
}
