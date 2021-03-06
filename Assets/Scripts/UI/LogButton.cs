﻿using UnityEngine;
using System.Collections;

// Special class for the Menu button present while in a quest
public class LogButton
{
    public LogButton()
    {
        Game game = Game.Get();
        TextButton qb;
        // For the editor button is moved to the right
        if (game.editMode) return;

        if (game.gameType is MoMGameType) return;

        qb = new TextButton(new Vector2(5.5f, UIScaler.GetBottom(-2.5f)), new Vector2(5, 2), "Log", delegate { Log(); });
        qb.SetFont(game.gameType.GetHeaderFont());
        // Untag as dialog so this isn't cleared away
        qb.ApplyTag("questui");
    }

    // When pressed bring up the approriate menu
    public void Log()
    {
        if (GameObject.FindGameObjectWithTag("dialog") == null)
        {
            new LogWindow();
        }
    }
}
