using BepInEx.Configuration;
using System;
using UnityEngine;

namespace JinxMod.Modules
{
    public static class Config
    {
        public static ConfigEntry<bool> voiceLines;
        public static ConfigEntry<bool> rocketJump;
        public static ConfigEntry<bool> enableCharacter;
        public static ConfigEntry<float> armorGrowth;
        public static ConfigEntry<float> baseArmor;
        public static ConfigEntry<float> baseCrit;
        public static ConfigEntry<float> baseDamage;
        public static ConfigEntry<float> baseHealth;
        public static ConfigEntry<float> baseMovementSpeed;
        public static ConfigEntry<float> baseRegen;
        public static ConfigEntry<float> bonusHealthCoefficient;
        public static ConfigEntry<float> healthGrowth;
        public static ConfigEntry<int> jumpCount;



        public static ConfigEntry<float> damageGrowth;
        public static ConfigEntry<float> regenGrowth;

        public static ConfigEntry<float> fishBonesDamageCoefficient;
        public static ConfigEntry<float> fishBonesProcCoefficient;

        public static ConfigEntry<float> powPowDamageCoefficient;
        public static ConfigEntry<float> powPowProcCoefficient;

        public static ConfigEntry<float> zapDamageCoefficient;
        public static ConfigEntry<float> zapProcCoefficient;
        public static ConfigEntry<float> zapCD;
        public static ConfigEntry<float> megaRocketDamageCoefficient;
        public static ConfigEntry<float> megaRocketProcCoefficient;
        public static ConfigEntry<float> megaRocketCD;
        public static ConfigEntry<float> rocketJumpForce;
        public static ConfigEntry<float> rocketJumpRadius;

