using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum CellType
{
    Company,
    Spend,
    Question,
    Corner
}
public enum CompanyGroup
{
    Perfume,
    Clothes,
    SocialMedia,
    Cars,
    Drinks,
    Airlines,
    FastFood,
    Hotels,
    Phones,
    Games
}

public interface ICellDetails { }

[System.Serializable]
public class CompanyData : ICellDetails
{
    public string name;
    public int[] price;
    public int []rent;
    public bool isBought;
    public int ownerID;
    public CompanyGroup group;
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
}
[System.Serializable]
public class CellData
{
    public int index;
    public string cellName;
    public CellType cellType;
    public CompanyData companyData;
    public SpendData spendData;
    public QuestionData questionData;
    public CornerData cornerData;
}

[CreateAssetMenu(fileName = "BoardConfig", menuName = "Monopoly/BoardConfig")]
public class BoardConfig : ScriptableObject
{
    public List<CellData> cells = new List<CellData>();
}
