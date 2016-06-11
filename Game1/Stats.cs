﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
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
    public class Stats
    {
        public float currentHealth;
        public float maxHealth;
        public float currentMana;
        public float maxMana;
        public float coreHealth;

        int healthRegen;
        int manaRegen;

        public Stats()
        {
            maxHealth = 100;
            maxMana = 200;
            currentHealth = maxHealth;
            currentMana = maxMana;

            coreHealth = 1000;

            healthRegen = 1;
            manaRegen = 25;
        }

        public void Update(GameTime gameTime)
        {
            if (currentMana < maxMana)
                currentMana = Math.Min(currentMana + (float)gameTime.ElapsedGameTime.TotalSeconds * manaRegen, maxMana);
            if (currentHealth < maxHealth)
                currentHealth = Math.Min(currentHealth + (float)gameTime.ElapsedGameTime.TotalSeconds * healthRegen, maxHealth);
        }
    }
}