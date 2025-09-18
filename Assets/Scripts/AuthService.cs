using UnityEngine;
using Photon.Pun;

public class AuthService : IAuthService
{
    private readonly PlayerAuthData playerData = new PlayerAuthData();

    public void Login(string nickname, string password)
    {
        playerData.Nickname = nickname.Trim();
        playerData.Password = password;

        PhotonNetwork.NickName = playerData.Nickname;

        Debug.Log($"Логин: {playerData.Nickname}, Пароль: {playerData.Password}");
    }

    public PlayerAuthData GetPlayerData()
    {
        return playerData;
    }
}
