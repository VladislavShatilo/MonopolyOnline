using NUnit.Framework;
using System.Collections.Generic;

[TestFixture]
public class TradeOfferTests
{
    private PlayerData playerA;
    private PlayerData playerB;
    private TradeOffer tradeOffer;

    [SetUp]
    public void SetUp()
    {
        playerA = new PlayerData("PlayerA", 1000, 1, null);
        playerB = new PlayerData("PlayerB", 1000, 2, null);
        tradeOffer = new TradeOffer(playerA, playerB);
    }

    [Test]
    public void SetFromCompanies_Should_AddCompanies()
    {
        var company1 = new Company(1, new CompanyData());
        company1.Price = 500;
        var company2 = new Company(2, new CompanyData());
        company2.Price = 300;
        var companies = new List<Company>
        {
            company1,
            company2
        };

        tradeOffer.SetFromCompanies(companies);

        Assert.AreEqual(2, tradeOffer.FromCompanies.Count);
        Assert.AreEqual(500, tradeOffer.FromCompanies[0].Price);
        Assert.AreEqual(300, tradeOffer.FromCompanies[1].Price);
    }

    [Test]
    public void SetToCompanies_Should_AddCompanies()
    {
        var company1 = new Company(1, new CompanyData());
        company1.Price = 200;
        var companies = new List<Company>
        {
            company1
        };

        tradeOffer.SetToCompanies(companies);

        Assert.AreEqual(1, tradeOffer.ToCompanies.Count);
        Assert.AreEqual(200, tradeOffer.ToCompanies[0].Price);
    }

    [Test]
    public void GetFromTotalValue_Should_CalculateCorrectly()
    {
        tradeOffer.FromMoney = 100;

        var company1 = new Company(1, new CompanyData());
        company1.Price = 50;
        var company2 = new Company(2, new CompanyData());
        company2.Price = 150;
        var companies = new List<Company>
        {
            company1,
            company2
        };
        tradeOffer.SetFromCompanies(companies);
        int total = tradeOffer.GetFromTotalValue();

        Assert.AreEqual(300, total); // 100 + 50 + 150
    }

    [Test]
    public void GetToTotalValue_Should_CalculateCorrectly()
    {
        tradeOffer.ToMoney = 200;

        var company1 = new Company(1, new CompanyData());
        company1.Price = 100;

        tradeOffer.SetToCompanies(new List<Company>
        {
           company1
        });

        int total = tradeOffer.GetToTotalValue();

        Assert.AreEqual(300, total); // 200 + 100
    }

    [Test]
    public void IsValid_ShouldReturnFalse_WhenFromOrToTotalIsZero()
    {
        tradeOffer.FromMoney = 0;
        tradeOffer.ToMoney = 0;

        Assert.IsFalse(tradeOffer.IsValid());
    }

    [Test]
    public void IsValid_ShouldReturnTrue_WhenRatioWithinLimits()
    {
        tradeOffer.FromMoney = 100;
        tradeOffer.ToMoney = 200;

        Assert.IsTrue(tradeOffer.IsValid()); // ratio = 0.5
    }

    [Test]
    public void IsValid_ShouldReturnFalse_WhenRatioTooLow()
    {
        tradeOffer.FromMoney = 50;
        tradeOffer.ToMoney = 200;

        Assert.IsFalse(tradeOffer.IsValid()); // ratio = 0.25
    }

    [Test]
    public void IsValid_ShouldReturnFalse_WhenRatioTooHigh()
    {
        tradeOffer.FromMoney = 500;
        tradeOffer.ToMoney = 200;

        Assert.IsFalse(tradeOffer.IsValid()); // ratio = 2.5
    }
}
