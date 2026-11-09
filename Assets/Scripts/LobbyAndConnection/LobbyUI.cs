using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;

public class LobbyUI : MonoBehaviour,IInitializable
{
    [SerializeField] private TextMeshProUGUI nicknameText;

    private IAuthService authService;

    [Inject]
    public void Construct(IAuthService authService)
    {
        this.authService = authService;
    }
    void IInitializable.Initialize()
    {
        nicknameText.text = authService.GetPlayerData().Nickname;
    }
   
}
