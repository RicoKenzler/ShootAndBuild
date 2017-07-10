using System.Collections.Generic;
using UnityEngine;

namespace SAB
{
	public class Buffable : MonoBehaviour
	{
		private List<GameObject> buffVisuals;

		void Awake()
		{
			buffs = new List<BuffData>();
			buffVisuals = new List<GameObject>();
		}

		//----------------------------------------------------------------------

		void Update()
		{
			foreach (BuffData buff in buffs)
			{
				buff.Update();
			}

			foreach (BuffData buff in buffs)
			{
				if (buff.IsFinished)
				{
					buff.OnRemove(gameObject);
				}
			}
			buffs.RemoveAll(x => x.IsFinished);
		}

		//----------------------------------------------------------------------

		void OnDisable()
		{
			buffs.ForEach(x => x.OnRemove(gameObject));
			buffs.Clear();
		}

		//----------------------------------------------------------------------

		public List<BuffData> buffs
		{
			get; private set;
		}

		//----------------------------------------------------------------------

		public void AddBuff(BuffData buff)
		{
			BuffData result = buffs.Find(x => x.type == buff.type);
			if (result == null)
			{
				BuffData newInstance = Instantiate<BuffData>(buff);
				buffs.Add(newInstance);

				newInstance.OnAdd(gameObject);
			}
			else
			{
				result.Reset();
			}
		}

		//----------------------------------------------------------------------

		public void AddBuffs(List<BuffData> buffs)
		{
			foreach (BuffData buff in buffs)
			{
				AddBuff(buff);
			}
		}

		//----------------------------------------------------------------------

		public float GetSpeedMultiplier()
		{
			float total = 0;
			foreach (BuffData buff in buffs)
			{
				total += buff.speedMultiplier;
			}

			return total + 1.0f;
		}

		//----------------------------------------------------------------------

		public float GetDamageMultiplier()
		{
			float total = 0;
			foreach (BuffData buff in buffs)
			{
				total += buff.damageMultiplier;
			}

			return total + 1.0f;
		}
	}
}
