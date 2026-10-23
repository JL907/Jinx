using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace JinxMod.Modules.LeagueSkins
{
    internal class LeagueSkinDefinition
    {
        internal string folder;
        internal string meshFile;
        internal string skeletonFile;
        internal string nameToken;
        internal string displayName;
        internal string iconTexture;
        internal string defaultTexture;
        internal Dictionary<string, string> partTextures = new Dictionary<string, string>();
        internal string[] hiddenParts = new string[0];
        // Retarget: give the skin its own skeleton that copies Jinx's animated bone rotations each frame
        // (keeps the skin's own joint positions/bone lengths). Use for skins whose rig differs from base Jinx.
        internal bool retarget;
        // Skin joint -> Unity bone for joints the base rig lacks (e.g. a weapon root). These follow that bone's position too.
        internal Dictionary<string, string> jointMap = new Dictionary<string, string>();
        // Alternate bindings used in a weapon mode (LeagueRigDriver.ModeRocket / ModeZap).
        internal List<LeagueModeBinding> modeBindings = new List<LeagueModeBinding>();
        // Parts that are only visible in one weapon mode (e.g. a zapper attachment during Zap).
        internal Dictionary<string, int> modeParts = new Dictionary<string, int>();
    }

    // In `mode`, drive `joint` from Unity bone `bone`, posed so that skin joint `skinAnchor` sits on base joint
    // `baseAnchor` (League rest poses) with the skin's axes rotated by `rotation` (League space).
    internal class LeagueModeBinding
    {
        internal int mode;
        internal string joint;
        internal string bone;
        internal string skinAnchor;
        internal string baseAnchor;
        internal Quaternion rotation = Quaternion.identity;
    }

    internal class BuiltLeagueSkin
    {
        internal LeagueSkinDefinition definition;
        internal Sprite icon;
        internal List<GameObject> parts = new List<GameObject>();
    }

    internal static class LeagueSkinBuilder
    {
        internal const float LeagueUnitScale = 0.01f;
        internal const string BaseFolder = "Base";
        internal const string BaseSkeletonFile = "jinx.skl";
        internal const string BaseMeshFile = "jinx.skn";
        internal const bool TransferExtraBoneWeights = true;
        internal static readonly string[] BaseExcludedParts = new string[0];
        internal static readonly string[] TransferIgnoredBoneKeywords = { "Buffbone", "Vfx" };
        internal const string FallbackBone = "Root";
        internal const bool ReverseWinding = true;
        internal const float MaxRotationRebind = 30f;
        internal static readonly string[] RigidAnchorPrefixes = { "Minigun", "Rocket_Launcher", "Pistol", "Cstm_" };

        private static bool IsRigidAnchor(string joint)
        {
            foreach (string prefix in RigidAnchorPrefixes)
            {
                if (joint.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        private static LeagueSkeleton baseSkeleton;

        internal static List<BuiltLeagueSkin> BuildAll(GameObject bodyModel, SkinnedMeshRenderer bodyRenderer, CharacterModel characterModel, GameObject displayModel)
        {
            List<BuiltLeagueSkin> built = new List<BuiltLeagueSkin>();
            if (!bodyModel || !bodyRenderer || !characterModel) return built;

            if (baseSkeleton == null)
            {
                byte[] baseBytes = LeagueResources.Load(BaseFolder, BaseSkeletonFile);
                if (baseBytes == null)
                {
                    Debug.LogError("[Jinx LeagueSkins] Missing embedded base skeleton " + BaseFolder + "/" + BaseSkeletonFile);
                    return built;
                }
                baseSkeleton = LeagueSkeleton.Read(baseBytes);
            }

            BaseWeightSource weightSource = null;
            if (TransferExtraBoneWeights)
            {
                try
                {
                    weightSource = BaseWeightSource.Create(baseSkeleton, bodyModel, bodyRenderer);
                }
                catch (Exception e)
                {
                    Debug.LogError("[Jinx LeagueSkins] Could not prepare base weights: " + e);
                }
            }

            Material template = characterModel.baseRendererInfos.Length > 0 ? characterModel.baseRendererInfos[0].defaultMaterial : null;
            List<CharacterModel.RendererInfo> rendererInfos = characterModel.baseRendererInfos.ToList();

            foreach (LeagueSkinDefinition definition in LeagueSkinCatalog.definitions)
            {
                try
                {
                    BuiltLeagueSkin skin = Build(definition, bodyModel, bodyRenderer, template, displayModel, rendererInfos, weightSource);
                    if (skin != null) built.Add(skin);
                }
                catch (Exception e)
                {
                    Debug.LogError("[Jinx LeagueSkins] Failed to build " + definition.folder + ": " + e);
                }
            }

            characterModel.baseRendererInfos = rendererInfos.ToArray();
            return built;
        }

        internal static void AddSkins(List<SkinDef> skins, List<BuiltLeagueSkin> leagueSkins, GameObject bodyModel, SkinnedMeshRenderer bodyRenderer, CharacterModel characterModel)
        {
            if (leagueSkins.Count == 0) return;

            List<GameObject> allParts = leagueSkins.SelectMany(s => s.parts).ToList();

            foreach (SkinDef skin in skins)
            {
                List<SkinDef.GameObjectActivation> activations = (skin.gameObjectActivations ?? new SkinDef.GameObjectActivation[0]).ToList();
                activations.Add(new SkinDef.GameObjectActivation { gameObject = bodyRenderer.gameObject, shouldActivate = true });
                foreach (GameObject part in allParts) activations.Add(new SkinDef.GameObjectActivation { gameObject = part, shouldActivate = false });
                skin.gameObjectActivations = activations.ToArray();
                // Jinx's Skins.CreateSkinDef bakes a SkinDefParams with no activations; drop it so the legacy
                // fields above (same renderer infos plus these activations) are used, like the League skins.
                skin.skinDefParams = null;
            }

            Sprite fallbackIcon = skins.Count > 0 ? skins[0].icon : null;

            foreach (BuiltLeagueSkin leagueSkin in leagueSkins)
            {
                LanguageAPI.Add(leagueSkin.definition.nameToken, "<color=#ffa700>" + leagueSkin.definition.displayName + "</color>");

                List<SkinDef.GameObjectActivation> activations = new List<SkinDef.GameObjectActivation>
                {
                    new SkinDef.GameObjectActivation { gameObject = bodyRenderer.gameObject, shouldActivate = false }
                };
                foreach (GameObject part in allParts)
                {
                    activations.Add(new SkinDef.GameObjectActivation { gameObject = part, shouldActivate = leagueSkin.parts.Contains(part) });
                }

                CharacterModel.RendererInfo[] rendererInfos = new CharacterModel.RendererInfo[characterModel.baseRendererInfos.Length];
                characterModel.baseRendererInfos.CopyTo(rendererInfos, 0);

                Skins.SkinDefInfo info = new Skins.SkinDefInfo
                {
                    Name = leagueSkin.definition.nameToken,
                    NameToken = leagueSkin.definition.nameToken,
                    Icon = leagueSkin.icon ? leagueSkin.icon : fallbackIcon,
                    UnlockableDef = null,
                    RootObject = bodyModel,
                    BaseSkins = new SkinDef[0],
                    MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0],
                    ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0],
                    GameObjectActivations = activations.ToArray(),
                    MeshReplacements = new SkinDef.MeshReplacement[0],
                    RendererInfos = rendererInfos
                };
                skins.Add(Skins.CreateSkinDef(info));
            }
        }

        private static Dictionary<string, Transform> CollectBones(GameObject root)
        {
            Dictionary<string, Transform> bones = new Dictionary<string, Transform>();
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (!bones.ContainsKey(t.name)) bones[t.name] = t;
            }
            return bones;
        }

        private static string PathFrom(Transform root, Transform target)
        {
            List<string> names = new List<string>();
            for (Transform t = target; t && t != root; t = t.parent) names.Insert(0, t.name);
            return string.Join("/", names);
        }

        internal static Matrix4x4 LeagueToRenderer(GameObject bodyModel, SkinnedMeshRenderer bodyRenderer)
        {
            return bodyRenderer.transform.worldToLocalMatrix * bodyModel.transform.localToWorldMatrix * Matrix4x4.Scale(Vector3.one * LeagueUnitScale);
        }

        internal static Dictionary<string, Transform> Bones(GameObject root)
        {
            return CollectBones(root);
        }

        private static BuiltLeagueSkin Build(LeagueSkinDefinition definition, GameObject bodyModel, SkinnedMeshRenderer bodyRenderer, Material template, GameObject displayModel, List<CharacterModel.RendererInfo> rendererInfos, BaseWeightSource weightSource)
        {
            byte[] meshBytes = LeagueResources.Load(definition.folder, definition.meshFile);
            byte[] skeletonBytes = LeagueResources.Load(definition.folder, definition.skeletonFile);
            if (meshBytes == null || skeletonBytes == null)
            {
                Debug.LogError("[Jinx LeagueSkins] Missing embedded mesh or skeleton for " + definition.folder);
                return null;
            }

            LeagueMesh mesh = LeagueMesh.Read(meshBytes);
            LeagueSkeleton skeleton = LeagueSkeleton.Read(skeletonBytes);

            if (definition.retarget)
            {
                return BuildRetargeted(definition, mesh, skeleton, bodyModel, bodyRenderer, template, displayModel, rendererInfos);
            }

            Dictionary<string, Transform> bodyBones = CollectBones(bodyModel);
            Transform rendererTransform = bodyRenderer.transform;
            Matrix4x4 leagueToRenderer = LeagueToRenderer(bodyModel, bodyRenderer);

            Dictionary<Transform, Matrix4x4> baseBindposes = new Dictionary<Transform, Matrix4x4>();
            Transform[] rendererBones = bodyRenderer.bones;
            Matrix4x4[] rendererBindposes = bodyRenderer.sharedMesh ? bodyRenderer.sharedMesh.bindposes : new Matrix4x4[0];
            for (int i = 0; i < rendererBones.Length && i < rendererBindposes.Length; i++)
            {
                if (rendererBones[i] && !baseBindposes.ContainsKey(rendererBones[i])) baseBindposes[rendererBones[i]] = rendererBindposes[i];
            }

            CheckAlignment(definition, bodyBones, baseBindposes, rendererTransform, leagueToRenderer);

            Transform fallback = bodyBones.TryGetValue(FallbackBone, out Transform root) ? root : bodyRenderer.rootBone;
            int jointCount = skeleton.names.Length;
            Transform[] jointTargets = new Transform[jointCount];
            Matrix4x4[] jointMatrices = new Matrix4x4[jointCount];
            bool[] jointIsExtra = new bool[jointCount];
            for (int j = 0; j < jointCount; j++)
            {
                int anchor = j;
                while (anchor >= 0 && !(baseSkeleton.index.ContainsKey(skeleton.names[anchor]) && bodyBones.ContainsKey(skeleton.names[anchor])))
                {
                    anchor = skeleton.parents[anchor];
                }

                // Extra joints under a weapon bone are rigid props (gun parts, cases, scopes): keep them on that
                // bone. Only soft extras (hair, cloth, necklaces) borrow weights from the nearest base vertex.
                jointIsExtra[j] = anchor != j && !(anchor >= 0 && IsRigidAnchor(skeleton.names[anchor]));
                if (anchor >= 0)
                {
                    string name = skeleton.names[anchor];
                    Matrix4x4 baseRest = baseSkeleton.rest[baseSkeleton.index[name]];
                    Matrix4x4 skinRest = skeleton.rest[anchor];
                    jointTargets[j] = bodyBones[name];
                    if (Quaternion.Angle(baseRest.rotation, skinRest.rotation) > MaxRotationRebind)
                    {
                        Vector3 offset = (Vector3)baseRest.GetColumn(3) - (Vector3)skinRest.GetColumn(3);
                        jointMatrices[j] = leagueToRenderer * Matrix4x4.Translate(offset);
                    }
                    else
                    {
                        jointMatrices[j] = leagueToRenderer * baseRest * skinRest.inverse;
                    }
                }
                else
                {
                    jointTargets[j] = fallback;
                    jointMatrices[j] = leagueToRenderer;
                }
            }

            BuiltLeagueSkin built = new BuiltLeagueSkin { definition = definition };
            byte[] iconBytes = string.IsNullOrEmpty(definition.iconTexture) ? null : LeagueResources.Load(definition.folder, definition.iconTexture);
            if (iconBytes != null) built.icon = LeagueTextures.LoadIcon(iconBytes, definition.nameToken + "_Icon");

            Dictionary<string, Material> materials = new Dictionary<string, Material>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> hidden = new HashSet<string>(definition.hiddenParts, StringComparer.OrdinalIgnoreCase);
            Transform displayRenderer = null;
            Dictionary<string, Transform> displayBones = null;
            if (displayModel)
            {
                displayRenderer = displayModel.transform.Find(PathFrom(bodyModel.transform, rendererTransform));
                displayBones = CollectBones(displayModel);
            }

            foreach (LeagueMeshPart part in mesh.parts)
            {
                if (hidden.Contains(part.name)) continue;

                List<Transform> bones = new List<Transform>();
                Mesh unityMesh = BuildPartMesh(definition, part, mesh, skeleton, jointTargets, jointMatrices, jointIsExtra, weightSource, bones);

                Matrix4x4[] bindposes = new Matrix4x4[bones.Count];
                for (int b = 0; b < bones.Count; b++)
                {
                    bindposes[b] = baseBindposes.TryGetValue(bones[b], out Matrix4x4 bindpose) ? bindpose : bones[b].worldToLocalMatrix * rendererTransform.localToWorldMatrix;
                }
                unityMesh.bindposes = bindposes;

                Material material = GetMaterial(definition, part.name, template, materials);
                string objectName = "LeagueSkin_" + definition.folder + "_" + part.name;

                GameObject bodyPart = CreatePart(objectName, rendererTransform, unityMesh, material, bones.ToArray(), bodyRenderer.rootBone, bodyRenderer.localBounds);
                built.parts.Add(bodyPart);
                rendererInfos.Add(new CharacterModel.RendererInfo
                {
                    renderer = bodyPart.GetComponent<SkinnedMeshRenderer>(),
                    defaultMaterial = material,
                    defaultShadowCastingMode = ShadowCastingMode.On,
                    ignoreOverlays = false,
                    hideOnDeath = false
                });

                if (displayRenderer && displayBones != null)
                {
                    Transform[] displayPartBones = bones.Select(b => displayBones.TryGetValue(b.name, out Transform t) ? t : null).ToArray();
                    Transform displayRoot = bodyRenderer.rootBone && displayBones.TryGetValue(bodyRenderer.rootBone.name, out Transform r) ? r : null;
                    CreatePart(objectName, displayRenderer, unityMesh, material, displayPartBones, displayRoot, bodyRenderer.localBounds);
                }
            }

            Debug.Log("[Jinx LeagueSkins] Built " + definition.folder + " with " + built.parts.Count + " parts");
            return built;
        }

        private static BuiltLeagueSkin BuildRetargeted(LeagueSkinDefinition definition, LeagueMesh mesh, LeagueSkeleton skeleton, GameObject bodyModel, SkinnedMeshRenderer bodyRenderer, Material template, GameObject displayModel, List<CharacterModel.RendererInfo> rendererInfos)
        {
            Transform rendererTransform = bodyRenderer.transform;
            Matrix4x4 leagueToRenderer = LeagueToRenderer(bodyModel, bodyRenderer);
            Dictionary<string, Transform> bodyBones = CollectBones(bodyModel);

            HashSet<string> hidden = new HashSet<string>(definition.hiddenParts, StringComparer.OrdinalIgnoreCase);

            // Mode-only parts get their own toggle joint (child of the part's main joint) that the driver scales to zero
            // outside that mode. Their vertices are bound rigidly to it.
            List<int[]> toggles = new List<int[]>();
            Dictionary<string, int> toggleForPart = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (LeagueMeshPart part in mesh.parts)
            {
                if (hidden.Contains(part.name) || !definition.modeParts.TryGetValue(part.name, out int partMode)) continue;
                toggleForPart[part.name] = toggles.Count;
                toggles.Add(new[] { MainJoint(part, mesh, skeleton), partMode });
            }

            Transform[] joints = CreateRig(definition, skeleton, bodyModel.transform, bodyBones, toggles, out LeagueRigDriver bodyDriver);

            Transform displayRenderer = null;
            Transform[] displayJoints = null;
            LeagueRigDriver displayDriver = null;
            Transform displayRoot = null;
            if (displayModel)
            {
                displayRenderer = displayModel.transform.Find(PathFrom(bodyModel.transform, rendererTransform));
                if (displayRenderer)
                {
                    Dictionary<string, Transform> displayBones = CollectBones(displayModel);
                    displayJoints = CreateRig(definition, skeleton, displayModel.transform, displayBones, toggles, out displayDriver);
                    if (bodyRenderer.rootBone) displayBones.TryGetValue(bodyRenderer.rootBone.name, out displayRoot);
                }
            }

            // Bindposes come from the rig at rest, relative to the body renderer (shared by the display copy,
            // which has the same model-root -> renderer relationship).
            Matrix4x4[] jointBindposes = new Matrix4x4[joints.Length];
            for (int j = 0; j < joints.Length; j++)
            {
                // Toggle joints rest at scale zero; bind them as if visible (scale one).
                Vector3 scale = joints[j].localScale;
                joints[j].localScale = Vector3.one;
                jointBindposes[j] = joints[j].worldToLocalMatrix * rendererTransform.localToWorldMatrix;
                joints[j].localScale = scale;
            }

            BuiltLeagueSkin built = new BuiltLeagueSkin { definition = definition };
            byte[] iconBytes = string.IsNullOrEmpty(definition.iconTexture) ? null : LeagueResources.Load(definition.folder, definition.iconTexture);
            if (iconBytes != null) built.icon = LeagueTextures.LoadIcon(iconBytes, definition.nameToken + "_Icon");

            Dictionary<string, Material> materials = new Dictionary<string, Material>(StringComparer.OrdinalIgnoreCase);
            List<Renderer> bodyParts = new List<Renderer>();
            List<Renderer> displayParts = new List<Renderer>();

            foreach (LeagueMeshPart part in mesh.parts)
            {
                if (hidden.Contains(part.name)) continue;

                List<int> usedJoints = new List<int>();
                int forcedJoint = toggleForPart.TryGetValue(part.name, out int toggle) ? skeleton.names.Length + toggle : -1;
                Mesh unityMesh = BuildRetargetedPartMesh(definition, part, mesh, skeleton, leagueToRenderer, usedJoints, forcedJoint);
                unityMesh.bindposes = usedJoints.Select(j => jointBindposes[j]).ToArray();

                Material material = GetMaterial(definition, part.name, template, materials);
                string objectName = "LeagueSkin_" + definition.folder + "_" + part.name;

                GameObject bodyPart = CreatePart(objectName, rendererTransform, unityMesh, material, usedJoints.Select(j => joints[j]).ToArray(), bodyRenderer.rootBone, bodyRenderer.localBounds);
                built.parts.Add(bodyPart);
                bodyParts.Add(bodyPart.GetComponent<Renderer>());
                rendererInfos.Add(new CharacterModel.RendererInfo
                {
                    renderer = bodyPart.GetComponent<SkinnedMeshRenderer>(),
                    defaultMaterial = material,
                    defaultShadowCastingMode = ShadowCastingMode.On,
                    ignoreOverlays = false,
                    hideOnDeath = false
                });

                if (displayRenderer && displayJoints != null)
                {
                    GameObject displayPart = CreatePart(objectName, displayRenderer, unityMesh, material, usedJoints.Select(j => displayJoints[j]).ToArray(), displayRoot, bodyRenderer.localBounds);
                    displayParts.Add(displayPart.GetComponent<Renderer>());
                }
            }

            bodyDriver.watch = bodyParts.ToArray();
            if (displayDriver) displayDriver.watch = displayParts.ToArray();

            Debug.Log("[Jinx LeagueSkins] Built " + definition.folder + " (retargeted, " + bodyDriver.joints.Length + " driven joints) with " + built.parts.Count + " parts");
            return built;
        }

        // Builds the skin's own League skeleton under the model root (League space = model space * LeagueUnitScale),
        // and a driver that copies the matching Unity bones' animated rotations onto it every frame.
        private static int MainJoint(LeagueMeshPart part, LeagueMesh mesh, LeagueSkeleton skeleton)
        {
            Dictionary<int, float> totals = new Dictionary<int, float>();
            for (int v = part.vertexStart; v < part.vertexStart + part.vertexCount && v < mesh.positions.Length; v++)
            {
                for (int k = 0; k < 4; k++)
                {
                    float weight = mesh.weights[v * 4 + k];
                    int influence = mesh.boneIndices[v * 4 + k];
                    if (weight <= 0f || influence >= skeleton.influences.Length) continue;
                    int joint = skeleton.influences[influence];
                    totals[joint] = (totals.TryGetValue(joint, out float total) ? total : 0f) + weight;
                }
            }
            return totals.Count > 0 ? totals.OrderByDescending(p => p.Value).First().Key : 0;
        }

        private static Transform[] CreateRig(LeagueSkinDefinition definition, LeagueSkeleton skeleton, Transform modelRoot, Dictionary<string, Transform> unityBones, List<int[]> toggles, out LeagueRigDriver driver)
        {
            GameObject rigObject = new GameObject("LeagueRig_" + definition.folder);
            rigObject.transform.SetParent(modelRoot, false);
            rigObject.transform.localPosition = Vector3.zero;
            rigObject.transform.localRotation = Quaternion.identity;
            rigObject.transform.localScale = Vector3.one * LeagueUnitScale;
            rigObject.layer = modelRoot.gameObject.layer;

            int count = skeleton.names.Length;
            Transform[] joints = new Transform[count + toggles.Count];
            for (int j = 0; j < count; j++)
            {
                // Prefixed so name lookups for Jinx's real bones never hit the copy.
                joints[j] = new GameObject("lr_" + skeleton.names[j]).transform;
                joints[j].gameObject.layer = rigObject.layer;
            }
            for (int j = 0; j < count; j++)
            {
                int parent = skeleton.parents[j];
                joints[j].SetParent(parent >= 0 && parent < count ? joints[parent] : rigObject.transform, false);
            }
            for (int j = 0; j < count; j++)
            {
                int parent = skeleton.parents[j];
                Matrix4x4 local = parent >= 0 && parent < count ? skeleton.rest[parent].inverse * skeleton.rest[j] : skeleton.rest[j];
                joints[j].localPosition = local.GetColumn(3);
                joints[j].localRotation = local.rotation;
                joints[j].localScale = Vector3.one;
            }

            Transform[] toggleJoints = new Transform[toggles.Count];
            int[] toggleModes = new int[toggles.Count];
            for (int t = 0; t < toggles.Count; t++)
            {
                Transform toggle = new GameObject("lr_toggle_" + t).transform;
                toggle.gameObject.layer = rigObject.layer;
                toggle.SetParent(joints[toggles[t][0]], false);
                toggle.localPosition = Vector3.zero;
                toggle.localRotation = Quaternion.identity;
                toggle.localScale = Vector3.zero;
                joints[count + t] = toggle;
                toggleJoints[t] = toggle;
                toggleModes[t] = toggles[t][1];
            }

            // Driven joints, parent-first so children see their parent's new pose.
            List<int> order = Enumerable.Range(0, count).OrderBy(j => Depth(skeleton, j)).ToList();
            bool[] mapped = new bool[count];
            List<Transform> drivenJoints = new List<Transform>();
            List<Transform> drivenBones = new List<Transform>();
            List<Quaternion> rotationOffsets = new List<Quaternion>();
            List<Vector3> positionOffsets = new List<Vector3>();
            List<bool> pinned = new List<bool>();

            foreach (int j in order)
            {
                string name = skeleton.names[j];
                bool explicitMap = definition.jointMap.TryGetValue(name, out string boneName);
                if (!explicitMap) boneName = name;
                if (!unityBones.TryGetValue(boneName, out Transform bone)) continue;
                if (!explicitMap && (baseSkeleton == null || !baseSkeleton.index.ContainsKey(name))) continue;

                mapped[j] = true;
                int parent = skeleton.parents[j];
                drivenJoints.Add(joints[j]);
                drivenBones.Add(bone);
                rotationOffsets.Add(Quaternion.Inverse(bone.rotation) * joints[j].rotation);
                positionOffsets.Add(bone.InverseTransformPoint(joints[j].position));
                // Joints under a driven parent keep the skin's own bone length; others are pinned to their bone.
                pinned.Add(explicitMap || parent < 0 || parent >= count || !mapped[parent]);
            }

            // Rocket-mode alternates (only for joints that are already driven).
            List<int> altJoint = new List<int>();
            List<int> altMode = new List<int>();
            List<Transform> altBones = new List<Transform>();
            List<Quaternion> altRotationOffsets = new List<Quaternion>();
            List<Vector3> altPositionOffsets = new List<Vector3>();
            foreach (LeagueModeBinding binding in definition.modeBindings)
            {
                if (!skeleton.index.TryGetValue(binding.joint, out int j) || !skeleton.index.TryGetValue(binding.skinAnchor, out int anchor)) continue;
                if (baseSkeleton == null || !baseSkeleton.index.TryGetValue(binding.baseAnchor, out int baseAnchor)) continue;
                if (!unityBones.TryGetValue(binding.bone, out Transform bone)) continue;
                int driven = drivenJoints.IndexOf(joints[j]);
                if (driven < 0) continue;

                // League-space rigid move: rotate about the skin anchor, then put the anchor on the base anchor.
                Vector3 skinAnchorPos = skeleton.rest[anchor].GetColumn(3);
                Vector3 baseAnchorPos = baseSkeleton.rest[baseAnchor].GetColumn(3);
                Matrix4x4 move = Matrix4x4.Translate(baseAnchorPos) * Matrix4x4.Rotate(binding.rotation) * Matrix4x4.Translate(-skinAnchorPos);
                Matrix4x4 desired = move * skeleton.rest[j];

                Vector3 desiredPosition = rigObject.transform.TransformPoint(desired.GetColumn(3));
                Quaternion desiredRotation = rigObject.transform.rotation * desired.rotation;

                altJoint.Add(driven);
                altMode.Add(binding.mode);
                altBones.Add(bone);
                altRotationOffsets.Add(Quaternion.Inverse(bone.rotation) * desiredRotation);
                altPositionOffsets.Add(bone.InverseTransformPoint(desiredPosition));
                pinned[driven] = true;
            }

            driver = rigObject.AddComponent<LeagueRigDriver>();
            driver.altJoint = altJoint.ToArray();
            driver.altMode = altMode.ToArray();
            driver.toggles = toggleJoints;
            driver.toggleModes = toggleModes;
            driver.altBones = altBones.ToArray();
            driver.altRotationOffsets = altRotationOffsets.ToArray();
            driver.altPositionOffsets = altPositionOffsets.ToArray();
            driver.joints = drivenJoints.ToArray();
            driver.bones = drivenBones.ToArray();
            driver.rotationOffsets = rotationOffsets.ToArray();
            driver.positionOffsets = positionOffsets.ToArray();
            driver.pinned = pinned.ToArray();
            driver.watch = new Renderer[0];
            return joints;
        }

        private static int Depth(LeagueSkeleton skeleton, int joint)
        {
            int depth = 0;
            for (int p = skeleton.parents[joint]; p >= 0 && depth < 512; p = skeleton.parents[p]) depth++;
            return depth;
        }

        private static Mesh BuildRetargetedPartMesh(LeagueSkinDefinition definition, LeagueMeshPart part, LeagueMesh mesh, LeagueSkeleton skeleton, Matrix4x4 leagueToRenderer, List<int> usedJoints, int forcedJoint)
        {
            Dictionary<int, int> slots = new Dictionary<int, int>();
            Dictionary<int, int> localIndex = new Dictionary<int, int>();
            List<int> sourceVertices = new List<int>();

            bool insideRange = true;
            bool belowCount = true;
            for (int i = 0; i < part.indexCount; i++)
            {
                int index = mesh.indices[part.indexStart + i];
                if (index < part.vertexStart || index >= part.vertexStart + part.vertexCount) insideRange = false;
                if (index >= part.vertexCount) belowCount = false;
            }
            bool relative = !insideRange && belowCount;

            int[] triangles = new int[part.indexCount];
            for (int i = 0; i < part.indexCount; i++)
            {
                int global = mesh.indices[part.indexStart + i] + (relative ? part.vertexStart : 0);
                if (!localIndex.TryGetValue(global, out int local))
                {
                    local = sourceVertices.Count;
                    sourceVertices.Add(global);
                    localIndex[global] = local;
                }
                triangles[i] = local;
            }

            int count = sourceVertices.Count;
            Vector3[] vertices = new Vector3[count];
            Vector3[] normals = new Vector3[count];
            Vector2[] uvs = new Vector2[count];
            BoneWeight[] boneWeights = new BoneWeight[count];
            int[] ids = new int[4];
            float[] weights = new float[4];

            for (int v = 0; v < count; v++)
            {
                int source = sourceVertices[v];
                vertices[v] = leagueToRenderer.MultiplyPoint3x4(mesh.positions[source]);
                normals[v] = leagueToRenderer.MultiplyVector(mesh.normals[source]).normalized;
                uvs[v] = mesh.uvs[source];

                int used = 0;
                if (forcedJoint >= 0)
                {
                    if (!slots.TryGetValue(forcedJoint, out int forcedSlot))
                    {
                        forcedSlot = usedJoints.Count;
                        usedJoints.Add(forcedJoint);
                        slots[forcedJoint] = forcedSlot;
                    }
                    ids[0] = forcedSlot;
                    weights[0] = 1f;
                    used = 1;
                }
                for (int k = 0; k < 4 && forcedJoint < 0; k++)
                {
                    float weight = mesh.weights[source * 4 + k];
                    if (weight <= 0f) continue;
                    int influence = mesh.boneIndices[source * 4 + k];
                    if (influence >= skeleton.influences.Length) continue;
                    int joint = skeleton.influences[influence];
                    if (joint >= skeleton.names.Length) continue;
                    if (!slots.TryGetValue(joint, out int slot))
                    {
                        slot = usedJoints.Count;
                        usedJoints.Add(joint);
                        slots[joint] = slot;
                    }
                    ids[used] = slot;
                    weights[used] = weight;
                    used++;
                }
                if (used == 0)
                {
                    if (!slots.TryGetValue(0, out int slot))
                    {
                        slot = usedJoints.Count;
                        usedJoints.Add(0);
                        slots[0] = slot;
                    }
                    ids[0] = slot;
                    weights[0] = 1f;
                    used = 1;
                }
                boneWeights[v] = ToBoneWeight(ids, weights, used);
            }

            if (ReverseWinding)
            {
                for (int i = 0; i + 2 < triangles.Length; i += 3)
                {
                    int swap = triangles[i + 1];
                    triangles[i + 1] = triangles[i + 2];
                    triangles[i + 2] = swap;
                }
            }

            Mesh unityMesh = new Mesh { name = "LeagueSkin_" + definition.folder + "_" + part.name };
            if (count > 65000) unityMesh.indexFormat = IndexFormat.UInt32;
            unityMesh.vertices = vertices;
            unityMesh.normals = normals;
            unityMesh.uv = uvs;
            unityMesh.boneWeights = boneWeights;
            unityMesh.triangles = triangles;
            unityMesh.RecalculateBounds();
            return unityMesh;
        }

        private static void CheckAlignment(LeagueSkinDefinition definition, Dictionary<string, Transform> bodyBones, Dictionary<Transform, Matrix4x4> baseBindposes, Transform rendererTransform, Matrix4x4 leagueToRenderer)
        {
            const string probe = "R_Hand";
            if (!bodyBones.TryGetValue(probe, out Transform bone) || !baseSkeleton.index.ContainsKey(probe)) return;

            Vector3 predicted = (leagueToRenderer * baseSkeleton.rest[baseSkeleton.index[probe]]).GetColumn(3);
            Vector3 actual = baseBindposes.TryGetValue(bone, out Matrix4x4 bindpose) ? (Vector3)bindpose.inverse.GetColumn(3) : rendererTransform.InverseTransformPoint(bone.position);
            float error = Vector3.Distance(predicted, actual);
            if (error > 0.05f)
            {
                Debug.LogWarning("[Jinx LeagueSkins] " + definition.folder + ": League skeleton does not line up with Jinx's rig (" + probe + " off by " + error + "). predicted " + predicted + " actual " + actual);
            }
        }

        private static Mesh BuildPartMesh(LeagueSkinDefinition definition, LeagueMeshPart part, LeagueMesh mesh, LeagueSkeleton skeleton, Transform[] jointTargets, Matrix4x4[] jointMatrices, bool[] jointIsExtra, BaseWeightSource weightSource, List<Transform> bones)
        {
            Dictionary<Transform, int> slots = new Dictionary<Transform, int>();
            Dictionary<int, int> localIndex = new Dictionary<int, int>();
            List<int> sourceVertices = new List<int>();
            bool relative = false;
            bool insideRange = true;
            bool belowCount = true;
            for (int i = 0; i < part.indexCount; i++)
            {
                int index = mesh.indices[part.indexStart + i];
                if (index < part.vertexStart || index >= part.vertexStart + part.vertexCount) insideRange = false;
                if (index >= part.vertexCount) belowCount = false;
            }
            if (!insideRange && belowCount) relative = true;

            int[] triangles = new int[part.indexCount];
            for (int i = 0; i < part.indexCount; i++)
            {
                int global = mesh.indices[part.indexStart + i] + (relative ? part.vertexStart : 0);
                if (!localIndex.TryGetValue(global, out int local))
                {
                    local = sourceVertices.Count;
                    sourceVertices.Add(global);
                    localIndex[global] = local;
                }
                triangles[i] = local;
            }
            int count = sourceVertices.Count;
            Vector3[] vertices = new Vector3[count];
            Vector3[] normals = new Vector3[count];
            Vector2[] uvs = new Vector2[count];
            BoneWeight[] boneWeights = new BoneWeight[count];

            List<Transform> weightBones = new List<Transform>(8);
            List<float> weightValues = new List<float>(8);
            int[] slotIds = new int[4];
            float[] slotWeights = new float[4];

            for (int v = 0; v < count; v++)
            {
                int source = sourceVertices[v];
                Vector3 position = mesh.positions[source];
                Vector3 normal = mesh.normals[source];
                Vector3 outPosition = Vector3.zero;
                Vector3 outNormal = Vector3.zero;
                float total = 0f;
                float extraWeight = 0f;
                weightBones.Clear();
                weightValues.Clear();

                for (int k = 0; k < 4; k++)
                {
                    float weight = mesh.weights[source * 4 + k];
                    if (weight <= 0f) continue;
                    int influence = mesh.boneIndices[source * 4 + k];
                    if (influence >= skeleton.influences.Length) continue;
                    int joint = skeleton.influences[influence];
                    if (joint >= jointMatrices.Length) continue;

                    Matrix4x4 matrix = jointMatrices[joint];
                    outPosition += weight * matrix.MultiplyPoint3x4(position);
                    outNormal += weight * matrix.MultiplyVector(normal);
                    total += weight;

                    if (jointIsExtra[joint] && weightSource != null) extraWeight += weight;
                    else AddWeight(weightBones, weightValues, jointTargets[joint], weight);
                }

                if (total <= 0f)
                {
                    outPosition = jointMatrices[0].MultiplyPoint3x4(position);
                    outNormal = jointMatrices[0].MultiplyVector(normal);
                    total = 1f;
                    AddWeight(weightBones, weightValues, jointTargets[0], 1f);
                }

                Vector3 finalPosition = outPosition / total;
                if (extraWeight > 0f)
                {
                    int nearest = weightSource.Nearest(finalPosition);
                    if (nearest >= 0)
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            Transform bone = weightSource.bones[nearest * 4 + k];
                            float weight = weightSource.weights[nearest * 4 + k];
                            if (bone && weight > 0f) AddWeight(weightBones, weightValues, bone, extraWeight * weight);
                        }
                    }
                }
                if (weightBones.Count == 0) AddWeight(weightBones, weightValues, jointTargets[0], 1f);

                int used = 0;
                foreach (int i in Enumerable.Range(0, weightBones.Count).OrderByDescending(i => weightValues[i]).Take(4))
                {
                    Transform target = weightBones[i];
                    if (!slots.TryGetValue(target, out int slot))
                    {
                        slot = bones.Count;
                        bones.Add(target);
                        slots[target] = slot;
                    }
                    slotIds[used] = slot;
                    slotWeights[used] = weightValues[i];
                    used++;
                }

                vertices[v] = finalPosition;
                normals[v] = outNormal.normalized;
                uvs[v] = mesh.uvs[source];
                boneWeights[v] = ToBoneWeight(slotIds, slotWeights, used);
            }

            if (ReverseWinding)
            {
                for (int i = 0; i + 2 < triangles.Length; i += 3)
                {
                    int swap = triangles[i + 1];
                    triangles[i + 1] = triangles[i + 2];
                    triangles[i + 2] = swap;
                }
            }

            Mesh unityMesh = new Mesh { name = "LeagueSkin_" + definition.folder + "_" + part.name };
            if (count > 65000) unityMesh.indexFormat = IndexFormat.UInt32;
            unityMesh.vertices = vertices;
            unityMesh.normals = normals;
            unityMesh.uv = uvs;
            unityMesh.boneWeights = boneWeights;
            unityMesh.triangles = triangles;
            unityMesh.RecalculateBounds();
            return unityMesh;
        }

        private static void AddWeight(List<Transform> bones, List<float> weights, Transform bone, float weight)
        {
            int index = bones.IndexOf(bone);
            if (index >= 0)
            {
                weights[index] += weight;
            }
            else
            {
                bones.Add(bone);
                weights.Add(weight);
            }
        }

        private static BoneWeight ToBoneWeight(int[] ids, float[] weights, int used)
        {
            int[] order = Enumerable.Range(0, used).OrderByDescending(i => weights[i]).ToArray();
            float sum = 0f;
            foreach (int i in order) sum += weights[i];
            if (sum <= 0f) sum = 1f;

            BoneWeight result = new BoneWeight();
            if (order.Length > 0) { result.boneIndex0 = ids[order[0]]; result.weight0 = weights[order[0]] / sum; }
            if (order.Length > 1) { result.boneIndex1 = ids[order[1]]; result.weight1 = weights[order[1]] / sum; }
            if (order.Length > 2) { result.boneIndex2 = ids[order[2]]; result.weight2 = weights[order[2]] / sum; }
            if (order.Length > 3) { result.boneIndex3 = ids[order[3]]; result.weight3 = weights[order[3]] / sum; }
            return result;
        }

        private static Material GetMaterial(LeagueSkinDefinition definition, string partName, Material template, Dictionary<string, Material> cache)
        {
            string file = definition.partTextures.TryGetValue(partName, out string mapped) ? mapped : definition.defaultTexture;
            if (cache.TryGetValue(file, out Material cached)) return cached;

            byte[] bytes = LeagueResources.Load(definition.folder, file);
            if (bytes == null && file != definition.defaultTexture)
            {
                Debug.LogWarning("[Jinx LeagueSkins] Missing texture " + file + " for " + definition.folder + "/" + partName + ", using " + definition.defaultTexture);
                file = definition.defaultTexture;
                if (cache.TryGetValue(file, out cached)) return cached;
                bytes = LeagueResources.Load(definition.folder, file);
            }

            Texture2D texture = LeagueTextures.Load(bytes, file, out bool hasAlpha);
            if (!texture) Debug.LogError("[Jinx LeagueSkins] Could not load texture " + definition.folder + "/" + file);

            // Jinx's own material is a Poiyomi toon material (dontHotpoo), so opaque League textures reuse it to match her look.
            // Textures with real alpha (e.g. PROJECT hair cards) use HGStandard, whose cutout is reliable at runtime.
            bool useTemplate = template && !hasAlpha;
            Material material = useTemplate ? new Material(template) : new Material(Assets.hotpoo);
            material.name = "LeagueSkin_" + definition.folder + "_" + System.IO.Path.GetFileNameWithoutExtension(file);
            material.mainTexture = texture;
            if (material.HasProperty("_NormalTex")) material.SetTexture("_NormalTex", null);
            if (material.HasProperty("_EmTex")) material.SetTexture("_EmTex", null);
            if (material.HasProperty("_EmColor")) material.SetColor("_EmColor", Color.black);
            if (!useTemplate && material.HasProperty("_Color")) material.SetColor("_Color", Color.white);
            if (hasAlpha)
            {
                material.EnableKeyword("CUTOUT");
                if (material.HasProperty("_EnableCutout")) material.SetFloat("_EnableCutout", 1f);
                if (material.HasProperty("_Cull")) material.SetFloat("_Cull", 0f);
            }

            cache[file] = material;
            return material;
        }

        private static GameObject CreatePart(string objectName, Transform template, Mesh mesh, Material material, Transform[] bones, Transform rootBone, Bounds localBounds)
        {
            GameObject part = new GameObject(objectName);
            part.transform.SetParent(template.parent, false);
            part.transform.localPosition = template.localPosition;
            part.transform.localRotation = template.localRotation;
            part.transform.localScale = template.localScale;
            part.layer = template.gameObject.layer;

            SkinnedMeshRenderer renderer = part.AddComponent<SkinnedMeshRenderer>();
            renderer.sharedMesh = mesh;
            renderer.sharedMaterial = material;
            renderer.bones = bones;
            renderer.rootBone = rootBone;
            renderer.localBounds = localBounds;
            renderer.updateWhenOffscreen = false;

            part.SetActive(false);
            return part;
        }
    }

    internal class BaseWeightSource
    {
        internal Vector3[] positions;
        internal Transform[] bones;
        internal float[] weights;
        private float cellSize;
        private readonly Dictionary<long, List<int>> grid = new Dictionary<long, List<int>>();
        private const int MaxRings = 30;

        internal static BaseWeightSource Create(LeagueSkeleton baseSkeleton, GameObject bodyModel, SkinnedMeshRenderer bodyRenderer)
        {
            byte[] bytes = LeagueResources.Load(LeagueSkinBuilder.BaseFolder, LeagueSkinBuilder.BaseMeshFile);
            if (bytes == null)
            {
                Debug.LogWarning("[Jinx LeagueSkins] Missing embedded base mesh, extra bones will stay rigid");
                return null;
            }

            LeagueMesh baseMesh = LeagueMesh.Read(bytes);
            Dictionary<string, Transform> bodyBones = LeagueSkinBuilder.Bones(bodyModel);
            Matrix4x4 leagueToRenderer = LeagueSkinBuilder.LeagueToRenderer(bodyModel, bodyRenderer);
            HashSet<string> excluded = new HashSet<string>(LeagueSkinBuilder.BaseExcludedParts, StringComparer.OrdinalIgnoreCase);

            List<int> vertexList = new List<int>();
            foreach (LeagueMeshPart part in baseMesh.parts)
            {
                if (excluded.Contains(part.name)) continue;
                for (int v = part.vertexStart; v < part.vertexStart + part.vertexCount && v < baseMesh.positions.Length; v++) vertexList.Add(v);
            }

            BaseWeightSource result = new BaseWeightSource
            {
                positions = new Vector3[vertexList.Count],
                bones = new Transform[vertexList.Count * 4],
                weights = new float[vertexList.Count * 4]
            };

            Bounds bounds = new Bounds();
            for (int i = 0; i < vertexList.Count; i++)
            {
                int source = vertexList[i];
                Vector3 position = leagueToRenderer.MultiplyPoint3x4(baseMesh.positions[source]);
                result.positions[i] = position;
                if (i == 0) bounds = new Bounds(position, Vector3.zero);
                else bounds.Encapsulate(position);

                for (int k = 0; k < 4; k++)
                {
                    float weight = baseMesh.weights[source * 4 + k];
                    int influence = baseMesh.boneIndices[source * 4 + k];
                    if (weight <= 0f || influence >= baseSkeleton.influences.Length) continue;
                    int joint = baseSkeleton.influences[influence];
                    if (joint >= baseSkeleton.names.Length) continue;
                    string jointName = baseSkeleton.names[joint];
                    if (IsIgnored(jointName)) continue;
                    if (bodyBones.TryGetValue(jointName, out Transform bone))
                    {
                        result.bones[i * 4 + k] = bone;
                        result.weights[i * 4 + k] = weight;
                    }
                }

                float kept = 0f;
                for (int k = 0; k < 4; k++) kept += result.weights[i * 4 + k];
                if (kept >= 0.5f)
                {
                    for (int k = 0; k < 4; k++) result.weights[i * 4 + k] /= kept;
                }
                else
                {
                    for (int k = 0; k < 4; k++)
                    {
                        result.bones[i * 4 + k] = null;
                        result.weights[i * 4 + k] = 0f;
                    }
                }
            }

            result.cellSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) / 40f;
            if (result.cellSize <= 0f) result.cellSize = 0.05f;
            for (int i = 0; i < result.positions.Length; i++)
            {
                if (result.weights[i * 4] <= 0f && result.weights[i * 4 + 1] <= 0f && result.weights[i * 4 + 2] <= 0f && result.weights[i * 4 + 3] <= 0f) continue;
                long key = result.Key(result.Cell(result.positions[i].x), result.Cell(result.positions[i].y), result.Cell(result.positions[i].z));
                if (!result.grid.TryGetValue(key, out List<int> list))
                {
                    list = new List<int>();
                    result.grid[key] = list;
                }
                list.Add(i);
            }

            return result;
        }

        private static bool IsIgnored(string jointName)
        {
            foreach (string keyword in LeagueSkinBuilder.TransferIgnoredBoneKeywords)
            {
                if (jointName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }
            return false;
        }

        private int Cell(float value)
        {
            return Mathf.FloorToInt(value / this.cellSize);
        }

        private long Key(int x, int y, int z)
        {
            return ((long)(x & 0x1FFFFF) << 42) | ((long)(y & 0x1FFFFF) << 21) | (long)(z & 0x1FFFFF);
        }

        internal int Nearest(Vector3 point)
        {
            int cx = this.Cell(point.x);
            int cy = this.Cell(point.y);
            int cz = this.Cell(point.z);
            int best = -1;
            float bestDistance = float.MaxValue;

            for (int ring = 0; ring <= MaxRings; ring++)
            {
                for (int dx = -ring; dx <= ring; dx++)
                {
                    for (int dy = -ring; dy <= ring; dy++)
                    {
                        for (int dz = -ring; dz <= ring; dz++)
                        {
                            if (Math.Max(Math.Abs(dx), Math.Max(Math.Abs(dy), Math.Abs(dz))) != ring) continue;
                            if (!this.grid.TryGetValue(this.Key(cx + dx, cy + dy, cz + dz), out List<int> list)) continue;
                            foreach (int i in list)
                            {
                                float distance = (this.positions[i] - point).sqrMagnitude;
                                if (distance < bestDistance)
                                {
                                    bestDistance = distance;
                                    best = i;
                                }
                            }
                        }
                    }
                }

                if (best >= 0)
                {
                    float reach = ring * this.cellSize;
                    if (bestDistance <= reach * reach) break;
                }
            }

            return best;
        }
    }
}
