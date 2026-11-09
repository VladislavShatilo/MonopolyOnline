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

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IAuthService authService)
    {
        this.authService = authService ?? throw new ArgumentNullException(nameof(authService));
        if(nicknameField == null )
            throw new NullReferenceException(nameof(nicknameField));
        if (passwordField == null)
            throw new NullReferenceException(nameof(passwordField));
        if (loginButton == null)
            throw new NullReferenceException(nameof(loginButton));

    }
    void IInitializable.Initialize()
    {
        if(nicknameField != null)
            nicknameField.onValueChanged.AddListener(_ => CheckFields());
        if (passwordField != null)
            passwordField.onValueChanged.AddListener(_ => CheckFields());
        if (loginButton != null)
            loginButton.onClick.AddListener(OnLoginButtonClick);


        //loginButton.interactable = false;
    }
    void IDisposable.Dispose()
    {
        if (nicknameField != null)
            nicknameField.onValueChanged.RemoveListener(_ => CheckFields());
        if (passwordField != null)
            passwordField.onValueChanged.RemoveListener(_ => CheckFields());
        if (loginButton != null)
            loginButton.onClick.RemoveListener(OnLoginButtonClick);

    }

    #endregion LIFE_CYCLE

    #region PRIVATE_METHODS

    private void CheckFields()
    {
      // loginButton.interactable = !string.IsNullOrWhiteSpace(nicknameField.text) &&
                                   //!string.IsNullOrWhiteSpace(passwordField.text);
    }

    #endregion PRIVATE_METHODS

    #region CALLBACKS
    private void OnLoginButtonClick()
    {
        authService.Login(nicknameField.text, passwordField.text);
        SceneManager.LoadScene("LobbyScene");
    }

    #endregion CALLBACKS

}
