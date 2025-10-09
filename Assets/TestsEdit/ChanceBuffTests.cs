using NUnit.Framework;

[TestFixture]
public class ChanceBuffTests
{
    [Test]
    public void Constructor_ShouldAssignProperties_WhenValuesProvided()
    {
        var buff = new ChanceBuff(BuffType.Teleport, 10, 50);

        Assert.AreEqual(BuffType.Teleport, buff.Type);
        Assert.AreEqual(10, buff.MinAmount);
        Assert.AreEqual(50, buff.MaxAmount);
    }

    [Test]
    public void Constructor_ShouldAssignDefaultValues_WhenAmountsNotProvided()
    {
        var buff = new ChanceBuff(BuffType.ReverseMove);

        Assert.AreEqual(BuffType.ReverseMove, buff.Type);
        Assert.AreEqual(0, buff.MinAmount);
        Assert.AreEqual(0, buff.MaxAmount);
    }
}
