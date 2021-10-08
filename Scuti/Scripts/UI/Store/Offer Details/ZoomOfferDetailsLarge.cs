using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Scuti.UI
{    
    public class ZoomOfferDetailsLarge : PanningAndPinchImage, IDragHandler, IBeginDragHandler, IEndDragHandler, IScrollHandler
    {
        [SerializeField] private bool isZoomAutomatic;

        // --------------------------------------------------------------------------- Event Trigger

        /// <summary>
        /// Method to detect mouse over enter on image
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerEnter(BaseEventData eventData)
        {
            if (isZoomAutomatic)
            {
                base.OnPointerEnter(eventData);
            }               
        }

        /// <summary>
        /// Method to detect mouse over exit on image
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerExit(BaseEventData eventData)
        {
            if(isZoomAutomatic)
            {
                base.OnPointerExit(eventData);
            }              

        }

        /// <summary>
        /// Method to detect touch down on the image
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPressDown(BaseEventData eventData)
        {
            base.OnPressDown(eventData);
        }

        /// <summary>
        /// Method to detect touch up on the image
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPressUp(BaseEventData eventData)
        {
            base.OnPressUp(eventData);
        }

    }


}
