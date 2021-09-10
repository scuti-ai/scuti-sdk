using Scuti;
using Scuti.Net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class WalletWidget : MonoBehaviour {

    public Text WalletLabel;
    public Image Icon;

    private void Start()
    {
        ScutiAPI.OnWalletUpdated += OnWalletUpdated;
#pragma warning disable 4014
        Refresh();
#pragma warning restore 4014
    }

    private void OnWalletUpdated(int balance)
    { 
        WalletLabel.text = String.Format("{0:n0}", balance);
    }

    public void DoRefresh()
    {
#pragma warning disable 4014
        Refresh();
#pragma warning restore 4014
    }

    private async Task Refresh()
    {
        if (ScutiNetClient.Instance.IsAuthenticated)
        {
            var rewards = await ScutiAPI.GetWallet(true); 
            WalletLabel.text = String.Format("{0:n0}", ScutiUtils.GetTotalWallet(rewards));
        } else
        {
            WalletLabel.text = "0"; 
        }
    }

    internal async void RefreshOverTime(int reward, float animationDuration)
    {
        try
        { 
            int startWalletAmount;
            int.TryParse(WalletLabel.text, NumberStyles.AllowThousands,
                 CultureInfo.InvariantCulture, out startWalletAmount);

            if (animationDuration < 1) animationDuration = 1; 
            var startTime = Time.time;
            var percent = 0f;
            int count = 0;
            while (percent < 1 && count < 100)
            {
                percent = (Time.time - startTime) / animationDuration;
                if (percent > 1) percent = 1;
                WalletLabel.text = String.Format("{0:n0}", (startWalletAmount + Math.Floor(reward * percent)));
                count++;
                await Task.Delay(30);
            }
            WalletLabel.text = String.Format("{0:n0}", startWalletAmount + reward); 
        } catch(Exception e)
        {
            ScutiLogger.LogException(e);
        }
        // Sanity check
        await Refresh();
    }
}
