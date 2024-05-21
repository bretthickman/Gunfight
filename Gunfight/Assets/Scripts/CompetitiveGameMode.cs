using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using static GameModeManager;
using UnityEngine.SceneManagement;
using System;

[System.Serializable]
public abstract class CompetitiveGameMode : NetworkBehaviour, IGameMode
{
    public static GameModeManager Instance;
    public MapManager mapManager;
    public CardManager cardManager;
    public GameModeUIController gameModeUIController;
    public CardUIController cardUIController;

    [SyncVar]
    public int currentRound = 0; // keeps track of the current round
    public int totalRounds = 3; // keeps track of total amount of rounds

    [SyncVar(hook = nameof(CheckWinCondition))]
    public int aliveNum; // get this from lobby

    protected CustomNetworkManager manager;

    public int playerCount;
    public bool hasGameStarted = false;
    public bool friendlyFireEnabled = false;

    // keeps track of the rankings
    public List<string> ranking = new List<string>();

    public bool quitClicked = false;

    [Header("Card Attributes")]
    private int winningCard;
    public bool useCards = true;

    public GameObject boxes; // parent game object of boxes in map
    public GameObject doors; // parent game object of doors in map
    public GameObject walls; // parent game bobject of destroyable walls in map
    public GameObject fountain;
    private int playersResetCount = 0;

    public GameObject PlayerStatsItemPrefab; 
    public List<PlayerStatsItem> PlayerStatsItems = new List<PlayerStatsItem>();
    //public GameObject RoundStatsList;

    public abstract string FindWinner();
    public abstract string FindOverallWinner();
    public abstract bool CheckOverallWin();
    public abstract void RankingList();
    public abstract void ResetOverallGame();
    public abstract bool CheckRoundWinCondition();
    public abstract void InitializeGameMode();
    public abstract void RpcInitStatsList();
    public abstract IEnumerator SetStatsList();
    public abstract void PlayerQuit();
    public abstract bool CheckIfFriendlyFire(RaycastHit2D hit, int teamNum);
    public abstract void SpawnWeaponsInGame();

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

    public bool CheckIfGameNeedsStart()
    {
        return !hasGameStarted && (SceneManager.GetActiveScene().name != "Lobby") && aliveNum != 0;
    }

    public void StartRound()
    {
        if (!isServer)
        {
            return;
        }

        // Ensure all clients are ready before proceeding
        if (!AreAllClientsReady())
        {
            Debug.Log("Not all clients are ready. Delaying start.");
            StartCoroutine(WaitForClientsReady());
            return;
        }

        playersResetCount = 0;
        // setup for round
        RpcResetGame();
        currentRound++; // increase round count
        Debug.Log("Round started: " + currentRound);
    }

    private bool AreAllClientsReady()
    {
        foreach (var conn in NetworkServer.connections)
        {
            if (!conn.Value.isReady)
            {
                return false;
            }
        }
        return true;
    }

    private IEnumerator WaitForClientsReady()
    {
        while (!AreAllClientsReady())
        {
            yield return new WaitForSeconds(1); // Check every second
        }
        StartRound();
    }

    public void EndRound()
    {
        if (!isServer)
        {
            return;
        }
        if (!CheckOverallWin()) // if there is not an overall winner
        {
            DeleteWeaponsInGame();


            // reset boxes
            boxes = GameObject.Find("Objects");
            foreach (Box child in boxes.GetComponentsInChildren<Box>())
            {
                child.RpcResetBox();
            }

            // reset doors
            doors = GameObject.Find("doors");
            if (doors != null)
            {
                foreach (Door child in doors.GetComponentsInChildren<Door>())
                {
                    child.RpcResetDoor();
                }
            }

            // resets broken walls
            walls = GameObject.Find("Interactables");
            if (walls != null)
            {
                foreach (Wall child in walls.GetComponentsInChildren<Wall>())
                {
                    child.RpcResetWall();
                }
            }

            fountain = GameObject.Find("fountain");
            if (fountain != null)
            {
                fountain.GetComponent<Fountain>().ResetHealth();
            }
            
            aliveNum = playerCount;
            StartRound();
        }
        else // if there is an overall winner
        {
            Debug.Log("End of game!");
            // gameModeUIController.DisplayQuitButton();
            string overallString = "Overall Winner: " + FindOverallWinner();
            string roundString = "Round: " + Mathf.Ceil(currentRound).ToString();
            gameModeUIController.RpcShowEndOfGamePanel(true, overallString, roundString);
            RankingList();

            //reset player stats
            ResetOverallGame();

            currentRound = 0;

            StartCoroutine(QuitCountdown());
        }
    }

