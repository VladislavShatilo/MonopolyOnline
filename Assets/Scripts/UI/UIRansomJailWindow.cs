using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRansomJailWindow : MonoBehaviour
{
    public static UIRansomJailWindow Instance { get; private set; }

    [SerializeField] private WindowAnimation windowAnimation;
    [SerializeField] private Button ransomButton;

    private int playerID;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        ransomButton.onClick.AddListener(Ransom);
    }
    public void ShowWindow(int playerID)
    {
        this.playerID = playerID;
        windowAnimation.ShowWindow();
    }

    public void HideWindow()
    {
        windowAnimation.HideWindow();
    }
    private void Ransom()
    {
        JailManager.Instance.ReleaseFromJail(playerID, true);
        HideWindow();
    }
}
