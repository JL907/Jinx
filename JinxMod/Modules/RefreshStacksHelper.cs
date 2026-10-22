using RoR2;

namespace JinxMod
{
	public static class RefreshStacksHelper
	{
		public static void RefreshStacks(this CharacterBody body, BuffIndex index, float duration, int maxStacks, bool setToMax = false)
		{
			var amount = 0;
			for (var i = 0; i < body.timedBuffs.Count; i++)
			{

				var timedBuff = body.timedBuffs[i];
				if (timedBuff.buffIndex != index) continue;
				amount++;
				if (timedBuff.timer < duration * amount)
				{
					timedBuff.timer = duration * amount;
				}
			}
			if (setToMax)
            {
				for (var i = amount; i < maxStacks; i++)
				{
					body.AddTimedBuff(BuffCatalog.GetBuffDef(index), duration * (i + 1), maxStacks);
				}

				BuffTimerSync.Send(body, index);
				return;
			}

			if (amount < maxStacks)
			{
				body.AddTimedBuff(BuffCatalog.GetBuffDef(index), duration * (amount + 1), maxStacks);
			}

			BuffTimerSync.Send(body, index);
		}
	}
}