using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Scuti.Net;

namespace Scuti.UI
{
    public class OfferSummaryPresenterUniversal : OfferSummaryPresenterBase 
    {
        //public bool HasNext
        //{
        //    get { return Next != null && !Next.ID.IsNullOrEmpty(); }
        //}



        //// ================================================
        //#region LICECYCLE
        //// ================================================
        //public override void SetData(Model data)
        //{
        //    if (m_Data != null)
        //    {
        //        m_Data.OnStateChanged -= OnNextStateChanged;
        //    }
        //    base.SetData(data);
        //}

        ////protected override void SwapToNext()
        ////{
        ////    if (HasNext)
        ////    {
        ////        if (Next != null) Next.OnStateChanged -= OnNextStateChanged;
        ////        Data = Next;
        ////        Next = null;
        ////        DisplayCurrentImage();
        ////        ResetTimer();
        ////        LoadCompleted();
        ////    } 
        ////}


        ////protected override bool IsFirstLoad()
        ////{
        ////    return !HasNext;
        ////}



        //#endregion


        //// ================================================
        //#region PRESENTER
        //// ================================================
        //protected override void OnSetDataState(Model.State state)
        //{ 
        //    switch (state)
        //    {
        //        case Model.State.Failed:
        //            Next = null;
        //            break;
        //    }
        //    base.OnSetDataState(state);

        //}

        //#endregion
    }
}
