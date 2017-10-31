using UnityEngine;
using System.Collections.Generic;

namespace SAB
{
    public class CameraController : MonoBehaviour
    {
        [Range(0, 20)]
        [SerializeField] private float m_HeightSlope		= 2.0f;        //< height-gain per player-bounding-sphere-radius

        [Range(0, 200)]
        [SerializeField] private float m_MinimumHeight		= 15.0f;

        [Range(0, 0.9999999f)]
        [SerializeField] private float m_InertiaMovement	= 0.93f;

        [Range(0, 0.9999999f)]
        [SerializeField] private float m_InertiaZoom		= 0.98f;

        [Range(0, 20)]
        [SerializeField] private float m_MinimumRadius		= 2.00f;     //< if players are nearer than this, camera height wont change

        [Range(0, 1)]
        [SerializeField] private float m_RotZ				= 0.1f;

        [SerializeField] private AudioListener m_AudioListener;

		///////////////////////////////////////////////////////////////////////////

        private Vector3 m_LastPlayerSphereCenter = new Vector3(0.0f, 0.0f, 0.0f);
        private float	m_LastPlayerSphereRadius = 1.0f;

		private CameraShakes m_CameraShakes = new CameraShakes();

		// TODO: Move this into editor
		[SerializeField] private CameraShakeParams m_TestShakeParams = new CameraShakeParams(1.0f, 0.1f, 1.0f);

		///////////////////////////////////////////////////////////////////////////

		public CameraShakeParams testShakeParams { get { return m_TestShakeParams; } }

		///////////////////////////////////////////////////////////////////////////

        void Awake()
        {
            Instance = this;
        }

		///////////////////////////////////////////////////////////////////////////

        void Start()
        {
			Vector3 terrainCenter = TerrainManager.Instance.GetTerrainCenter3D(); 

			m_LastPlayerSphereCenter = terrainCenter + new Vector3(0.0f, 10.0f, 0.0f);

            GetPlayerBoundingSphere(out m_LastPlayerSphereCenter, out m_LastPlayerSphereRadius);

			// Init audio Listener
			Update();
        }

		///////////////////////////////////////////////////////////////////////////

        void GetPlayerBoundingSphere(out Vector3 center, out float radius)
        {
            List<InputController> allPlayers = PlayerManager.instance.allAlivePlayers;

            if (allPlayers.Count == 0)
            {
                center = m_LastPlayerSphereCenter;
                radius = m_LastPlayerSphereRadius;

                return;
            }

            Bounds playerBB = new Bounds(allPlayers[0].transform.position, Vector3.zero);

            foreach (InputController player in allPlayers)
            {
                playerBB.Encapsulate(player.transform.position);
            }

            radius = Mathf.Max(playerBB.extents.x, playerBB.extents.z);
            center = playerBB.center;

            radius = Mathf.Max(m_MinimumRadius, radius);
        }

		///////////////////////////////////////////////////////////////////////////

        void Update()
        {
            Vector3 playerSphereCenter;
            float playerSphereRadius;
            GetPlayerBoundingSphere(out playerSphereCenter, out playerSphereRadius);

            playerSphereCenter = Vector3.Lerp(playerSphereCenter, m_LastPlayerSphereCenter, m_InertiaMovement);
            playerSphereRadius = Mathf.Lerp(playerSphereRadius, m_LastPlayerSphereRadius, m_InertiaZoom);

            m_LastPlayerSphereRadius = playerSphereRadius;
            m_LastPlayerSphereCenter = playerSphereCenter;

            Vector3 heighOffsetDirection = new Vector3(0.0f, 1.0f, -m_RotZ);
            float heightOffsetAmount = (m_MinimumHeight + (playerSphereRadius * m_HeightSlope));

            Vector3 newCameraPos	= playerSphereCenter + heightOffsetAmount * heighOffsetDirection;
			Vector3 newLookatPoint	= playerSphereCenter;

			m_CameraShakes.TickOffset();
			Vector3 shakeOffset = GetCameraShakeOffset();

			newCameraPos	+= shakeOffset;
			newLookatPoint	+= shakeOffset;

            transform.position = newCameraPos;
            transform.LookAt(newLookatPoint);

            m_AudioListener.transform.position = playerSphereCenter;
            m_AudioListener.transform.rotation = Quaternion.AngleAxis(180.0f, new Vector3(0.0f, 1.0f, 0.0f)) * transform.rotation;
        }

