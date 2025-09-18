using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class GameStarter : MonoBehaviourPunCallbacks
{
    private IPhotonTurnManager photonTurnManager;
    private IBoardService boardService;
    private IEventBus eventBus;

    [Inject]
    public void Construct(IPhotonTurnManager photonTurnManager, IBoardService boardService, IEventBus eventBus)
    {
        this.photonTurnManager = photonTurnManager;
        this.boardService = boardService;
        this.eventBus = eventBus;
    }

    public void Start()
    {
        CheckStartGame();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        CheckStartGame();
    }

    private void CheckStartGame()
    {
        if (!Photon.Pun.PhotonNetwork.InRoom)
            return;
        if (Photon.Pun.PhotonNetwork.CurrentRoom.PlayerCount == Photon.Pun.PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            StartCoroutine(InitializeAllPlayers());

            if (Photon.Pun.PhotonNetwork.IsMasterClient)
            {
                photonTurnManager.RequestStartRandomTurn();
            }
        }
    }

    private IEnumerator InitializeAllPlayers()
    {
        yield return new WaitForSeconds(0.5f);
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var pm in players)
        {
            pm.GetComponent<PlayerMove>().Initialize(boardService, eventBus);
        }
    }
}