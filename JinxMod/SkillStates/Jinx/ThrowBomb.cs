﻿using EntityStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace JinxMod.SkillStates
{
    public class ThrowBomb : BaseSkillState
    {
        public static float damageCoefficient = 16f;
        public static float procCoefficient = 1f;
        public static float baseDuration = 0.65f;
        public static float throwForce = 80f;

        private float duration;
        private float fireTime;
        private bool hasFired;
        private Animator animator;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = ThrowBomb.baseDuration / this.attackSpeedStat;
            this.fireTime = 0.35f * this.duration;
            base.characterBody.SetAimTimer(2f);
            this.animator = base.GetModelAnimator();

            if (this.animator.GetBool("isMoving") || (!(this.animator.GetBool("isGrounded"))))
            {
                base.PlayAnimation("Gesture, Override", "fishbonesattack");
            }
            else if ((!(this.animator.GetBool("isMoving"))) && this.animator.GetBool("isGrounded"))
            {
                base.PlayAnimation("FullBody, Override", "fishbonesattack");
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
            GameObject projectilePrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/MissileProjectile");
            float num = 3f;
            bool isCrit = Util.CheckRoll(this.characterBody.crit, this.characterBody.master);
            MissileUtils.FireMissile(this.characterBody.corePosition, this.characterBody, default(ProcChainMask), null, this.characterBody.damage * num, isCrit, projectilePrefab, DamageColorIndex.Item, aimRay.direction, 200f, true);
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