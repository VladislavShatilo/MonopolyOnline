public class PhotonLocalPlayerService : ILocalPlayerService
{
    #region PUBLIC_METHODS

    public int GetLocalPlayerId() => Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber;

    #endregion PUBLIC_METHODS
}