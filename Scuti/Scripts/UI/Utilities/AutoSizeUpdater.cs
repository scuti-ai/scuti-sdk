using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

public class AutoSizeUpdater : MonoBehaviour
{
    public bool verbose = false;

    private RectTransform rt;
    public Image[] Images;
    public Text[] Texts;
    public RectTransform[] RectTrans;

    public float MinHeight = 0;

    public float Offset = 0;

    public bool updateWidth = false;

    public UnityEvent OnUpdateFinished;

    private ArrayList ImagesLst;

    public ArrayList ImagesLst1
    {
        get { return ImagesLst; }
        set { ImagesLst = value; }
    }

    private ArrayList TextsLst;

    public ArrayList TextsLst1
    {
        get { return TextsLst; }
        set { TextsLst = value; }
    }

    private ArrayList RectTransLst;

    public ArrayList RectTransLst1
    {
        get { return RectTransLst; }
        set { RectTransLst = value; }
    }

    public float _height;
    private float _width;

    void Awake()
    {
        rt = (RectTransform)gameObject.GetComponent(typeof(RectTransform)); // Acessing the RectTransform 

        ImagesLst = new ArrayList(Images);
        TextsLst = new ArrayList(Texts);
        RectTransLst = new ArrayList(RectTrans);
    }

    // Use this for initialization
    void Start()
    {
        //UpdateHeight();
        _width = rt.rect.width;
        RepeatUpdate();
    }

    public void RepeatUpdate()
    {
        if (IsInvoking("UpdateHeight"))
        {
            CancelInvoke("UpdateHeight");
            CancelInvoke("CancelUpdate");
        }
        InvokeRepeating("UpdateHeight", 0.1f, 0.1f);
        Invoke("CancelUpdate", 2f);
    }

    private void CancelUpdate()
    {
        OnUpdateFinished?.Invoke();
        CancelInvoke("UpdateHeight");
    }

    public void UpdateHeight()
    {
        /*if (verbose)
            print(gameObject.name + " ====================================");*/

        _height = 0;
        foreach (Image img in ImagesLst)
        {
            /*if (verbose)
                print("img:: " + img.rectTransform.rect.height);*/

            _height += img.rectTransform.rect.height;
        }
        foreach (Text txt in TextsLst)
        {
            /*if (verbose)
                 print("txt:: " + txt.preferredHeight);*/

            _height += txt.preferredHeight;
        }
        foreach (RectTransform crt in RectTransLst)
        {
            /*if (verbose)
                print("crt:: " + crt.rect.height);*/

            _height += crt.rect.height;
        }

        /*if (verbose)
            print(" MAX: " + (_height + Offset) + " <==> " + MinHeight);*/

        _height = Mathf.Max(_height + Offset, MinHeight);

        if (verbose)
            print("rt.rect.width:: " + _width + "  _height::" + _height);

        rt.sizeDelta = new Vector2(updateWidth ? _width : 0, _height); // Setting the height to equal the height of text
        //rt.rect.Set(0,0,_width, _height); // Setting the height to equal the height of text
        //rt.rect.size = new Vector2(_width, _height); // Setting the height to equal the height of text
    }

    public float GetHeight()
    {
        return _height;
    }
}
