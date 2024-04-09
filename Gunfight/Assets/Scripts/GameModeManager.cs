using System.Collections;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class GameModeManager : NetworkBehaviour
{
    public static GameModeManager Instance;
    public MapManager mapManager;
    public CardManager cardManager;
    public CardUIController cardUIController;
    public GameModeUIController gameModeUIController;

    private Coroutine coroutine; // variable to start and stop coroutine

    private CustomNetworkManager manager;

    [SerializeField]
    public IGameMode currentGameMode;

    public SurvivalMode survivalMode;
    public FreeForAllMode freeForAllMode;
    public GunfightMode gunfightMode;

    public string gameMode;

    private bool playersQuit = false;
    public GameObject boxes; // parent game object of boxes in map

    private CustomNetworkManager Manager
    {
        get
        {
            if (manager != null)
            {
                return manager;
            }
            return manager = CustomNetworkManager.singleton as CustomNetworkManager;
        }
    }

    private void Awake()
    {
        currentGameMode = freeForAllMode;
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!isServer)
        {
            return;
        }
        if (currentGameMode.CheckIfGameNeedsStart())
        {
            currentGameMode.InitializeGameMode();
        }
        else if (playersQuit && (SceneManager.GetActiveScene().name != "Lobby"))
        {
            StopCoroutine(coroutine);
            Debug.Log("Stop coroutine");
            playersQuit = false;
            Invoke("ToLobby", 0.2f);
        }
    }
}
