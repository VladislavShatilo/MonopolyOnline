//using Moq;
//using NUnit.Framework;
//using System.Collections;
//using UnityEngine;
//using UnityEngine.TestTools;
//using UnityEngine.UI;

//public class CompanyWindowPopupTests
//{
//    private GameObject go;
//    private CompanyWindowPopup popup;
//    private Mock<ITradeService> tradeServiceMock;
//    private Mock<ICompanyRepository> companyRepoMock;
//    private Button button;

//    [SetUp]
//    public void SetUp()
//    {
//        go = new GameObject();
//        popup = go.AddComponent<CompanyWindowPopup>();

//        // создаём кнопку и присваиваем
//        var btnGO = new GameObject();
//        button = btnGO.AddComponent<Button>();
//        typeof(CompanyWindowPopup)
//            .GetField("showWindowButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
//            .SetValue(popup, button);

//        tradeServiceMock = new Mock<ITradeService>();
//        companyRepoMock = new Mock<ICompanyRepository>();

//        popup.Construct(tradeServiceMock.Object, companyRepoMock.Object);
//    }

//    [TearDown]
//    public void TearDown()
//    {
//        GameObject.DestroyImmediate(go);
//    }

//    [Test]
//    public void Init_ShouldAddButtonListener_AndCreateInteractor()
//    {
//        popup.Init(5);

//        // проверяем, что listener добавлен
//        Assert.IsTrue(button.onClick.GetPersistentEventCount() > 0);

//        // проверяем, что приватный interactor != null
//        var interactor = typeof(CompanyWindowPopup)
//            .GetField("interactor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
//            .GetValue(popup);
//        Assert.IsNotNull(interactor);
//    }

//    [Test]
//    public void OnClick_ShouldInvokeEvent_WhenInteractorReturnsFalse()
//    {
//        popup.Init(10);
//        // заменяем interactor на мок, чтобы вернуть false
//        var interactor = new Mock<CompanyOfferInteractor>(tradeServiceMock.Object, companyRepoMock.Object);
//        interactor.Setup(x => x.TryToggleCompanyInOffer(10)).Returns(false);

//        typeof(CompanyWindowPopup)
//            .GetField("interactor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
//            .SetValue(popup, interactor.Object);

//        int calledId = -1;
//        popup.OnCompanyClicked += id => calledId = id;

//        button.onClick.Invoke();

//        Assert.AreEqual(10, calledId);
//    }

//    [Test]
//    public void OnClick_ShouldNotInvokeEvent_WhenInteractorReturnsTrue()
//    {
//        popup.Init(10);
//        var interactor = new Mock<CompanyOfferInteractor>(tradeServiceMock.Object, companyRepoMock.Object);
//        interactor.Setup(x => x.TryToggleCompanyInOffer(10)).Returns(true);

//        typeof(CompanyWindowPopup)
//            .GetField("interactor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
//            .SetValue(popup, interactor.Object);

//        bool eventCalled = false;
//        popup.OnCompanyClicked += id => eventCalled = true;

//        button.onClick.Invoke();

//        Assert.IsFalse(eventCalled);
//    }
//}
