using Photon.Pun;

public class PhotonNetworkWrapper : IPhotonNetworkWrapper
{
    public bool IsMasterClient => PhotonNetwork.IsMasterClient;
}