using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAuthService
{
    void Login(string nickname, string password);
    PlayerAuthData GetPlayerData();
}