using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SiblingIndexChangeWatcher: MonoBehaviour
{
    public bool Enable = false;

    public UnityEvent OnSiblingIndexChanged;

    int lastIndex = -1;
    bool started = false;

    int indexToChange = 0;

    // Update is called once per frame
    void Update()
    {
        //if (!Enable)
        //    return;

        if (!started)
        {
            started = true;
            lastIndex = transform.GetSiblingIndex();
            //indexToChange = transform.parent.childCount > 3 ? transform.parent.childCount - 2 : transform.parent.childCount - 1;
        }

        //Debug.LogWarning(currentIndex+"   "+transform.parent.childCount);

        if (lastIndex != transform.GetSiblingIndex() && (/*transform.GetSiblingIndex() == 0 ||*/ transform.GetSiblingIndex() == indexToChange))
        {
            OnSiblingIndexChanged?.Invoke();
        }
        lastIndex = transform.GetSiblingIndex();
    }
}
