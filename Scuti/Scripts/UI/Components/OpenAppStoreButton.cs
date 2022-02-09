using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OpenAppStoreButton : MonoBehaviour
{
	public TextMeshProUGUI DebugTxt;
	public string m_appID_IOS = "1536954123";
	public string m_appID_Android = "edu.uhtimes.bella";

	public void OnButtonClick()
    {
		try
		{
			if (!Application.isEditor)
			{
#if UNITY_IPHONE
				AppstoreHandler.Instance.OpenAppInStore(m_appID_IOS);
#endif

#if UNITY_ANDROID
				AppstoreHandler.Instance.OpenAppInStore(m_appID_Android);
#endif
			}
			else
			{
				Debug.Log("AppstoreTestScene:: Cannot view app in Editor.");
			}
		}catch(System.Exception e)
        {
			DebugTxt.text = "Error " + e.Message;

		}
		
	}
}
