using UnityEngine;

namespace SAB
{

    public class Thrower : MonoBehaviour
    {
        [SerializeField] private GameObject m_ProjectilePrefab;
        [SerializeField] private AudioData	m_ThrowSound;
		[SerializeField] private float		m_ThrowStrength = 10;

		///////////////////////////////////////////////////////////////////////////

        public void Throw(ThrowableData throwableData)
        {
            GameObject projectileContainer = Projectile.GetOrCreateProjectilesContainer();

            GameObject instance = Instantiate(m_ProjectilePrefab, projectileContainer.transform);
            instance.transform.position = transform.position + transform.forward * 0.5f;
            instance.GetComponent<Grenade>().owner = this;
			instance.GetComponent<Grenade>().ownerFaction = GetComponent<Attackable>().faction;

            Rigidbody body = instance.GetComponent<Rigidbody>();
            body.velocity = transform.forward * m_ThrowStrength;

            AudioManager.instance.PlayAudio(m_ThrowSound, instance.transform.position);

			// Player Counter
			InputController inputController = GetComponent<InputController>();

			if (inputController)
			{
				CounterManager.instance.AddToCounters(inputController.playerID, CounterType.ItemsUsed, 1, throwableData.gameObject.ToString());
			}
        }
    }
}