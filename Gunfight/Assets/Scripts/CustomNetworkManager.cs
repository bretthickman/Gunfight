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

    // delay lobby -> game scene to ensure loading and syncing
    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);
        if (sceneName == "Map1" || sceneName == "Map2" || sceneName == "Ruins") 
        {
            StartCoroutine(InitializeGameScene());
        }
    }

    private IEnumerator InitializeGameScene()
    {
        yield return new WaitForSeconds(1); // Give some time for all objects to initialize
        GameModeManager.Instance.startGame();
    }

    public void StartGame(string SceneName)
    {
        ServerChangeScene(SceneName);
    }
}
