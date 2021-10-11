using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Scuti.UI
{
    public abstract class PanningAndPinchImage : MonoBehaviour
    {
        [Header("Testing")]
        public bool isTestingMobile;

        [Header("Components")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform rectScroll;
        [SerializeField] private RectTransform rectImagenToZoom;

        [Header("Parameters")]
        [SerializeField] private float zoomSpeed = 0.1f;
        [SerializeField] private float maxZoomMobile = 10f;
        [SerializeField] private float maxZoomDesktop = 5f;
        [SerializeField] private bool isScale;
        [SerializeField] private bool isEnterToImage;

        [HideInInspector] public int counterTouch;
        public Vector3 initialScale;
        private PointerEventData cachedEventData;
        private Vector2 lastMousePosition;
        private Vector2 averagePointBetweenTouch;


        // ----------------------------------------------------------------------------------------------

        void Awake()
        {
            initialScale = transform.localScale;            
        }
        // ----------------------------------------------------------------------------------------------


        void Update()
        {
            if (counterTouch >= 2)
            {
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                // Find the position in the previous frame of each touch.
                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                averagePointBetweenTouch = (touchZeroPrevPos + touchOnePrevPos) / 2;

                // Find the magnitude of the vector (the distance) between the touches in each frame.
                float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                // Find the difference in the distances between each frame.
                float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

                MouseDown(cachedEventData);

                // For scale image
                var delta = Vector3.one * (deltaMagnitudeDiff * -zoomSpeed);
                var desiredScale = transform.localScale + delta;
                desiredScale = ClampDesiredScale(desiredScale);

                transform.localScale = desiredScale;

            }

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            if (!isTestingMobile)
            {
                // For desktop version
                if (isEnterToImage)
                {
                    MouseDown(cachedEventData);
                }
            }
            #endif
        }

        private void MouseDown(PointerEventData eventData)
        {
            if (eventData == null)
                return;

            Vector2 localCursor;
            var pos1 = eventData.position;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectScroll, pos1,
                null, out localCursor))
                return;

            int xpos = (int)(localCursor.x);
            int ypos = (int)(localCursor.y);

            if (xpos < 0) xpos = xpos + (int)rectScroll.rect.width / 2;
            else xpos += (int)rectScroll.rect.width / 2;

            if (ypos > 0) ypos = ypos + (int)rectScroll.rect.height / 2;
            else ypos += (int)rectScroll.rect.height / 2;
            // Calculate point in scale
            float widthScroll = rectScroll.rect.width;
            float heightScroll = rectScroll.rect.height;

            float widthScroll2 = rectImagenToZoom.rect.width;
            float heightScroll2 = rectImagenToZoom.rect.height;

            // Move image in zoom 
            rectImagenToZoom.anchoredPosition = new Vector2(widthScroll2 / 2, heightScroll2 / 2) - new Vector2(xpos, ypos);
            rectImagenToZoom.anchoredPosition = new Vector2(rectImagenToZoom.anchoredPosition.x * (maxZoomDesktop * 0.95f), rectImagenToZoom.anchoredPosition.y * (maxZoomDesktop * 0.95f));

        }

        // ----------------------------------------------------------------------------------------------

        // This method anchors the maximum and minimum values of the scale to mobile
        private Vector3 ClampDesiredScale(Vector3 desiredScale)
        {
            desiredScale = Vector3.Max(initialScale, desiredScale);
            desiredScale = Vector3.Min(initialScale * maxZoomMobile, desiredScale);
            return desiredScale;
        }

        /// <summary>
        /// IScrollHandler: This method is called when scrolling
        /// </summary>
        /// <param name="eventData"></param>
        public void OnScroll(PointerEventData eventData)
        {
            var delta = Vector3.one * (eventData.scrollDelta.y * zoomSpeed);
            var desiredScale = transform.localScale + delta;

            desiredScale = ClampDesiredScale(desiredScale);

            transform.localScale = desiredScale;
        }    

        /// <summary>
        /// This method will be called on the start of the mouse drag
        /// </summary>
        /// <param name="eventData">mouse pointer event data</param>
        public void OnBeginDrag(PointerEventData eventData)
        {
            lastMousePosition = eventData.position;
            averagePointBetweenTouch = eventData.position;
        }

        /// <summary>
        /// This method will be called during the mouse/touch drag
        /// </summary>
        /// <param name="eventData">mouse pointer event data</param>
        public virtual void OnDrag(PointerEventData eventData)
        {
            scrollRect.enabled = false;

            Vector2 currentMousePosition = new Vector2(0, 0);

            // Get de position touch
            if (counterTouch == 1)
            {
                currentMousePosition = eventData.position;
            }

            if (counterTouch >= 2)
            {
                currentMousePosition = averagePointBetweenTouch;
            }

            Vector2 diff = currentMousePosition - lastMousePosition;

            Vector3 newPosition = rectImagenToZoom.position + new Vector3(diff.x, diff.y, 0);
            Vector3 oldPos = rectImagenToZoom.position;
            rectImagenToZoom.position = newPosition;

            lastMousePosition = currentMousePosition;

        }

        /// <summary>
        /// This method will be called at the end of mouse drag
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnEndDrag(PointerEventData eventData)
        {
            scrollRect.enabled = true;
        }

        // --------------------------------------------------------------------------- Event Trigger

        /// <summary>
        /// Method to detect mouse over enter on image
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnPointerEnter(BaseEventData eventData)
        {
            if (!isTestingMobile)
            {

#if UNITY_EDITOR || UNITY_STANDALONE_WIN

                // Cast BaseEventData as PointerEvenData for tracking mouse
                PointerEventData pointerData = eventData as PointerEventData;
                cachedEventData = pointerData;

                isEnterToImage = true;

                var delta = Vector3.one * (maxZoomDesktop);
                var desiredScale = transform.localScale + delta;

                desiredScale = ClampDesiredScale(desiredScale);

                transform.localScale = desiredScale;

#endif
            }

        }

        /// <summary>
        /// Method to detect mouse over exit on image
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnPointerExit(BaseEventData eventData)
        {
            if (!isTestingMobile)
            {

#if UNITY_EDITOR || UNITY_STANDALONE_WIN

                isEnterToImage = false;
                transform.localScale = initialScale;
#endif
            }

        }

        /// <summary>
        /// Method to detect touch down on the image
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPressDown(BaseEventData eventData)
        {
            counterTouch++;
        }

        /// <summary>
        /// Method to detect touch up on the image
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPressUp(BaseEventData eventData)
        {
            counterTouch--;
        }


    }


}
