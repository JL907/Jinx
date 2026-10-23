using RoR2;
using RoR2.Projectile;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace JinxMod.Controllers
{
    [RequireComponent(typeof(ProjectileController))]
    public class JinxMegaImpact : ProjectileExplosion, IProjectileImpactBehavior
    {
        private float age;
        public static float bonusDamageCoefficient = 0.25f;
        public override void Awake()
        {
            base.Awake();
        }

        public void FixedUpdate()
        {
            age += Time.fixedDeltaTime;
        }
        public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
        {
            List<HurtBox> HurtBoxes = new SphereSearch
            {
                radius = 16f,
                mask = LayerIndex.entityPrecise.mask,
                origin = impactInfo.estimatedPointOfImpact
            }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(this.projectileController.teamFilter.teamIndex)).FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes().ToList();

            List<HealthComponent> hitHealthComponent = new List<HealthComponent>();
            foreach (HurtBox hurtBox in HurtBoxes)
            {
                if (hurtBox && hurtBox.healthComponent && hurtBox.healthComponent.alive && !hitHealthComponent.Contains(hurtBox.healthComponent))
                {
                    float damageDistance = this.projectileDamage.damage * Mathf.Min(1, age * 0.20f);
                    DamageInfo damageInfo = new DamageInfo();
                    damageInfo.damage = damageDistance + (hurtBox.healthComponent.missingCombinedHealth * bonusDamageCoefficient);
                    damageInfo.crit = false;
                    damageInfo.damageColorIndex = DamageColorIndex.Item;
                    damageInfo.attacker = this.projectileController.owner ? this.projectileController.owner.gameObject : null;
                    damageInfo.inflictor = base.gameObject;
                    damageInfo.position = hurtBox.healthComponent.gameObject.transform.position;
                    damageInfo.force = this.projectileDamage.force * base.transform.forward;
                    damageInfo.procChainMask = this.projectileController.procChainMask;
                    damageInfo.procCoefficient = this.projectileController.procCoefficient;
                    hurtBox.healthComponent.TakeDamage(damageInfo);
                    hitHealthComponent.Add(hurtBox.healthComponent);
                }
            }
        }
    }
}
