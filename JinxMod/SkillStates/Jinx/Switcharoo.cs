﻿
using EntityStates;
using JinxMod.Controller;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace JinxMod.SkillStates
{
    public class Switcharoo : GenericCharacterMain
    {
        public static float baseDuration = 0.266f;
        private Animator animator;
        private RocketController rocketController;
        private AnimatorOverrideController animatorOverrideController;
        private float stopwatch;

        public override void OnEnter()
        {
            base.OnEnter();
            this.animator = GetModelAnimator();
            this.rocketController = base.GetComponent<RocketController>();
            if (!rocketController.isRocket)
            {
                this.rocketController.isRocket = true;
                base.PlayAnimation("Gesture, Override", "equip_rocket");
                this.animatorOverrideController = Modules.Assets.mainAssetBundle.LoadAsset<AnimatorOverrideController>("RocketOverrideController");
                animator.runtimeAnimatorController = this.animatorOverrideController;
            }
            else
            {
                this.rocketController.isRocket = false;
                base.PlayAnimation("Gesture, Override", "equip_gat");
                this.animatorOverrideController = Modules.Assets.mainAssetBundle.LoadAsset<AnimatorOverrideController>("GatOverrideController");
                animator.runtimeAnimatorController = this.animatorOverrideController;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch += Time.fixedDeltaTime;
            if (base.isAuthority && stopwatch >= baseDuration)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.PlayAnimation("Gesture, Override", "BufferEmpty");
            base.OnExit();
        }
    }
}
