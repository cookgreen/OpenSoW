using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenRA.FileSystem;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.DarkColony.Traits
{
	class TransferToOppsiteWhenDiedInfo : ITraitInfo
	{
		public object Create(ActorInitializer init) { return new TransferToOppsiteWhenDied(init.World, this); }
	}

	class TransferToOppsiteWhenDied : INotifyKilled, INotifyDamage
	{
		private World world;
		private readonly TransferToOppsiteWhenDiedInfo info;
		private Dictionary<Player, int> playerDamageDic;

		public TransferToOppsiteWhenDied(World world, TransferToOppsiteWhenDiedInfo info)
		{
			this.world = world;
			this.info = info;
			playerDamageDic = new Dictionary<Player, int>();
		}

		public void Killed(Actor self, AttackInfo e)
		{
			if (self.Owner.InternalName != "Netrual")
			{
				var killer = e.Attacker;
				self.ChangeOwner(killer.Owner);
			}
			else
			{
				var result = (from pair in playerDamageDic
							 orderby pair.Value descending
							 select pair).ToList();
				var maxDamagePlayer = result.First().Key;
				self.ChangeOwner(maxDamagePlayer);
			}
		}

		public void Damaged(Actor self, AttackInfo e)
		{
			if (self.Owner.InternalName == "Netural")
			{
				if (playerDamageDic.ContainsKey(e.Attacker.Owner))
				{
					playerDamageDic[e.Attacker.Owner] += e.Damage.Value;
				}
				else
				{
					playerDamageDic.Add(e.Attacker.Owner, e.Damage.Value);
				}
			}
		}
	}
}
