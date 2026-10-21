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
            if (!missilePrefab) return;
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
            projectileSimple.desiredForwardSpeed = 100f;
            projectileSimple.oscillate = false;
            projectileSimple.updateAfterFiring = true;
            projectileSimple.enableVelocityOverLifetime = false;

            Rigidbody rigidBody = missilePrefab.GetComponent<Rigidbody>();
            rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            ProjectileController projectileController = missilePrefab.GetComponent<ProjectileController>();

            projectileController.allowPrediction = false;

            missilePrefab.transform.localScale *= 5;
            SetupScaledGhost(projectileController, "MissileGhost", 5f, 2.5f);

            LoopSoundDef loopSoundDef = ScriptableObject.CreateInstance<LoopSoundDef>();
            loopSoundDef.startSoundName = "Play_JinxRocketLoop";
            loopSoundDef.stopSoundName = "Stop_JinxRocketLoop";
            projectileController.flightSoundLoop = loopSoundDef;

            BoxCollider boxCollider = missilePrefab.GetComponent<BoxCollider>();
            boxCollider.size = new Vector3(0.035f, 0.035f, 0.035f);

            missilePrefab.AddComponent<ProjectileImpactEventCaller>();
            RocketJumpController rocketJumpController = missilePrefab.AddComponent<RocketJumpController>();
            rocketJumpController.explosionForce = Modules.Config.rocketJumpFishBonesForce.Value;
        }

        private static void CreateMegaRocket()
        {
            megaRocketPrefab = CloneProjectilePrefab("MissileProjectile", "JinxMegaRocketProjectile");
            if (!megaRocketPrefab) return;
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
            rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            ProjectileController projectileController = megaRocketPrefab.GetComponent<ProjectileController>();
            megaRocketPrefab.transform.localScale *= 10;
            SetupScaledGhost(projectileController, "MegaRocketGhost", 10f, 7.5f);

            LoopSoundDef loopSoundDef = ScriptableObject.CreateInstance<LoopSoundDef>();
            loopSoundDef.startSoundName = "Play_JinxRocketLoop";
            loopSoundDef.stopSoundName = "Stop_JinxRocketLoop";
            projectileController.flightSoundLoop = loopSoundDef;

            BoxCollider boxCollider = megaRocketPrefab.GetComponent<BoxCollider>();
            boxCollider.size = new Vector3(0.075f, 0.075f, 0.075f);

            megaRocketPrefab.AddComponent<ProjectileImpactEventCaller>();
            RocketJumpController rocketJumpController = megaRocketPrefab.AddComponent<RocketJumpController>();
            rocketJumpController.explosionForce = Modules.Config.rocketJumpMegaRocketForce.Value;

        }

        private static void CreateZap()
        {
            zapPrefab = CloneProjectilePrefab("CaptainTazer", "JinxZapProjectile");
            if (!zapPrefab) return;
            GameObject.Destroy(zapPrefab.GetComponent<ProjectileStickOnImpact>());
            ProjectileImpactExplosion ImpactExplosion = zapPrefab.GetComponent<ProjectileImpactExplosion>();
            ImpactExplosion.destroyOnWorld = true;
            ImpactExplosion.lifetimeExpiredSound = Modules.Assets.CreateNetworkSoundEventDef("Play_JinxZapImpact");
            ImpactExplosion.blastRadius *= 6f;
        }
        private static void CreateBomb()
        {
            bombPrefab = CloneProjectilePrefab("CommandoGrenadeProjectile", "HenryBombProjectile");
            if (!bombPrefab) return;

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
            if (projectileToAdd) Modules.Content.AddProjectilePrefab(projectileToAdd);
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

        // Clones the vanilla missile ghost, scales it up, and widens its trail/flare.
        // Newer RoR2 builds can leave ProjectileController.ghostPrefab null (ghost is referenced via Addressables instead),
        // so resolve it defensively and skip the customization rather than crashing plugin Awake().
        private static void SetupScaledGhost(ProjectileController projectileController, string newGhostName, float ghostScale, float trailScale)
        {
            GameObject originalGhost = ResolveGhostPrefab(projectileController);
            if (!originalGhost)
            {
                Log.Warning($"[JinxMod] Could not find vanilla missile ghost; {newGhostName} will use the default visuals.");
                return;
            }

            GameObject ghostPrefab = PrefabAPI.InstantiateClone(originalGhost, newGhostName, false);
            ghostPrefab.transform.localScale *= ghostScale;

            Transform trail = ghostPrefab.transform.Find("Trail");
            if (trail)
            {
                trail.localScale *= trailScale;
                TrailRenderer trailRenderer = trail.GetComponent<TrailRenderer>();
                if (trailRenderer)
                {
                    trailRenderer.time = 1f;
                    trailRenderer.startWidth = trailScale;
                    trailRenderer.endWidth = trailScale;
                }
            }

            Transform flare = ghostPrefab.transform.Find("Flare");
            if (flare) flare.localScale *= trailScale;

            projectileController.ghostPrefab = ghostPrefab;
            // make sure the game doesn't load the vanilla ghost over ours via an addressable reference
            ClearGhostPrefabReference(projectileController);
        }

        private static GameObject ResolveGhostPrefab(ProjectileController projectileController)
        {
            if (projectileController.ghostPrefab) return projectileController.ghostPrefab;

            // look for an AssetReference-style ghost field (name varies by game version), without a hard compile-time dependency
            foreach (var field in typeof(ProjectileController).GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic))
            {
                if (!field.Name.ToLower().Contains("ghost")) continue;
                if (field.GetValue(projectileController) is UnityEngine.AddressableAssets.AssetReference assetRef && assetRef.RuntimeKeyIsValid())
                {
                    GameObject loaded = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(assetRef.RuntimeKey).WaitForCompletion();
                    if (loaded) return loaded;
                }
            }

            return GameAssets.Load<GameObject>("Prefabs/ProjectileGhosts/MissileGhost", "MissileGhost.prefab");
        }

        private static void ClearGhostPrefabReference(ProjectileController projectileController)
        {
            foreach (var field in typeof(ProjectileController).GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic))
            {
                if (field.Name.ToLower().Contains("ghost") && typeof(UnityEngine.AddressableAssets.AssetReference).IsAssignableFrom(field.FieldType))
                {
                    field.SetValue(projectileController, null);
                }
            }
        }

        private static GameObject CloneProjectilePrefab(string prefabName, string newPrefabName)
        {
            GameObject original = GameAssets.Load<GameObject>("Prefabs/Projectiles/" + prefabName, prefabName + ".prefab");
            if (!original)
            {
                Log.Error("Vanilla projectile " + prefabName + " not found, " + newPrefabName + " will not be created");
                return null;
            }
            return PrefabAPI.InstantiateClone(original, newPrefabName);
        }
    }
}