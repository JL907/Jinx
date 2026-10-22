using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace JinxMod.Controllers
{
	public class RocketJumpFallProtection : MonoBehaviour
	{
		private bool isProtected;

		public static void Grant(CharacterBody body)
		{
			if (!NetworkServer.active || !body) return;
			var protection = body.GetComponent<RocketJumpFallProtection>();
			if (!protection) protection = body.gameObject.AddComponent<RocketJumpFallProtection>();
			protection.isProtected = true;
		}

		private static bool IsProtected(CharacterBody body)
		{
			if (!body) return false;
			var protection = body.GetComponent<RocketJumpFallProtection>();
			return protection && protection.isProtected;
		}

		private static void Clear(CharacterBody body)
		{
			if (!body) return;
			var protection = body.GetComponent<RocketJumpFallProtection>();
			if (protection) protection.isProtected = false;
		}

		internal static void Hook()
		{
			On.RoR2.HealthComponent.TakeDamage += (orig, self, damageInfo) =>
			{
				if (NetworkServer.active && damageInfo != null && self
					&& (damageInfo.damageType.damageType & DamageType.FallDamage) != 0
					&& IsProtected(self.body))
				{
					return;
				}
				orig(self, damageInfo);
			};

			On.RoR2.GlobalEventManager.OnCharacterHitGroundServer += (orig, self, characterBody, hitGroundInfo) =>
			{
				orig(self, characterBody, hitGroundInfo);
				Clear(characterBody);
			};
		}
	}
}
