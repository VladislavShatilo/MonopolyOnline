using UnityEngine;

public static class PlayerColorExtensions
{
    #region PUBLIC_METHODS

    public static Color ToUnityColor(this PlayerColor color)
    {
        return new Color(color.R, color.G, color.B);
    }

    #endregion PUBLIC_METHODS
}