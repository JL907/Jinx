using System.Collections.Generic;

namespace JinxMod.Modules.LeagueSkins
{
    internal static class LeagueSkinCatalog
    {
        private static readonly string[] EmoteProps = { "Recall", "Emote" };

        private static LeagueSkinDefinition Skin(int id, string displayName, string defaultTexture, Dictionary<string, string> partTextures, params string[] hiddenParts)
        {
            string number = id.ToString("00");
            List<string> hidden = new List<string>(EmoteProps);
            hidden.AddRange(hiddenParts);
            return new LeagueSkinDefinition
            {
                folder = "Skin" + number,
                meshFile = "jinx_skin" + number + ".skn",
                skeletonFile = "jinx_skin" + number + ".skl",
                nameToken = JinxPlugin.DEVELOPER_PREFIX + "_JINX_SKIN" + number + "_NAME",
                displayName = displayName,
                iconTexture = "jinxloadscreen_" + id + ".tex",
                defaultTexture = defaultTexture,
                partTextures = partTextures ?? new Dictionary<string, string>(),
                hiddenParts = hidden.ToArray()
            };
        }

        private static LeagueSkinDefinition Retarget(LeagueSkinDefinition definition, params string[] jointPairs)
        {
            definition.retarget = true;
            for (int i = 0; i + 1 < jointPairs.Length; i += 2) definition.jointMap[jointPairs[i]] = jointPairs[i + 1];
            return definition;
        }

        private static LeagueSkinDefinition Mode(LeagueSkinDefinition definition, int mode, string joint, string bone, string skinAnchor, string baseAnchor, UnityEngine.Vector3 forward, UnityEngine.Vector3 up)
        {
            definition.modeBindings.Add(new LeagueModeBinding
            {
                mode = mode,
                joint = joint,
                bone = bone,
                skinAnchor = skinAnchor,
                baseAnchor = baseAnchor,
                rotation = UnityEngine.Quaternion.LookRotation(forward, up)
            });
            return definition;
        }

        private static LeagueSkinDefinition ModePart(LeagueSkinDefinition definition, int mode, params string[] parts)
        {
            foreach (string part in parts) definition.modeParts[part] = mode;
            return definition;
        }

        internal static readonly LeagueSkinDefinition[] definitions =
        {
            Skin(1, "Crime City", "jinx_skin01_tx_cm.tex", null,
                "Jinx_Skin01_Bottle_Mat", "Jinx_Skin01_Gramophone_Mat"),

            Skin(2, "Firecracker", "jinx_skin02_tx_cm.tex", null),

            Skin(3, "Zombie Slayer", "jinx_skin03_tx_cm.tex", null,
                "Zombie_Hand"),

            Skin(4, "Star Guardian", "jinx_skin04_tx_cm.tex", null,
                "Rocket_Ult", "Mirror", "Familiar_Dark", "Familiar_Light"),

            Skin(12, "Ambitious Elf", "jinx_skin12_tx_cm.tex", null,
                "Skin13_Recall_MAT"),

            Skin(13, "Odyssey", "jinx_skin13_tx_cm.tex", null),

            Skin(20, "PROJECT", "jinx_skin20_tx_cm.tex",
                new Dictionary<string, string>
                {
                    { "Hair", "jinx_skin20_hair_tx_cm.tex" }
                },
                "Drone_MAT"),

            Skin(29, "Heartseeker", "jinx_vday_body_tx_cm.tex",
                new Dictionary<string, string>
                {
                    { "weapon", "jinx_vday_weapon_tx_cm.tex" }
                },
                "chomper"),

            Skin(37, "Arcane Enemy", "jinx_skin37_tx_cm.tex", null,
                "Photo"),

            Skin(38, "Battle Cat", "jinx_skin38_main_tx_cm.tex",
                new Dictionary<string, string>
                {
                    { "Weapon", "jinx_skin38_weapon_tx_cm.tex" }
                },
                "JellySpikes", "JellyHead", "JellyBase"),

            Skin(40, "Prestige Battle Cat", "jinx_skin40_tx_cm.tex",
                new Dictionary<string, string>
                {
                    { "Weapon", "jinx_skin40_weapon_tx_cm.tex" }
                },
                "JellySpikes", "JellyHead", "JellyBase"),

            Skin(51, "Cafe Cuties", "jinx_skin51_main_tx_cm.tex",
                new Dictionary<string, string>
                {
                    { "Weapon", "jinx_skin51_weapon_tx_cm.tex" }
                }),

            ModePart(
            Mode(
            Mode(Retarget(Skin(60, "Arcane Fractured", "jinx_skin60_shimmerbody_tx_cm.tex",
                new Dictionary<string, string>
                {
                    { "Weapon", "jinx_skin60_weapon_bw_engine_tx_cm.tex" },
                    { "Zapper_Mesh", "jinx_skin60_weapon_bw_engine_tx_cm.tex" }
                },
                "Torch", "Helmet", "SlotMachine", "PowderBody", "Flag", "Hood", "Hood_Hair", "Body", "Hair"),
                "Weapon_Root", "Minigun"),
                LeagueRigDriver.ModeRocket, "Weapon_Root", "Rocket_Launcher", "Rocketlauncher_Grip", "Rocket_Launcher_Handle",
                new UnityEngine.Vector3(0f, -1f, 0f), new UnityEngine.Vector3(0f, 0f, -1f)),
                LeagueRigDriver.ModeZap, "Weapon_Root", "Pistol", "Rocketlauncher_Grip", "Pistol",
                new UnityEngine.Vector3(0.049f, 0.957f, 0.285f), new UnityEngine.Vector3(0.14f, 0.28f, -0.95f)),
                LeagueRigDriver.ModeZap, "Zapper_Mesh"),

            Skin(62, "T1", "jinx_skin62_tx_cm.tex",
                new Dictionary<string, string>
                {
                    { "Weapon", "jinx_skin62_weapon_tx_cm.tex" }
                },
                "Babywolf", "MIniGunVFX01", "MIniGunVFX02", "RocketVFX01", "RocketVFX02", "RocketVFX03"),

            Skin(65, "Ocean Song", "jinx_skin65_tx_cm.tex",
                new Dictionary<string, string>
                {
                    { "Weapon", "jinx_skin65_weapon_tx_cm.tex" },
                    { "Weapon03", "jinx_skin65_weapon_tx_cm.tex" },
                    { "Skirt", "jinx_skin65_skirt_tx_cm.tex" }
                },
                "WeaponVFX")
        };
    }
}
