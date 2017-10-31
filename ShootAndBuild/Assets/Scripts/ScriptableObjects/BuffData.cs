using UnityEngine;
using UnityEngine.Serialization;

namespace SAB
{
	public enum BuffType
	{
		Slow,
		DamageIncrease
	}

	///////////////////////////////////////////////////////////////////////////

	[CreateAssetMenu(menuName = "Custom/BuffData", fileName = "BuffData")]
	public class BuffData : ScriptableObject
	{
		[FormerlySerializedAs("type")]
		[SerializeField] private BuffType m_Type = BuffType.Slow;

		[FormerlySerializedAs("duration")]
		[SerializeField] private float m_Duration = 0;

		[FormerlySerializedAs("damageMultiplier")]
		[SerializeField] private float m_DamageMultiplier = 0;

		[FormerlySerializedAs("speedMultiplier")]
		[SerializeField] private float m_SpeedMultiplier = 0;

		[FormerlySerializedAs("visual")]
		[SerializeField] private GameObject m_Visual = null;

		///////////////////////////////////////////////////////////////////////////

		public BuffType type				{ get { return m_Type; } }
		public float	speedMultiplier		{ get { return m_SpeedMultiplier; } }
		public float	damageMultiplier	{ get { return m_DamageMultiplier; } }

		///////////////////////////////////////////////////////////////////////////

		private float livingTime = 0;
		private GameObject visualInstance = null;

		///////////////////////////////////////////////////////////////////////////

		public void Update()
		{
			livingTime += Time.deltaTime;
			livingTime = Mathf.Clamp(livingTime, 0, m_Duration);
		}

		///////////////////////////////////////////////////////////////////////////

		public float Progress
		{
			get { return livingTime / m_Duration; }
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
			return (m_DamageMultiplier > 0 || m_SpeedMultiplier > 0);
		}

		///////////////////////////////////////////////////////////////////////////

		public void OnAdd(GameObject parent)
		{
			visualInstance = Instantiate(m_Visual, parent.transform);
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
