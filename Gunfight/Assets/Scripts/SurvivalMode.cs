using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using static GameModeManager;
using UnityEngine.SceneManagement;
using System.Linq;
//using System;

[System.Serializable]
public class SurvivalMode : NetworkBehaviour, IGameMode
{
    public static GameModeManager Instance;
    public MapManager mapManager;
    public CardManager cardManager;
    public GameModeUIController gameModeUIController;
    public CardUIController cardUIController;

    private CustomNetworkManager manager;

    public GameObject enemyPrefab;
    public int startingNumberOfEnemies;
    public float enemyMultiplier = 1.15f;
    public int enemiesSpawnedThisRound;

    public int playerCount;
    public bool hasGameStarted = false;
    public bool useCards = true;

    [SyncVar(hook = nameof(CheckWinCondition))]
    public int currentNumberOfEnemies;
    [SyncVar(hook = nameof(CheckLossCondition))]
    public int aliveNum; // get this from lobby

    [SyncVar]
    public int currentRound = 0; // keeps track of the current round
    public int totalRounds = 9999; // keeps track of total amount of rounds

    public bool quitClicked = false;

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

    //-------------Wave Mode-exclusive Methods--------------

    private void initEnemy()
    {
        if (!isServer)
        {
            return;
        }
        currentNumberOfEnemies = startingNumberOfEnemies;
        enemiesSpawnedThisRound = startingNumberOfEnemies;
        for (int i = 0; i < startingNumberOfEnemies; i++)
        {
            float x = (i % 2 == 0) ? mapManager.mapWidth / 2 : -mapManager.mapWidth / 2;
            float y = (i < 2) ? (mapManager.mapHeight - mapManager.heightOffset) / 2 : -(mapManager.mapHeight - mapManager.heightOffset) / 2;

            Vector3 spawnPos = new Vector3(x, y, 0);

            GameObject enemyInstance = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            NetworkServer.Spawn(enemyInstance);
        }
    }

    public void spawnEnemies()
    {
        if (!isServer)
        {
            return;
        }
        for (int i = 0; i < enemiesSpawnedThisRound; i++)
        {
            float x, y;

            if (i % 2 == 0)
            {
                // Even index, spawn on the top or bottom edge
                x = Random.Range(-mapManager.mapWidth / 2, mapManager.mapWidth / 2);
                y = (i < 2) ? (mapManager.mapHeight - mapManager.heightOffset) / 2 : -(mapManager.mapHeight - mapManager.heightOffset) / 2;
            }
            else
            {
                // Odd index, spawn on the left or right edge
                x = (i < 2) ? mapManager.mapWidth / 2 : -mapManager.mapWidth / 2;
                y = Random.Range(-(mapManager.mapHeight - mapManager.heightOffset) / 2, (mapManager.mapHeight - mapManager.heightOffset) / 2);
            }

            Vector3 spawnPos = new Vector3(x, y, 0);

            GameObject enemyInstance = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            NetworkServer.Spawn(enemyInstance);
        }
        increaseSpeed();
        increaseDamage();
    }

