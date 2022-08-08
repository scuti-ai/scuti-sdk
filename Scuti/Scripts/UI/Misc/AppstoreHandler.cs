using UnityEngine;
using System.Runtime.InteropServices;

namespace Scuti
{
	public class AppstoreHandler : Singleton<AppstoreHandler>
	{
#if UNITY_IPHONE
	[DllImport ("__Internal")] private static extern void _ScutiOpenAppInStore(int appID);
#endif

#if UNITY_ANDROID
		private static AndroidJavaObject jo;
#endif

		protected override void Awake()
		{
			base.Awake();

			if (!Application.isEditor)
			{
#if UNITY_ANDROID
				jo = new AndroidJavaObject("com.scuti.nativestore.NativeAppstore");
#endif
			}
		}

		public void OpenAppInStore(string appID)
		{
			if (!Application.isEditor)
			{
#if UNITY_IPHONE
			int appIDIOS;

			if(int.TryParse(appID, out appIDIOS))
			{	_ScutiOpenAppInStore(appIDIOS);
			}
#endif

#if UNITY_ANDROID
				jo.Call("ScutiOpenInAppStore", "market://details?id=" + appID);
#endif
			}
			else
			{
				Debug.Log("AppstoreHandler:: Cannot open Appstore in Editor.");
			}
		}

		public void AppstoreClosed()
		{
			//Debug.Log("AppstoreHandler:: Appstore closed.");
		}
	}
}
