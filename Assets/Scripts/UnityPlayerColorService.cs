using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityPlayerColorService : IPlayerColorService
{
    private readonly Color[] colors;

    #region PUBLIC_METHODS

    public PlayerColor GetColorForPlayer(int actorNumber)
    {
        if (colors == null || colors.Length == 0)
            return new PlayerColor(1f, 1f, 1f); // белый по умолчанию

        var c = colors[(actorNumber - 1) % colors.Length];
        return new PlayerColor(c.r, c.g, c.b);
    }

    #endregion PUBLIC_METHODS


}