    public IEnumerator DelayedEndRound()
    {
        if (isServer && SceneManager.GetActiveScene().name != "Lobby" && aliveNum != playerCount)
        {
            if (useCards)
            {
                // gets the Card Manager game object
                if (cardManager == null)
                {
                    cardManager = FindObjectOfType<CardManager>();
                    if (cardManager == null)
                    {
                        Debug.Log("Couldnt find card manager (DelayedEndRound)");
                    }
                }

                Debug.Log("Found card manager: " + (cardManager != null));
            }

            cardUIController = FindObjectOfType<CardUIController>();
            gameModeUIController = FindObjectOfType<GameModeUIController>();

            // If only one player is alive or there is a team winner, end round 
            if (CheckRoundWinCondition())
            {
                RpcDisableGameInteraction();
                string winner = FindWinner();
                if (!CheckOverallWin())
                {
                    if (useCards)
                    {
                        cardUIController.RpcShowCardPanel(true);
                        cardUIController.RpcChangeTitle("Choose a card");
                    }
                    
                    string roundString = "Round: " + Mathf.Ceil(currentRound).ToString();
                    gameModeUIController.RpcShowRoundStats(true, roundString);
                    if (currentRound == 1)
                    {
                        RpcInitStatsList(); // initialize player stats
                    }
                    yield return StartCoroutine(SetStatsList());

                    if (useCards)
                    {
                        // start 10s timer 
                        int count = 10;

                        while (count > 0)
                        {
                            gameModeUIController.RpcShowTimer(Mathf.Ceil(count).ToString());
                            // if everyone voted stop countdown
                            if (cardManager.CheckIfEveryoneVoted(playerCount))
                            {
                                Debug.Log("Break countdown");
                                break;
                            }
                            yield return new WaitForSeconds(1f);
                            count--;
                            Debug.Log("Card countdown: " + count);
                        }

                        gameModeUIController.RpcStopShowTimer();

                        // find the card voted the most
                        winningCard = cardManager.FindMaxVote();
                        cardUIController.RpcChangeTitle("Winning Card");
                        Debug.Log("Winning card: " + winningCard);

                        cardUIController.RpcShowWinningCard(winningCard); // only displaying the winning card
                        yield return new WaitForSeconds(5f); // pause to show winning card
                        cardUIController.RpcShowCardPanel(false);
                        gameModeUIController.RpcShowRoundStats(false, "");
                    }
                    else // if cards are not being used
                    {
                        yield return new WaitForSeconds(5f);
                    }

                    StartCoroutine(PreroundCountdown());
                    yield return new WaitForSeconds(5f);
                }
                EndRound();
            }
        }
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
            countdownTime -= Time.deltaTime;
        }

        gameModeUIController.RpcStopShowCount();
    }

    public void ToLobby()
    {
        manager.StartGame("Lobby");
    }

    public IEnumerator QuitCountdown()
    {
        // 10s countdown 
        int count = 10;
        while (count > 0)
        {
            // if (quitClicked)
            // {
            //     break;
            // }
            // Update the countdown text on the UI
            gameModeUIController.RpcShowTimer(Mathf.Ceil(count).ToString());
            yield return new WaitForSeconds(1f);
            count--;
        }
        gameModeUIController.RpcStopShowTimer();
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
        gameModeUIController.RpcShowEndOfGamePanel(false, "", "");
        // quitClicked = false;
        ToLobby();
    }

    public void CheckWinCondition(int oldAliveNum, int newAliveNum)
    {
        GameModeManager.Instance.coroutine = StartCoroutine(DelayedEndRound());
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

    public void ToggleFriendlyFire()
    {
        this.friendlyFireEnabled = !this.friendlyFireEnabled;
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

    // public void SetQuitClicked(bool b)
    // {
    //     this.quitClicked = b;
    // }

    public void DecrementCurrentNumberOfEnemies()
    {
        Debug.Log("Attempted to decrement number of enemies in non-surival game mode.");
    }

    [ClientRpc]
    public void RpcDisableGameInteraction()
    {
        // Call the disable game interaction for all players
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            player.GetComponent<PlayerController>().enabled = false;
        }
    }

    [ClientRpc]
    public void RpcResetGame()
    {
        Debug.Log("rpc resetting");
        // Call the reset function for all players
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            Debug.Log(player.name + "running");
            player.GetComponent<PlayerController>().enabled = true;
            player.GetComponent<PlayerController>().Respawn();
            player.isAlive = true;
            player.GetComponent<PlayerController>().CmdNotifyResetComplete();
        }
    }

    [ClientRpc]
    public void RpcResetPlayerStats()
    {
        // reset wins for all players
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            player.wins = 0;
        }
    }

    [ClientRpc]
    public void RpcAssignWeapon(PlayerObjectController player, WeaponInfo weapon)
    {
        PlayerController controller = player.GetComponent<PlayerController>();

        player.GetComponent<PlayerWeaponController>().ChangeSprite(weapon.id);
        controller.weaponInfo.setWeaponInfo(weapon);
        controller.cooldownTimer = 0f;
        controller.isFiring = false;

        Debug.Log("Assigned player " + player + " weapon " + weapon);
    }

    public void PlayerResetComplete()
    {
       playersResetCount++;
       if (playersResetCount >= Manager.GamePlayers.Count)
       {
            Debug.Log("SPAWNING WEAPONS");
            SpawnWeaponsInGame();
       }
    }
}
