using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using static GameModeManager;

public class GunfightMode : CompetitiveGameMode
{
    public int[] teamAlive = { 0, 0 }; // keeps track of how many players on each team is alive
    public int[] teamWins = { 0, 0 }; // keeps track of how many wins each team has
    private int teamWinNum;
    public GameObject[] weaponList;
    [SerializeField] private GameObject[] weaponSpawnOrder;
    [SerializeField] private int weaponSpawnOffest = 2;


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
        GenerateWeaponSpawnOrder();
        Debug.Log("WHERE AM I??");
        StartRound(); // starts the first round after Awake
    }

    public override void ResetOverallGame()
    {
        teamWins[0] = 0;
        teamWins[1] = 0;
    }

    private void GetTeamPlayers()
    {
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

        Debug.Log("Team 1 alive = " + teamOneAlive + "; Team 2 alive = " + teamTwoALive);

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

    public override bool CheckIfFriendlyFire(RaycastHit2D hit, int teamNum)
    {
        GameObject otherPlayer = hit.collider.gameObject;
        PlayerObjectController otherPoc = otherPlayer.GetComponent<PlayerObjectController>();

        bool sameTeam;
        if (otherPoc == null)
        {
            sameTeam = false;
        }
        else
        {
            sameTeam = otherPoc.Team == teamNum;
        }

        if (otherPlayer.CompareTag("Player") && !friendlyFireEnabled && sameTeam) { return false; }

        return true;
    }

    // Assigns each player the next weapon in the weapon spawn order
    [Server]
    public override void SpawnWeaponsInGame()
    {
        GameObject newWeapon = weaponSpawnOrder[currentRound];
        //WeaponInfo newWeaponInfo = newWeapon.GetComponent<WeaponInfo>();

        // go to each player object in game and assign its weapon from weaponSpawnOrder[roundNumber]
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            Vector3 spawnPosition = new Vector3(player.transform.position.x, player.transform.position.y + weaponSpawnOffest, player.transform.position.z);
            Quaternion spawnRotation = Quaternion.Euler(0, 0, 0);
            GameObject weaponInstance = Instantiate(newWeapon, spawnPosition, spawnRotation);
            NetworkServer.Spawn(weaponInstance);        
        }
    }

    // generates the weapon spawn order for the entire match at random, with repitition
    public void GenerateWeaponSpawnOrder()
    {
        weaponSpawnOrder = new GameObject[totalRounds * 2 - 1];
        for(int i = 0; i < weaponSpawnOrder.Length; i++)
        {
            int choice = Random.Range(0, weaponList.Length);
            Debug.Log("Generating weapon spawn order, adding " + choice + " to the list.");
            weaponSpawnOrder[i] = weaponList[choice];
        }
    }
}
