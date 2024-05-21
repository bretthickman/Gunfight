using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : NetworkBehaviour
{
    // buttons for user input
    public Button card1;
    public Button card2;
    public Button card3;

    public CardUIController cardUIController;

    public List<int> votes = new List<int>(); // keeps track of the votes

    private CustomNetworkManager manager;

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

    private void Start()
    {
        // make sure card panel is not displayed 
        cardUIController.DisplayCardPanel(false);
        votes.Add(0);
        votes.Add(0);
        votes.Add(0);
        Debug.Log("New votes: " + votes[0] + ", " + votes[1] + ", " + votes[2]);
    }

    public void Card1Selected()
    {
        if (!isClient) return;
        // when user clicks card 1
        cardUIController.InteractableCards(false);
        cardUIController.StopDisplayCards(card1, card2, card3);
        Debug.Log("Card 1 pressed");
    
        CmdCard1Vote();
    }

    public void Card2Selected()
    {
        if (!isClient) return;
        // when user clicks card 2
        cardUIController.InteractableCards(false);
        cardUIController.StopDisplayCards(card2, card1, card3);
        Debug.Log("Card 2 pressed");
        
        CmdCard2Vote();
    }

    public void Card3Selected()
    {
        if (!isClient) return;
        // when user clicks card 3
        cardUIController.InteractableCards(false);
        cardUIController.StopDisplayCards(card3, card1, card2);
        Debug.Log("Card 3 pressed");
        
        CmdCard3Vote();
    }

    [Command(requiresAuthority = false)]
    public void CmdCard1Vote()
    {
        votes[0]++;
        Debug.Log("Sending card 1 vote to server");
    }

    [Command(requiresAuthority = false)]
    public void CmdCard2Vote()
    {
        votes[1]++;
        Debug.Log("Sending card 2 vote to server");
    }

    [Command(requiresAuthority = false)]
    public void CmdCard3Vote()
    {
        votes[2]++;
        Debug.Log("Sending card 3 vote to server");
    }

    public bool CheckIfEveryoneVoted(int playerCount)
    {
        // add up all the votes
        int total = 0;
        foreach(int vote in votes)
        {
            total += vote;
        }

        Debug.Log("Current votes: " + votes[0] + ", " + votes[1] + ", " + votes[2]);
        Debug.Log("Total Votes: " + total);

        // if everyone voted return true
        if (total == playerCount)
        {
            Debug.Log("Voting complete");
            return true;
        }

        // if not everyone voted return false
        return false;
    }

    public int FindMaxVote()
    {
        int card = votes.IndexOf(votes.Max());
        Debug.Log("Max votes is card " + card);
        ResetVotes();
        return card;
    }

    public void ResetVotes()
    {
        votes[0] = 0;
        votes[1] = 0;
        votes[2] = 0;
        Debug.Log("Reset votes: " + votes[0] + ", " + votes[1] + ", " + votes[2]);
    }
}