        public static void ReadConfig()
        {
            enableCharacter = JinxPlugin.instance.Config.Bind<bool>(new ConfigDefinition("00 - Other", "Enable Character"), true, new ConfigDescription("Enable Character", null, Array.Empty<object>()));
            voiceLines = JinxPlugin.instance.Config.Bind<bool>(new ConfigDefinition("00 - Other", "Voice Lines"), true, new ConfigDescription("Enable Voice Lines", null, Array.Empty<object>()));
            rocketJump = JinxPlugin.instance.Config.Bind<bool>(new ConfigDefinition("00 - Other", "Rocket Jumping"), true, new ConfigDescription("Enable Rocket Jumping", null, Array.Empty<object>()));
            rocketJumpForce = JinxPlugin.instance.Config.Bind<float>(new ConfigDefinition("00 - Other", "Rocket Jump Force"), 4000f, new ConfigDescription("Rocket Jump Force", null, Array.Empty<object>()));
            rocketJumpRadius = JinxPlugin.instance.Config.Bind<float>(new ConfigDefinition("00 - Other", "Rocket Jump Radius"), 8f, new ConfigDescription("Rocket Jump Radius", null, Array.Empty<object>()));

            baseHealth = JinxPlugin.instance.Config.Bind<float>(new ConfigDefinition("01 - Character Stats", "Base Health"), 110f, new ConfigDescription("", null, Array.Empty<object>()));
            healthGrowth = JinxPlugin.instance.Config.Bind<float>(new ConfigDefinition("01 - Character Stats", "Health Growth"), 30f, new ConfigDescription("", null, Array.Empty<object>()));

            baseRegen = JinxPlugin.instance.Config.Bind<float>(new ConfigDefinition("01 - Character Stats", "Base Health Regen"), 1f, new ConfigDescription("", null, Array.Empty<object>()));
            regenGrowth = JinxPlugin.instance.Config.Bind<float>(new ConfigDefinition("01 - Character Stats", "Health Regen Growth"), 0.2f, new ConfigDescription("", null, Array.Empty<object>()));

            baseArmor = JinxPlugin.instance.Config.Bind<float>(new ConfigDefinition("01 - Character Stats", "Base Armor"), 20f, new ConfigDescription("", null, Array.Empty<object>()));
            armorGrowth = JinxPlugin.instance.Config.Bind<float>(new ConfigDefinition("01 - Character Stats", "Armor Growth"), 0f, new ConfigDescription("", null, Array.Empty<object>()));

            baseDamage = JinxPlugin.instance.Config.Bind<float>(new ConfigDefinition("01 - Character Stats", "Base Damage"), 12f, new ConfigDescription("", null, Array.Empty<object>()));
            damageGrowth = JinxPlugin.instance.Config.Bind<float>(new ConfigDefinition("01 - Character Stats", "Damage Growth"), 2.4f, new ConfigDescription("", null, Array.Empty<object>()));

            baseMovementSpeed = JinxPlugin.instance.Config.Bind<float>(new ConfigDefinition("01 - Character Stats", "Base Movement Speed"), 7f, new ConfigDescription("", null, Array.Empty<object>()));

            baseCrit = JinxPlugin.instance.Config.Bind<float>(new ConfigDefinition("01 - Character Stats", "Base Crit"), 1f, new ConfigDescription("", null, Array.Empty<object>()));

            jumpCount = JinxPlugin.instance.Config.Bind<int>(new ConfigDefinition("01 - Character Stats", "Jump Count"), 1, new ConfigDescription("", null, Array.Empty<object>()));

            fishBonesDamageCoefficient = JinxPlugin.instance.Config.Bind<float>(new ConfigDefinition("02 - Fish Bones", "Fish Bones Damage Coefficient"), 6.5f, new ConfigDescription("", null, Array.Empty<object>()));
            fishBonesProcCoefficient = JinxPlugin.instance.Config.Bind<float>(new ConfigDefinition("02 - Fish Bones", "Fish Bones Proc Coefficient"), 1f, new ConfigDescription("", null, Array.Empty<object>()));

            powPowDamageCoefficient = JinxPlugin.instance.Config.Bind<float>(new ConfigDefinition("02 - Pow Pow", "Pow Pow Damage Coefficient"), 1.65f, new ConfigDescription("", null, Array.Empty<object>()));
            powPowProcCoefficient = JinxPlugin.instance.Config.Bind<float>(new ConfigDefinition("02 - Pow Pow", "Pow Pow Proc Coefficient"), 0.7f, new ConfigDescription("", null, Array.Empty<object>()));

            zapDamageCoefficient = JinxPlugin.instance.Config.Bind<float>(new ConfigDefinition("03 - Zap", "Zap Damage Coefficient"), 10f, new ConfigDescription("", null, Array.Empty<object>()));
            zapProcCoefficient = JinxPlugin.instance.Config.Bind<float>(new ConfigDefinition("03 - Zap", "Zap Proc Coefficient"), 1f, new ConfigDescription("", null, Array.Empty<object>()));
            zapCD = JinxPlugin.instance.Config.Bind<float>(new ConfigDefinition("03 - Zap", "Zap CD"), 6f, new ConfigDescription("", null, Array.Empty<object>()));

            megaRocketDamageCoefficient = JinxPlugin.instance.Config.Bind<float>(new ConfigDefinition("04 - Mega Rocket", "Mega Rocket Damage Coefficient"), 13.5f, new ConfigDescription("", null, Array.Empty<object>()));
            megaRocketProcCoefficient = JinxPlugin.instance.Config.Bind<float>(new ConfigDefinition("04 - Mega Rocket", "Mega Rocket Proc Coefficient"), 1f, new ConfigDescription("", null, Array.Empty<object>()));
            megaRocketCD = JinxPlugin.instance.Config.Bind<float>(new ConfigDefinition("04 - Mega Rocket", "Mega Rocket CD"), 10f, new ConfigDescription("", null, Array.Empty<object>()));


        }

        // this helper automatically makes config entries for disabling survivors
        public static ConfigEntry<bool> CharacterEnableConfig(string characterName, string description = "Set to false to disable this character", bool enabledDefault = true) {

            return JinxPlugin.instance.Config.Bind<bool>("General",
                                                          "Enable " + characterName,
                                                          enabledDefault,
                                                          description);
        }
    }
}