using Scuti;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopBarView : View {

    public WalletWidget Wallet;

    public void Refresh()
    {
        if (Wallet)
            Wallet.DoRefresh();
    }
}
