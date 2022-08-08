using UnityEngine;
using Scuti;
using Scuti.Net;
using System;
using UnityEngine.EventSystems;

public class ScutiButton : MonoBehaviour
{
    public GameObject NotificationIcon;
    public GameObject NewItems;
    public GameObject Button;

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

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(Button);

    }

    public void OnClick()
    {
        ScutiSDK.Instance.LoadUI();
        NotificationIcon.SetActive(false);
    }

    private async void CheckNewOffers()
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