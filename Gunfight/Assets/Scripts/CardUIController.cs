using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class CardUIController : NetworkBehaviour
{
    public GameObject CardPanel;
    public TMP_Text title;
    public Button card1;
    public Button card2;
    public Button card3;

    public void DisplayCardPanel(bool tOrF)
    {
        CardPanel.SetActive(tOrF);
        card1.gameObject.SetActive(tOrF);
        card2.gameObject.SetActive(tOrF);
        card3.gameObject.SetActive(tOrF);
        InteractableCards(tOrF);
    }

    public void InteractableCards(bool tOrF)
    {
        card1.interactable = tOrF;
        card2.interactable = tOrF;
        card3.interactable = tOrF;
    }

    public void StopDisplayCards(Button firstCard, Button secondCard, Button thirdCard)
    {
        firstCard.gameObject.SetActive(true);
        secondCard.gameObject.SetActive(false);
        thirdCard.gameObject.SetActive(false);
    }

    public void ChangeTitle(string name)
    {
        title.text = name;
    }

    [ClientRpc]
    public void RpcChangeTitle(string name)
    {
        ChangeTitle(name);
    }

    [ClientRpc]
    public void RpcShowCardPanel(bool tOrF)
    {
        DisplayCardPanel(tOrF);
    }

    [ClientRpc]
    public void RpcShowWinningCard(int winningCard)
    {
        switch(winningCard)
        {
            case 0:
                StopDisplayCards(card1, card2, card3);
                break;
            case 1:
                StopDisplayCards(card2, card1, card3);
                break;
            case 2:
                StopDisplayCards(card3, card1, card2);
                break;
            default:
                break;
        }

        InteractableCards(false);
    }
}
