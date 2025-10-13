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
    private IPhotonNetworkWrapper networkWrapper;
    private IPhotonViewWrapper photonViewWrapper;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IPhotonTurnManager photonTurnManager, IBoardService boardService, IEventBus eventBus, IPhotonNetworkWrapper networkWrapper, IPhotonViewWrapper photonViewWrapper)
    {
        this.photonTurnManager = photonTurnManager;
        this.boardService = boardService;
        this.eventBus = eventBus;
        this.networkWrapper = networkWrapper;
        this.photonViewWrapper = photonViewWrapper;
    }

    public void Start()
    {
        CheckStartGame();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        CheckStartGame();
    }

    #endregion LIFE_CYCLE

    #region PRIVATE_METHODS

    private void CheckStartGame()
    {
        if (!PhotonNetwork.InRoom)
            return;
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            StartCoroutine(InitializeAllPlayers());

            if (networkWrapper.IsMasterClient)
            {
                photonTurnManager.RequestStartRandomTurn();
            }
        }
    }

    private IEnumerator InitializeAllPlayers()
    {
        yield return new WaitForSeconds(0.5f);

        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            PlayerMove playerMove = player.GetComponent<PlayerMove>();
            PlayerSkin playerSkin = player.GetComponent<PlayerSkin>();

            if (playerMove != null)
            {
                playerMove.Initialize(boardService, eventBus);
                eventBus.Publish(new PlayerOccupancyRegisterEvent(0, player.GetComponent<PlayerMove>()));
            }
            if (playerSkin != null)
            {
                player.GetComponent<PlayerSkin>().Initialize(eventBus, photonViewWrapper);
            }
        }
        yield return new WaitForSeconds(0.5f);
    }

    #endregion PRIVATE_METHODS

}