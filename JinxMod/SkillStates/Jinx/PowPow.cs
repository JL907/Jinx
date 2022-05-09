using EntityStates;
using JinxMod.Controller;
using RoR2;
using RoR2.Audio;
using System;
using R2API.Networking;
using R2API.Networking.Interfaces;
using UnityEngine;
using UnityEngine.Networking;
using static RoR2.BulletAttack;

namespace JinxMod.SkillStates
{
    public class PowPow : BaseSkillState
    {
        public static float damageCoefficient = 1.65f;
        public static float procCoefficient = .7f;
        public static float baseDuration = 0.8f;
        public static float force = 400f;
        public static float recoil = 0.5f;
        public static float range = 256f;
        public static GameObject tracerEffectPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerGoldGat");

        private float duration;
        private float fireTime;
        private bool hasFired;
        private string muzzleString;
        public Animator animator { get; private set; }

        private float bulletStopWatch;
        private int bulletCount = 3;
        private RocketController rocketController;
        private bool hasHit = false;
        private bool hasBuff = false;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = PowPow.baseDuration / this.attackSpeedStat;
            this.fireTime = 0.1f * this.duration;
            base.StartAimMode(duration, false);
            this.muzzleString = "PowPowMuzzle";
            this.animator = base.GetModelAnimator();

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
            base.AddRecoil(-1f * PowPow.recoil, -2f * PowPow.recoil, -0.5f * PowPow.recoil, 0.5f * PowPow.recoil);
            if (base.isAuthority)
            {
                new BulletAttack
                {
                    bulletCount = 1,
                    aimVector = base.inputBank.aimDirection,
                    origin = base.inputBank.aimOrigin,
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
                    hitCallback = BulletHitCallback,
                }.Fire();
            }
        }

        private bool BulletHitCallback(BulletAttack bulletAttack, ref BulletHit hitInfo)
        {
            var result = BulletAttack.defaultHitCallback(bulletAttack, ref hitInfo);
            HealthComponent healthComponent = hitInfo.hitHurtBox ? hitInfo.hitHurtBox.healthComponent : null;
            if (healthComponent && healthComponent.alive && hitInfo.hitHurtBox.teamIndex != base.teamComponent.teamIndex)
            {
                this.hasHit = true;
                PointSoundManager.EmitSoundLocal(NetworkSoundEventCatalog.GetAkIdFromNetworkSoundEventIndex(Modules.Assets.bulletHitSoundEvent.index), hitInfo.hitHurtBox.transform.position);
            }
            return result;
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

            if (this.hasHit && !this.hasBuff)
            {
                this.hasBuff = true;
                new RefreshStacksMessage(characterBody, Modules.Buffs.revdUp.buffIndex, 2.5f, 3).Send(NetworkDestination.Server);
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