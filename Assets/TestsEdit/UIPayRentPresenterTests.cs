using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

public class UIPayRentPresenterTests
{
    private UIPayRentPresenter presenter;

    [Test]
    public void GetPayRentState_PlayerCanPay_ShouldReturnCanPayTrue()
    {
        // Arrange
        var color = new PlayerColor(1, 0, 0);

        var player = new PlayerData("Vlad", 200, 0, color);
        float rent = 150;
        presenter = new UIPayRentPresenter(player, 15, rent);

        // Act
        var result = presenter.GetState();

        // Assert
        UnityEngine.Assertions.Assert.IsTrue(result.CanPay);
        UnityEngine.Assertions.Assert.AreEqual("Заплатите 150", result.RentText);
    }

    [Test]
    public void GetPayRentState_PlayerCannotPay_ShouldReturnCanPayFalse()
    {
        // Arrange
        var color = new PlayerColor(1, 0, 0);

        var player = new PlayerData("Vlad", 50, 0, color);
        float rent = 100;
        presenter = new UIPayRentPresenter(player, 15, rent);
        // Act
        var result = presenter.GetState();

        // Assert
        UnityEngine.Assertions.Assert.IsFalse(result.CanPay);
        UnityEngine.Assertions.Assert.AreEqual("Заплатите 100", result.RentText);
    }

    [Test]
    public void GetPayRentState_NullPlayer_ShouldThrow()
    {
        // Arrange
        PlayerData player = null;
        presenter = new UIPayRentPresenter(player, 15, 120);

        // Act + Assert
        NUnit.Framework.Assert.Throws<NullReferenceException>(() => presenter.OnPayRent());
    }
}
