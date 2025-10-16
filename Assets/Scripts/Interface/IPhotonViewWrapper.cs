using Photon.Pun;
using Photon.Realtime;

public interface IPhotonViewWrapper
{
    void RPC(PhotonView view, string methodName, RpcTarget target, params object[] parameters);
    void RPC(PhotonView view, string methodName, Player targetPlayer, params object[] parameters);
    void RPC(PhotonView view, string methodName, int playerId, params object[] parameters);

}