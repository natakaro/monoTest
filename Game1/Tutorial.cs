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
        GameSettings settings;
        int step;

        const float delay = 0.1f;
        float age;

        Vector2 spellsOffset;
        Vector2 barsOffset;
        Vector2 barsOffsetRight;
        Vector2 minimapOffset;
        Vector2 iconOffset;

        public Tutorial(Game game, ScreenManager ScreenManager, HUDManager hudManager, Stats stats, GameSettings settings)
        {
            this.game = game;
            this.ScreenManager = ScreenManager;
            this.hudManager = hudManager;
            this.stats = stats;
            this.settings = settings;

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
                    if (settings.Polish)
                        message = "Sterowanie: [W], [A], [S], [D]\nWybór czaru: Kółko myszki lub [1], [2], [3], [4]\nRzucenie czaru: Lewy przycisk myszy";
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
                    if (settings.Polish)
                        message = "Twoim pierwszym czarem jest klasyczna kula ognia.\n\nPrzytrzymaj lewy przycisk myszy, by przygotować czar, a następnie puść by go rzucić.";
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
                    if (settings.Polish)
                        message = "Twoim drugim czarem jest lodowa lanca.\nZadaje mniej obrażeń i wolniej się przemieszcza,\nale wrogowie trafieni przez nią zostają spowolnieni na krótki czas.\n\nWciśnij [2] by wybrać lodową lancę.";
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
                    if (settings.Polish)
                        message = "Czerwony pasek przestawia twoje Zdrowie - zmniejsza się, gdy wrogowie wchodzą do portalu.\nNiebieski pasek przestawia twoją Manę, używaną do rzucania czarów.";
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
                    string message = "The yellow bar displays your Experience.\nYou will gain a level and unlock new abilities when it fills up.\n\nGain experience by defeating enemies.";
                    if (settings.Polish)
                        message = "Żółty pasek przedstawia twoje Doświadczenie.\nOtrzymasz nowy poziom doświadczenia i odblokujesz nowe umiejętności, gdy się wypełni.\n\nDoświadczenie zdobywasz pokonując wrogów.";
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
                    string message = "This is the minimap.\nYou can see the path your enemies will take,\nthe rifts that spawn them during the night,\nand their current location.\n\nThe circle around the minimap shows the current time of day.\n\nPress [Tab] or [M] to see the full map.";
                    if (settings.Polish)
                        message = "To minimapa.\nWidać na niej ścieżkę, po której będą szli wrogowie,\nszczeliny, z których wychodzą w nocy,\noraz ich obecną pozycję.\n\nOkrąg wokół minimapy wskazuje aktualną porę dnia.\n\nWciśnij [Tab] lub [M] by zobaczyć pełną mapę.";
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
                    if (settings.Polish)
                        message = "Ostatnia grupa wrogów zbliża się do portalu.\n\nPrzepędź ich ze swojego wymiaru zanim do niego dotrą!";
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
                    string message = "You have gained a level!\n\nYou can use two new spells.";
                    if (settings.Polish)
                        message = "Otrzymałeś nowy poziom doświadczenia!\n\nMasz do dyspozycji dwa nowe czary.";
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
                    if (settings.Polish)
                        message = "Pierwszy z nich pozwala ci poruszać terenem w górę lub w dół.\n\nJest aktywny jedynie w trakcie dnia.\nMożesz modyfikować w ten sposób ścieżkę, którą obiorą wrogowie,\nale nie możesz jej zablokować całkowicie.\n\nWciśnij [3] aby wybrać czar,\nprzytrzymaj lewy przycisk myszy, by unieść teren\nlub prawy, by go opuścić.";
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
                    if (settings.Polish)
                        message = "Twój drugi nowy czar pozwala ci tworzyć wieżyczki.\n\nWciśnij [4] by wybrać czar,\nprzytrzymaj lewy przycisk myszy by przygotować czar, po czym puść by stworzyć wieżyczkę.\nPrawy przycisk myszy pozwala ci zniszczyć postawioną wieżyczkę.";
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
                    if (settings.Polish)
                        message = "Nowa wieżyczka jest nieaktywna.\nAktywuj ją rzucając na nią czar - wieżyczka będzie go używać na twoich wrogach.";
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
                    string message = "The purple bar displays your Essence, which is used to create turrets.\n\nEssence does not regenerate - you have to collect it from fallen enemies.";
                    if (settings.Polish)
                        message = "Fioletowy pasek przedstawia twoją Esencję, która służy do tworzenia wieżyczek.\n\nEsencja nie regeneruje się - musisz zbierać ją z poległych wrogów.";
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
                    string message = "You can modify the terrain to alter your enemies' paths, but only during the day.\nUse the time wisely.\n\nRemember to place turrets near the highlighted paths.\nTurrets can be created anytime.";
                    if (settings.Polish)
                        message = "Możesz modyfikować teren by zmienić scieżkę swoich wrogów, ale jedynie w trakcie dnia.\nWykorzystaj mądrze ten czas.\n\nPamiętaj, by stawiać wieżyczki niedaleko podświetlonych ścieżek.\nMożesz tworzyć wieżyczki zarówno w trakcie dnia, jak i nocy.";
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
                    hudManager.PhaseMessage.Enable = false;
                    stats.rightModeEnabled = true;
                    string message = "You have gained a level!\n\nYou now have access to an alternate mode of Fire and Ice spells.";
                    if (settings.Polish)
                        message = "Otrzymałeś nowy poziom doświadczenia!\n\nMasz teraz dostęp do alternatywnego trybu czaru Ognia i Lodu.";
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
                    string message = "Hold the right mouse button to cast a short-range continuous stream of fire or ice.\nIt quickly depletes your Mana, but hits all enemies in range.\n\nRemember that you can also use it on turrets to attune them to this type of spell.";
                    if (settings.Polish)
                        message = "Przytrzymaj prawy przycisk myszy, by rzucić krótkozasięgowy strumień ognia lub lodu.\nSzybko zużywa twoją Manę, ale trafia wszystkich wrogów w zasięgu.\n\nPamiętaj, że możesz użyć go na wieżyczce, by dostroić ją do tego typu czaru.";
                    MessageBoxScreen messageBox = new MessageBoxScreen(message, spellsOffset, true, true);
                    ScreenManager.AddScreen(messageBox);
                    age = 0;
                    step++;
                }
            }
        }
    }
}
