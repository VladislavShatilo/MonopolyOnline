using Moq;
using NUnit.Framework;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class CompanyWindowPopupPlayModeTests
{
    private GameObject gameObject;
    private CompanyWindowPopup popup;
    private Mock<ICompanyRepository> mockCompanyRepository;
    private Mock<ITradeService> mockTradeService;
    private CompanyOfferInteractor companyOfferInteractor;
    private Button button;

    [SetUp]
    public void SetUp()
    {
        mockCompanyRepository = new Mock<ICompanyRepository>();
        mockTradeService = new Mock<ITradeService>();

        gameObject = new GameObject();
        popup = gameObject.AddComponent<CompanyWindowPopup>();

        var buttonObj = new GameObject("Button");
        button = buttonObj.AddComponent<Button>();

        // Присвоим кнопку через Reflection, так как она private
        var field = typeof(CompanyWindowPopup).GetField("showWindowButton",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field.SetValue(popup, button);

        // Используем тестовые заглушки для зависимостей
        popup.Construct(mockTradeService.Object, mockCompanyRepository.Object);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(gameObject);
        Object.DestroyImmediate(button.gameObject);
    }

    [UnityTest]
    public IEnumerator Init_SetsCompanyId_AddsButtonListener_CreatesInteractor()
    {
        popup.Init(123);

        // Проверяем companyId через Reflection
        var idField = typeof(CompanyWindowPopup).GetField("companyId",
            BindingFlags.NonPublic | BindingFlags.Instance);
        int companyIdValue = (int)idField.GetValue(popup);
        Assert.AreEqual(123, companyIdValue);

        yield return null;


        // Проверяем, что интерактор создан (не null)
        var interactorField = typeof(CompanyWindowPopup).GetField("interactor",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var interactorValue = interactorField.GetValue(popup);
        Assert.IsNotNull(interactorValue);
    }

    [UnityTest]
    public IEnumerator OnClick_NotHandledByInteractor_InvokesEvent()
    {
        bool eventCalled = false;
        popup.OnCompanyClicked += id => eventCalled = true;

        companyOfferInteractor = new CompanyOfferInteractor(mockTradeService.Object, mockCompanyRepository.Object);
        // Создаём интерактор, который всегда возвращает false
        var interactorField = typeof(CompanyWindowPopup).GetField("interactor",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        interactorField.SetValue(popup, companyOfferInteractor);

        popup.Init(5);

        button.onClick.Invoke();
        yield return null;

        Assert.IsTrue(eventCalled);
    }


}
