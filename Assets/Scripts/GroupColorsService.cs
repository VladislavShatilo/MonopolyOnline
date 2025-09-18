using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupColorsService : IGroupColors
{
    public IReadOnlyList<Color> Colors { get; }

    public GroupColorsService()
    {
        Colors = new List<Color>
        {
            ParseHex("#EC87C1"), // Блестящий пурпурный
            ParseHex("#DA4553"), // Кирпично-красный
            ParseHex("#E0B439"), // Старое золото
            ParseHex("#37BC9B"), // Зеленые джунгли (поправил некорректный код)
            ParseHex("#7F1F0F"), // Насыщенный красно-коричневый
            ParseHex("#4B89DC"), // Синяя сталь
            ParseHex("#8CC152"), // Блестящий желтовато-зеленый
            ParseHex("#4FC1E9"), // Умеренно-бирюзовый
            ParseHex("#967BDC"), // Средний пурпурный
            ParseHex("#636A75")  // Перламатрово-ежевичный
        };
    }

    private Color ParseHex(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out var color))
            return color;
        return Color.magenta; // fallback
    }
}
