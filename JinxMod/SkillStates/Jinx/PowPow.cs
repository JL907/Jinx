﻿using EntityStates;
using JinxMod.Controller;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace JinxMod.SkillStates
{
    public class PowPow : BaseSkillState
    {
        public static float damageCoefficient = 2f;
        public static float procCoefficient = 1f;
        public static float baseDuration = 1.0f;
        public static float force = 400f;
        public static float recoil = 1f;
        public static float range = 256f;
        public static GameObject tracerEffectPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerGoldGat");

        private float duration;
        private float fireTime;
        private bool hasFired;
        private string muzzleString;

        private RevdUpController revdUpController;
        public Animator animator { get; private set; }

        private float bulletStopWatch;
        private int bulletCount = 3;
        private RocketController rocketController;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = PowPow.baseDuration / this.attackSpeedStat;
            this.fireTime = 0.2f * this.duration;
            base.characterBody.SetAimTimer(2f);
            this.muzzleString = "PowPowMuzzle";
            this.animator = base.GetModelAnimator();
            this.revdUpController = base.GetComponent<RevdUpController>();
            if (NetworkServer.active)
            {
                this.revdUpController.AddStack();
            }

            if (this.animator.GetBool("isMoving") || (!(this.animator.GetBool("isGrounded"))))
            {
                base.PlayAnimation("Gesture, Override", "powpowattack");
            }
            else if ((!(this.animator.GetBool("isMoving"))) && this.animator.GetBool("isGrounded"))
            {
                base.PlayAnimation("FullBody, Override", "powpowattack");
            }
            Util.PlaySound("Play_JinxPowPowShoot", base.gameObject);
            this.rocketController = base.GetComponent<RocketController>();
            if (this.rocketController)
            {
                this.rocketController.attacks++;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        private void Fire()
        {
            base.characterBody.AddSpreadBloom(1.5f);
            EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, base.gameObject, this.muzzleString, false);
            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();
                base.AddRecoil(-1f * PowPow.recoil, -2f * PowPow.recoil, -0.5f * PowPow.recoil, 0.5f * PowPow.recoil);

                new BulletAttack
                {
                    bulletCount = 1,
                    aimVector = aimRay.direction,
                    origin = aimRay.origin,
                    damage = PowPow.damageCoefficient * this.damageStat,
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = DamageType.Generic,
                    falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                    maxDistance = PowPow.range,
                    force = PowPow.force,
                    hitMask = LayerIndex.CommonMasks.bullet,
                    minSpread = 0f,
                    maxSpread = base.characterBody.spreadBloomAngle,
                    isCrit = base.RollCrit(),
                    owner = base.gameObject,
                    muzzleName = muzzleString,
                    smartCollision = true,
                    procChainMask = default(ProcChainMask),
                    procCoefficient = procCoefficient,
                    radius = 0.75f,
                    sniper = false,
                    stopperMask = LayerIndex.CommonMasks.bullet,
                    weapon = null,
                    tracerEffectPrefab = PowPow.tracerEffectPrefab,
                    spreadPitchScale = 0f,
                    spreadYawScale = 0f,
                    queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                    hitEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol2.hitEffectPrefab,
                }.Fire();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (bulletStopWatch < this.fireTime / 3f)
            {
                bulletStopWatch += Time.fixedDeltaTime;
            }
            if (base.fixedAge >= this.fireTime && bulletStopWatch > this.fireTime / 3f && bulletCount > 0)
            {
                bulletStopWatch = 0f;
                bulletCount--;
                if (!this.hasFired) this.Fire();
                if (bulletCount <= 0)
                {
                    this.hasFired = true;
                }
            }

            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}