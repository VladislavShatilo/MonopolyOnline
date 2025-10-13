using Photon.Pun;
using Photon.Realtime;

public class PhotonViewWrapper : IPhotonViewWrapper
{

    public void RPC(PhotonView view, string methodName, RpcTarget target, params object[] parameters)
    {
        view.RPC(methodName, target, parameters);
    }
    public void RPC(PhotonView view, string methodName, Player targetPlayer, params object[] parameters)
    {
        view.RPC(methodName, targetPlayer, parameters);
    }
}