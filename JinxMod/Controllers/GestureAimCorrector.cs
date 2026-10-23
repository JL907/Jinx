using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace JinxMod.Controller
{
    [DefaultExecutionOrder(9000)]
    public class GestureAimCorrector : MonoBehaviour
    {
        public string layerName = "Gesture, Override";
        public float maxYaw = 150f;
        public float blendSpeed = 10f;

        private class Weapon
        {
            internal int stateHash;
            internal Transform back;
            internal Transform muzzle;
            internal Transform detached;
        }

        private Animator animator;
        private CharacterBody body;
        private int layer = -1;
        private Transform[] spine;
        private readonly List<Weapon> weapons = new List<Weapon>();
        private float weight;
        private float yaw;
        private Weapon lastWeapon;

        private void Start()
        {
            animator = GetComponent<Animator>();
            CharacterModel characterModel = GetComponent<CharacterModel>();
            body = characterModel ? characterModel.body : null;
            if (animator) layer = animator.GetLayerIndex(layerName);

            Dictionary<string, Transform> bones = new Dictionary<string, Transform>();
            foreach (Transform t in GetComponentsInChildren<Transform>(true))
            {
                if (!bones.ContainsKey(t.name)) bones[t.name] = t;
            }
            Transform Bone(string n) => bones.TryGetValue(n, out Transform found) ? found : null;

            spine = new[] { Bone("Spine1"), Bone("Spine2"), Bone("Spine3") };
            AddWeapon("fishbonesattack", Bone("Rocket_Launcher_End"), Bone("Rocket_Launcher_Front"), null);
            AddWeapon("powpowattack", Bone("Minigun"), Bone("Minigun_Barrel1"), Bone("Minigun"));
            AddWeapon("zap", Bone("Pistol"), Bone("Cstm_Buffbone_Pistol"), Bone("Pistol"));
        }

        private void AddWeapon(string state, Transform back, Transform muzzle, Transform detached)
        {
            if (!back || !muzzle) return;
            weapons.Add(new Weapon { stateHash = Animator.StringToHash(state), back = back, muzzle = muzzle, detached = detached });
        }

        private void LateUpdate()
        {
            if (!animator || layer < 0 || !body || !body.inputBank || spine[0] == null) return;

            Weapon active = null;
            if (animator.GetLayerWeight(layer) > 0f)
            {
                AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(layer);
                foreach (Weapon weapon in weapons)
                {
                    if (info.shortNameHash == weapon.stateHash)
                    {
                        active = weapon;
                        break;
                    }
                }
            }

            weight = Mathf.MoveTowards(weight, active != null ? 1f : 0f, blendSpeed * Time.deltaTime);
            if (weight <= 0f) return;

            if (active != null)
            {
                lastWeapon = active;
                Vector3 muzzle = Vector3.ProjectOnPlane(active.muzzle.position - active.back.position, Vector3.up);
                Vector3 aim = Vector3.ProjectOnPlane(body.inputBank.aimDirection, Vector3.up);
                if (muzzle.sqrMagnitude > 1e-6f && aim.sqrMagnitude > 1e-6f)
                {
                    yaw = Mathf.Clamp(Vector3.SignedAngle(muzzle, aim, Vector3.up), -maxYaw, maxYaw);
                }
            }

            float applied = yaw * weight;
            if (Mathf.Abs(applied) < 0.01f) return;

            Vector3 pivot = spine[0].position;
            int count = 0;
            foreach (Transform s in spine) if (s) count++;
            Quaternion step = Quaternion.AngleAxis(applied / count, Vector3.up);
            foreach (Transform s in spine)
            {
                if (s) s.rotation = step * s.rotation;
            }

            if (lastWeapon != null && lastWeapon.detached)
            {
                Quaternion full = Quaternion.AngleAxis(applied, Vector3.up);
                Transform w = lastWeapon.detached;
                w.position = pivot + full * (w.position - pivot);
                w.rotation = full * w.rotation;
            }
        }
    }
}
