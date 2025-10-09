using NUnit.Framework;
using UnityEngine;

public class PlayerColorExtensionsTests
{
   
    [Test]
    public void ToUnityColor_ReturnsCorrectUnityColor()
    {
        // Arrange
        var playerColor = new PlayerColor(0.1f, 0.5f, 0.9f);

        // Act
        var result = playerColor.ToUnityColor();

        // Assert
        Assert.AreEqual(new Color(0.1f, 0.5f, 0.9f), result);
    }

    [Test]
    public void ToUnityColor_ReturnsDifferentInstances()
    {
        // Arrange
        var playerColor = new PlayerColor(0.2f, 0.3f, 0.4f);

        // Act
        var color1 = playerColor.ToUnityColor();
        var color2 = playerColor.ToUnityColor();

        // Assert
        Assert.AreNotSame(color1, color2); // разные экземпл€ры
    }

    [Test]
    public void ToUnityColor_ZeroValues_ReturnsBlack()
    {
        // Arrange
        var playerColor = new PlayerColor(0f, 0f, 0f);

        // Act
        var result = playerColor.ToUnityColor();

        // Assert
        Assert.AreEqual(Color.black, result);
    }

    [Test]
    public void ToUnityColor_MaxValues_ReturnsWhite()
    {
        // Arrange
        var playerColor = new PlayerColor(1f, 1f, 1f);

        // Act
        var result = playerColor.ToUnityColor();

        // Assert
        Assert.AreEqual(Color.white, result);
    }
}
