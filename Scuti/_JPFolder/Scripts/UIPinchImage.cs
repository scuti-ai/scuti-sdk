using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIPinchImage : MonoBehaviour, IScrollHandler
{

    private Vector3 initialScale;

    [SerializeField]
    private float zoomSpeed = 0.1f;

    [SerializeField]
    private float maxZoom = 10f;

    //public float perspectiveZoomSpeed = 0.5f;
    //public float ortoZoomSpeed = 0.5f;
    //public Camera camera;
    public Text desireScaleText;

    public bool isScale;

    void Awake()
    {
        initialScale = transform.localScale;
    }
    public void OnScroll(PointerEventData eventData)
    {
        Debug.Log("OnScroll UIPInch: " + eventData.scrollDelta.y);

        var delta = Vector3.one * (eventData.scrollDelta.y * zoomSpeed);
        var desiredScale = transform.localScale + delta;

        desiredScale = ClampDesiredScale(desiredScale);

        transform.localScale = desiredScale;
    }

    private Vector3 ClampDesiredScale(Vector3 desiredScale)
    {
        desiredScale = Vector3.Max(initialScale, desiredScale);
        desiredScale = Vector3.Min(initialScale * maxZoom, desiredScale);
        return desiredScale;
    }


    private void Update()
    {
        
        if(isScale)
        //if(Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            /*if(GetComponent<Camera>().orthographic)
            {
                GetComponent<Camera>().orthographicSize += deltaMagnitudeDiff * ortoZoomSpeed;
                GetComponent<Camera>().orthographicSize = Mathf.Max(GetComponent<Camera>().orthographicSize, 0.1f);

            }
            else
            {
                GetComponent<Camera>().fieldOfView += deltaMagnitudeDiff * perspectiveZoomSpeed;
                GetComponent<Camera>().fieldOfView = Mathf.Clamp(GetComponent<Camera>().fieldOfView, 0.1f, 179.9f);
            }*/

            // For scale image
            var delta = Vector3.one * (deltaMagnitudeDiff * - zoomSpeed);
            var desiredScale = transform.localScale + delta;
            Debug.Log("desireScale: "+desiredScale);
            //desireScaleText.text = desiredScale.ToString();
            desiredScale = ClampDesiredScale(desiredScale);

            transform.localScale = desiredScale;

        }
    }


}
