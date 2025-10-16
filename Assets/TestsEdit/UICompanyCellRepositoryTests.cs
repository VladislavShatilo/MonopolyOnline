using NUnit.Framework;
using UnityEngine;
using Moq;
using UnityEngine.TestTools;

public class UICompanyCellRepositoryTests
{
    private UICompanyCellRepository repository;

    [SetUp]
    public void SetUp()
    {
        repository = new UICompanyCellRepository();
    }

    [Test]
    public void Register_ShouldAddView_WhenNotExists()
    {
        // Arrange
        var mockView = new Mock<IUICompanyCellView>();
        mockView.Setup(v => v.CompanyId()).Returns(1);

        // Act
        repository.Register(mockView.Object);

        // Assert
        var result = repository.GetByCompanyId(1);
        Assert.AreEqual(mockView.Object, result);
    }

    [Test]
    public void Register_ShouldNotAddDuplicate_WhenAlreadyRegistered()
    {
        // Arrange
        var mockView1 = new Mock<IUICompanyCellView>();
        mockView1.Setup(v => v.CompanyId()).Returns(1);

        var mockView2 = new Mock<IUICompanyCellView>();
        mockView2.Setup(v => v.CompanyId()).Returns(1);

        repository.Register(mockView1.Object);

        // Act
        repository.Register(mockView2.Object);

        // Assert
        var result = repository.GetByCompanyId(1);
        Assert.AreEqual(mockView1.Object, result);
    }

    [Test]
    public void Unregister_ShouldRemoveView_WhenExists()
    {
        // Arrange
        var mockView = new Mock<IUICompanyCellView>();
        mockView.Setup(v => v.CompanyId()).Returns(1);

        repository.Register(mockView.Object);

        // Act
        repository.Unregister(mockView.Object);

        // Assert
        var result = repository.GetByCompanyId(1);
        Assert.IsNull(result);
    }

    [Test]
    public void GetByCompanyId_ShouldReturnNull_WhenNotRegistered()
    {
        // Act
        var result = repository.GetByCompanyId(99);

        // Assert
        Assert.IsNull(result);
    }

   
}
