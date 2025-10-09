using NUnit.Framework;
using Moq;

[TestFixture]
public class BankServiceTests
{
    private BankService bankService;
    private Mock<IPlayerRepository> playerRepository;
    private Mock<IBankNotifier> notifier;
    private PlayerData player1;
    private PlayerData player2;

    [SetUp]
    public void Setup()
    {
        playerRepository = new Mock<IPlayerRepository>();
        notifier = new Mock<IBankNotifier>();

        player1 = new PlayerData("player1", 1000, 1, null);
        player2 = new PlayerData("player2", 500, 2, null);

        playerRepository.Setup(r => r.GetPlayerById(1)).Returns(player1);
        playerRepository.Setup(r => r.GetPlayerById(2)).Returns(player2);

        bankService = new BankService();
        bankService.Consturct(playerRepository.Object, notifier.Object);
    }

    [Test]
    public void AddMoney_ShouldIncreasePlayerMoneyAndNotify()
    {
        bankService.AddMoney(1, 200);

        Assert.AreEqual(1200, player1.Money);
        notifier.Verify(n => n.NotifyBalanceChanged(player1), Times.Once);
    }

    [Test]
    public void AddMoney_NegativeAmount_ShouldDoNothing()
    {
        bankService.AddMoney(1, -50);

        Assert.AreEqual(1000, player1.Money);
        notifier.Verify(n => n.NotifyBalanceChanged(It.IsAny<PlayerData>()), Times.Never);
    }

    [Test]
    public void RemoveMoney_ShouldDecreasePlayerMoneyAndNotify_WhenEnoughMoney()
    {
        bool result = bankService.RemoveMoney(1, 500);

        Assert.IsTrue(result);
        Assert.AreEqual(500, player1.Money);
        notifier.Verify(n => n.NotifyBalanceChanged(player1), Times.Once);
    }

    [Test]
    public void RemoveMoney_NotEnoughMoney_ShouldReturnFalseAndDoNothing()
    {
        bool result = bankService.RemoveMoney(2, 600);

        Assert.IsFalse(result);
        Assert.AreEqual(500, player2.Money);
        notifier.Verify(n => n.NotifyBalanceChanged(It.IsAny<PlayerData>()), Times.Never);
    }

    [Test]
    public void RemoveMoney_NegativeAmount_ShouldReturnFalse()
    {
        bool result = bankService.RemoveMoney(1, -100);

        Assert.IsFalse(result);
        Assert.AreEqual(1000, player1.Money);
        notifier.Verify(n => n.NotifyBalanceChanged(It.IsAny<PlayerData>()), Times.Never);
    }

    [Test]
    public void HasEnoughMoney_ShouldReturnTrue_WhenPlayerHasEnough()
    {
        bool result = bankService.HasEnoughMoney(1, 500);
        Assert.IsTrue(result);
    }

    [Test]
    public void HasEnoughMoney_ShouldReturnFalse_WhenPlayerDoesNotHaveEnough()
    {
        bool result = bankService.HasEnoughMoney(2, 600);
        Assert.IsFalse(result);
    }

    [Test]
    public void TransferMoney_ShouldMoveMoneyBetweenPlayers_WhenEnoughFunds()
    {
        bool result = bankService.TransferMoney(1, 2, 300);

        Assert.IsTrue(result);
        Assert.AreEqual(700, player1.Money);
        Assert.AreEqual(800, player2.Money);
        notifier.Verify(n => n.NotifyBalanceChanged(player1), Times.Once);
        notifier.Verify(n => n.NotifyBalanceChanged(player2), Times.Once);
    }

    [Test]
    public void TransferMoney_ShouldFail_WhenNotEnoughFunds()
    {
        bool result = bankService.TransferMoney(2, 1, 600);

        Assert.IsFalse(result);
        Assert.AreEqual(500, player2.Money);
        Assert.AreEqual(1000, player1.Money);
        notifier.Verify(n => n.NotifyBalanceChanged(It.IsAny<PlayerData>()), Times.Never);
    }

    [Test]
    public void TransferMoney_NegativeAmount_ShouldFail()
    {
        bool result = bankService.TransferMoney(1, 2, -100);

        Assert.IsFalse(result);
        Assert.AreEqual(1000, player1.Money);
        Assert.AreEqual(500, player2.Money);
        notifier.Verify(n => n.NotifyBalanceChanged(It.IsAny<PlayerData>()), Times.Never);
    }
}
