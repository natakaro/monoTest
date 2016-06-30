using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1
{
    public enum StatsValue
    {
        Health = 0,
        Mana = 1,
        CoreHealth = 2
    };

    public enum SpellCharging
    {
        None = 0,
        Left = 1,
        Right = 2,
        Dual = 3
    };

    public class Stats
    {
        public float currentHealth;
        public float maxHealth;
        public float currentMana;
        public float maxMana;
        public float currentEssence;
        public float maxEssence;

        public float currentExp;
        public float maxExp;
        public int level;

        public Enemy lastTargetedEnemy;
        public Enemy currentTargetedEnemy;

        public SpellCharging spellCharging;
        public float castSpeed;
        public Stopwatch castTimer;

        int healthRegen;
        int manaRegen;
        int essenceRegen;

        public bool fireEnabled;
        public bool iceEnabled;
        public bool moveTerrainEnabled;
        public bool createTurretEnabled;

        public bool rightModeEnabled;

        public Stats()
        {
            maxHealth = 250;
            maxMana = 250;
            maxEssence = 250;

            maxExp = 1000;
            
            currentHealth = maxHealth;
            currentMana = maxMana;
            currentEssence = maxEssence;

            currentExp = 0;

            level = 1;
            
            healthRegen = 1;
            manaRegen = 30;
            essenceRegen = 0;

            lastTargetedEnemy = null;
            currentTargetedEnemy = null;

            fireEnabled = true;
            iceEnabled = true;
            moveTerrainEnabled = true;
            createTurretEnabled = true;

            rightModeEnabled = true;
        }

        public void Update(GameTime gameTime)
        {
            if (currentMana < maxMana)
                currentMana = Math.Min(currentMana + (float)gameTime.ElapsedGameTime.TotalSeconds * manaRegen, maxMana);
            if (currentHealth < maxHealth)
                currentHealth = Math.Min(currentHealth + (float)gameTime.ElapsedGameTime.TotalSeconds * healthRegen, maxHealth);
            if (currentEssence < maxEssence)
                currentEssence = Math.Min(currentEssence + (float)gameTime.ElapsedGameTime.TotalSeconds * essenceRegen, maxEssence);
            if (currentExp >= maxExp)
            {
                currentExp -= maxExp;
                level++;
                maxHealth += 50;
                maxMana += 50;
                maxEssence += 50;
                manaRegen += 5;
                healthRegen += 1;
            }
        }

        public void SpellStatus(SpellCharging spellCharging, float castSpeed = 0, Stopwatch castTimer = null)
        {
            this.spellCharging = spellCharging;
            this.castSpeed = castSpeed;
            this.castTimer = castTimer;
        }
    }
}
