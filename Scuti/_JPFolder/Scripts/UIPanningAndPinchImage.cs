﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Scuti.UI
{
    public class UIPanningAndPinchImage : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler,
 /*IDragHandler,*/ IScrollHandler
    {

        [Header("Sub Widget")]
        [SerializeField] private UIDisplayImageZoom displayLargeImage;

        private Vector3 initialScale;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private float zoomSpeed = 0.1f;
        [SerializeField] private float maxZoomMobile = 10f;
        [SerializeField] private float maxZoomDesktop = 5f;
        [SerializeField] private bool isScale;
        [SerializeField] private bool isEnterToImage;
        [SerializeField] private bool isDragDetected;

        [Header("Zoomable image")]
        [SerializeField] private RectTransform zoomableImage;
        private Image imageToShow;

        private RectTransform rectScroll;
        PointerEventData cachedEventData;
        private int counterTouch;
        private Vector2 lastMousePosition;
        private Vector2 averagePointBetweenTouch;


        [Header("Testing")]
        [SerializeField]
        Text textForDebug;
        [SerializeField]
        Image imageTestPointMouse;
        [SerializeField]
        bool isTestingMobile;

        float timerClick;


        // Start is called before the first frame update
        void Awake()
        {
            initialScale = transform.localScale;
            scrollRect = GetComponentInParent<ScrollRect>();
            rectScroll = scrollRect.GetComponent<RectTransform>();
            displayLargeImage.Init();
        }

        // ---------------------------------------------------------------------------------------

        public void OnPointerDown(PointerEventData eventData)
        {

        }

        public void OnPointerUp(PointerEventData eventData)
        {

        }

        /// <summary>
        /// IScrollHandler: This method is called when scrolling
        /// </summary>
        /// <param name="eventData"></param>
        public void OnScroll(PointerEventData eventData)
        {
            Debug.Log("OnScroll...");
            //textForDebug.text = "OnScroll";
            var delta = Vector3.one * (eventData.scrollDelta.y * zoomSpeed);
            var desiredScale = transform.localScale + delta;

            desiredScale = ClampDesiredScale(desiredScale);

            transform.localScale = desiredScale;
        }

        // This method anchors the maximum and minimum values of the scale to mobile
        private Vector3 ClampDesiredScale(Vector3 desiredScale)
        {
            desiredScale = Vector3.Max(initialScale, desiredScale);
            desiredScale = Vector3.Min(initialScale * maxZoomMobile, desiredScale);
            return desiredScale;
        }

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

                Debug.DrawLine(Vector3.zero, touchZero.position, Color.red);
                Debug.DrawLine(Vector3.zero, touchOne.position, Color.blue);
                Debug.DrawLine(Vector3.zero, averagePointBetweenTouch, Color.yellow);


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

        // -------------------------------------------------------------------------------------

        /// <summary>
        /// This method will be called on the start of the mouse drag
        /// </summary>
        /// <param name="eventData">mouse pointer event data</param>
        public void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Log("Begin Drag");
            lastMousePosition = eventData.position;
            averagePointBetweenTouch = eventData.position;
        }

        /// <summary>
        /// This method will be called during the mouse/touch drag
        /// </summary>
        /// <param name="eventData">mouse pointer event data</param>
        public void OnDrag(PointerEventData eventData)
        {
            isDragDetected = true;
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
            RectTransform rect = GetComponent<RectTransform>();

            Vector3 newPosition = rect.position + new Vector3(diff.x, diff.y, 0/*transform.position.z*/);
            Vector3 oldPos = rect.position;
            rect.position = newPosition;

            lastMousePosition = currentMousePosition;

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
            scrollRect.enabled = true;           

            Debug.Log("End Drag");

        }

        // ----------------------------------------------------------------------------------------------

        private void MouseDown(PointerEventData dat)
        {
            if (dat == null)
                return;

            Vector2 localCursor;
            //var rect1 = scrollRect.GetComponent<RectTransform>();
            var pos1 = dat.position;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectScroll, pos1,
                null, out localCursor))
                return;

            int xpos = (int)(localCursor.x);
            int ypos = (int)(localCursor.y);

            if (xpos < 0) xpos = xpos + (int)rectScroll.rect.width / 2;
            else xpos += (int)rectScroll.rect.width / 2;

            if (ypos > 0) ypos = ypos + (int)rectScroll.rect.height / 2;
            else ypos += (int)rectScroll.rect.height / 2;

           //  Debug.Log("Correct Cursor Pos: " + xpos + " " + ypos);

            // Calculate point in scale
            float widthScroll = rectScroll.rect.width;
            float heightScroll = rectScroll.rect.height;

            // Debug.Log("Scroll: Width - Height: " + widthScroll + " " + heightScroll);

            var rect2 = GetComponent<RectTransform>();

            float widthScroll2 = rect2.rect.width;
            float heightScroll2 = rect2.rect.height;

            // Debug.Log("Image: Width - Height: " + widthScroll2 * maxZoomMobile + " " + heightScroll2 * maxZoomMobile);

            // Move image in zoom 
            rect2.anchoredPosition = new Vector2(widthScroll2 / 2, heightScroll2 / 2) - new Vector2(xpos, ypos);
            //rect2.anchoredPosition = new Vector2(widthScroll2 / 2, heightScroll2 / 2) - averagePointBetweenTouch;

            //Debug.DrawLine(Vector3.zero, new Vector2 (widthScroll2 / 2, heightScroll2 / 2), Color.blue);
            //Debug.DrawLine(Vector3.zero, new Vector2(xpos, ypos), Color.green);
            //Debug.DrawLine(Vector3.zero, rect2.anchoredPosition, Color.red);
            rect2.anchoredPosition = new Vector2(rect2.anchoredPosition.x * (maxZoomDesktop * 0.95f), rect2.anchoredPosition.y * (maxZoomDesktop * 0.95f));
            //Debug.DrawLine(Vector3.zero, rect2.anchoredPosition, Color.yellow);
        }

        // --------------------------------------------------------------------------- Event Trigger

        /// <summary>
        /// Method to detect mouse over enter on image
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerEnter(BaseEventData eventData)
        {

            if(!isTestingMobile)
            {
                Debug.Log("UIPinch: Enter mouse to Image ...");
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
        public void OnPointerExit(BaseEventData eventData)
        {

            if (!isTestingMobile)
            {
                Debug.Log("UIPinch: Exit Mouse to Image");
#if UNITY_EDITOR || UNITY_STANDALONE_WIN

                isEnterToImage = false;
                transform.localScale = initialScale;

#endif
            }

        }

        /// <summary>
        /// Method to detect pointer click on the image
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(BaseEventData eventData)
        {
            if (isDragDetected)
                return;

            Debug.Log("OnCLick Image...");
            //displayLargeImage.Show(GetComponent<Image>().sprite);
        }

        /// <summary>
        /// Method to detect pointer click on thImage click
        /// /// </summary>
        /// <param name="eventData"></param>
        public void OnPressSelected(BaseEventData eventData)
        {
            Debug.Log("UIPanning: PressSelected");
        }

        /// <summary>
        /// Method to detect touch down on the image
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPressDown(BaseEventData eventData)
        {
            Debug.Log("UIPanning: Press DOWN");
            counterTouch++;
        }

        /// <summary>
        /// Method to detect touch down on the image
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPressMove(BaseEventData eventData)
        {
            Debug.Log("UIPanning: Press move");
        }

        /// <summary>
        /// Method to detect touch up on the image
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPressUp(BaseEventData eventData)
        {
            Debug.Log("UIPanning: Press UP");
            counterTouch--;
        }

    }

}

