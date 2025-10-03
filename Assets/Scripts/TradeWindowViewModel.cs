using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TradeWindowViewModel
{
    public string LeftPlayerName { get; set; }
    public string RightPlayerName { get; set; }
    public int LeftMoney { get; set; }
    public int RightMoney { get; set; }
    public int LeftTotal { get; set; }
    public int RightTotal { get; set; }
    public List<Company> LeftCompanies { get; set; }
    public List<Company> RightCompanies { get; set; }
    public bool CanAccept { get; set; }
    public bool CanCancel { get; set; }
}
