using Scuti.Net;
using System;
using System.Collections;
using System.Collections.Generic;
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
        WalletLabel.text = balance.ToString();
    }

    private async Task Refresh()
    {
        if (ScutiNetClient.Instance.IsAuthenticated)
        {
            var rewards = await ScutiAPI.GetWallet(true);
            WalletLabel.text = ScutiUtils.GetTotalWallet(rewards).ToString();
        } else
        {
            WalletLabel.text = "0";
        }
    }

    internal async Task RefreshOverTime(int reward, float v)
    {
        var value = Convert.ToInt32(WalletLabel.text);

        var time = Time.time;
        var percent = (Time.time - time) / v;
        while (percent<1)
        {
            percent = (Time.time - time) / v;
            if (percent > 1) percent = 1;
            WalletLabel.text = (value+Math.Floor(reward*percent)).ToString();
            v -= 0.03f;
            await Task.Delay(30);
        }

        // Sanity check
        await Refresh();
    }
}
