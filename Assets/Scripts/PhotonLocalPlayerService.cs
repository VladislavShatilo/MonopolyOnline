public class PhotonLocalPlayerService : ILocalPlayerService
{
    public int GetLocalPlayerId() => Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber;
}