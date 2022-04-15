﻿using RoR2;
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
        public Rigidbody rigidbody;
        public ProjectileImpactEventCaller projectileImpactEventCaller;
        private RocketJumpController.OwnerInfo owner;
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
            this.rigidbody = base.GetComponent<Rigidbody>();
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
            Collider[] objectsInRange = Physics.OverlapSphere(projectileImpactInfo.estimatedPointOfImpact, 25f);

            foreach (Collider collision in objectsInRange)
            {
                CharacterBody characterBody = collision.GetComponent<CharacterBody>();
                if (this.owner.characterBody == characterBody && characterBody && characterBody?.bodyIndex == BodyCatalog.FindBodyIndex("JinxBody"))
                {
                    this.owner.characterMotor.onHitGroundServer += CharacterMotor_onHitGroundServer;
                    this.owner.characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
                    AddExplosionForce(characterBody, 4000f, projectileImpactInfo.estimatedPointOfImpact, 25f, 0f);
                }
            }
        }

        private void CharacterMotor_onHitGroundServer(ref CharacterMotor.HitGroundInfo hitGroundInfo)
        {
            this.owner.characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;
        }

        private void AddExplosionForce(CharacterBody characterBody, float explosionForce, Vector3 explosionPosition, float explosionRadius, float upliftModifier = 0)
        {
            var dir = (characterBody.corePosition - explosionPosition);
            float wearoff = 1 - (dir.magnitude / explosionRadius);
            Vector3 baseForce = dir.normalized * explosionForce * wearoff;
            characterBody.characterMotor.ApplyForce(baseForce);

            if (upliftModifier != 0)
            {
                float upliftWearoff = 1 - upliftModifier / explosionRadius;
                Vector3 upliftForce = Vector3.up * explosionForce * upliftWearoff;
                characterBody.characterMotor.ApplyForce(upliftForce);
            }
        }
    }
}
