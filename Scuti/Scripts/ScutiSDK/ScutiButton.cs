using UnityEngine;
using Scuti;

public class ScutiButton : MonoBehaviour
{
    public void OnClick()
    { 
        ScutiSDK.Instance.LoadUI();
    }
}