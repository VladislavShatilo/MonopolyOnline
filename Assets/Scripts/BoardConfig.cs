using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CellType
{
    Company,
    FieldCompany,
    DiceCompany,
    Spend,
    Question,
    Corner
}

public enum CornerType
{
    Start,
    ChillJail,
    Caisno,
    Police
}

public enum CompanyGroup
{
    Perfume,
    Cars,
    Clothes,
    SocialMedia,
    Games,
    Drinks,
    Airlines,
    FastFood,
    Hotels,
    Phones
}

[System.Serializable]
public abstract class CompanyBaseData : ICellDetails
{
    public string name;
    public CompanyGroup group;
    public int price;
    public int pledgePrice;
    public int buyoutPrice;
    public StatsWindowPosition popupData;
}

public interface ICellDetails
{
}

[System.Serializable]
public class CompanyData : CompanyBaseData
{
    public int[] rent;
    public int branchPrice;
}

[System.Serializable]
public class FieldCompanyData : CompanyBaseData
{
    public int[] rentField;
}

[System.Serializable]
public class DiceCompanyData : CompanyBaseData
{
    public int[] rentMultiplier;
}

[System.Serializable]
public class SpendData : ICellDetails
{
    public Sprite spendSprite;
    public int amount;
}

[System.Serializable]
public class QuestionData : ICellDetails
{
    public string description;
    public string effect;
}

[System.Serializable]
public class CornerData : ICellDetails
{
    public Sprite logoSprite;
    public CornerType type;
}

[System.Serializable]
public class CellData
{
    public int index;
    public string cellName;
    public CellType cellType;
    public CompanyData companyData;
    public FieldCompanyData fieldCompanyData;
    public DiceCompanyData diceCompanyData;
    public SpendData spendData;
    public QuestionData questionData;
    public CornerData cornerData;
    public List<PlayerMove> PlayersOnCell = new List<PlayerMove>();
}

[CreateAssetMenu(fileName = "BoardConfig", menuName = "Monopoly/BoardConfig")]
public class BoardConfig : ScriptableObject
{
    public List<CellData> cells = new List<CellData>();
}