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

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IJailWindow jailWindow, ILocalPlayerService localPlayerService, IEventBus eventBus, IPlayerRepository playerRepository,
       IRansomJailWindow ransomJailWindow, IPhotonDiceManager photonDiceManager, IJailService jailService, IBankService bankService, GameSettings gameSettings)
    {
        this.jailWindow = jailWindow ?? throw new ArgumentNullException(nameof(jailWindow));
        this.localPlayerService = localPlayerService ?? throw new ArgumentNullException(nameof(localPlayerService));
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus)); ;
        this.playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        this.ransomJailWindow = ransomJailWindow ?? throw new ArgumentNullException(nameof(ransomJailWindow));
        this.photonDiceManager = photonDiceManager ?? throw new ArgumentNullException(nameof(photonDiceManager));
        this.jailService = jailService ?? throw new ArgumentNullException(nameof(jailService));
        this.bankService = bankService ?? throw new ArgumentNullException(nameof(bankService));
        this.gameSettings = gameSettings ?? throw new ArgumentNullException(nameof(gameSettings));
    }
    

    public void Initialize()
    {
        eventBus.Subscribe<StartTurnJailEvent>(OnStartTurn);

        jailWindow.SetThrowDiceAction(OnThrowDiceClicked);
        jailWindow.SetRansomAction(OnRansomClicked);
        ransomJailWindow.SetRansomAction(OnRansomClicked);
    }

    public void Dispose()
    {
        eventBus.Unsubscribe<StartTurnJailEvent>(OnStartTurn);
    }

    #endregion LIFE_CYCLE

    #region CALLBACKS

    private void OnStartTurn(StartTurnJailEvent e)
    {
        PlayerData player = playerRepository.GetPlayerById(e.PlayerId) ?? throw new InvalidOperationException(nameof(jailWindow));
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
        jailWindow.Hide();
        ransomJailWindow.Hide();
    }

    #endregion CALLBACKS
}