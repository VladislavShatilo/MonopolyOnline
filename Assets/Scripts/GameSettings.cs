using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/GameSettings")]
public class GameSettings : ScriptableObject
{
    [Header("Game")]
    public int startPlayerMoney = 100_000;
    public int lapMoney = 2_000;

    [Header("Mortgage")]
    public int mortgageTurns = 7;

    [Header("Timers")]
    public int turnTime = 90;

    public int auctionTime = 15;
    public int tradeTime = 15;

    [Header("Jail")]
    public int jailTurns = 3;

    public int jailRansom = 500;
    public int jailCellId = 10;

    [Header("Branch")]
    public int maxBranchLevel = 5;

    [Header("Loan")]
    public int loanAmount = 5000;

    public int loanAmountBack = 5500;
}