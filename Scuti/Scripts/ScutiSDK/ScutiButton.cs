using UnityEngine;
using Scuti;
using Scuti.Net;
using System;

public class ScutiButton : MonoBehaviour
{
    public GameObject NotificationIcon;
    public GameObject NewItems;

    private GameObject toast;
    private bool canConnect;
    private void Awake()
    {
        NotificationIcon.SetActive(true);
        NewItems?.SetActive(false);
    }

    public void Start()
    {
        if (ScutiNetClient.Instance != null)
        {
            if (ScutiNetClient.Instance.IsInitialized && ScutiNetClient.Instance.IsAuthenticated)
            {
                CheckRewards();
            }
            else
            {
                ScutiNetClient.Instance.OnAuthenticated += CheckRewards;
            }

            if (ScutiNetClient.Instance.IsInitialized)
            {
                CheckNewOffers();
            }
            else
            {
                ScutiNetClient.Instance.OnInitialization += CheckNewOffers;
            }
        }
    }

    public void OnClick()
    {
        if (canConnect)
        {
            ScutiSDK.Instance.LoadUI();
            NotificationIcon.SetActive(false);
        }
    }

    private async void CheckNewOffers()
    {
        try
        {
            var stats = await ScutiAPI.GetCategoryStatistics();
            NewItems?.SetActive(false);
            if (stats != null)
            {
                if (stats.NewOffers.HasValue && stats.NewOffers.Value > 0)
                {
                    NewItems?.SetActive(true);
                }
            }
            canConnect = true;
        }
        catch (Exception)
        {
            Debug.LogError("couldn't find offers");
            canConnect = false;
            LoadToast();
        }
    }

    private void LoadToast()
    {
        if(toast == null)
        {
            toast = Instantiate(Resources.Load<GameObject>(ScutiConstants.UI_TOAST_NAME));
        }
    }

    float timeToCheck = 0;

    private void Update()
    {
        if (!canConnect)
        {
            if(timeToCheck < 1)
            {
                timeToCheck += Time.deltaTime;
            }
            else
            {
                timeToCheck = 0;
                CheckNewOffers();
            }
        }
    }


    private async void CheckRewards()
    {
        var rewards = await ScutiAPI.GetRewards();
        foreach (var reward in rewards)
        {
            if (reward.Activated == false)
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
            ScutiNetClient.Instance.OnInitialization -= CheckNewOffers;
        }
    }
}