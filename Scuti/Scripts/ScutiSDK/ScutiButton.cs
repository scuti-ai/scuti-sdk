using UnityEngine;
using Scuti;
using Scuti.Net;
using System;

public class ScutiButton : MonoBehaviour
{
    public GameObject NotificationIcon;

    private void Awake()
    {
        NotificationIcon.SetActive(true);
    }

    public void Start()
    {
        if(ScutiNetClient.Instance != null)
        {
            if (ScutiNetClient.Instance.IsInitialized && ScutiNetClient.Instance.IsAuthenticated)
            {
                CheckRewards();
            }
            else
            {
                ScutiNetClient.Instance.OnAuthenticated += CheckRewards;
            }
        }  
    }

    public void OnClick()
    { 
        ScutiSDK.Instance.LoadUI();
        NotificationIcon.SetActive(false);
    }

    private async void CheckRewards()
    {
        var rewards = await ScutiAPI.GetRewards(); 
        foreach(var reward in rewards)
        {
            if(reward.Activated==false)
            {
                NotificationIcon.SetActive(true);
                return;
            }
        }
        NotificationIcon.SetActive(false);
    }


    private void OnDestroy()
    {
        if (ScutiNetClient.Instance != null)
        {
            ScutiNetClient.Instance.OnAuthenticated -= CheckRewards;
        }
    }


}