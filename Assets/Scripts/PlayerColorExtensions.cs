using UnityEngine;

public static class PlayerColorExtensions
{
    public static Color ToUnityColor(this PlayerColor color)
    {
        return new Color(color.R, color.G, color.B);
    }
}