using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Scuti.UI
{
    public class ZoomOfferDetails : PanningAndPinchImage, IDragHandler, IBeginDragHandler, IEndDragHandler, IScrollHandler
    {
        [Header("Sub Widget")]
        [SerializeField] private UIDisplayImageZoom displayLargeImage;

        [SerializeField] private bool isDragDetected;

        // ---------------------------------------------------------------------------------------

        void Awake()
        {
            displayLargeImage.Init();
        }

        public void ResetSizeImage()
        {
            transform.localScale = initialScale;
        }

        // ---------------------------------------------------------------------------------------

        /// <summary>
        /// IScrollHandler: This method is called when scrolling
        /// </summary>
        /// <param name="eventData"></param>
        public void OnScroll(PointerEventData eventData)
        {
            base.OnScroll(eventData);
        }

        /// <summary>
        /// This method will be called on the start of the mouse drag
        /// </summary>
        /// <param name="eventData">mouse pointer event data</param>
        public void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
        }

        /// <summary>
        /// This method will be called during the mouse/touch drag
        /// </summary>
        /// <param name="eventData">mouse pointer event data</param>
        public void OnDrag(PointerEventData eventData)
        {
            isDragDetected = true;
            base.OnDrag(eventData);
        }


        /// <summary>
        /// This method will be called at the end of mouse drag
        /// </summary>
        /// <param name="eventData"></param>
        public void OnEndDrag(PointerEventData eventData)
        {
            if (counterTouch == 0)
            {
                isDragDetected = false;
            }

            base.OnEndDrag(eventData);
        }


        // --------------------------------------------------------------------------- Event Trigger

        /// <summary>
        /// Method to detect mouse over enter on image
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerEnter(BaseEventData eventData)
        {
            base.OnPointerEnter(eventData);
        }

        /// <summary>
        /// Method to detect mouse over exit on image
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerExit(BaseEventData eventData)
        {
            base.OnPointerExit(eventData);

        }

        /// <summary>
        /// Method to detect pointer click on the image
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(BaseEventData eventData)
        {
            if (isDragDetected)
                return;

            displayLargeImage.Show(GetComponent<Image>().sprite);

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
