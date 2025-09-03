using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class UICasinoWindow : UIWindowBase<UICasinoWindow>
{
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button playButton;
    [SerializeField] private Button dice1Button;
    [SerializeField] private Button dice2Button;
    [SerializeField] private Button dice3Button;
    [SerializeField] private Button dice4Button;
    [SerializeField] private Button dice5Button;
    [SerializeField] private Button dice6Button;

    private List<int> selectedNumbers = new List<int>(); // выбранные числа
    private List<Button> diceButtons;

 
    private void Start()
    {
        playButton.onClick.AddListener(PlayCasino);
        cancelButton.onClick.AddListener(CancelCasino);
        diceButtons = new List<Button> { dice1Button, dice2Button, dice3Button, dice4Button, dice5Button, dice6Button };

        for (int i = 0; i < diceButtons.Count; i++)
        {
            int number = i + 1;
            diceButtons[i].onClick.AddListener(() => ToggleNumber(number));
        }
    }

    public override void ShowWindow()
    {
        selectedNumbers.Clear();
        ResetHighlights();
        windowAnimation.ShowWindow();
    }


    private void ToggleNumber(int number)
    {
        if (selectedNumbers.Contains(number))
        {
            selectedNumbers.Remove(number);
        }
        else
        {
            if (selectedNumbers.Count >= 3) return; // максимум 3 выбора
            selectedNumbers.Add(number);
        }
        UpdateHighlights();
    }

    private void UpdateHighlights()
    {
        for (int i = 0; i < diceButtons.Count; i++)
        {
            int num = i + 1;
            var colors = diceButtons[i].colors;
            colors.normalColor = selectedNumbers.Contains(num) ? Color.green : Color.white;
            diceButtons[i].colors = colors;
        }
    }

    private void ResetHighlights()
    {
        foreach (var btn in diceButtons)
        {
            var colors = btn.colors;
            colors.normalColor = Color.white;
            btn.colors = colors;
        }
    }

    private void PlayCasino()
    {
        if (selectedNumbers.Count == 0) return;

        CasinoManager.Instance.PlayGame(selectedNumbers.ToArray());
        HideWindow();
    }

    private void CancelCasino()
    {
        HideWindow();
        TurnManager.Instance.RequestEndTurn();
    }
}
