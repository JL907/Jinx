using BepInEx.Configuration;
using EntityStates;
using JinxMod.Controller;
using JinxMod.Modules.Characters;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace JinxMod.Modules.Survivors
{
    internal class MyCharacter : SurvivorBase
    {
        public override string bodyName => "Jinx";

        public const string JINX_PREFIX = JinxPlugin.DEVELOPER_PREFIX + "_JINX_BODY_";
        //used when registering your survivor's language tokens
        public override string survivorTokenPrefix => JINX_PREFIX;

        public static SkillDef fishBonesSkillDef;

        public override BodyInfo bodyInfo { get; set; } = new BodyInfo
        {
            bodyName = "JinxBody",
            bodyNameToken = JinxPlugin.DEVELOPER_PREFIX + "_JINX_BODY_NAME",
            subtitleNameToken = JinxPlugin.DEVELOPER_PREFIX + "_JINX_BODY_SUBTITLE",

            characterPortrait = Assets.mainAssetBundle.LoadAsset<Texture>("JinxSquareTex"),
            bodyColor = Color.cyan,

            crosshair = Modules.Assets.LoadCrosshair("Standard"),
            podPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod"),

            maxHealth = 110f,
            healthRegen = 1f,
            armor = 0f,

            damage = 12f,

            jumpCount = 1,

            sortPosition = 1f,
        };

        public override CustomRendererInfo[] customRendererInfos { get; set; } = new CustomRendererInfo[] 
        {
                new CustomRendererInfo
                {
                    childName = "Model",
                    dontHotpoo = true
                }
        };

        public override UnlockableDef characterUnlockableDef => null;

        public override Type characterMainState => typeof(EntityStates.GenericCharacterMain);

        //public override ItemDisplaysBase itemDisplays => new JinxItemDisplays();

                                                                          //if you have more than one character, easily create a config to enable/disable them like this
        public override ConfigEntry<bool> characterEnabledConfig => null; //Modules.Config.CharacterEnableConfig(bodyName);

        private static UnlockableDef masterySkinUnlockableDef;

        public override void InitializeCharacter()
        {
            base.InitializeCharacter();
            bodyPrefab.AddComponent<RocketController>();
            displayPrefab.AddComponent<MenuController>();
            CharacterDeathBehavior characterDeathBehavior = bodyPrefab.GetComponent<CharacterDeathBehavior>();
            characterDeathBehavior.deathState = new SerializableEntityStateType(typeof(JinxMod.SkillStates.BaseStates.DeathState));
        }

        public override void InitializeUnlockables()
        {
            //uncomment this when you have a mastery skin. when you do, make sure you have an icon too
            //masterySkinUnlockableDef = Modules.Unlockables.AddUnlockable<Modules.Achievements.MasteryAchievement>();
        }

        public override void InitializeHitboxes()
        {
            ChildLocator childLocator = bodyPrefab.GetComponentInChildren<ChildLocator>();
            GameObject model = childLocator.gameObject;

            //example of how to create a hitbox
            //Transform hitboxTransform = childLocator.FindChild("SwordHitbox");
            //Modules.Prefabs.SetupHitbox(model, hitboxTransform, "Sword");
        }

        public override void InitializeSkills()
        {

            Modules.Skills.CreateSkillFamilies(bodyPrefab);
            string prefix = JinxPlugin.DEVELOPER_PREFIX;

            #region Primary
            //Creates a skilldef for a typical primary 
            SkillDef primarySkillDef = Modules.Skills.CreateSkillDef(new SkillDefInfo
                (prefix + "_JINX_BODY_PRIMARY_POWPOW_NAME",
                prefix + "_JINX_BODY_PRIMARY_POWPOW_DESCRIPTION",
                Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("Pow-Pow"),
                new EntityStates.SerializableEntityStateType(typeof(SkillStates.PowPow)),
                "Weapon",
                false));
            ((ScriptableObject)primarySkillDef).name = "JinxPowPow";
            SkillDef fish = Modules.Skills.CreateSkillDef(new SkillDefInfo
                (prefix + "_JINX_BODY_PRIMARY_FISHBONES_NAME",
                prefix + "_JINX_BODY_PRIMARY_FISHBONES_DESCRIPTION",
                Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("JinxQ"),
                new EntityStates.SerializableEntityStateType(typeof(SkillStates.FishBones)),
                "Weapon",
                true));
            ((ScriptableObject)fish).name = "JinxFishBones";
            fishBonesSkillDef = fish;

            Modules.Skills.AddPrimarySkills(bodyPrefab, primarySkillDef);
            #endregion

            #region Secondary
            SkillDef shootSkillDef = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = prefix + "_JINX_BODY_SECONDARY_GUN_NAME",
                skillNameToken = prefix + "_JINX_BODY_SECONDARY_GUN_NAME",
                skillDescriptionToken = prefix + "_JINX_BODY_SECONDARY_GUN_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("JinxW"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.Zap)),
                activationStateMachineName = "Slide",
                baseMaxStock = 1,
                baseRechargeInterval = 6f,
                beginSkillCooldownOnSkillEnd = true,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.Skill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = true,
                mustKeyPress = true,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,
                keywordTokens = new string[] { "KEYWORD_SHOCKING" }
            });
            ((ScriptableObject)shootSkillDef).name = "JinxZap";
            Modules.Skills.AddSecondarySkills(bodyPrefab, shootSkillDef);
            #endregion

            #region Utility
            SkillDef rollSkillDef = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = prefix + "_JINX_BODY_UTILITY_ROLL_NAME",
                skillNameToken = prefix + "_JINX_BODY_UTILITY_ROLL_NAME",
                skillDescriptionToken = prefix + "_JINX_BODY_UTILITY_ROLL_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("Switch"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.Switcharoo)),
                activationStateMachineName = "Weapon",
                baseMaxStock = 1,
                baseRechargeInterval = 1f,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = true,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = false,
                mustKeyPress = true,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,
                keywordTokens = new string[] { "KEYWORD_AGILE" }
            });
            rollSkillDef.dontAllowPastMaxStocks = true;
            Modules.Skills.AddUtilitySkills(bodyPrefab, rollSkillDef);
            #endregion

            #region Special
            SkillDef bombSkillDef = Modules.Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = prefix + "_JINX_BODY_SPECIAL_BOMB_NAME",
                skillNameToken = prefix + "_JINX_BODY_SPECIAL_BOMB_NAME",
                skillDescriptionToken = prefix + "_JINX_BODY_SPECIAL_BOMB_DESCRIPTION",
                skillIcon = Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("JinxR"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(SkillStates.MegaRocket)),
                activationStateMachineName = "Body",
                baseMaxStock = 1,
                baseRechargeInterval = 10f,
                beginSkillCooldownOnSkillEnd = true,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = true,
                mustKeyPress = true,
                cancelSprintingOnActivation = true,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1
            });
            ((ScriptableObject)bombSkillDef).name = "JinxMegaRocket";
            Modules.Skills.AddSpecialSkills(bodyPrefab, bombSkillDef);
            #endregion
        }

        public override void InitializeSkins()
        {
            GameObject model = bodyPrefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            CharacterModel characterModel = model.GetComponent<CharacterModel>();

            ModelSkinController skinController = model.AddComponent<ModelSkinController>();
            ChildLocator childLocator = model.GetComponent<ChildLocator>();

            SkinnedMeshRenderer mainRenderer = characterModel.mainSkinnedMeshRenderer;

            CharacterModel.RendererInfo[] defaultRenderers = characterModel.baseRendererInfos;

            List<SkinDef> skins = new List<SkinDef>();

            #region DefaultSkin
            SkinDef defaultSkin = Modules.Skins.CreateSkinDef(JinxPlugin.DEVELOPER_PREFIX + "_JINX_BODY_DEFAULT_SKIN_NAME",
                Assets.mainAssetBundle.LoadAsset<Sprite>("JinxSquare"),
                defaultRenderers,
                mainRenderer,
                model);

            defaultSkin.meshReplacements = new SkinDef.MeshReplacement[]
            {
                //place your mesh replacements here
                //unnecessary if you don't have multiple skins
                //new SkinDef.MeshReplacement
                //{
                //    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("meshJinxSword"),
                //    renderer = defaultRenderers[0].renderer
                //},
                //new SkinDef.MeshReplacement
                //{
                //    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("meshJinxGun"),
                //    renderer = defaultRenderers[1].renderer
                //},
                //new SkinDef.MeshReplacement
                //{
                //    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("meshJinx"),
                //    renderer = defaultRenderers[2].renderer
                //}
            };

            skins.Add(defaultSkin);
            #endregion

            //uncomment this when you have a mastery skin
            #region MasterySkin
            /*
            Material masteryMat = Modules.Materials.CreateHopooMaterial("matJinxAlt");
            CharacterModel.RendererInfo[] masteryRendererInfos = SkinRendererInfos(defaultRenderers, new Material[]
            {
                masteryMat,
                masteryMat,
                masteryMat,
                masteryMat
            });

            SkinDef masterySkin = Modules.Skins.CreateSkinDef(JinxPlugin.DEVELOPER_PREFIX + "_JINX_BODY_MASTERY_SKIN_NAME",
                Assets.mainAssetBundle.LoadAsset<Sprite>("texMasteryAchievement"),
                masteryRendererInfos,
                mainRenderer,
                model,
                masterySkinUnlockableDef);

            masterySkin.meshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("meshJinxSwordAlt"),
                    renderer = defaultRenderers[0].renderer
                },
                new SkinDef.MeshReplacement
                {
                    mesh = Modules.Assets.mainAssetBundle.LoadAsset<Mesh>("meshJinxAlt"),
                    renderer = defaultRenderers[2].renderer
                }
            };

            skins.Add(masterySkin);
            */
            #endregion

            skinController.skins = skins.ToArray();
        }
    }
}