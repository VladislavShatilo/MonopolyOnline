using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIJailWindow : MonoBehaviour
{
    public static UIJailWindow Instance { get; private set; }

    [SerializeField] private WindowAnimation windowAnimation;
    [SerializeField] private Button ransomButton;
    [SerializeField] private Button throwDiceButton;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    void Start()
    {
        throwDiceButton.onClick.AddListener(ThrowDice);

    }
    public void ShowWindow()
    {
        windowAnimation.ShowWindow();
    }

    public void HideWindow()
    {
        windowAnimation.HideWindow();
    }
    private void ThrowDice()
    {
        HideWindow();
        TurnManager.Instance.RequestEndTurn();
    }
}
