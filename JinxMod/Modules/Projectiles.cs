using JinxMod.Controller;
using JinxMod.Controllers;
using R2API;
using RoR2;
using RoR2.Audio;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace JinxMod.Modules
{
    internal static class Projectiles
    {
        internal static GameObject bombPrefab;
        internal static GameObject missilePrefab;
        internal static GameObject megaRocketPrefab;
        internal static GameObject zapPrefab;

        internal static void RegisterProjectiles()
        {
            CreateBomb();
            CreateMissile();
            CreateMegaRocket();
            CreateZap();
            AddProjectile(bombPrefab);
            AddProjectile(missilePrefab);
            AddProjectile(megaRocketPrefab);
            AddProjectile(zapPrefab);
        }
        private static void CreateMissile()
        {
            missilePrefab = CloneProjectilePrefab("MissileProjectile", "JinxMissileProjectile");
            GameObject.Destroy(missilePrefab.GetComponent<MissileController>());
            GameObject.Destroy(missilePrefab.GetComponent<ProjectileSingleTargetImpact>());

            ProjectileSphereTargetFinder projectileSphereTargetFinder = missilePrefab.AddComponent<ProjectileSphereTargetFinder>();
            projectileSphereTargetFinder.lookRange = 4f;
            projectileSphereTargetFinder.onlySearchIfNoTarget = true;
            projectileSphereTargetFinder.allowTargetLoss = true;
            projectileSphereTargetFinder.testLoS = true;
            projectileSphereTargetFinder.targetSearchInterval = 0.1f;

            missilePrefab.AddComponent<JinxMissileController>();

            ProjectileImpactExplosion ImpactExplosion = missilePrefab.AddComponent<ProjectileImpactExplosion>();
            InitializeImpactExplosion(ImpactExplosion);

            ImpactExplosion.blastRadius = 8f;
            ImpactExplosion.destroyOnEnemy = true;
            ImpactExplosion.destroyOnWorld = true;
            ImpactExplosion.lifetime = 12f;
            ImpactExplosion.explosionEffect = Modules.Assets.bombExplosionEffect;
            ImpactExplosion.timerAfterImpact = true;
            ImpactExplosion.lifetimeAfterImpact = 0.1f;

            ProjectileSimple projectileSimple = missilePrefab.AddComponent<ProjectileSimple>();
            projectileSimple.desiredForwardSpeed = 60f;
            projectileSimple.oscillate = false;
            projectileSimple.updateAfterFiring = true;
            projectileSimple.enableVelocityOverLifetime = false;

            Rigidbody rigidBody = missilePrefab.GetComponent<Rigidbody>();
            rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            ProjectileController projectileController = missilePrefab.GetComponent<ProjectileController>();

            projectileController.allowPrediction = false;

            var ghostPrefab = PrefabAPI.InstantiateClone(projectileController.ghostPrefab, "MissileGhost", false);
            missilePrefab.transform.localScale *= 5;
            ghostPrefab.transform.localScale *= 5;

            var Trail = ghostPrefab.GetComponentInChildren<Transform>().Find("Trail");
            Trail.transform.localScale *= 2.5f;
            TrailRenderer trailRenderer = Trail.GetComponent<TrailRenderer>();
            trailRenderer.time = 1f;
            trailRenderer.startWidth = 2.5f;
            trailRenderer.endWidth = 2.5f;
            var Flare = ghostPrefab.GetComponentInChildren<Transform>().Find("Flare");
            Flare.transform.localScale *= 2.5f;

            projectileController.ghostPrefab = ghostPrefab;

            LoopSoundDef loopSoundDef = ScriptableObject.CreateInstance<LoopSoundDef>();
            loopSoundDef.startSoundName = "Play_JinxRocketLoop";
            loopSoundDef.stopSoundName = "Stop_JinxRocketLoop";
            projectileController.flightSoundLoop = loopSoundDef;

            BoxCollider boxCollider = missilePrefab.GetComponent<BoxCollider>();
            boxCollider.size = new Vector3(0.075f, 0.075f, 0.075f);

            missilePrefab.AddComponent<ProjectileImpactEventCaller>();
            RocketJumpController rocketJumpController = missilePrefab.AddComponent<RocketJumpController>();
            rocketJumpController.explosionForce = 6000f;
        }

        private static void CreateMegaRocket()
        {
            megaRocketPrefab = CloneProjectilePrefab("MissileProjectile", "JinxMegaRocketProjectile");
            GameObject.Destroy(megaRocketPrefab.GetComponent<MissileController>());
            GameObject.Destroy(megaRocketPrefab.GetComponent<ProjectileSingleTargetImpact>());

            ProjectileImpactExplosion ImpactExplosion = megaRocketPrefab.AddComponent<ProjectileImpactExplosion>();
            InitializeImpactExplosion(ImpactExplosion);

            ImpactExplosion.blastRadius = 16f;
            ImpactExplosion.destroyOnEnemy = true;
            ImpactExplosion.destroyOnWorld = true;
            ImpactExplosion.lifetime = 12f;
            ImpactExplosion.explosionEffect = Modules.Assets.megaExplosionEffect;
            ImpactExplosion.timerAfterImpact = true;
            ImpactExplosion.lifetimeAfterImpact = 0.1f;

            megaRocketPrefab.AddComponent<JinxMegaImpact>();

            ProjectileSimple projectileSimple = megaRocketPrefab.AddComponent<ProjectileSimple>();
            projectileSimple.desiredForwardSpeed = 90f;
            projectileSimple.oscillate = false;
            projectileSimple.updateAfterFiring = true;
            projectileSimple.enableVelocityOverLifetime = false;

            Rigidbody rigidBody = megaRocketPrefab.GetComponent<Rigidbody>();
            rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            ProjectileController projectileController = megaRocketPrefab.GetComponent<ProjectileController>();
            var ghostPrefab = PrefabAPI.InstantiateClone(projectileController.ghostPrefab, "MegaRocketGhost", false);
            megaRocketPrefab.transform.localScale *= 10;
            ghostPrefab.transform.localScale *= 10;

            var Trail = ghostPrefab.GetComponentInChildren<Transform>().Find("Trail");
            Trail.transform.localScale *= 7.5f;
            TrailRenderer trailRenderer = Trail.GetComponent<TrailRenderer>();
            trailRenderer.time = 1f;
            trailRenderer.startWidth = 7.5f;
            trailRenderer.endWidth = 7.5f;

            var Flare = ghostPrefab.GetComponentInChildren<Transform>().Find("Flare");
            Flare.transform.localScale *= 7.5f;

            projectileController.ghostPrefab = ghostPrefab;

            LoopSoundDef loopSoundDef = ScriptableObject.CreateInstance<LoopSoundDef>();
            loopSoundDef.startSoundName = "Play_JinxRocketLoop";
            loopSoundDef.stopSoundName = "Stop_JinxRocketLoop";
            projectileController.flightSoundLoop = loopSoundDef;

            BoxCollider boxCollider = megaRocketPrefab.GetComponent<BoxCollider>();
            boxCollider.size = new Vector3(0.075f, 0.075f, 0.075f);

            megaRocketPrefab.AddComponent<ProjectileImpactEventCaller>();
            RocketJumpController rocketJumpController = megaRocketPrefab.AddComponent<RocketJumpController>();
            rocketJumpController.explosionForce = 12000f;

        }

        private static void CreateZap()
        {
            zapPrefab = CloneProjectilePrefab("CaptainTazer", "JinxZapProjectile");
            GameObject.Destroy(zapPrefab.GetComponent<ProjectileStickOnImpact>());
            ProjectileImpactExplosion ImpactExplosion = zapPrefab.GetComponent<ProjectileImpactExplosion>();
            ImpactExplosion.destroyOnWorld = true;
            ImpactExplosion.lifetimeExpiredSound = Modules.Assets.CreateNetworkSoundEventDef("Play_JinxZapImpact");
            ImpactExplosion.blastRadius *= 6f;
        }
        private static void CreateBomb()
        {
            bombPrefab = CloneProjectilePrefab("CommandoGrenadeProjectile", "HenryBombProjectile");

            ProjectileImpactExplosion bombImpactExplosion = bombPrefab.GetComponent<ProjectileImpactExplosion>();
            InitializeImpactExplosion(bombImpactExplosion);

            bombImpactExplosion.blastRadius = 16f;
            bombImpactExplosion.destroyOnEnemy = true;
            bombImpactExplosion.lifetime = 12f;
            bombImpactExplosion.impactEffect = Modules.Assets.bombExplosionEffect;
            //bombImpactExplosion.lifetimeExpiredSound = Modules.Assets.CreateNetworkSoundEventDef("HenryBombExplosion");
            bombImpactExplosion.timerAfterImpact = true;
            bombImpactExplosion.lifetimeAfterImpact = 0.1f;

            ProjectileController bombController = bombPrefab.GetComponent<ProjectileController>();
            if (Modules.Assets.mainAssetBundle.LoadAsset<GameObject>("HenryBombGhost") != null) bombController.ghostPrefab = CreateGhostPrefab("HenryBombGhost");
            bombController.startSound = "";
        }

        internal static void AddProjectile(GameObject projectileToAdd)
        {
            Modules.Content.AddProjectilePrefab(projectileToAdd);
        }

        private static void InitializeImpactExplosion(ProjectileImpactExplosion projectileImpactExplosion)
        {
            projectileImpactExplosion.blastDamageCoefficient = 1f;
            projectileImpactExplosion.blastProcCoefficient = 1f;
            projectileImpactExplosion.blastRadius = 1f;
            projectileImpactExplosion.bonusBlastForce = Vector3.zero;
            projectileImpactExplosion.childrenCount = 0;
            projectileImpactExplosion.childrenDamageCoefficient = 0f;
            projectileImpactExplosion.childrenProjectilePrefab = null;
            projectileImpactExplosion.destroyOnEnemy = false;
            projectileImpactExplosion.destroyOnWorld = false;
            projectileImpactExplosion.falloffModel = RoR2.BlastAttack.FalloffModel.None;
            projectileImpactExplosion.fireChildren = false;
            projectileImpactExplosion.impactEffect = null;
            projectileImpactExplosion.lifetime = 0f;
            projectileImpactExplosion.lifetimeAfterImpact = 0f;
            projectileImpactExplosion.lifetimeRandomOffset = 0f;
            projectileImpactExplosion.offsetForLifetimeExpiredSound = 0f;
            projectileImpactExplosion.timerAfterImpact = false;

            projectileImpactExplosion.GetComponent<ProjectileDamage>().damageType = DamageType.Generic;
        }

        private static GameObject CreateGhostPrefab(string ghostName)
        {
            GameObject ghostPrefab = Modules.Assets.mainAssetBundle.LoadAsset<GameObject>(ghostName);
            if (!ghostPrefab.GetComponent<NetworkIdentity>()) ghostPrefab.AddComponent<NetworkIdentity>();
            if (!ghostPrefab.GetComponent<ProjectileGhostController>()) ghostPrefab.AddComponent<ProjectileGhostController>();

            Modules.Assets.ConvertAllRenderersToHopooShader(ghostPrefab);

            return ghostPrefab;
        }

        private static GameObject CloneProjectilePrefab(string prefabName, string newPrefabName)
        {
            GameObject newPrefab = PrefabAPI.InstantiateClone(RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/" + prefabName), newPrefabName);
            return newPrefab;
        }
    }
}