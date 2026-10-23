using JinxMod.SkillStates;
using JinxMod.SkillStates.BaseStates;

namespace JinxMod.Modules
{
    public static class States
    {
        internal static void RegisterStates()
        {
            Modules.Content.AddEntityState(typeof(Switcharoo));
            Modules.Content.AddEntityState(typeof(PowPow));
            Modules.Content.AddEntityState(typeof(FishBones));
            Modules.Content.AddEntityState(typeof(MegaRocket));
            Modules.Content.AddEntityState(typeof(DeathState));
        }
    }
}