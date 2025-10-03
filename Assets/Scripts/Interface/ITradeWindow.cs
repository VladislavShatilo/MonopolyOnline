using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITradeWindow
{
    void Show(TradeOffer tradeOffer);
    void Hide();
    void Clear();
    void SetOfferAction(System.Action onAuction);
    void SetCloseAction(System.Action onBuyAction);
    void UpdateTrade(TradeOffer tradeOffer);
}