		///////////////////////////////////////////////////////////////////////////

		public Vector3 GetListenerPosition()
		{
			return m_AudioListener.transform.position;
		}

		///////////////////////////////////////////////////////////////////////////

		public float GetListenerWidth()
		{
			return m_LastPlayerSphereRadius * 1.5f;
		}

		///////////////////////////////////////////////////////////////////////////

		public void AddCameraShake(CameraShakeParams shakeParams)
		{
			if (shakeParams.Strength <= 0 || shakeParams.Duration <= 0)
			{
				return;
			}

			m_CameraShakes.StartNewShake(shakeParams);
		}

		///////////////////////////////////////////////////////////////////////////

        public static CameraController Instance
        {
            get; private set;
        }

		///////////////////////////////////////////////////////////////////////////

		public Vector3 GetCameraShakeOffset()
		{
			return m_CameraShakes.OffsetSum.To3D(0.0f);
		}
    }

	///////////////////////////////////////////////////////////////////////////

	[System.Serializable]
	public struct CameraShakeParams
	{
		[Range(0.00f, 5)]
		public float Strength;

		[Range(0, 0.999f)]
		public float Smoothness;

		[Range(0.00f, 10.0f)]
		public float Duration;

		public CameraShakeParams(float strength = 0.5f, float smoothness = 0.1f, float duration = 0.2f)
		{
			Strength	= strength;
			Smoothness	= smoothness;
			Duration	= duration;
		}
	}

	///////////////////////////////////////////////////////////////////////////

	class CameraShakes
	{
		private List<CameraShake>	ActiveShakes	= new List<CameraShake>();
		public Vector2				OffsetSum		= Vector2.zero;

		public void StartNewShake(CameraShakeParams shakeParams)
		{
			CameraShake newShake = new CameraShake(shakeParams);

			ActiveShakes.Add(newShake);
		}

		public void TickOffset()
		{
			OffsetSum = Vector2.zero;

			foreach (CameraShake shake in ActiveShakes)
			{
				shake.TickShake();
				OffsetSum += shake.Offset;
			}

			ActiveShakes.RemoveAll(shake => shake.DurationLeft <= 0.0f);
		}
	}

	///////////////////////////////////////////////////////////////////////////

	class CameraShake
	{
		public CameraShakeParams ShakeParams;
		
		public float	DurationLeft;
		public Vector2	Offset;

		public CameraShake(CameraShakeParams shakeParams)
		{
			ShakeParams		= shakeParams;
			DurationLeft	= shakeParams.Duration;
			Offset			= (Random.insideUnitCircle * shakeParams.Strength) * (1.0f - ShakeParams.Smoothness);
		}

		public void TickShake()
		{
			DurationLeft -= Time.deltaTime;

			if (Time.deltaTime == 0.0)
			{
				return;
			}

			if (DurationLeft <= 0.0)
			{
				Offset			= Vector2.zero;
				DurationLeft	= 0.0f;
				return;
			}

			float timeLeftRelative = (DurationLeft / ShakeParams.Duration);

			Debug.Assert(timeLeftRelative <= 1 && timeLeftRelative >= 0);

			float strength = ShakeParams.Strength * timeLeftRelative;

			Vector2 newOffset = Random.insideUnitCircle * strength;

			Offset = Vector2.Lerp(newOffset, Offset, ShakeParams.Smoothness);
		}
	}
}