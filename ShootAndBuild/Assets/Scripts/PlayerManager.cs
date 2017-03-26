using System.Collections.Generic;
using UnityEngine;

public enum AxisType
{
    LeftAxisH,
    LeftAxisV,

    RightAxisH,
    RightAxisV,
}

public enum ButtonType
{
    Shoot,
}

public enum PlayerID
{
    Player1,
    Player2,
    Player3,
    Player4
}

public delegate void PlayerHandler(PlayerID id);

public class PlayerManager : MonoBehaviour
{
    public GameObject playerPrefab;

    enum InputMethod
    {
        Keyboard,

        Gamepad1,
        Gamepad2,
        Gamepad3,
        Gamepad4,
    }

    class Player
    {
        public GameObject playerObject;
        public InputMethod inputMethod;
        public bool isAlive;
    }

    private Dictionary<PlayerID, Player> activePlayersById = new Dictionary<PlayerID, Player>();
    private Dictionary<InputMethod, PlayerID> inputMethodToPlayerID = new Dictionary<InputMethod, PlayerID>();

    void Awake()
    {
        instance = this;
        allPlayers = new List<GameObject>();
    }

    void Update()
    {
        // always listen to spawn-button-presses
        TrySpawnNewPlayers();

        TryRespawnDeadPlayers();
    }

    private void TrySpawnNewPlayers()
    {
        foreach (InputMethod inputMethod in System.Enum.GetValues(typeof(InputMethod)))
        {
            if (inputMethodToPlayerID.ContainsKey(inputMethod))
            {
                // this controller already controls a player
                continue;
            }

            if (IsButtonDown(inputMethod, ButtonType.Shoot))
            {
                // 1) Try spawn NEW players
                foreach (PlayerID playerID in System.Enum.GetValues(typeof(PlayerID)))
                {
                    if (activePlayersById.ContainsKey(playerID))
                    {
                        // Player already exists
                        continue;
                    }

                    inputMethodToPlayerID[inputMethod] = playerID;
                    SpawnNewPlayer(playerID, inputMethod);

                    // enought spaning for this input method
                    break;
                }
            }
        }
    }

    private void TryRespawnDeadPlayers()
    {
        foreach (InputMethod inputMethod in System.Enum.GetValues(typeof(InputMethod)))
        {
            PlayerID playerID;

            if (!inputMethodToPlayerID.TryGetValue(inputMethod, out playerID))
            {
                continue;
            }

            Player player;
            if (!activePlayersById.TryGetValue(playerID, out player))
            {
                Debug.Assert(false, "InputMethod " + inputMethod + " registered to invalid player " + playerID);
                continue;
            }

            if (player.isAlive)
            {
                continue;
            }

            if (IsButtonDown(inputMethod, ButtonType.Shoot))
            {
                RespawnDeadPlayer(player);
            }
        }
    }

    private void RespawnDeadPlayer(Player player)
    {
        player.isAlive = true;
        player.playerObject.SetActive(true);
        player.playerObject.GetComponent<Attackable>().OnRespawn();
    }

    private void OnPlayerDies(PlayerID playerID)
    {
        Player player;
        if (!activePlayersById.TryGetValue(playerID, out player))
        {
            Debug.Assert(false, "Deleting player " + playerID + "that was not registered.");
            return;
        }

        Debug.Assert(player.isAlive);

        player.isAlive = false;
        player.playerObject.SetActive(false);
    }

    private void OnPlayerDies(Player player)
    {
        player.isAlive = false;
        player.playerObject.SetActive(false);
    }

    private void SpawnNewPlayer(PlayerID playerID, InputMethod inputMethod)
    {
        GameObject newPlayerObject = Instantiate(playerPrefab, gameObject.transform);

        float randRadius = 5.0f;
        newPlayerObject.transform.position = new Vector3(Random.Range(-randRadius, randRadius), 0.0f, Random.Range(-randRadius, randRadius));
        newPlayerObject.GetComponent<InputController>().playerID = playerID;
        newPlayerObject.GetComponent<Attackable>().Die += OnPlayerDies;

        Player newPlayer = new Player();
        newPlayer.playerObject = newPlayerObject;
        newPlayer.inputMethod = inputMethod;
        newPlayer.isAlive = true;

        activePlayersById[playerID] = newPlayer;
        allPlayers.Add(newPlayerObject);
    }

    private Player GetPlayer(PlayerID playerID)
    {
        if (!activePlayersById.ContainsKey(playerID))
        {
            Debug.Log("Accessing invalid Player " + playerID);
            return new Player();
        }

        return activePlayersById[playerID];
    }

    private InputMethod GetInputMethod(PlayerID playerID)
    {
        return GetPlayer(playerID).inputMethod;
    }

    private string InputMethodToPostfix(InputMethod inputMethod)
    {
        switch (inputMethod)
        {
            case InputMethod.Keyboard:
                return " Keyboard";
            case InputMethod.Gamepad1:
                return " P1";
            case InputMethod.Gamepad2:
                return " P2";
            case InputMethod.Gamepad3:
                return " P3";
            case InputMethod.Gamepad4:
                return " P4";
        }

        return " InvalidInputMethod";
    }

    private string AxisToPrefix(AxisType axisType)
    {
        switch (axisType)
        {
            case AxisType.LeftAxisH:
                return "Left Horizontal";
            case AxisType.LeftAxisV:
                return "Left Vertical";
            case AxisType.RightAxisH:
                return "Right Horizontal";
            case AxisType.RightAxisV:
                return "Right Vertical";
        }

        return "InvalidAxis ";
    }

    private string ButtonToPrefix(ButtonType buttonType)
    {
        switch (buttonType)
        {
            case ButtonType.Shoot:
                return "Fire";
        }

        return "InvalidButton ";
    }

    private float GetAxisValue(InputMethod inputMethod, AxisType axisType)
    {
        string inputName = AxisToPrefix(axisType) + InputMethodToPostfix(inputMethod);

        return Input.GetAxis(inputName);
    }

    public float GetAxisValue(PlayerID playerID, AxisType axisType)
    {
        InputMethod inputMethod = GetInputMethod(playerID);

        return GetAxisValue(inputMethod, axisType);
    }

    private bool IsButtonDown(InputMethod inputMethod, ButtonType buttonType)
    {
        string inputName = ButtonToPrefix(buttonType) + InputMethodToPostfix(inputMethod);

        return Input.GetButton(inputName);
    }

    public bool IsButtonDown(PlayerID playerID, ButtonType buttonType)
    {
        InputMethod inputMethod = GetInputMethod(playerID);

        return IsButtonDown(inputMethod, buttonType);
    }

    public List<GameObject> allPlayers
    {
        get; private set;
    }

    public static PlayerManager instance
    {
        get; private set;
    }
}