    public void increaseSpeed()
    {
        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemyObject in enemyObjects)
        {
            // Check if the GameObject has the EnemyObjectController script attached
            EnemyObjectController controller = enemyObject.GetComponent<EnemyObjectController>();

            if (controller != null)
            {
                // Call the updateSpeed function
                controller.updateSpeed(currentRound);
            }
            else
            {
                Debug.LogWarning("EnemyObjectController script not found on GameObject: " + enemyObject.name);
            }
        }
    }

    public void increaseDamage()
    {
        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemyObject in enemyObjects)
        {
            // Check if the GameObject has the EnemyObjectController script attached
            EnemyObjectController controller = enemyObject.GetComponent<EnemyObjectController>();

            if (controller != null)
            {
                // Call the updateSpeed function
                controller.updateDamage(currentRound);
            }
            else
            {
                Debug.LogWarning("EnemyObjectController script not found on GameObject: " + enemyObject.name);
            }
        }
    }

    public void CheckLossCondition(int oldAliveNum, int newAliveNum)
    {
        if(aliveNum <= 0)
        {
            EndGame();
        }
    }

    public void EndGame()
    {
        Debug.Log("End of game!");

        if(gameModeUIController == null) { gameModeUIController = FindObjectOfType<GameModeUIController>(); }

        gameModeUIController.DisplayRoundPanel(true);
        if (isServer) {
            RankingList();
            //reset player stats
            RpcResetOverallGame();
        }
        
        StartCoroutine(QuitCountdown());
    }

    //------------------Game Mode Interface Methods------------------------------

    public bool CheckIfGameNeedsStart()
    {
        bool result = !hasGameStarted && (SceneManager.GetActiveScene().name != "Lobby") && aliveNum != 0;
        Debug.Log("Checking if game needs start: " + result);
        return !hasGameStarted && (SceneManager.GetActiveScene().name != "Lobby") && aliveNum != 0;
    }

    public void InitializeGameMode()
    {
        mapManager = GameObject.Find("MapManager").GetComponent<MapManager>();

        totalRounds = 9999;
        initEnemy();

        playerCount = aliveNum;
        hasGameStarted = true;
        StartRound();
    }

    public void StartRound()
    {
        if (!isServer)
        {
            return;
        }
        // respawns all players
        RpcResetGame();
        currentRound++; 

        //end these
        Debug.Log("Round started: " + currentRound);
    }

    public void EndRound()
    {
        if (!isServer) { return; }
        DeleteWeaponsInGame();
        SpawnWeaponsInGame();

        enemiesSpawnedThisRound = Mathf.RoundToInt(enemiesSpawnedThisRound * enemyMultiplier);
        currentNumberOfEnemies = enemiesSpawnedThisRound;
        spawnEnemies();

        StartRound();
        
    }
    public void ToLobby()
    {
        Manager.StartGame("Lobby");
    }

    public IEnumerator QuitCountdown()
    {
        int count = 3;
        while (count > 0)
        {
            if (quitClicked)
            {
                break;
            }
            yield return new WaitForSeconds(1f);
            count--;
        }
        Debug.Log("Quit game");
        ToLobby();
    }

    public void PlayerDied(PlayerController player)
    {
        player.poc.isAlive = false;
        aliveNum--;
    }

    public void QuitGame()
    {
        // quits back to the lobby
        // gameModeUIController.StopDisplayQuitButton();
        gameModeUIController.RpcShowRoundPanel(false, "", "");
        quitClicked = false;
        ToLobby();
    }

    public IEnumerator DelayedEndRound()
    {
        
        // gets the Card Manager game object
        if (cardManager == null)
        {
            cardManager = FindObjectOfType<CardManager>();
            if (cardManager == null)
            {
                Debug.Log("Couldnt find card manager on delayed end round");
            }
        }

        cardUIController = FindObjectOfType<CardUIController>();
        gameModeUIController = FindObjectOfType<GameModeUIController>();

        cardUIController.RpcShowCardPanel(true);
        gameModeUIController.RpcShowWinner("Round: " + currentRound);
        yield return new WaitForSeconds(10.0f);
        gameModeUIController.RpcStopShowWinner();
        cardUIController.RpcShowCardPanel(false);

        StartCoroutine(PreroundCountdown());
        yield return new WaitForSeconds(5f);
        EndRound();
    }

    public IEnumerator PreroundCountdown()
    {
        float countdownTime = 5f;

        while (countdownTime > 0)
        {
            // Update the countdown text on the UI
            gameModeUIController.RpcShowCount(Mathf.Ceil(countdownTime).ToString());

            // Wait for the next frame
            yield return null;

            // Reduce the countdown time
            countdownTime -= Time.deltaTime;
        }

        // Clear the countdown text when the countdown is complete
        gameModeUIController.RpcStopShowCount();
    }

    public void CheckWinCondition(int oldCurrentNumberOfEnemies, int newCurrentNumberOfEnemies)
    {
        if (isServer && SceneManager.GetActiveScene().name != "Lobby" &&
                currentNumberOfEnemies <= 0) // changed from curNumNME != startingNum 
        {
            StartCoroutine(DelayedEndRound());
        }
    }

    public void RankingList()
    {
        string rankingString = "";
        string killsString = "";

        List<PlayerObjectController> players = new List<PlayerObjectController>();
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            players.Add(player);
        }

        players = players.OrderByDescending(player => player.kills).ToList();

        // creates strings with the values from the list
        for (int i = 0; i < playerCount; i++)
        {
            rankingString += players[i].PlayerName + "\n";
            killsString += players[i].kills + "\n";
        }

        Debug.Log("Ranking names: " + rankingString);
        Debug.Log("Ranking kills: " + killsString);

        gameModeUIController.RpcShowRanking(rankingString, killsString);
    }

    [ClientRpc]
    public void RpcResetOverallGame()
    {
        RpcResetPlayerStats();
        RpcResetGame();
        hasGameStarted = false;
    }

    [ClientRpc]
    public void RpcResetPlayerStats()
    {
        // reset kills for all players
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            player.kills = 0;
        }
    }


    [ClientRpc]
    public void RpcResetGame()
    {
        // Call the reset function for all players
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            player.GetComponent<PlayerController>().enabled = true;
            player.GetComponent<PlayerController>().Respawn();
            player.isAlive = true;
        }
    }

    public void SpawnWeaponsInGame()
    {
        // Find the WeaponSpawning script in the "game" scene
        WeaponSpawning weaponSpawning = FindObjectOfType<WeaponSpawning>();

        if (weaponSpawning != null)
        {
            weaponSpawning.SpawnWeapons();
        }
        else
        {
            Debug.LogError("WeaponSpawning script not found in the 'game' scene.");
        }
    }

    public void DeleteWeaponsInGame()
    {
        // Find the WeaponSpawning script in the "game" scene
        WeaponSpawning weaponSpawning = FindObjectOfType<WeaponSpawning>();

        if (weaponSpawning != null)
        {
            weaponSpawning.DeleteWeapons();
        }
        else
        {
            Debug.LogError("WeaponSpawning script not found in the 'game' scene.");
        }
    }

    public int GetAliveNum()
    {
        return this.aliveNum;
    }

    public void SetAliveNum(int num)
    {
        this.aliveNum = num;
    }

    public void SetUseCards(bool usingCards)
    {
        this.useCards = usingCards;
    }

    public bool GetUseCards()
    {
        return this.useCards;
    }

    public void SetTotalRounds(int rounds)
    {
        this.totalRounds = rounds;
    }

    public void DecrementCurrentNumberOfEnemies()
    {
        this.currentNumberOfEnemies--;
    }

    public void SetQuitClicked(bool b)
    {
        this.quitClicked = b;
    }
}
