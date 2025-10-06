using UnityEngine;
using Photon.Pun;

public class AuthService : IAuthService
{
    private readonly PlayerAuthData playerData = new();

    #region PUBLIC_METHODS

    public void Login(string nickname, string password)
    {
        playerData.Nickname = nickname.Trim();
        playerData.Password = password;

        PhotonNetwork.NickName = playerData.Nickname;
    }

    public PlayerAuthData GetPlayerData()
    {
        return playerData;
    }

    #endregion PUBLIC_METHODS

}
