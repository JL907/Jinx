﻿using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace JinxMod.Modules
{
    public static class Buffs
    {

        internal static BuffDef revdUp;
        internal static BuffDef getExcitedSpeedBuff;

        internal static void RegisterBuffs()
        {
            revdUp = AddNewBuff("JinxRevdUp", Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("Rev_d_up"), Color.white, true, false);
            getExcitedSpeedBuff = AddNewBuff("GetExcitedMovementSpeedBuff", Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("Jinx_Passive"), Color.white, true, false);

        }

        // simple helper method
        internal static BuffDef AddNewBuff(string buffName, Sprite buffIcon, Color buffColor, bool canStack, bool isDebuff)
        {
            BuffDef buffDef = ScriptableObject.CreateInstance<BuffDef>();
            buffDef.name = buffName;
            buffDef.buffColor = buffColor;
            buffDef.canStack = canStack;
            buffDef.isDebuff = isDebuff;
            buffDef.eliteDef = null;
            buffDef.iconSprite = buffIcon;

            Modules.Content.AddBuffDef(buffDef);

            return buffDef;
        }
    }
}