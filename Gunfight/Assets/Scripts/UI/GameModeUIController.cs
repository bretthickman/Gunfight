using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class GameModeUIController : NetworkBehaviour
{
    public Text Timer;

    // for between rounds
    public GameObject RoundStats;
    public Text Countdown;
    public Text RoundNumber;

    // for end of game
    public GameObject EndOfGameStats;
    public Text Winner;
    public Text EndRoundNumber;
    public Text Ranking;
    public Text Wins;

    public void DisplayRoundStats(bool tOrF)
    {
        RoundStats.SetActive(tOrF);
    }

    public void DisplayEndOfGamePanel(bool tOrF)
    {
        EndOfGameStats.SetActive(tOrF);
    }
    
    public void DisplayWinner(string newText)
    {
        Winner.enabled = true;
        Winner.text = newText;
    }

    public void StopDisplayWinner()
    {
        Winner.enabled = false;
    }

    public void DisplayCount(string newText)
    {
        Countdown.enabled = true;
        Countdown.text = newText;
    }

    public void StopDisplayCount()
    {
        Countdown.enabled = false;
    }

    public void DisplayTimer(string newText)
    {
        Timer.enabled = true;
        Timer.text = newText;
    }

    public void StopDisplayTimer()
    {
        Timer.enabled = false;
    }

    public void DisplayRoundNumber(string newText)
    {
        RoundNumber.enabled = true;
        RoundNumber.text = newText;
    }

    public void StopDisplayRoundNumber()
    {
        RoundNumber.enabled = false;
    }

    public void DisplayEndRoundNumber(string newText)
    {
        EndRoundNumber.enabled = true;
        EndRoundNumber.text = newText;
    }

    public void StopDisplayEndRoundNumber()
    {
        EndRoundNumber.enabled = false;
    }

    public void DisplayRanking(string newRanking, string newWins)
    {
        Ranking.enabled = true;
        Wins.enabled = true;
        Ranking.text = newRanking;
        Wins.text = newWins;
    }

    public void StopDisplayRanking()
    {
        Ranking.enabled = false;
        Wins.enabled = false;
    }

    [ClientRpc]
    public void RpcShowRoundStats(bool tOrF, string round)
    {
        DisplayRoundStats(tOrF);
        RoundNumber.text = round;
    }

    [ClientRpc]
    public void RpcShowEndOfGamePanel(bool tOrF, string winner, string round)
    {
        DisplayEndOfGamePanel(tOrF);
        if (tOrF == true)
        {
            DisplayWinner(winner);
            DisplayEndRoundNumber(round);
        }
        else
        {
            StopDisplayWinner();
            StopDisplayEndRoundNumber();
            StopDisplayRanking();
        }
    }

    [ClientRpc]
    public void RpcShowRanking(string rankings, string wins)
    {
        DisplayRanking(rankings, wins);
    }

    [ClientRpc]
    public void RpcShowWinner(string winner)
    {
        DisplayWinner(winner);
    }

    [ClientRpc]
    public void RpcStopShowWinner()
    {
        StopDisplayWinner();
    }

    [ClientRpc]
    public void RpcShowCount(string count)
    {
        DisplayCount(count);
    }

    [ClientRpc]
    public void RpcStopShowCount()
    {
        StopDisplayCount();
    }

    [ClientRpc]
    public void RpcShowTimer(string count)
    {
        DisplayTimer(count);
    }

    [ClientRpc]
    public void RpcStopShowTimer()
    {
        StopDisplayTimer();
    }
}
