using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class LoanService : ILoanService
{
    private IPlayerRepository playerRepository;
    private IPhotonLoanManager photonLoanManager;

    [Inject]
    public void Construct(IPlayerRepository playerRepository, IPhotonLoanManager network)
    {
        this.playerRepository = playerRepository;
        this.photonLoanManager = network;
    }

    public void RequestTakeLoan(int playerId)
    {
        var player = playerRepository.GetPlayerById(playerId);
        if (player.HasLoan) return;

        photonLoanManager.SendTakeLoan(playerId);
    }

    public void RequestPayLoan(int playerId)
    {
        var player = playerRepository.GetPlayerById(playerId);
        if (!player.HasLoan) return;

        photonLoanManager.SendPayLoan(playerId);
    }

    public void OnPlayerTurnStart(int playerId)
    {
        var player = playerRepository.GetPlayerById(playerId);
        if (!player.HasLoan) return;

        player.LoanTurnsLeft--;

        if (player.LoanTurnsLeft <= 0)
        {
            photonLoanManager.ShowLoanWindow(playerId);
        }
        else
        {
            EventBus.Publish(new OnTakeLoanEvent(player));
        }
    }

    // Этот метод вызывается **только из RPC**
    public void TakeLoanConfirmed(int playerId)
    {
        var player = playerRepository.GetPlayerById(playerId);
       // Bank.Instance.AddMoney(playerId, 5000);
        player.HasLoan = true;
        player.LoanTurnsLeft = 1;

        EventBus.Publish(new OnTakeLoanEvent(player));
        EventBus.Publish(new OnUpdatePlayerMoneyEvent(player));
    }

    public void PayLoanConfirmed(int playerId)
    {
        var player = playerRepository.GetPlayerById(playerId);
      //  Bank.Instance.RemoveMoney(playerId, 5500);

        player.HasLoan = false;
        player.LoanTurnsLeft = 0;

        EventBus.Publish(new OnTakeLoanEvent(player));
        EventBus.Publish(new OnUpdatePlayerMoneyEvent(player));
    }
}
