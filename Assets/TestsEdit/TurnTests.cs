using NUnit.Framework;

[TestFixture]
public class TurnTests
{
    private Turn turn;

    [SetUp]
    public void SetUp()
    {
        turn = new Turn();
    }

    [Test]
    public void StartTurn_ShouldSetCurrentPlayerAndActivateTurn()
    {
        turn.StartTurn(1);

        Assert.AreEqual(1, turn.CurrentPlayerId);
        Assert.IsTrue(turn.IsActive);
        Assert.AreEqual(TurnMode.Normal, turn.Mode);
    }

    [Test]
    public void EndTurn_ShouldDeactivateTurn()
    {
        turn.StartTurn(1);
        turn.EndTurn();

        Assert.IsFalse(turn.IsActive);
    }

    [Test]
    public void SetMode_ShouldChangeTurnMode()
    {
        turn.SetMode(TurnMode.Auction);
        Assert.AreEqual(TurnMode.Auction, turn.Mode);

        turn.SetMode(TurnMode.Normal);
        Assert.AreEqual(TurnMode.Normal, turn.Mode);
    }

    [Test]
    public void AddExtraTurn_ShouldRegisterExtraTurnForPlayer()
    {
        turn.AddExtraTurn(1);

        Assert.IsTrue(turn.HasExtraTurn(1));
        Assert.IsFalse(turn.HasExtraTurn(2));
    }

    [Test]
    public void RemoveExtraTurn_ShouldUnregisterExtraTurnForPlayer()
    {
        turn.AddExtraTurn(1);
        turn.RemoveExtraTurn(1);

        Assert.IsFalse(turn.HasExtraTurn(1));
    }

    [Test]
    public void HasExtraTurn_ShouldReturnCorrectValue()
    {
        Assert.IsFalse(turn.HasExtraTurn(1));

        turn.AddExtraTurn(1);
        Assert.IsTrue(turn.HasExtraTurn(1));

        turn.AddExtraTurn(2);
        Assert.IsTrue(turn.HasExtraTurn(2));

        turn.RemoveExtraTurn(1);
        Assert.IsFalse(turn.HasExtraTurn(1));
        Assert.IsTrue(turn.HasExtraTurn(2));
    }
    [Test]
    public void AddExtraTurn_ShouldBeIdempotent()
    {
        turn.AddExtraTurn(1);
        turn.AddExtraTurn(1);
        Assert.IsTrue(turn.HasExtraTurn(1));
    }

    [Test]
    public void RemoveExtraTurn_ShouldHandleNonExistingPlayerGracefully()
    {
        turn.RemoveExtraTurn(99); // не должно вызывать ошибку
        Assert.Pass();
    }

    [Test]
    public void ExtraTurnRemainsAfterEndTurn()
    {
        turn.StartTurn(1);
        turn.AddExtraTurn(1);
        turn.EndTurn();
        Assert.IsTrue(turn.HasExtraTurn(1));
        Assert.IsFalse(turn.IsActive);
    }

}
