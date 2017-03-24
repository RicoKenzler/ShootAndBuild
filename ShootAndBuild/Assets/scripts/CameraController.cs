using UnityEngine;

public class CameraController : MonoBehaviour
{
	[Range(0, 20)]
    public float heightSlope        =  2.0f;		//< height-gain per player-bounding-sphere-radius

	[Range(0, 200)]
    public float minimumHeight      = 15.0f;		

	[Range(0,0.9999999f)]
    public float inertiaMovement    = 0.93f;	

	[Range(0,0.9999999f)]
    public float inertiaZoom        = 0.98f;

	[Range(0, 20)]
    public float minimumRadius      = 2.00f;		//< if players are nearer than this, camera height wont change

	[Range(0,1)]
	public float rotZ				=  0.1f;

    private Vector3 lastPlayerSphereCenter	= new Vector3(0.0f, 0.0f, 0.0f);
    private float lastPlayerSphereRadius	= 1.0f;


	void Start()
	{
        GetPlayerBoundingSphere(out lastPlayerSphereCenter, out lastPlayerSphereRadius);
	}
    
    void GetPlayerBoundingSphere(out Vector3 center, out float radius)
    {
        InputController[] allPlayers = FindObjectsOfType<InputController>();

        if (allPlayers.Length == 0)
        {
			center = lastPlayerSphereCenter;
			radius = lastPlayerSphereRadius;

            Debug.Assert(false, "No player found!");
            return;
        }

        Bounds playerBB = new Bounds(allPlayers[0].transform.position, Vector3.zero);

        foreach (InputController player in allPlayers)
        {
            playerBB.Encapsulate(player.transform.position);
        }

        radius = Mathf.Max(playerBB.extents.x, playerBB.extents.z);
		center = playerBB.center;

        radius = Mathf.Max(minimumRadius, radius);
    }

	void Update()
    {
        Vector3 playerSphereCenter;
        float   playerSphereRadius;
        GetPlayerBoundingSphere(out playerSphereCenter, out playerSphereRadius);

        playerSphereCenter = Vector3.Lerp(playerSphereCenter, lastPlayerSphereCenter, inertiaMovement);
        playerSphereRadius = Mathf.Lerp(  playerSphereRadius, lastPlayerSphereRadius, inertiaZoom);

        lastPlayerSphereRadius  = playerSphereRadius;
        lastPlayerSphereCenter  = playerSphereCenter;

        Vector3 heighOffsetDirection	= new Vector3(0.0f, 1.0f, -rotZ);
        float heightOffsetAmount		= (minimumHeight + (playerSphereRadius * heightSlope));

        Vector3 newCameraPos = playerSphereCenter + heightOffsetAmount * heighOffsetDirection;

        transform.position = newCameraPos;
        transform.LookAt(playerSphereCenter);

		AudioListener listener = FindObjectOfType<AudioListener>();

		if (!listener)
		{
			Debug.Log("No Listener found");
		}
		else
		{
			listener.transform.position = playerSphereCenter;
		}

    }
}
