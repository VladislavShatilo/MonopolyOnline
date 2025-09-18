using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

public class AuthUI :MonoBehaviour, IInitializable,IDisposable
{
    [SerializeField] private TMP_InputField nicknameField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private Button loginButton;

    private IAuthService authService;

    [Inject]
    public void Construct(IAuthService authService)
    {
        this.authService = authService;
    }
    void IInitializable.Initialize()
    {
        nicknameField.onValueChanged.AddListener(_ => CheckFields());
        passwordField.onValueChanged.AddListener(_ => CheckFields());
        loginButton.onClick.AddListener(OnLoginButtonClick);

        loginButton.interactable = false;
    }
    void IDisposable.Dispose()
    {
        nicknameField.onValueChanged.RemoveListener(_ => CheckFields());
        passwordField.onValueChanged.RemoveListener(_ => CheckFields());
        loginButton.onClick.RemoveListener(OnLoginButtonClick);

    }
    private void CheckFields()
    {
        loginButton.interactable = !string.IsNullOrWhiteSpace(nicknameField.text) &&
                                   !string.IsNullOrWhiteSpace(passwordField.text);
    }

    private void OnLoginButtonClick()
    {
        authService.Login(nicknameField.text, passwordField.text);
        SceneManager.LoadScene("LobbyScene");
    }
}
