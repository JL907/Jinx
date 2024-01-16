using EntityStates;
using EntityStates.Commando.CommandoWeapon;
using JinxMod.Controller;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace JinxMod.SkillStates
{
    public class FishBones : BaseSkillState
    {
        public static float damageCoefficient = Modules.Config.fishBonesDamageCoefficient.Value;
        public static float procCoefficient = Modules.Config.fishBonesProcCoefficient.Value;
        public static float baseDuration = 1.50f;
        public static float throwForce = 80f;

        private float duration;
        private float fireTime;
        private bool hasFired;
        private Animator animator;
        //private RevdUpController revdUpController;
        private RocketController rocketController;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = FishBones.baseDuration / this.attackSpeedStat;
            this.fireTime = 0.2f;
            if (this.duration < this.fireTime)
            {
                this.fireTime = this.duration * 0.2f;
            }
            base.StartAimMode(duration, false);
            this.animator = base.GetModelAnimator();
            //this.revdUpController = base.GetComponent<RevdUpController>();
            this.rocketController = base.GetComponent<RocketController>();
            if (this.rocketController)
            {
                this.rocketController.attacks++;
            }

            if (this.animator.GetBool("isMoving") || (!(this.animator.GetBool("isGrounded"))))
            {
                base.PlayAnimation("Gesture, Override", "fishbonesattack");
            }
            else if ((!(this.animator.GetBool("isMoving"))) && this.animator.GetBool("isGrounded"))
            {
                base.PlayAnimation("FullBody, Override", "fishbonesattack");
            }

            if (base.characterBody.GetBuffCount(Modules.Buffs.revdUp) > 0)
            {
                //this.revdUpController.ResetStopWatch();
                if (NetworkServer.active)
                {
                    base.characterBody.RemoveOldestTimedBuff(Modules.Buffs.revdUp);
                }
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
                Util.PlaySound("Play_JinxFishBonesShoot", base.gameObject);
                FireMissile();
            }
        }

        private void FireMissile()
        {
            base.AddRecoil(-1f * 3f, -2f * 3f, -0.5f * 3f, 0.5f * 3f);
            base.characterBody.AddSpreadBloom(1.5f);
            GameObject projectilePrefab = Modules.Projectiles.missilePrefab;
            bool isCrit = Util.CheckRoll(this.characterBody.crit, this.characterBody.master);
            if (FireRocket.effectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(FireRocket.effectPrefab, base.gameObject, "FishBonesMuzzle", false);
            }
            if (base.isAuthority)
            {
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
                InputBankTest component = base.characterBody.GetComponent<InputBankTest>();
                int num2 = num ?? 0;
                float num3 = Mathf.Max(1f, 1f + 0.5f * (float)(num2 - 1));
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    projectilePrefab = projectilePrefab,
                    position = component.aimOrigin,
                    rotation = Util.QuaternionSafeLookRotation(component.aimDirection),
                    procChainMask = default(ProcChainMask),
                    target = null,
                    owner = this.characterBody.gameObject,
                    damage = (this.characterBody.damage * FishBones.damageCoefficient) * num3,
                    crit = isCrit,
                    force = 600f,
                    damageColorIndex = DamageColorIndex.Default
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                Quaternion quaternion = Quaternion.LookRotation(component.aimDirection);
                if (num2 > 0)
                {
                    FireProjectileInfo fireProjectileInfo2 = fireProjectileInfo;
                    fireProjectileInfo2.rotation = quaternion * Quaternion.Euler(0f, -15, 0f);
                    fireProjectileInfo2.position = fireProjectileInfo.position + UnityEngine.Random.insideUnitSphere * 1f;
                    ProjectileManager.instance.FireProjectile(fireProjectileInfo2);
                    FireProjectileInfo fireProjectileInfo3 = fireProjectileInfo;
                    fireProjectileInfo3.rotation = quaternion * Quaternion.Euler(0f, 15, 0f) ;
                    fireProjectileInfo3.position = fireProjectileInfo.position + UnityEngine.Random.insideUnitSphere * 1f;
                    ProjectileManager.instance.FireProjectile(fireProjectileInfo3);
                }
            }
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