using RoR2;
using RoR2.Audio;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace JinxMod.Controllers
{
    [RequireComponent(typeof(ProjectileController))]
    public class RocketJumpController : MonoBehaviour
    {
        private ProjectileController projectileController;
        private ProjectileImpactExplosion projectileImpactExplosion;
        public Rigidbody rigidbody;
        public ProjectileImpactEventCaller projectileImpactEventCaller;
        private RocketJumpController.OwnerInfo owner;
        public float explosionForce = 4000f;
        public float explosionRadius = Modules.Config.rocketJumpRadius.Value;
        private struct OwnerInfo
        {
            public OwnerInfo(GameObject ownerGameObject)
            {
                this = default(RocketJumpController.OwnerInfo);
                this.gameObject = ownerGameObject;
                if (this.gameObject)
                {
                    this.characterBody = this.gameObject.GetComponent<CharacterBody>();
                    this.characterMotor = this.gameObject.GetComponent<CharacterMotor>();
                    this.rigidbody = this.gameObject.GetComponent<Rigidbody>();
                    this.hasEffectiveAuthority = Util.HasEffectiveAuthority(this.gameObject);
                }
            }
            public readonly GameObject gameObject;
            public readonly CharacterBody characterBody;
            public readonly CharacterMotor characterMotor;
            public readonly Rigidbody rigidbody;
            public readonly bool hasEffectiveAuthority;
        }
        private void Start()
        {
            this.owner = new RocketJumpController.OwnerInfo(this.projectileController.owner);
        }

        private void Awake()
        {
            this.projectileController = base.GetComponent<ProjectileController>();
            this.projectileImpactExplosion = base.GetComponent<ProjectileImpactExplosion>();
            this.rigidbody = base.GetComponent<Rigidbody>();
            this.explosionRadius = this.projectileImpactExplosion.blastRadius;
            if (NetworkServer.active)
            {
                this.projectileImpactEventCaller = GetComponent<ProjectileImpactEventCaller>();
                if (projectileImpactEventCaller)
                {
                    projectileImpactEventCaller.impactEvent.AddListener(new UnityAction<ProjectileImpactInfo>(this.OnImpact));
                }
            }
        }

        private void OnDestroy()
        {
            if (projectileImpactEventCaller)
            {
                projectileImpactEventCaller.impactEvent.RemoveListener(new UnityAction<ProjectileImpactInfo>(this.OnImpact));
            }
        }
        private void OnImpact(ProjectileImpactInfo projectileImpactInfo)
        {
            if (projectileImpactInfo.collider.transform.gameObject.layer != LayerIndex.world.intVal)
            {
                return;
            }

            Collider[] objectsInRange = Physics.OverlapSphere(projectileImpactInfo.estimatedPointOfImpact, explosionRadius);

            foreach (Collider collision in objectsInRange)
            {
                CharacterBody characterBody = collision.GetComponent<CharacterBody>();
                if (this.owner.characterBody == characterBody && characterBody && characterBody?.bodyIndex == BodyCatalog.FindBodyIndex("JinxBody"))
                {
                    this.owner.characterMotor.onHitGround += CharacterMotor_onHitGround;
                    this.owner.characterMotor.Motor.ForceUnground();
                    this.owner.characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
                    AddExplosionForce(characterBody, explosionForce, projectileImpactInfo.estimatedPointOfImpact, explosionRadius, 0f);
                }
            }
        }

        private void CharacterMotor_onHitGround(ref CharacterMotor.HitGroundInfo hitGroundInfo)
        {
            this.owner.characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;
        }

        private void AddExplosionForce(CharacterBody characterBody, float explosionForce, Vector3 explosionPosition, float explosionRadius, float upliftModifier = 0)
        {
            var dir = (characterBody.corePosition - explosionPosition);
            float wearoff = 1 - (dir.magnitude / explosionRadius);
            Vector3 baseForce = dir.normalized * explosionForce * wearoff;
            if (Modules.Config.rocketJump.Value) characterBody.characterMotor.ApplyForce(baseForce);

            if (upliftModifier != 0)
            {
                float upliftWearoff = 1 - upliftModifier / explosionRadius;
                Vector3 upliftForce = Vector3.up * explosionForce * upliftWearoff;
                if (Modules.Config.rocketJump.Value) characterBody.characterMotor.ApplyForce(upliftForce);
            }
        }
    }
}
