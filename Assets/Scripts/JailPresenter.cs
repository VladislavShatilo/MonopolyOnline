using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class JailPresenter : IInitializable, IDisposable
{
    private IJailWindow jailWindow;
    private IRansomJailWindow ransomJailWindow;
    private IJailService jailService;
    private IBankService bankService;
    private IPlayerRepository playerRepository;
    private ILocalPlayerService localPlayerService;
    private IEventBus eventBus;
    private IPhotonDiceManager photonDiceManager;
    private GameSettings gameSettings;

    
    [Inject]
    public void Construct(IJailWindow jailWindow, ILocalPlayerService localPlayerService, IEventBus eventBus, IPlayerRepository playerRepository,
        IRansomJailWindow ransomJailWindow, IPhotonDiceManager photonDiceManager, IJailService jailService, IBankService bankService, GameSettings gameSettings)
    {

        this.jailWindow = jailWindow;
        this.localPlayerService = localPlayerService;
        this.eventBus = eventBus;
        this.playerRepository = playerRepository;
        this.ransomJailWindow = ransomJailWindow;
        this.photonDiceManager = photonDiceManager;
        this.jailService = jailService;
        this.bankService = bankService;
    }

    void IInitializable.Initialize()
    {
        //eventBus.Subscribe<OfferJailEvent>(ShowJailWindow);
        eventBus.Subscribe<StartTurnJailEvent>(OnStartTurn);

        jailWindow.SetThrowDiceAction(OnThrowDiceClicked);
        jailWindow.SetRansomAction(OnRansomClicked);
        ransomJailWindow.SetRansomAction(OnRansomClicked);
    }

    void IDisposable.Dispose()
    {
        // eventBus.Unsubscribe<OfferJailEvent>(ShowJailWindow);
        eventBus.Unsubscribe<StartTurnJailEvent>(OnStartTurn);
    }

    private void OnStartTurn(StartTurnJailEvent e)
    {
        PlayerData player = playerRepository.GetPlayerById(e.PlayerId);
        int localId = localPlayerService.GetLocalPlayerId();

        if (player.JailTurnsLeft > 0)
        {
            if (e.PlayerId == localId)
            {
                bool canAfford = player.Money >= gameSettings.jailRansom;
                jailWindow.Show(e.PlayerId, gameSettings.jailRansom, canAfford);
            }
            else
            {
                jailWindow.HardHide();
            }
        }
        else
        {
            if (e.PlayerId == localId)
            {
                bool canAfford = player.Money >= gameSettings.jailRansom;
                ransomJailWindow.Show(e.PlayerId, gameSettings.jailRansom, canAfford);
            }
            else
            {
                ransomJailWindow.HardHide();
            }
        }
    }

    private void OnThrowDiceClicked(int playerId)
    {
        photonDiceManager.RequestDiceRoll(localPlayerService.GetLocalPlayerId(), true);

        jailWindow.Hide();
    }

    private void OnRansomClicked(int playerId)
    {
        jailService.ReleasePlayer(playerId, true);
        photonDiceManager.RequestDiceRoll(localPlayerService.GetLocalPlayerId(), false);
        bankService.RemoveMoney(playerId, gameSettings.jailRansom);
        //eventBus.Publish(new ReleaseFromJailEvent(playerId, true));
        jailWindow.Hide();
        ransomJailWindow.Hide();
    }
}

public class OfferJailEvent
{
    public int PlayerId { get; }
    public int PlayerMoney { get; }

    public OfferJailEvent(int playerId, int playerMoney)
    {
        PlayerId = playerId;
        PlayerMoney = playerMoney;
    }
}

public class ShowMandatoryRansomEvent
{
    public int PlayerID { get; }
    public int Fine { get; }

    public ShowMandatoryRansomEvent(int playerId, int fine)
    {
        PlayerID = playerId;
        Fine = fine;
    }
}

public class ShowJailOfferEvent
{
    public int PlayerID { get; }
    public int Fine { get; }
    public int TurnsLeft { get; }

    public ShowJailOfferEvent(int playerId, int fine, int turnsLeft)
    {
        PlayerID = playerId;
        Fine = fine;
        TurnsLeft = turnsLeft;
    }
}