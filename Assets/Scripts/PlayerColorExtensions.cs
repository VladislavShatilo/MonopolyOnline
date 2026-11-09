using UnityEngine;

public static class PlayerColorExtensions
{
    #region PUBLIC_METHODS

    public static Color ToUnityColor(this PlayerColor color)
    {
        if (color == null) return Color.aliceBlue;
        return new Color(color.R, color.G, color.B);
    }

    #endregion PUBLIC_METHODS
}