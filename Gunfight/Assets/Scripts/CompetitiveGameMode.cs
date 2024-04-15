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


    public abstract string FindWinner();
    public abstract string FindOverallWinner();
    public abstract bool CheckOverallWin();
    public abstract void RankingList();
    public abstract void ResetOverallGame();
    public abstract bool CheckRoundWinCondition();
    public abstract void InitializeGameMode();
    public abstract bool CheckIfFriendlyFire(RaycastHit2D hit);
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
        // setup for round
        RpcResetGame();
        SpawnWeaponsInGame();
        currentRound++; // increase round count
        Debug.Log("Round started: " + currentRound);
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
            if (isServer)
                RpcResetGame();
            //SpawnWeaponsInGame();
            aliveNum = playerCount;
            StartRound();
            // TODO: Reset Map (pots / boxes)
        }
        else // if there is an overall winner
        {
            Debug.Log("End of game!");
            // gameModeUIController.DisplayQuitButton();
            string overallString = "Overall Winner: " + FindOverallWinner();
            string roundString = "Round: " + Mathf.Ceil(currentRound).ToString();
            gameModeUIController.RpcShowRoundPanel(true, overallString, roundString);
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
                    }
                    
                    string winnerString = "Winner: " + winner;
                    string roundString = "Round: " + Mathf.Ceil(currentRound).ToString();
                    gameModeUIController.RpcShowRoundPanel(true, winnerString, roundString);
                    RankingList(); // displays the rankings

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
                        Debug.Log("Winning card: " + winningCard);

                        cardUIController.RpcShowWinningCard(winningCard); // only displaying the winning card
                        yield return new WaitForSeconds(5f); // pause to show winning card
                    }
                    else // if cards are not being used
                    {
                        yield return new WaitForSeconds(5f);
                    }

                    if (useCards)
                    {
                        cardUIController.RpcShowCardPanel(false);
                    }

                    gameModeUIController.RpcShowRoundPanel(false, "", "");
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

    public void CheckWinCondition(int oldAliveNum, int newAliveNum)
    {
        StartCoroutine(DelayedEndRound());
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

    public void SetQuitClicked(bool b)
    {
        this.quitClicked = b;
    }

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
        // Call the reset function for all players
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            player.GetComponent<PlayerController>().enabled = true;
            player.GetComponent<PlayerController>().Respawn();
            player.isAlive = true;
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
}
