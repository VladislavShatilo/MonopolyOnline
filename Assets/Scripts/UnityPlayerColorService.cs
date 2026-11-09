using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UnityPlayerColorService : IPlayerColorService
{
    private GameSettings gameSettings;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(GameSettings gameSettings)
    {
        this.gameSettings = gameSettings ?? throw new ArgumentNullException(nameof(gameSettings));
    }

    #endregion LIFE_CYCLE


    #region PUBLIC_METHODS

    public PlayerColor GetColorForPlayer(int actorNumber)
    {
        if (gameSettings.playerColors == null || gameSettings.playerColors.Length == 0)
            return new PlayerColor(1f, 1f, 1f); // белый по умолчанию

        var c = gameSettings.playerColors[(actorNumber - 1) % gameSettings.playerColors.Length];
        return new PlayerColor(c.r, c.g, c.b);
    }

    #endregion PUBLIC_METHODS


}
