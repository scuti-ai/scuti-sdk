using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    bool _pressed;

    public float UpDuration = 0.1f;
    public float DownDuration = 0.05f;

    private float _remaining = 0f;

    public Vector3 PressedScale = new Vector3(0.95f, 0.95f, 0.95f);
    public Vector3 DefaultScale = new Vector3(1f, 1f, 1f);

    // needs to be late update, otherwise it fights with layout elements
    private void LateUpdate()
    {
        if(_pressed || _remaining > 0)
        {
            _remaining -= Time.deltaTime;

            Vector3 targetScale = DefaultScale;
            var duration = UpDuration;
            Vector3 initScale = PressedScale;
            if (_pressed)
            {
                duration = DownDuration;
                targetScale = PressedScale;
                initScale = DefaultScale;
            }
            var percent = Mathf.Clamp((1 - (_remaining / duration)), 0f, 1f);
            transform.localScale = Vector3.Lerp(initScale, targetScale, percent);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _pressed = true;
        _remaining = DownDuration;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        EndPress();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        EndPress();
    }

    private void EndPress()
    { 
        if(_pressed)
        {
            _pressed = false;
            _remaining = UpDuration;
        }
    }
}
