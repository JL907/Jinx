using EntityStates;
using System;

namespace JinxMod.SkillStates.BaseStates
{
    public class BaseTimedSkillState : BaseSkillState
    {
        public static float TimedBaseDuration;

        public static float TimedBaseCastStartTime;
        public static float TimedBaseCastEndTime;

        protected float duration;
        protected float castStartTime;
        protected float castEndTime;
        protected bool hasFired;
        protected bool isFiring;
        protected bool hasExited;

        protected virtual void InitDurationValues(float baseDuration, float baseCastStartTime, float baseCastEndTime = 1)
        {
            TimedBaseDuration = baseDuration;
            TimedBaseCastStartTime = baseCastStartTime;
            TimedBaseCastEndTime = baseCastEndTime;

            duration = TimedBaseDuration / base.attackSpeedStat;
            castStartTime = baseCastStartTime * duration;
            castEndTime = baseCastEndTime * duration;
        }

        protected virtual void OnCastEnter() { }
        protected virtual void OnCastFixedUpdate() { }
        protected virtual void OnCastUpdate() { }
        protected virtual void OnCastExit() { }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if(!hasFired && fixedAge > castStartTime)
            {
                hasFired = true;
                OnCastEnter();
            }

            bool fireStarted = fixedAge >= castStartTime;
            bool fireEnded = fixedAge >= castEndTime;
            isFiring = false;

            if ((fireStarted && !fireEnded) || (fireStarted && fireEnded && !this.hasFired))
            {
                isFiring = true;
                OnCastFixedUpdate();
            }

            if(fireEnded && !hasExited)
            {
                hasExited = true;
                OnCastExit();
            }

            if(fixedAge > duration)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override void Update()
        {
            base.Update();
            if (isFiring)
            {
                OnCastUpdate();
            }
        }
    }

    public class ExampleTimedSkillState : BaseTimedSkillState
    {
        public static float SkillBaseDuration = 1.5f;
        public static float SkillStartTime = 0.2f;
        public static float SkillEndTime =  0.9f;

        public override void OnEnter()
        {
            base.OnEnter();

            InitDurationValues(SkillBaseDuration, SkillStartTime, SkillEndTime);
        }

        protected override void OnCastEnter()
        {
        }

        protected override void OnCastFixedUpdate()
        {
        }

        protected override void OnCastExit()
        {
        }
    }

    public class ExampleDelayedSkillState : BaseTimedSkillState
    {
        public static float SkillBaseDuration = 1.5f;
        public static float SkillStartTime = 0.2f;

        public override void OnEnter()
        {
            base.OnEnter();

            InitDurationValues(SkillBaseDuration, SkillStartTime);
        }

        protected override void OnCastEnter()
        {
        }
    }
}