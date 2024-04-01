using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FreeForAllMode : CompetitiveGameMode
{
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

    public override void InitializeGameMode()
    {
        mapManager = GameObject.Find("MapManager").GetComponent<MapManager>();

        playerCount = aliveNum;
        hasGameStarted = true;
        StartRound();
    }

    public override bool CheckIfFriendlyFire(RaycastHit2D objectHit)
    {
        return false;
    }

    public override void ResetOverallGame()
    {
        RpcResetPlayerStats();
        RpcResetGame();
    }

    public override bool CheckRoundWinCondition()
    {
        return aliveNum <= 1;
    }

    public override string FindWinner()
    {
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            if (player.isAlive)
            {
                player.wins++;
                return player.PlayerName;
            }
        }
        return "No one";
    }

    public override string FindOverallWinner()
    {
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            if (player.wins == totalRounds)
            {
                return player.PlayerName;
            }
        }
        return "No one";
    }

    public override bool CheckOverallWin()
    {
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            // checks if a player has the required amount of wins
            if (player.wins == totalRounds)
            {
                return true;
            }
        }
        return false;
    }

    public override void RankingList()
    {
        string rankingString = "";
        string winsString = "";

        List<PlayerObjectController> players = new List<PlayerObjectController>();
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            players.Add(player);
        }

        players = players.OrderByDescending(player => player.wins).ToList();

        // creates strings with the values from the list
        for (int i = 0; i < playerCount; i++)
        {
            rankingString += players[i].PlayerName + "\n";
            winsString += players[i].wins + "\n";
        }

        Debug.Log("Ranking names: " + rankingString);
        Debug.Log("Ranking wins: " + winsString);

        gameModeUIController.RpcShowRanking(rankingString, winsString);
    }

}
