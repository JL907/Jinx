using EntityStates;
using JinxMod.Controller;
using RoR2;
using UnityEngine;

namespace JinxMod.Modules.LeagueSkins
{
    [DefaultExecutionOrder(10000)]
    public class LeagueRigDriver : MonoBehaviour
    {
        public const int ModeDefault = 0;
        public const int ModeRocket = 1;
        public const int ModeZap = 2;

        public Transform[] joints;
        public Transform[] bones;
        public Quaternion[] rotationOffsets;
        public Vector3[] positionOffsets;
        public bool[] pinned;
        public Renderer[] watch;

        public int[] altJoint;
        public int[] altMode;
        public Transform[] altBones;
        public Quaternion[] altRotationOffsets;
        public Vector3[] altPositionOffsets;

        public Transform[] toggles;
        public int[] toggleModes;

        private CharacterBody body;
        private RocketController rocketController;
        private EntityStateMachine[] stateMachines;
        private bool searched;

        private int CurrentMode()
        {
            if (!searched)
            {
                searched = true;
                CharacterModel characterModel = GetComponentInParent<CharacterModel>();
                body = characterModel ? characterModel.body : null;
                if (body)
                {
                    rocketController = body.GetComponent<RocketController>();
                    stateMachines = body.GetComponents<EntityStateMachine>();
                }
            }
            if (!body) return ModeDefault;

            if (stateMachines != null)
            {
                for (int i = 0; i < stateMachines.Length; i++)
                {
                    EntityState state = stateMachines[i] ? stateMachines[i].state : null;
                    if (state is JinxMod.SkillStates.Zap) return ModeZap;
                }
            }
            return rocketController && rocketController.isRocket ? ModeRocket : ModeDefault;
        }

        private void LateUpdate()
        {
            if (watch == null || joints == null) return;

            bool visible = false;
            for (int i = 0; i < watch.Length; i++)
            {
                if (watch[i] && watch[i].gameObject.activeInHierarchy)
                {
                    visible = true;
                    break;
                }
            }
            if (!visible) return;

            int mode = CurrentMode();

            for (int i = 0; i < joints.Length; i++)
            {
                Transform joint = joints[i];
                if (!joint) continue;

                int a = FindAlt(i, mode);
                if (a >= 0)
                {
                    joint.position = altBones[a].TransformPoint(altPositionOffsets[a]);
                    joint.rotation = altBones[a].rotation * altRotationOffsets[a];
                    continue;
                }

                Transform bone = bones[i];
                if (!bone) continue;
                if (pinned[i]) joint.position = bone.TransformPoint(positionOffsets[i]);
                joint.rotation = bone.rotation * rotationOffsets[i];
            }

            if (toggles != null)
            {
                for (int t = 0; t < toggles.Length; t++)
                {
                    if (toggles[t]) toggles[t].localScale = toggleModes[t] == mode ? Vector3.one : Vector3.zero;
                }
            }
        }

        private int FindAlt(int joint, int mode)
        {
            if (altJoint == null) return -1;
            for (int a = 0; a < altJoint.Length; a++)
            {
                if (altJoint[a] == joint && altMode[a] == mode && altBones[a]) return a;
            }
            return -1;
        }
    }
}
