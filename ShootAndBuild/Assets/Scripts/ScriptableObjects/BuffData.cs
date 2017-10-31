using UnityEngine;

namespace SAB
{
	public enum BuffType
	{
		Slow,
		DamageIncrease
	}

	[CreateAssetMenu(menuName = "Custom/BuffData", fileName = "BuffData")]
	public class BuffData : ScriptableObject
	{
		public BuffType type = BuffType.Slow;
		public float duration = 0;
		public float damageMultiplier = 0;
		public float speedMultiplier = 0;
		public GameObject visual = null;

		private float livingTime = 0;
		private GameObject visualInstance = null;

		///////////////////////////////////////////////////////////////////////////

		public void Update()
		{
			livingTime += Time.deltaTime;
			livingTime = Mathf.Clamp(livingTime, 0, duration);
		}

		///////////////////////////////////////////////////////////////////////////

		public float Progress
		{
			get { return livingTime / duration; }
		}

		///////////////////////////////////////////////////////////////////////////

		public bool IsFinished
		{
			get { return Progress >= 1.0f; }
		}

		///////////////////////////////////////////////////////////////////////////

		public void Reset()
		{
			livingTime = 0;
		}

		///////////////////////////////////////////////////////////////////////////

		bool IsPositiveBuff()
		{
			return (damageMultiplier > 0 || speedMultiplier > 0);
		}

		public void OnAdd(GameObject parent)
		{
			visualInstance = Instantiate(visual, parent.transform);
			visualInstance.transform.localPosition = Vector3.zero;

			if (parent.GetComponent<InputController>())
			{
				MusicManager.instance.OnAddPlayerBuffCount(1, IsPositiveBuff());
			}
		}

		///////////////////////////////////////////////////////////////////////////

		public void OnRemove(GameObject parent)
		{
			Destroy(visualInstance);

			if (parent.GetComponent<InputController>())
			{
				MusicManager.instance.OnAddPlayerBuffCount(-1, IsPositiveBuff());
			}
		}

		///////////////////////////////////////////////////////////////////////////
	}
}
