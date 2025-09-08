using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Zenject;

public class GameStarter : MonoBehaviourPunCallbacks
{
    private IPhotonTurnManager photonTurnManager;
    [Inject] public void Construct(IPhotonTurnManager photonTurnManager) 
    {
        this.photonTurnManager = photonTurnManager;
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
        if (Photon.Pun.PhotonNetwork.CurrentRoom.PlayerCount == Photon.Pun.PhotonNetwork.CurrentRoom.MaxPlayers && Photon.Pun.PhotonNetwork.IsMasterClient)
        {
            Debug.Log("TryStartGame");
            photonTurnManager.RequestStartRandomTurn();
        }
    }
}
