using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using Steamworks;

public class CustomNetworkManager : NetworkManager
{
    [SerializeField] private PlayerObjectController GamePlayerPrefab;
    public List<PlayerObjectController> GamePlayers { get; } = new List<PlayerObjectController>();
    public GameObject loadScreen;

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            PlayerObjectController GamePlayerInstance = Instantiate(GamePlayerPrefab);

            GamePlayerInstance.ConnectionID = conn.connectionId;
            GamePlayerInstance.PlayerIdNumber = GamePlayers.Count + 1;
            GamePlayerInstance.PlayerSteamID = (ulong)SteamMatchmaking.GetLobbyMemberByIndex((CSteamID)SteamLobby.Instance.CurrentLobbyID, GamePlayers.Count);

            NetworkServer.AddPlayerForConnection(conn, GamePlayerInstance.gameObject);
        }

    }

    // enable load screen to ensure syncronization
    public override void OnServerSceneChanged(string sceneName)
    {
        if (sceneName == "Map1" || sceneName == "Map2" || sceneName == "Map3" || sceneName == "Ruins") 
        {
            StartCoroutine(InitializeGameScene());
        }
        base.OnServerSceneChanged(sceneName);
    }

    private IEnumerator InitializeGameScene()
    {
        //if (loadScreen == null)
        //{
        //    loadScreen = GameObject.Find("LoadScreen"); 
        //    if (loadScreen == null)
        //    {
        //        Debug.LogError("Load Screen GameObject not found.");
        //    }
        //}

        if(loadScreen != null)
        {
            Instantiate(loadScreen, gameObject.transform);
        }
        else
        {
            Debug.Log("Load screen not found");
        }

        yield return new WaitForSeconds(1); // Give some time for all objects to initialize

        GameModeManager.Instance.startGame();

        if (loadScreen != null)
        {
            Destroy(loadScreen);
        }
    }

    public void StartGame(string SceneName)
    {
        ServerChangeScene(SceneName);
    }
}
