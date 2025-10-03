using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITradeReviewWindow
{
    void Show(bool isRecipient, TradeOffer tradeOffer);
    void SetAcceptAction(System.Action onAccept);
    void SetCancelAction(System.Action onCancelAction);
    void Hide();
}
