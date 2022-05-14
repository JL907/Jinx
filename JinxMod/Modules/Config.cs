using BepInEx.Configuration;
using System;
using UnityEngine;

namespace JinxMod.Modules
{
    public static class Config
    {
        public static ConfigEntry<bool> voiceLines;
        public static ConfigEntry<bool> enableCharacter;
        public static void ReadConfig()
        {
            enableCharacter = JinxPlugin.instance.Config.Bind<bool>(new ConfigDefinition("00 - Other", "Enable Character"), true, new ConfigDescription("Enable Voice Lines", null, Array.Empty<object>()));
            voiceLines = JinxPlugin.instance.Config.Bind<bool>(new ConfigDefinition("00 - Other", "Voice Lines"), true, new ConfigDescription("Enable Voice Lines", null, Array.Empty<object>()));
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