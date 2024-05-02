using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameModeManager;
using Mirror;

public class GunfightMode : CompetitiveGameMode
{
    public int[] teamAlive = { 0, 0 }; // keeps track of how many players on each team is alive
    public int[] teamWins = { 0, 0 }; // keeps track of how many wins each team has
    private int teamWinNum;

    // not sure if having this in subclass will break it
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
        GetTeamPlayers();
        StartRound(); // starts the first round after Awake
    }

    public override void ResetOverallGame()
    {
        teamWins[0] = 0;
        teamWins[1] = 0;
    }

    private void GetTeamPlayers()
    {
        teamAlive[0] = 0;
        teamAlive[1] = 0;
        
        //assigns the number of players on each team
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            if (player.Team == 1)
            {
                teamAlive[0]++;
            }
            else if (player.Team == 2)
            {
                teamAlive[1]++;
            }
        }

        Debug.Log("Team 1 players alive: " + teamAlive[0]);
        Debug.Log("Team 2 players alive: " + teamAlive[1]);
    }

    public override bool CheckRoundWinCondition()
    {
        int teamOneAlive = 0;
        int teamTwoALive = 0;

        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            if (player.isAlive)
            {
                if (player.Team == 1)
                {
                    teamOneAlive++;
                }
                else if (player.Team == 2)
                {
                    teamTwoALive++;
                }
            }
        }

        if (teamOneAlive == 0 || teamTwoALive == 0)
        {
            if (teamOneAlive == 0)
            {
                teamWinNum = 2;
            }
            else if (teamTwoALive == 0)
            {
                teamWinNum = 1;
            }
            return true;
        }
        return false;
    }

    public override string FindWinner()
    {
        if (teamWinNum == 1)
        {
            teamWins[0]++;
            return "Team 1";
        }
        else if (teamWinNum == 2)
        {
            teamWins[1]++;
            return "Team 2";
        }
        return "No one";
    }

    public override string FindOverallWinner()
    {
        if (teamWins[0] == totalRounds)
        {
            return "Team 1";
        }
        else if (teamWins[1] == totalRounds)
        {
            return "Team 2";
        }
        return "No one";
    }

    public override bool CheckOverallWin()
    {
        // check if a team has the required number of wins
        if (teamWins[0] == totalRounds || teamWins[1] == totalRounds)
        {
            Debug.Log("A team has won");
            return true;
        }
        return false;
    }

    public override void RankingList()
    {
        string rankingString = "";
        string winsString = "";

        if (teamWinNum == 1)
        {
            rankingString = "Team 1 \nTeam 2\n";
            winsString = teamWins[0] + "\n" + teamWins[1] + "\n";
        }

        if (teamWinNum == 2)
        {
            rankingString = "Team 2 \nTeam 1\n";
            winsString = teamWins[1] + "\n" + teamWins[0] + "\n";
        }

        Debug.Log("Ranking names: " + rankingString);
        Debug.Log("Ranking wins: " + winsString);

        gameModeUIController.RpcShowRanking(rankingString, winsString);
    }

    public override void PlayerQuit()
    {
        int team1 = 0;
        int team2 = 0;

        // check how many players are on each team
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            if (player.Team == 1)
            {
                team1++;
            }
            else if (player.Team == 2)
            {
                team2++;
            }
        }

        // check if any team has 0 players left
        if (team1 == 0 || team2 == 0)
        {
            Debug.Log("One team left");

            // reset stats
            teamWins[0] = 0;
            teamWins[1] = 0;
            currentRound = 0;
            GameModeManager.Instance.playersQuit = true;
        }
    }

    [ClientRpc]
    public override void RpcInitStatsList()
    {
        if (PlayerStatsItems.Count != 2)
        {
            for (int i = 1; i < 3; i++)
            {
                // only need two objects, one for each team
                GameObject NewTeamStatsItem = Instantiate(PlayerStatsItemPrefab) as GameObject;
                PlayerStatsItem NewStatsItemScript = NewTeamStatsItem.GetComponent<PlayerStatsItem>();
                NewStatsItemScript.Team = i;
                Color teamColor;
                if (i == 1)
                {
                    teamColor = Color.red;
                }
                else
                {
                    teamColor = Color.blue;
                }
                NewStatsItemScript.SetTeamStats(teamWins[i - 1], teamColor);

                GameObject canvas = GameObject.Find("Canvas");
                // gets the Teams object in the RoundStats object
                GameObject statsList = canvas.transform.GetChild(6).GetChild(0).GetChild(1).gameObject;

                NewTeamStatsItem.transform.SetParent(statsList.transform);

                PlayerStatsItems.Add(NewStatsItemScript);
            }
        }
        // else
        // {
        //     foreach(PlayerStatsItem TeamStatsScript in PlayerStatsItems)
        //     {
        //         if (TeamStatsScript.Team == 1)
        //         {
        //             TeamStatsScript.SetTeamStats(teamWins[0], Color.red);
        //         }
        //         else
        //         {
        //             TeamStatsScript.SetTeamStats(teamWins[1], Color.blue);
        //         }
        //     }
        // }
    }

    public override void SetStatsList()
    {
        // sets for the server
        foreach(PlayerStatsItem TeamStatsScript in PlayerStatsItems)
        {
            if (TeamStatsScript.Team == 1)
            {
                TeamStatsScript.SetTeamStats(teamWins[0], Color.red);
                RpcSetStatsList(1, teamWins[0]);
            }
            else
            {
                TeamStatsScript.SetTeamStats(teamWins[1], Color.blue);
                RpcSetStatsList(2, teamWins[1]);
            }
        }
    }

    [ClientRpc]
    public void RpcSetStatsList(int team, int wins)
    {
        // sets stats for the client
        foreach(PlayerStatsItem TeamStatsScript in PlayerStatsItems)
        {
            if (TeamStatsScript.Team == team)
            {
                TeamStatsScript.SetTeamStats(wins, TeamStatsScript.PlayerIcon.color);
            }
        }
    }
}
