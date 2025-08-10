using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nicknameText;
    void Start()
    {
        nicknameText.text = PlayerAuthData.Nickname;
    }
}
