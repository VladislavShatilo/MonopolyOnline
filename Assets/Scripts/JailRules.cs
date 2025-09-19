
public class JailRules
{
    public int MaxTurns { get; }
    public int Fine { get; }

    public JailRules(int maxTurns, int fine)
    {
        MaxTurns = maxTurns;
        Fine = fine;
    }
}