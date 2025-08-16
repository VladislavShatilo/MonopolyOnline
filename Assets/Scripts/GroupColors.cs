using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GroupColors
{
    public static Color[] Colors = new Color[]
     {
        ParseHex("#EC87C1"),    // Блестящий пурпурный
        ParseHex("#DA4553"),    // Кирпично-красный
        ParseHex("#E0B439"),    // Старое золото
        ParseHex("#37ВС9В"),    // Зеленые джунгли
        ParseHex("#7F1F0F"),    // Насыщенный красно-коричневый
        ParseHex("#4B89DC") ,   // Синяя сталь
        ParseHex("#8CC152"),    // Блестящий желтовато-зеленый
        ParseHex("#4FC1E9"),    // Умеренно-бирюзовый
        ParseHex("#967BDC"),    // Средний пурпурный
        ParseHex("#636A75")     // Перламатрово-ежевичный
     };

    private static Color ParseHex(string hex)
    {
        Color color;
        if (ColorUtility.TryParseHtmlString(hex, out color))
        {
            return color;
        }
        return Color.magenta; // Возвращаем цвет по умолчанию при ошибке
    }
}