using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public interface IPhotonJailManager 
{
    void SendToJail(int playerId);
    void CheckDice(int playerId, int d1, int d2);
}
