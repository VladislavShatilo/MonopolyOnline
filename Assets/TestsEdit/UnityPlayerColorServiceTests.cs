using NUnit.Framework;
using UnityEngine;

public class UnityPlayerColorServiceTests
{
    private UnityPlayerColorService service;
    private GameSettings gameSettings;

    [SetUp]
    public void SetUp()
    {
        gameSettings = ScriptableObject.CreateInstance<GameSettings>();
        service = new UnityPlayerColorService();
        service.Construct(gameSettings);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(gameSettings);
    }

    private void AssertColorsEqual(PlayerColor expected, PlayerColor actual)
    {
        Assert.AreEqual(expected.R, actual.R, 0.0001f);
        Assert.AreEqual(expected.G, actual.G, 0.0001f);
        Assert.AreEqual(expected.B, actual.B, 0.0001f);
    }

    [Test]
    public void GetColorForPlayer_ShouldReturnWhite_WhenPlayerColorsArrayIsNull()
    {
        gameSettings.playerColors = null;

        var color = service.GetColorForPlayer(1);

        AssertColorsEqual(new PlayerColor(1f, 1f, 1f), color);
    }

    [Test]
    public void GetColorForPlayer_ShouldReturnWhite_WhenPlayerColorsArrayIsEmpty()
    {
        gameSettings.playerColors = new Color[0];

        var color = service.GetColorForPlayer(1);

        AssertColorsEqual(new PlayerColor(1f, 1f, 1f), color);
    }

    [Test]
    public void GetColorForPlayer_ShouldReturnCorrectColor_WhenColorsExist()
    {
        gameSettings.playerColors = new Color[]
        {
            new Color(1, 0, 0),
            new Color(0, 1, 0),
            new Color(0, 0, 1)
        };

        var color1 = service.GetColorForPlayer(1);
        var color2 = service.GetColorForPlayer(2);
        var color3 = service.GetColorForPlayer(3);

        AssertColorsEqual(new PlayerColor(1, 0, 0), color1);
        AssertColorsEqual(new PlayerColor(0, 1, 0), color2);
        AssertColorsEqual(new PlayerColor(0, 0, 1), color3);
    }

    [Test]
    public void GetColorForPlayer_ShouldWrapAround_WhenActorNumberExceedsArrayLength()
    {
        gameSettings.playerColors = new Color[]
        {
            new Color(1, 0, 0),
            new Color(0, 1, 0)
        };

        var color3 = service.GetColorForPlayer(3);

        AssertColorsEqual(new PlayerColor(1, 0, 0), color3);
    }
}
