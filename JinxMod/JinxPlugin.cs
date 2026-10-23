using BepInEx;
using JinxMod.Controller;
using JinxMod.Modules.Survivors;
using JinxMod.SkillStates;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using R2API.Networking;
using UnityEngine;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace JinxMod
{
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.prefab", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.language", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.sound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.networking", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.recalculatestats", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.damagetype", BepInDependency.DependencyFlags.HardDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(MODUID, MODNAME, MODVERSION)]

    public class JinxPlugin : BaseUnityPlugin
    {
        public const string MODUID = "com.Lemonlust.JinxMod";
        public const string MODNAME = "JinxMod";
        public const string MODVERSION = "1.5.0";

        public const string DEVELOPER_PREFIX = "Lemonlust";

        public static JinxPlugin instance;
        public static DamageAPI.ModdedDamageType jinxDamage;

        private void Awake()
        {
            instance = this;
            try
            {
                jinxDamage = DamageAPI.ReserveDamageType();
                Log.Init(Logger);
                Modules.Assets.Initialize();
                Modules.Config.ReadConfig();
                Modules.States.RegisterStates();
                Modules.Buffs.RegisterBuffs();
                Modules.Projectiles.RegisterProjectiles();
                Modules.Tokens.AddTokens();
                NetworkingAPI.RegisterMessageType<RefreshStacksMessage>();
                NetworkingAPI.RegisterMessageType<SyncBuffTimersMessage>();

                new MyCharacter().Initialize();

                new Modules.ContentPacks().Initialize();

                Hook();
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message + " - " + e.StackTrace);
            }
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]


        private void Hook()
        {
            RoR2.GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            R2API.RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            JinxMod.Controllers.RocketJumpFallProtection.Hook();

        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(Modules.Buffs.revdUp))
            {
                args.attackSpeedMultAdd += ((sender.GetBuffCount(Modules.Buffs.revdUp) * 0.18f));
            }

            if (sender.HasBuff(Modules.Buffs.getExcitedSpeedBuff))
            {
                args.moveSpeedMultAdd += sender.GetBuffCount(Modules.Buffs.getExcitedSpeedBuff) * 0.125f;
                args.attackSpeedMultAdd += .25f;
            }
        }

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport damageReport)
        {
            if (damageReport is null) return;
            if (damageReport.victimBody is null) return;
            if (damageReport.attackerBody is null) return;

            if (damageReport.victimTeamIndex != TeamIndex.Player && damageReport.attackerBody.bodyIndex == BodyCatalog.FindBodyIndex("JinxBody") && (damageReport.victimIsChampion || damageReport.victimIsBoss || damageReport.victimIsElite))
            {
                damageReport.attackerBody.RefreshStacks(Modules.Buffs.getExcitedSpeedBuff.buffIndex, 1, 6, true);
            }
        }
    }
}