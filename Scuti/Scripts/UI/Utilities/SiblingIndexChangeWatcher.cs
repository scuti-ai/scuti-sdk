using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class SiblingIndexChangeWatcher: MonoBehaviour
{
    public bool Enable = false;

    public UnityEvent OnSiblingIndexChanged;

    int lastIndex = -1;
    bool started = false;

    int indexToChange = 0;

    Transform _parent;
    SiblingIndexChangeWatcher _last;

    private void Start()
    {
        _parent = transform.parent;
    }

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
            _last = transform.parent.GetComponentsInChildren<SiblingIndexChangeWatcher>().OrderBy(i => i.transform.GetComponent<RectTransform>().anchoredPosition.y).FirstOrDefault();

            //Debug.LogWarning(" -------->>>> " + _last.name + "  " + _last.transform.GetComponent<RectTransform>().anchoredPosition.y);

            _last.Dispatchevent();
            /*foreach (var item in transform.parent.GetComponentsInChildren<SiblingIndexChangeWatcher>())
            {
                Debug.LogWarning("item:: "+item.name);
            }*/
            /*if (_last && _last.transform.Equals(transform))
            {
                Debug.LogWarning("--------------------------------------:: "+transform.name);
               
            }*/
        }
        lastIndex = transform.GetSiblingIndex();
    }

    public void Dispatchevent()
    {
        OnSiblingIndexChanged?.Invoke();
    }
}
