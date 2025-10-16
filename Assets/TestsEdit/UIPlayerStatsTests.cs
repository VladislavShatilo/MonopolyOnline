using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIPlayerStatsTests
{
    private GameObject _go;
    private UIPlayerStats _stats;

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject("PlayerStats");
        _stats = _go.AddComponent<UIPlayerStats>();

        // Инициализация UI-элементов
        _stats.GetType().GetField("namePlayerText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_stats, CreateTMP("NamePlayerText"));

        _stats.GetType().GetField("moneyPlayerText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_stats, CreateTMP("MoneyPlayerText"));

        _stats.GetType().GetField("capitalText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_stats, CreateTMP("CapitalText"));

        _stats.GetType().GetField("liquidText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_stats, CreateTMP("LiquidText"));

        _stats.GetType().GetField("timerGO", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_stats, new GameObject("TimerGO"));

        _stats.GetType().GetField("timerText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_stats, CreateTMP("TimerText"));

        _stats.GetType().GetField("highlightTurnImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_stats, CreateImage("HighlightTurn"));

        _stats.GetType().GetField("highlightAuctionImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_stats, CreateImage("HighlightAuction"));

        _stats.GetType().GetField("loanContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_stats, new GameObject("LoanContainer"));

        _stats.GetType().GetField("loanTurnsLeftText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_stats, CreateTMP("LoanTurnsLeftText"));

        _stats.GetType().GetField("takeLoanButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_stats, CreateButton("TakeLoanButton"));

        _stats.GetType().GetField("payLoanButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_stats, CreateButton("PayLoanButton"));

        _stats.GetType().GetField("tradeButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_stats, CreateButton("TradeButton"));

        _stats.GetType().GetField("leaveButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_stats, CreateButton("LeaveButton"));
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_go);
    }

    // Вспомогательные методы
    private TextMeshProUGUI CreateTMP(string name)
    {
        var go = new GameObject(name);
        return go.AddComponent<TextMeshProUGUI>();
    }

    private Button CreateButton(string name)
    {
        var go = new GameObject(name);
        return go.AddComponent<Button>();
    }

    private Image CreateImage(string name)
    {
        var go = new GameObject(name);
        return go.AddComponent<Image>();
    }
    [Test]
    public void SetName_SetsText()
    {
        _stats.SetName("Player1");
        var text = _stats.GetType().GetField("namePlayerText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        .GetValue(_stats) as TextMeshProUGUI;

        Assert.AreEqual("Player1", text.text);
    }

    [Test]
    public void SetMoney_SetsFormattedText()
    {
        _stats.SetMoney(12345);
        var text = _stats.GetType().GetField("moneyPlayerText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        .GetValue(_stats) as TextMeshProUGUI;

        Assert.AreEqual("12,345", text.text);
    }

    [Test]
    public void SetCapital_SetsBothTexts()
    {
        _stats.SetCapital(50000, 20000);
        var capital = _stats.GetType().GetField("capitalText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            .GetValue(_stats) as TextMeshProUGUI;
        var liquid = _stats.GetType().GetField("liquidText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            .GetValue(_stats) as TextMeshProUGUI;

        Assert.AreEqual("50,000", capital.text);
        Assert.AreEqual("20,000", liquid.text);
    }
    [Test]
    public void SetTimer_ActivatesObjects()
    {
        _stats.SetTimer(true, 3.7f, true, false);

        var timerGO = _stats.GetType().GetField("timerGO", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            .GetValue(_stats) as GameObject;
        var highlightTurn = _stats.GetType().GetField("highlightTurnImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                  .GetValue(_stats) as Image;
        var highlightAuction = _stats.GetType().GetField("highlightAuctionImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                     .GetValue(_stats) as Image;
        var timerText = _stats.GetType().GetField("timerText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                              .GetValue(_stats) as TextMeshProUGUI;

        Assert.IsTrue(timerGO.activeSelf);
        Assert.IsTrue(highlightTurn.gameObject.activeSelf);
        Assert.IsFalse(highlightAuction.gameObject.activeSelf);
        Assert.AreEqual("4", timerText.text); // Mathf.Ceil
    }

    [Test]
    public void SetLoan_SetsObjectsAndTexts()
    {
        _stats.SetLoan(true, 2, true);

        var loanContainer = _stats.GetType().GetField("loanContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                  .GetValue(_stats) as GameObject;
        var loanText = _stats.GetType().GetField("loanTurnsLeftText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                             .GetValue(_stats) as TextMeshProUGUI;
        var takeButton = _stats.GetType().GetField("takeLoanButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                               .GetValue(_stats) as Button;
        var payButton = _stats.GetType().GetField("payLoanButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                              .GetValue(_stats) as Button;

        Assert.IsTrue(loanContainer.activeSelf);
        Assert.AreEqual("2", loanText.text);
        Assert.IsFalse(takeButton.gameObject.activeSelf);
        Assert.IsTrue(payButton.gameObject.activeSelf);
    }
    [Test]
    public void BindTradeAction_InvokesAction()
    {
        bool clicked = false;
        _stats.BindTradeAction(() => clicked = true);

        var tradeButton = _stats.GetType().GetField("tradeButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                .GetValue(_stats) as Button;

        tradeButton.onClick.Invoke();
        Assert.IsTrue(clicked);
    }

    [Test]
    public void BindTakeLoanAction_InvokesAction()
    {
        bool clicked = false;
        _stats.BindTakeLoanAction(() => clicked = true);

        var takeButton = _stats.GetType().GetField("takeLoanButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                               .GetValue(_stats) as Button;

        takeButton.onClick.Invoke();
        Assert.IsTrue(clicked);
    }
    [Test]
    public void BindPayLoanAction_InvokesAction()
    {
        bool clicked = false;
        _stats.BindPayLoanAction(() => clicked = true);

        var payButton = _stats.GetType().GetField("payLoanButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                              .GetValue(_stats) as Button;
        payButton.onClick.Invoke();

        Assert.IsTrue(clicked);
    }

    [Test]
    public void BindLeaveButton_VisibleAndCanToggle()
    {
        var leaveButton = _stats.GetType().GetField("leaveButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                .GetValue(_stats) as Button;

        _stats.SetLeaveButtonVisible(true);
        Assert.IsTrue(leaveButton.gameObject.activeSelf);

        _stats.SetLeaveButtonVisible(false);
        Assert.IsFalse(leaveButton.gameObject.activeSelf);
    }

    // --- Тесты для видимости элементов ---
    [Test]
    public void SetTimerGOVisible_WorksCorrectly()
    {
        var timerGO = _stats.GetType().GetField("timerGO", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            .GetValue(_stats) as GameObject;
        _stats.SetTimerGOVisible(true);
        Assert.IsTrue(timerGO.activeSelf);
        _stats.SetTimerGOVisible(false);
        Assert.IsFalse(timerGO.activeSelf);
    }

    [Test]
    public void SetLoanContainerVisible_WorksCorrectly()
    {
        var loanContainer = _stats.GetType().GetField("loanContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                   .GetValue(_stats) as GameObject;
        _stats.SetLoanContainerVisible(true);
        Assert.IsTrue(loanContainer.activeSelf);
        _stats.SetLoanContainerVisible(false);
        Assert.IsFalse(loanContainer.activeSelf);
    }

    [Test]
    public void SetTurnHighlightVisible_WorksCorrectly()
    {
        var highlightTurn = _stats.GetType().GetField("highlightTurnImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                  .GetValue(_stats) as Image;
        _stats.SetTurnHighlightVisible(true);
        Assert.IsTrue(highlightTurn.gameObject.activeSelf);
        _stats.SetTurnHighlightVisible(false);
        Assert.IsFalse(highlightTurn.gameObject.activeSelf);
    }

    [Test]
    public void SetAuctionHighlightVisible_WorksCorrectly()
    {
        var highlightAuction = _stats.GetType().GetField("highlightAuctionImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                     .GetValue(_stats) as Image;
        _stats.SetAuctionHighlightVisible(true);
        Assert.IsTrue(highlightAuction.gameObject.activeSelf);
        _stats.SetAuctionHighlightVisible(false);
        Assert.IsFalse(highlightAuction.gameObject.activeSelf);
    }

    [Test]
    public void SetTradeButtonVisible_WorksCorrectly()
    {
        var tradeButton = _stats.GetType().GetField("tradeButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                .GetValue(_stats) as Button;
        _stats.SetTradeButtonVisible(true);
        Assert.IsTrue(tradeButton.gameObject.activeSelf);
        _stats.SetTradeButtonVisible(false);
        Assert.IsFalse(tradeButton.gameObject.activeSelf);
    }

    [Test]
    public void SetLoanButtonsVisible_WorksCorrectly()
    {
        var takeButton = _stats.GetType().GetField("takeLoanButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                               .GetValue(_stats) as Button;
        var payButton = _stats.GetType().GetField("payLoanButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                              .GetValue(_stats) as Button;

        _stats.SetLoanButtonsVisible(true, false);
        Assert.IsTrue(takeButton.gameObject.activeSelf);
        Assert.IsFalse(payButton.gameObject.activeSelf);

        _stats.SetLoanButtonsVisible(false, true);
        Assert.IsFalse(takeButton.gameObject.activeSelf);
        Assert.IsTrue(payButton.gameObject.activeSelf);
    }

}