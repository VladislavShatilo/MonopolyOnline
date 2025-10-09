using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using Moq;

public class CompanyWindowPopupTests
{
    private GameObject _gameObject;
    private CompanyWindowPopup _popup;
    private Button _button;

    private Mock<ITradeService> _mockTradeService;
    private Mock<ICompanyRepository> _mockCompanyRepository;

    [SetUp]
    public void SetUp()
    {
        // Создаем объект с компонентами
        _gameObject = new GameObject();
        _popup = _gameObject.AddComponent<CompanyWindowPopup>();
        _button = _gameObject.AddComponent<Button>();

        // Присваиваем кнопку
        typeof(CompanyWindowPopup)
            .GetField("showWindowButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_popup, _button);

        // Моки зависимостей
        _mockTradeService = new Mock<ITradeService>();
        _mockCompanyRepository = new Mock<ICompanyRepository>();

        // Инжектим зависимости
        _popup.Construct(_mockTradeService.Object, _mockCompanyRepository.Object);

        // Инициализация
        _popup.Init(42);
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.Destroy(_gameObject);
    }

    [UnityTest]
    public System.Collections.IEnumerator OnClick_TriggersInteractorAndEvent_WhenNotHandled()
    {
        bool eventTriggered = false;
        int receivedId = -1;

        _popup.OnCompanyClicked += id =>
        {
            eventTriggered = true;
            receivedId = id;
        };

        // Создаем мок для CompanyOfferInteractor (TryToggleCompanyInOffer всегда false)
        var interactorField = typeof(CompanyWindowPopup).GetField("interactor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        interactorField.SetValue(_popup, new FakeInteractor(false));

        // Симулируем клик
        _button.onClick.Invoke();

        yield return null;

        Assert.IsTrue(eventTriggered);
        Assert.AreEqual(42, receivedId);
    }

    [UnityTest]
    public System.Collections.IEnumerator OnClick_DoesNotTriggerEvent_WhenHandledByInteractor()
    {
        bool eventTriggered = false;
        _popup.OnCompanyClicked += id => eventTriggered = true;

        var interactorField = typeof(CompanyWindowPopup).GetField("interactor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        interactorField.SetValue(_popup, new FakeInteractor(true));

        _button.onClick.Invoke();
        yield return null;

        Assert.IsFalse(eventTriggered);
    }

    // Вспомогательный класс для подмены CompanyOfferInteractor
    private class FakeInteractor : CompanyOfferInteractor
    {
        private bool _result;

        public FakeInteractor(bool result) : base(null, null)
        {
            _result = result;
        }

        
    }
}
