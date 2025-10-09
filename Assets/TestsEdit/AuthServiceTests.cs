using NUnit.Framework;
using Photon.Pun;

[TestFixture]
public class AuthServiceTests
{
    private AuthService authService;

    [SetUp]
    public void Setup()
    {
        authService = new AuthService();
    }

    [Test]
    public void Login_ShouldTrimNicknameAndSetPassword()
    {
        // Arrange
        string nickname = "  Player1  ";
        string password = "pass123";

        // Act
        authService.Login(nickname, password);
        var data = authService.GetPlayerData();

        // Assert
        Assert.AreEqual("Player1", data.Nickname);
        Assert.AreEqual(password, data.Password);
        Assert.AreEqual("Player1", PhotonNetwork.NickName);
    }

    [Test]
    public void GetPlayerData_ShouldReturnCurrentPlayerData()
    {
        // Arrange
        string nickname = "TestUser";
        string password = "123";

        authService.Login(nickname, password);

        // Act
        var data = authService.GetPlayerData();

        // Assert
        Assert.AreEqual("TestUser", data.Nickname);
        Assert.AreEqual("123", data.Password);
    }

    [Test]
    public void Login_WithEmptyNickname_ShouldTrimToEmptyString()
    {
        authService.Login("   ", "abc");

        var data = authService.GetPlayerData();

        Assert.AreEqual(string.Empty, data.Nickname);
        Assert.AreEqual("abc", data.Password);
        Assert.AreEqual(string.Empty, PhotonNetwork.NickName);
    }
}
