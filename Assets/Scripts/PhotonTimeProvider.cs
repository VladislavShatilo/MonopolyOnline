public class PhotonTimeProvider : ITimeProvider
{
    public double Now => Photon.Pun.PhotonNetwork.Time;
}
