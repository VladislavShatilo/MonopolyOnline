using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AuthUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField nicknameField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private Button loginButton;

    private void Start()
    {
        // При изменении текста будем проверять
        nicknameField.onValueChanged.AddListener(_ => CheckFields());
        passwordField.onValueChanged.AddListener(_ => CheckFields());
        loginButton.onClick.AddListener(() => OnLoginButtonClick());
        // Сразу выключим кнопку
        loginButton.interactable = false;
    }

    private void CheckFields()
    {
        bool nicknameFilled = !string.IsNullOrWhiteSpace(nicknameField.text);
        bool passwordFilled = !string.IsNullOrWhiteSpace(passwordField.text);

        loginButton.interactable = nicknameFilled && passwordFilled;
    }

    public void OnLoginButtonClick()
    {
        PlayerAuthData.Nickname = nicknameField.text.Trim();
        PlayerAuthData.Password = passwordField.text;

        Debug.Log($"Логин: {PlayerAuthData.Nickname}, Пароль: {PlayerAuthData.Password}");

        Photon.Pun.PhotonNetwork.NickName = PlayerAuthData.Nickname;

        if (SceneFadeManager.instance != null)
        {
            SceneFadeManager.instance.LoadLobbyScene();
        }
        else
        {
            SceneManager.LoadScene("LobbyScene");
        }
    }
}
