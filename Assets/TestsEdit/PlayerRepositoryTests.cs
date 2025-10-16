using NUnit.Framework;
using System.Collections.Generic;

[TestFixture]
public class PlayerRepositoryTests
{
    private PlayerRepository repository;

    [SetUp]
    public void Setup()
    {
        repository = new PlayerRepository();
    }

    [Test]
    public void AddPlayer_ShouldAddPlayerToList()
    {
        // Arrange
        var player = new PlayerData("player1", 500, 1, null);


        // Act
        repository.AddPlayer(player);

        // Assert
        var result = repository.GetPlayerById(1);
        Assert.AreEqual(player, result);
    }

    [Test]
    public void GetPlayerById_ShouldReturnCorrectPlayer()
    {
        var player1 = new PlayerData("player1", 500, 1, null);
        var player2 = new PlayerData("player2", 500, 2, null);

        // Arrange
     
        repository.AddPlayer(player1);
        repository.AddPlayer(player2);

        // Act
        var result = repository.GetPlayerById(2);

        // Assert
        Assert.AreEqual("player2", result.Name);
    }

    [Test]
    public void GetPlayerById_ShouldReturnNull_WhenNotFound()
    {
        // Act
        var result = repository.GetPlayerById(99);

        // Assert
        Assert.IsNull(result);
    }

    [Test]
    public void GetAllPlayers_ShouldReturnReadOnlyList()
    {
        // Arrange
        var player1 = new PlayerData("player1", 500, 1, null);

        repository.AddPlayer(player1);

        // Act
        var allPlayers = repository.GetAllPlayers();

        // Assert
        Assert.AreEqual(1, allPlayers.Count);
        Assert.AreEqual(player1, allPlayers[0]);
        Assert.IsInstanceOf<IReadOnlyList<PlayerData>>(allPlayers);
    }

    [Test]
    public void RemovePlayer_ShouldRemoveExistingPlayer()
    {
        var player1 = new PlayerData("player1", 500, 1, null);

        // Arrange
        repository.AddPlayer(player1);

        // Act
        repository.RemovePlayer(1);

        // Assert
        Assert.IsNull(repository.GetPlayerById(1));
    }

    [Test]
    public void RemovePlayer_ShouldNotThrow_WhenPlayerNotFound()
    {
        // Act & Assert
        Assert.DoesNotThrow(() => repository.RemovePlayer(123));
    }

    [Test]
    public void GetNextPlayerId_ShouldReturnNextPlayer()
    {
        // Arrange
        var player1 = new PlayerData("player1", 500, 1, null);
        var player2 = new PlayerData("player2", 500, 2, null);
        var player3 = new PlayerData("player3", 500, 3, null);

        repository.AddPlayer(player1);
        repository.AddPlayer(player2);
        repository.AddPlayer(player3);

        // Act
        var next = repository.GetNextPlayerId(2);

        // Assert
        Assert.AreEqual(3, next.Id);
    }

    [Test]
    public void GetNextPlayerId_ShouldWrapAround_WhenLastPlayer()
    {
        // Arrange
        var player1 = new PlayerData("player1", 500, 1, null);
        var player2 = new PlayerData("player2", 500, 2, null);
        repository.AddPlayer(player1);
        repository.AddPlayer(player2);

        // Act
        var next = repository.GetNextPlayerId(2);

        // Assert
        Assert.AreEqual(1, next.Id);
    }

    [Test]
    public void GetNextPlayerId_ShouldThrow_WhenListIsEmpty()
    {
        // Act & Assert
        Assert.Throws<System.ArgumentOutOfRangeException>(() => repository.GetNextPlayerId(1));
    }
    [Test]
    public void GetNextPlayerId_ShouldThrow_WhenPlayerIdNotFound()
    {
        var player1 = new PlayerData("player1", 500, 1, null);
        repository.AddPlayer(player1);

        Assert.Throws<System.ArgumentException>(() => repository.GetNextPlayerId(999));
    }

}
