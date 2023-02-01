using EntityStates;
using EntityStates.Commando.CommandoWeapon;
using JinxMod.Controller;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace JinxMod.SkillStates
{
    public class MegaRocket : BaseSkillState
    {
        public static float damageCoefficient = 13.5f;
        public static float procCoefficient = 1f;
        public static float baseDuration = 1.0f;
        public static float throwForce = 80f;

        private float duration;
        private float fireTime;
        private bool hasFired;
        private Animator animator;
        private RocketController rocketController;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = MegaRocket.baseDuration;
            this.fireTime = 0.53f;
            base.StartAimMode(duration, false);
            this.animator = base.GetModelAnimator();
            this.rocketController = base.GetComponent<RocketController>();
            if (this.rocketController)
            {
                this.rocketController.attacks++;
            }
            base.PlayAnimation("FullBody, Override", "megarocket");
            Util.PlaySound("Play_JinxMegaRocketShootInitial", base.gameObject);
            Vector3 effectPosition = base.characterBody.corePosition + Vector3.up * 1f;
            EffectManager.SpawnEffect(Modules.Assets.chargeEffect, new EffectData
            {
                origin = effectPosition,
                scale = 1f,
            }, false) ;
            if (Modules.Config.voiceLines.Value) Util.PlaySound("Play_JinxMegaRocketVO", base.gameObject);
        }

        public override void OnExit()
        {
            base.PlayAnimation("FullBody, Override", "BufferEmpty");
            base.OnExit();
        }

        private void Fire()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;
                Util.PlaySound("Play_JinxMegaRocketShoot", base.gameObject);

                if (base.isAuthority)
                {
                    FireMissile();
                }
            }
        }

        private void FireMissile()
        {
            Ray aimRay = base.GetAimRay();
            base.AddRecoil(-1f * 6f, -2f * 6f, -0.5f * 6f, 0.5f * 6f);
            base.characterBody.AddSpreadBloom(1.5f);
            GameObject projectilePrefab = Modules.Projectiles.megaRocketPrefab;
            bool isCrit = Util.CheckRoll(this.characterBody.crit, this.characterBody.master);
            if (FireRocket.effectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(FireRocket.effectPrefab, base.gameObject, "FishBonesMuzzle", false);
            }

            int? num;
            if (base.characterBody == null)
            {
                num = null;
            }
            else
            {
                Inventory inventory = base.characterBody.inventory;
                num = ((inventory != null) ? new int?(inventory.GetItemCount(DLC1Content.Items.MoreMissile)) : null);
            }

            int num2 = num ?? 0;
            float num3 = Mathf.Max(1f, 1f + 0.5f * (float)(num2 - 1));

            FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
            {
                projectilePrefab = projectilePrefab,
                position = aimRay.origin,
                rotation = Util.QuaternionSafeLookRotation(aimRay.direction),
                procChainMask = default(ProcChainMask),
                target = null,
                owner = this.characterBody.gameObject,
                damage = (this.characterBody.damage * MegaRocket.damageCoefficient) * num3,
                crit = isCrit,
                force = 1200f,
                damageColorIndex = DamageColorIndex.Default
            };
            ProjectileManager.instance.FireProjectile(fireProjectileInfo);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            Ray aimRay = base.GetAimRay();
            if (base.characterDirection && aimRay.direction != Vector3.zero)
            {
                base.characterDirection.moveVector = aimRay.direction;
            }

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