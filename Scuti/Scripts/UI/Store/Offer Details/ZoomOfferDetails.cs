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
            if(displayLargeImage != null)
                displayLargeImage.Init();
        }

        public void ResetSizeImage()
        {
            transform.localScale = initialScale;
        }

        // ---------------------------------------------------------------------------------------


        /// <summary>
        /// This method will be called during the mouse/touch drag
        /// </summary>
        /// <param name="eventData">mouse pointer event data</param>
        public override void OnDrag(PointerEventData eventData)
        {
            isDragDetected = true;
            base.OnDrag(eventData);
        }


        /// <summary>
        /// This method will be called at the end of mouse drag
        /// </summary>
        /// <param name="eventData"></param>
        public override void OnEndDrag(PointerEventData eventData)
        {
            if (counterTouch == 0)
            {
                isDragDetected = false;
            }

            base.OnEndDrag(eventData);
        }


        // --------------------------------------------------------------------------- Event Trigger


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

        
    }
}
