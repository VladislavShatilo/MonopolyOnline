using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBuyWindow
{
    void Show(int playerId, int cellIndex, int price, bool canAfford);
    void Hide();
    void SetBuyAction(Action<int> buyAction);
    void SetAuctionAction(Action<int> onAuction);
}
