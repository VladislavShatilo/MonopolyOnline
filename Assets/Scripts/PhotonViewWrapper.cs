using Photon.Pun;
using Photon.Realtime;
using static UnityEngine.GraphicsBuffer;

public class PhotonViewWrapper : IPhotonViewWrapper
{

    public void RPC(PhotonView view, string methodName, RpcTarget target, params object[] parameters)
    {
        if (view != null)
        {
            view.RPC(methodName, target, parameters);
        }
    }
    public void RPC(PhotonView view, string methodName, Player targetPlayer, params object[] parameters)
    {
        if (view != null)
        {
            view.RPC(methodName, targetPlayer, parameters);
        }
    }
    public void RPC(PhotonView view, string methodName, int playerId, params object[] parameters)
    {
        var player = PhotonNetwork.CurrentRoom.GetPlayer(playerId);

        if (view != null && player!= null)
        {
            view.RPC(methodName, player, parameters);
        }
    }
}