using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIPanningAndPinchImage : MonoBehaviour ,IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, 
    /*IDragHandler,*/ IScrollHandler
{
    private Vector3 initialScale;
    [SerializeField]
    private ScrollRect scrollRect;
    [SerializeField]
    private float zoomSpeed = 0.1f;
    [SerializeField]
    private float maxZoom = 10f;
    [SerializeField]
    private bool isScale;
    [SerializeField]
    private bool isEnterToImage;

    [Header("Testing")]
    [SerializeField]
    Text textForDebug;
    [SerializeField]
    Image imageTestPointMouse;

    private int counterTouch;

    PointerEventData cachedEventData;

    // Start is called before the first frame update
    void Awake()
    {       
        initialScale = transform.localScale;
    }

   /* public void OnDrag(PointerEventData eventData)
    {
        if (countClick >= 2)
        {
            isScale = true;           
        }
        else
        {
            isScale = false;
        }
    }*/


    public void OnPointerDown(PointerEventData eventData)
    {
        cachedEventData = eventData;
        MouseDown(eventData);
        /*
        countClick++;
        //m_forDebug2.text = "JPDebug OnPointerDown Count: " + eventData.clickCount;
        Debug.Log("JPDebug OnPointerDown: " + countClick);
        textForDebug.text = "JPDebug OnPointerDown: " + countClick;
        */
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        /*
        countClick--;
        //m_forDebug2.text = "JPDebug OnPointerDown Count: " + eventData.clickCount;
        Debug.Log("JPDebug OnPointerUp: " + countClick);
        textForDebug.text = "JPDebug OnPointerUp: " + countClick;
        */
    }

    public void OnScroll(PointerEventData eventData)
    {
        Debug.Log("OnScroll...");
        //textForDebug.text = "OnScroll";
        var delta = Vector3.one * (eventData.scrollDelta.y * zoomSpeed);
        var desiredScale = transform.localScale + delta;

        desiredScale = ClampDesiredScale(desiredScale);

        transform.localScale = desiredScale;
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="desiredScale"></param>
    /// <returns></returns>
    private Vector3 ClampDesiredScale(Vector3 desiredScale)
    {
        desiredScale = Vector3.Max(initialScale, desiredScale);
        desiredScale = Vector3.Min(initialScale * maxZoom, desiredScale);
        return desiredScale;
    }

    Vector2 midPoint;

    void Update()
    {
        if(counterTouch >= 2)
       // if (countClick >= 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);         

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            midPoint = (touchZeroPrevPos + touchOnePrevPos) / 2;

            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            // For scale image
            var delta = Vector3.one * (deltaMagnitudeDiff * -zoomSpeed);
            var desiredScale = transform.localScale + delta;
            Debug.Log("desireScale: " + desiredScale);
            //desireScaleText.text = desiredScale.ToString();
            desiredScale = ClampDesiredScale(desiredScale);

            transform.localScale = desiredScale;

        }

        if(isEnterToImage)
        {
            MouseDown(cachedEventData);

        }
     
        /*if (counterTouch >= 1)
        {
            Touch touchZero2 = Input.GetTouch(0);
            // Moving
            Vector3 pos = touchZero2.position;
            pos.z = rect.position.z;
            rect.position = Camera.main.ScreenToViewportPoint(pos);

        }*/





        /*else
        {
            transform.localScale = initialScale;
        }*/

    }

    /*void OnMouseOver()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y));

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            //Code that uses hit instead of PointerEventData
            
        }
    }*/

    // -------------------------------------------------------------------------------------

    private Vector2 lastMousePosition;

    /// <summary>
    /// This method will be called on the start of the mouse drag
    /// </summary>
    /// <param name="eventData">mouse pointer event data</param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Begin Drag");
        lastMousePosition = eventData.position;
        midPoint = eventData.position;
    }

    /// <summary>
    /// This method will be called during the mouse drag
    /// </summary>
    /// <param name="eventData">mouse pointer event data</param>
    public void OnDrag(PointerEventData eventData)
    {
        if(counterTouch == 1)
        {
            Vector2 currentMousePosition = eventData.position;
            Vector2 diff = currentMousePosition - lastMousePosition;
            RectTransform rect = GetComponent<RectTransform>();

            Vector3 newPosition = rect.position + new Vector3(diff.x, diff.y, 0/*transform.position.z*/);
            Vector3 oldPos = rect.position;
            rect.position = newPosition;
            //if (!IsRectTransformInsideSreen(rect))
            //{
            // rect.position = oldPos;
            //}
            lastMousePosition = currentMousePosition;
        }

        if(counterTouch >= 2)
        {
            Vector2 currentMousePosition = midPoint;
            Vector2 diff = currentMousePosition - lastMousePosition;
            RectTransform rect = GetComponent<RectTransform>();

            Vector3 newPosition = rect.position + new Vector3(diff.x, diff.y, 0/*transform.position.z*/);
            Vector3 oldPos = rect.position;
            rect.position = newPosition;
            //if (!IsRectTransformInsideSreen(rect))
            //{
            // rect.position = oldPos;
            //}
            lastMousePosition = currentMousePosition;
        }

    }

    private bool IsRectTransformInsideSreen(RectTransform rectTransform)
    {
        /*bool isInside = false;
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        int visibleCorners = 0;
        Rect rect = new Rect(0, 0, Screen.width, Screen.height);
        foreach (Vector3 corner in corners)
        {
            if (rect.Contains(corner))
            {
                visibleCorners++;
            }
        }
        if (visibleCorners == 4)
        {
            isInside = true;
        }
        return isInside;*/
        return false;
    }

    /// <summary>
    /// This method will be called at the end of mouse drag
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("End Drag");
        //Implement your funtionlity here
    }

    // ----------------------------------------------------------------------------------------------

    private void MouseDown(PointerEventData dat)
    {
        if (dat == null)
            return;


        Vector2 localCursor;
        var rect1 = scrollRect.GetComponent<RectTransform>();
        var pos1 = dat.position;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect1, pos1,
            null, out localCursor))
            return;

        int xpos = (int)(localCursor.x);
        int ypos = (int)(localCursor.y);

        if (xpos < 0) xpos = xpos + (int)rect1.rect.width / 2;
        else xpos += (int)rect1.rect.width / 2;

        if (ypos > 0) ypos = ypos + (int)rect1.rect.height / 2;
        else ypos += (int)rect1.rect.height / 2;

        Debug.Log("Correct Cursor Pos: " + xpos + " " + ypos);

        // Calculate point in scale
        imageTestPointMouse.rectTransform.anchoredPosition = new Vector2(xpos, ypos);

        float widthScroll = rect1.rect.width;
        float heightScroll = rect1.rect.height;

        Debug.Log("Scroll: Width - Height: " + widthScroll + " " + heightScroll);

        var rect2 = GetComponent<RectTransform>();

        float widthScroll2 = rect2.rect.width;
        float heightScroll2 = rect2.rect.height;

        Debug.Log("Image: Width - Height: " + widthScroll2 * maxZoom + " " + heightScroll2 * maxZoom);

        // Move image
        rect2.anchoredPosition = new Vector2(widthScroll2 / 2, heightScroll2 / 2) - new Vector2(xpos, ypos);
        rect2.anchoredPosition = new Vector2(rect2.anchoredPosition.x * (maxZoom * 0.9f), rect2.anchoredPosition.y * (maxZoom * 0.9f));

    }



    // ---------------------------------------------------------------------------

    public void OnPointerEnter(BaseEventData eventData)
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN

        isEnterToImage = true;
        Debug.Log("Enter mouse to Image");
        Debug.Log("OnScroll...");
        //textForDebug.text = "OnScroll";
        var delta = Vector3.one * (maxZoom);
        var desiredScale = transform.localScale + delta;

        desiredScale = ClampDesiredScale(desiredScale);

        transform.localScale = desiredScale;
#endif

    }

    public void OnPointerExit(BaseEventData eventData)
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        isEnterToImage = false;
        Debug.Log("Exit Mouse to Image");        
        transform.localScale = initialScale;


#endif
    }  

    public void OnPressDown(BaseEventData eventData)
    {
        counterTouch++;

    }

    public void OnPressUp(BaseEventData eventData)
    {     
        counterTouch--;
    }

    // ------------------------------------------------------



}

