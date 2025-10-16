using System;

public class PhotonLocalPlayerService : ILocalPlayerService
{
    #region PUBLIC_METHODS
    
    public int GetLocalPlayerId() 
    {
        if (Photon.Pun.PhotonNetwork.LocalPlayer == null)
            throw new InvalidOperationException("Локальный игрок ещё не подключен к Photon.");

        return Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber;
    }

    #endregion PUBLIC_METHODS
}