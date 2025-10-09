using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;

public class PlayerDataTests
{
    private PlayerData player;
    private PlayerColor color;

    [SetUp]
    public void SetUp()
    {
        color = new PlayerColor (1,0.5f,0);
        player = new PlayerData("Alice", 1000, 1, color);
    }

    [Test]
    public void Constructor_ShouldInitializeFieldsCorrectly()
    {
        Assert.AreEqual("Alice", player.Name);
        Assert.AreEqual(1000, player.Money);
        Assert.AreEqual(1, player.Id);
        Assert.AreEqual(color, player.PlayerColor);
        Assert.AreEqual(0, player.CurrentCellId);
        Assert.IsFalse(player.IsInJail);
        Assert.IsFalse(player.HasLoan);
        Assert.IsFalse(player.SkipNextTurn);
        Assert.IsFalse(player.NextMoveBackward);
        Assert.NotNull(player.OwnedCompanies);
        Assert.IsEmpty(player.OwnedCompanies);
    }

    [Test]
    public void Constructor_ShouldAssignPhotonPlayer_WhenProvided()
    {
        var photonPlayer = new Player("PhotonPlayer", 5);
        var playerData = new PlayerData("Bob", 500, 2, color, photonPlayer);

        Assert.AreEqual(photonPlayer, playerData.photonPlayer);
    }

    [Test]
    public void SendToJail_ShouldSetJailStateCorrectly()
    {
        var settings = new GameSettings { jailCellId = 10, jailTurns = 3 };

        player.SendToJail(settings);

        Assert.IsTrue(player.IsInJail);
        Assert.AreEqual(10, player.CurrentCellId);
        Assert.AreEqual(3, player.JailTurnsLeft);
    }

    [Test]
    public void Release_ShouldClearJailState()
    {
        var settings = new GameSettings { jailCellId = 10, jailTurns = 3 };
        player.SendToJail(settings);

        player.Release();

        Assert.IsFalse(player.IsInJail);
        Assert.AreEqual(0, player.JailTurnsLeft);
    }

    [Test]
    public void OwnedCompanies_ShouldBeMutable()
    {
        var company = new Company(1, new CompanyData());
        company.Name = "TestCorp";
        company.Price = 100;
        player.OwnedCompanies.Add(company);

        Assert.AreEqual(1, player.OwnedCompanies.Count);
        Assert.AreEqual("TestCorp", player.OwnedCompanies[0].Name);
    }
}
