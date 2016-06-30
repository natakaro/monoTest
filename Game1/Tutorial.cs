using Game1.HUD;
using Game1.Screens;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1
{
    public class Tutorial
    {
        Game game;
        ScreenManager ScreenManager;
        HUDManager hudManager;
        Stats stats;
        int step;

        const float delay = 1.0f;
        float age;

        Vector2 spellsOffset;
        Vector2 barsOffset;
        Vector2 barsOffsetRight;
        Vector2 minimapOffset;
        Vector2 iconOffset;

        public Tutorial(Game game, ScreenManager ScreenManager, HUDManager hudManager, Stats stats)
        {
            this.game = game;
            this.ScreenManager = ScreenManager;
            this.hudManager = hudManager;
            this.stats = stats;

            step = 0;

            hudManager.EssenceBar.Enable = false;
            hudManager.Icons.EssenceIcon.Enable = false;

            hudManager.Minimap.Enable = false;

            stats.iceEnabled = false;
            hudManager.Icons.IceIcon.Enable = false;
            stats.moveTerrainEnabled = false;
            hudManager.Icons.MoveTerrainIcon.Enable = false;
            stats.createTurretEnabled = false;
            hudManager.Icons.CreateTurretIcon.Enable = false;
            stats.rightModeEnabled = false;
            hudManager.Icons.UpdateIconPositions();

            spellsOffset = new Vector2(0, hudManager.BackbufferHeight / 4);
            barsOffset = new Vector2(-hudManager.BackbufferWidth / 6, hudManager.BackbufferHeight / 4);
            barsOffsetRight = new Vector2(hudManager.BackbufferWidth / 6, hudManager.BackbufferHeight / 4);
            minimapOffset = new Vector2(hudManager.BackbufferWidth / 6, -hudManager.BackbufferHeight / 3);
            iconOffset = new Vector2(-34, -6);

            hudManager.HealthBar.Position = new Vector2(hudManager.BackbufferWidth / 2 - hudManager.BackbufferWidth / 2 + 100, hudManager.BackbufferHeight - 120);
            hudManager.Icons.HealthIcon.Position = hudManager.HealthBar.Position + iconOffset;
            hudManager.ManaBar.Position = new Vector2(hudManager.BackbufferWidth / 2 - hudManager.BackbufferWidth / 2 + 100, hudManager.BackbufferHeight - 80);
            hudManager.Icons.ManaIcon.Position = hudManager.ManaBar.Position + iconOffset;

            stats.currentExp = 800;
        }

        public void Update(GameTime gameTime)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            age += elapsedTime;

            if (step == 0)
            {
                if (age > delay)
                {
                    string message = "Move: [W], [A], [S], [D]\nSelect spells: Mousewheel or [1], [2], [3], [4]\nCast selected spell: Left Mouse Button";
                    MessageBoxScreen messageBox = new MessageBoxScreen(message, Vector2.Zero, true, true);
                    ScreenManager.AddScreen(messageBox);
                    age = 0;
                    step++;
                }
            }

            if (step == 1)
            {
                if (age > delay)
                {
                    string message = "Your first spell is a basic fireball.\n\nHold the left mouse button to charge, then release to shoot.";
                    MessageBoxScreen messageBox = new MessageBoxScreen(message, spellsOffset, true, true);
                    ScreenManager.AddScreen(messageBox);
                    age = 0;
                    step++;
                }
            }

            if (step == 2)
            {
                if (age > delay)
                {
                    hudManager.Icons.IceIcon.Enable = true;
                    stats.iceEnabled = true;
                    hudManager.Icons.UpdateIconPositions();
                    string message = "Your second spell is an ice bolt.\nIt deals less damage and is slower,\nbut enemies hit by the spell are slowed for a short time.\n\nPress [2] to select the spell.";
                    MessageBoxScreen messageBox = new MessageBoxScreen(message, spellsOffset, true, true);
                    ScreenManager.AddScreen(messageBox);
                    age = 0;
                    step++;
                }
            }

            if (step == 3)
            {
                if (age > delay)
                {
                    string message = "The red bar displays your Health - it depletes when enemies enter the portal.\nThe blue bar displays your Mana which is used for casting spells.";
                    MessageBoxScreen messageBox = new MessageBoxScreen(message, barsOffset, true, true);
                    ScreenManager.AddScreen(messageBox);
                    age = 0;
                    step++;
                }
            }

            if (step == 4)
            {
                if (age > delay)
                {
                    string message = "The yellow bar displays your Experience.\nYou will level up and unlock new abilities when it fills up.\n\nGain experience by defeating enemies.";
                    MessageBoxScreen messageBox = new MessageBoxScreen(message, barsOffsetRight, true, true);
                    ScreenManager.AddScreen(messageBox);
                    age = 0;
                    step++;
                }
            }

            if (step == 5)
            {
                if (age > delay)
                {
                    hudManager.Minimap.Enable = true;
                    string message = "This is the minimap.\nYou can see the path your enemies will take,\nthe rifts that spawn them during the night,\nand their current location.\n\nThe circle around the minimap shows the current time of day.\n\nPress Tab or M to see the full map.";
                    MessageBoxScreen messageBox = new MessageBoxScreen(message, minimapOffset, true, true);
                    ScreenManager.AddScreen(messageBox);
                    age = 0;
                    step++;
                }
            }

            if (step == 6)
            {
                if (age > delay)
                {
                    string message = "The last group of enemies is closing in on your portal.\n\nBanish them from your realm before they reach it!";
                    MessageBoxScreen messageBox = new MessageBoxScreen(message, Vector2.Zero, true, true);
                    ScreenManager.AddScreen(messageBox);
                    age = 0;
                    step++;
                }
            }

            if (step == 7 && GameplayScreen.phaseManager.Phase == Phase.Day)
            {
                if (stats.level <= 1)
                {
                    stats.currentExp = stats.maxExp;
                }
                if (age > delay)
                {
                    hudManager.Icons.MoveTerrainIcon.Enable = true;
                    stats.moveTerrainEnabled = true;
                    hudManager.Icons.CreateTurretIcon.Enable = true;
                    stats.createTurretEnabled = true;
                    hudManager.Icons.UpdateIconPositions();
                    string message = "You have leveled up!\n\nYou can use two new spells.";
                    MessageBoxScreen messageBox = new MessageBoxScreen(message, Vector2.Zero, true, true);
                    ScreenManager.AddScreen(messageBox);
                    age = 0;
                    step++;
                }
            }

            if (step == 8)
            {
                if (age > delay)
                {
                    string message = "Your first new spell allows you to move terrain up or down.\n\nYou can only use the spell during the day.\nYou can use it to alter your enemies' path,\nbut you can't block it completely.\n\nPress [3] to select the spell,\nhold the left mouse button to raise a tile,\nor the right mouse button to lower it.";
                    MessageBoxScreen messageBox = new MessageBoxScreen(message, spellsOffset, true, true);
                    ScreenManager.AddScreen(messageBox);
                    age = 0;
                    step++;
                }
            }

            if (step == 9)
            {
                if (age > delay)
                {
                    string message = "Your second new spell allows you to create turrets.\n\nPress [4] to select the spell,\nhold the left mouse button to charge, and release to create a turret.\nUse the right mouse button to destroy a turret.";
                    MessageBoxScreen messageBox = new MessageBoxScreen(message, spellsOffset, true, true);
                    ScreenManager.AddScreen(messageBox);
                    age = 0;
                    step++;
                }
            }

            if (step == 10)
            {
                if (age > delay)
                {
                    string message = "A new turret is dormant and does nothing.\nActivate it by casting a spell on it, and the turret will use that spell on your enemies.";
                    MessageBoxScreen messageBox = new MessageBoxScreen(message, spellsOffset, true, true);
                    ScreenManager.AddScreen(messageBox);
                    age = 0;
                    step++;
                }
            }

            if (step == 11)
            {
                if (age > delay)
                {
                    hudManager.HealthBar.Position = new Vector2(hudManager.BackbufferWidth / 2 - hudManager.BackbufferWidth / 2 + 100, hudManager.BackbufferHeight - 160);
                    hudManager.Icons.HealthIcon.Position = hudManager.HealthBar.Position + iconOffset;
                    hudManager.ManaBar.Position = new Vector2(hudManager.BackbufferWidth / 2 - hudManager.BackbufferWidth / 2 + 100, hudManager.BackbufferHeight - 120);
                    hudManager.Icons.ManaIcon.Position = hudManager.ManaBar.Position + iconOffset;
                    hudManager.EssenceBar.Enable = true;
                    hudManager.Icons.EssenceIcon.Enable = true;
                    string message = "The purple bar displays your Essence, which is used to create turrets.\n\nEssence does not regenerate - you have to collect it from your fallen enemies.";
                    MessageBoxScreen messageBox = new MessageBoxScreen(message, barsOffset, true, true);
                    ScreenManager.AddScreen(messageBox);
                    age = 0;
                    step++;
                }
            }

            if (step == 12)
            {
                if (age > delay)
                {
                    hudManager.EssenceBar.Enable = true;
                    hudManager.Icons.EssenceIcon.Enable = true;
                    string message = "During the day, you can modify the terrain to alter your enemies' paths.\nUse the time wisely.\n\nRemember to place turrets near the highlighted paths.\nYou can create turrets after nightfall too.";
                    MessageBoxScreen messageBox = new MessageBoxScreen(message, Vector2.Zero, true, true);
                    ScreenManager.AddScreen(messageBox);
                    age = 0;
                    step++;
                }
            }

            if (step == 13 && stats.level > 2)
            {
                if (age > delay)
                {
                    stats.rightModeEnabled = true;
                    string message = "You have leveled up!\n\nYou now have access to an alternate mode of Fire and Ice spells.";
                    MessageBoxScreen messageBox = new MessageBoxScreen(message, Vector2.Zero, true, true);
                    ScreenManager.AddScreen(messageBox);
                    age = 0;
                    step++;
                }
            }

            if (step == 14)
            {
                if (age > delay)
                {
                    string message = "Hold the right mouse button to cast a short-range continuous stream of fire or ice.\nIt quickly depletes your Mana but hits all enemies in range.\n\nRemember that you can also use it on turrets to attune them to this type of spell.";
                    MessageBoxScreen messageBox = new MessageBoxScreen(message, spellsOffset, true, true);
                    ScreenManager.AddScreen(messageBox);
                    age = 0;
                    step++;
                }
            }
        }
    }
}
