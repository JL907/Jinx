﻿using EntityStates;
using EntityStates.Captain.Weapon;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace JinxMod.SkillStates
{
    public class Zap : BaseSkillState
    {
        public static float damageCoefficient = 12f;
        public static float procCoefficient = 1f;
        public static float baseDuration = 1.18f;
        public static float throwForce = 80f;

        private float duration;
        private float fireTime;
        private bool hasFired;
        private Animator animator;
        private GameObject projectilePrefab = Modules.Projectiles.zapPrefab;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = FishBones.baseDuration;
            this.fireTime = 0.5f;
            base.characterBody.SetAimTimer(2f);
            this.animator = base.GetModelAnimator();

            if (this.animator.GetBool("isMoving") || (!(this.animator.GetBool("isGrounded"))))
            {
                base.PlayAnimation("Gesture, Override", "zap");
            }
            else if ((!(this.animator.GetBool("isMoving"))) && this.animator.GetBool("isGrounded"))
            {
                base.PlayAnimation("FullBody, Override", "zap");
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        private void Fire()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;
                Util.PlaySound("JinxBombThrow", base.gameObject);

                if (base.isAuthority)
                {
                    FireMissile();
                }
            }
        }

        private void FireMissile()
        {
            Ray aimRay = base.GetAimRay();
            if (this.projectilePrefab != null)
            {
                bool isCrit = Util.CheckRoll(this.characterBody.crit, this.characterBody.master);
                base.characterBody.AddSpreadBloom(1.5f);
                base.AddRecoil(-1f * FireTazer.recoilAmplitude, -1.5f * FireTazer.recoilAmplitude, -0.25f * FireTazer.recoilAmplitude, 0.25f * FireTazer.recoilAmplitude);
                if (FireTazer.muzzleflashEffectPrefab)
                {
                    EffectManager.SimpleMuzzleFlash(FireTazer.muzzleflashEffectPrefab, base.gameObject, "Muzzle", false);
                }
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    projectilePrefab = this.projectilePrefab,
                    position = aimRay.origin,
                    rotation = Util.QuaternionSafeLookRotation(aimRay.direction),
                    procChainMask = default(ProcChainMask),
                    target = null,
                    owner = this.characterBody.gameObject,
                    damage = this.characterBody.damage * Zap.damageCoefficient,
                    crit = isCrit,
                    force = 200f,
                    damageColorIndex = DamageColorIndex.Default
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
            
            //MissileUtils.FireMissile(aimRay.origin, this.characterBody, default(ProcChainMask), null, this.characterBody.damage * num, isCrit, projectilePrefab, DamageColorIndex.Default, aimRay.direction, 200f, true);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= this.fireTime)
            {
                this.Fire();
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