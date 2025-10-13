//using NUnit.Framework;
//using Moq;
//using UnityEngine;
//using UnityEngine.UI;
//using System;

//[TestFixture]
//public class CompanyWindowPopupTests
//{
//    private GameObject gameObject;
//    private CompanyWindowPopup popup;
//    private Button button;
//    private Mock<ITradeService> mockTradeService;
//    private Mock<ICompanyRepository> mockCompanyRepo;

//    [SetUp]
//    public void SetUp()
//    {
//        // Создаём GameObject с компонентом
//        gameObject = new GameObject();
//        popup = gameObject.AddComponent<CompanyWindowPopup>();

//        // Создаём кнопку
//        var buttonGO = new GameObject("Button");
//        button = buttonGO.AddComponent<Button>();
//        // Присваиваем кнопку через Reflection
//        typeof(CompanyWindowPopup)
//            .GetField("showWindowButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
//            .SetValue(popup, button);

//        // Моки для зависимостей
//        mockTradeService = new Mock<ITradeService>();
//        mockCompanyRepo = new Mock<ICompanyRepository>();
//        popup.Construct(mockTradeService.Object, mockCompanyRepo.Object);
//    }

//    [TearDown]
//    public void TearDown()
//    {
//        GameObject.DestroyImmediate(gameObject);
//        GameObject.DestroyImmediate(button.gameObject);
//    }

//    [Test]
//    public void Init_AddsButtonListener_AndCreatesInteractor()
//    {
//        bool clicked = false;
//        popup.OnCompanyClicked += _ => clicked = true;

//        popup.Init(42);
//        button.onClick.Invoke();

//        Assert.IsTrue(clicked); // Проверяем, что кнопка реально вызывает логику
//    }

//    [Test]
//    public void OnClick_HandledByInteractor_DoesNotInvokeEvent()
//    {
//        popup.Init(10);

//        // Мокаем interactor через partial class
//        var interactorField = typeof(CompanyWindowPopup)
//            .GetField("interactor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

//        var mockInteractor = new Mock<CompanyOfferInteractor>(mockTradeService.Object, mockCompanyRepo.Object);
//        mockInteractor.Setup(x => x.TryToggleCompanyInOffer(10)).Returns(true);

//        interactorField.SetValue(popup, mockInteractor.Object);

//        bool eventCalled = false;
//        popup.OnCompanyClicked += id => eventCalled = true;

//        button.onClick.Invoke();

//        Assert.IsFalse(eventCalled);
//        mockInteractor.Verify(x => x.TryToggleCompanyInOffer(10), Times.Once);
//    }

//    [Test]
//    public void OnClick_NotHandledByInteractor_InvokesEvent()
//    {
//        popup.Init(5);

//        var interactorField = typeof(CompanyWindowPopup)
//            .GetField("interactor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

//        var mockInteractor = new Mock<CompanyOfferInteractor>(mockTradeService.Object, mockCompanyRepo.Object);
//        mockInteractor.Setup(x => x.TryToggleCompanyInOffer(5)).Returns(false);

//        interactorField.SetValue(popup, mockInteractor.Object);

//        int? receivedId = null;
//        popup.OnCompanyClicked += id => receivedId = id;

//        button.onClick.Invoke();

//        Assert.AreEqual(5, receivedId);
//        mockInteractor.Verify(x => x.TryToggleCompanyInOffer(5), Times.Once);
//    }
//}
