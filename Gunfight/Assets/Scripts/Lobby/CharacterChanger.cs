using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using Mirror;

public class CharacterChanger : MonoBehaviour
{
    public int currentBodyIndex = 0;
    public int currentHairIndex = 0;
    public int currentEyesIndex = 0;
    public int currentColorIndex = 0;
    public Color[] playerColors;
    public string[] colorNames;
    public Image displaySpriteBody;
    public Image displaySpriteHair;
    public Image displaySpriteEyes;
    private PlayerController player;
    public Text currentColorText;

    private void Start()
    {
        currentBodyIndex = PlayerPrefs.GetInt("currentBodyIndex", 0); // allows persistent variables even on game restart
        LobbyController.Instance.LocalPlayerController.CmdUpdatePlayerBody(currentBodyIndex);
        player = GameObject.Find("LocalGamePlayer").GetComponent<PlayerController>();

        currentHairIndex = PlayerPrefs.GetInt("currentHairIndex", 0); // allows persistent variables even on game restart
        LobbyController.Instance.LocalPlayerController.CmdUpdatePlayerHair(currentHairIndex);

        currentEyesIndex = PlayerPrefs.GetInt("currentEyesIndex", 0); // allows persistent variables even on game restart
        LobbyController.Instance.LocalPlayerController.CmdUpdatePlayerEyes(currentEyesIndex);

        currentColorIndex = PlayerPrefs.GetInt("currentColorIndex", 0);
        LobbyController.Instance.LocalPlayerController.CmdUpdatePlayerColor(currentColorIndex);
    }

    private void Update()
    {
        currentColorText.text = colorNames[currentColorIndex];
        displaySpriteHair.color = player.spriteRendererHair.color;
        displaySpriteBody.sprite = player.spriteRendererBody.sprite;
        displaySpriteHair.sprite = player.spriteRendererHair.sprite;
        displaySpriteEyes.sprite = player.spriteRendererEyes.sprite;
    }

    public void NextColor()
    {
        currentColorIndex = (currentColorIndex + 1) % player.playerColors.Length;
        PlayerPrefs.SetInt("currentColorIndex", currentColorIndex);
        LobbyController.Instance.LocalPlayerController.CmdUpdatePlayerColor(currentColorIndex);
    }

    public void PrevColor()
    {
        currentColorIndex = (currentColorIndex - 1 + player.playerColors.Length) % player.playerColors.Length;
        PlayerPrefs.SetInt("currentColorIndex", currentColorIndex);
        LobbyController.Instance.LocalPlayerController.CmdUpdatePlayerColor(currentColorIndex);
    }

    public void NextBody()
    {
        currentBodyIndex = (currentBodyIndex + 1) % player.bodySpriteLibraryArray.Length;
        PlayerPrefs.SetInt("currentBodyIndex", currentBodyIndex);
        LobbyController.Instance.LocalPlayerController.CmdUpdatePlayerBody(currentBodyIndex);
    }

    public void PrevBody()
    {
        currentBodyIndex = (currentBodyIndex - 1 + player.bodySpriteLibraryArray.Length) % player.bodySpriteLibraryArray.Length;
        PlayerPrefs.SetInt("currentBodyIndex", currentBodyIndex);
        LobbyController.Instance.LocalPlayerController.CmdUpdatePlayerBody(currentBodyIndex);
    }

    public void NextHair()
    {
        currentHairIndex = (currentHairIndex + 1) % player.hairSpriteLibraryArray.Length;
        PlayerPrefs.SetInt("currentHairIndex", currentHairIndex);
        LobbyController.Instance.LocalPlayerController.CmdUpdatePlayerHair(currentHairIndex);
    }

    public void PrevHair()
    {
        currentHairIndex = (currentHairIndex - 1 + player.hairSpriteLibraryArray.Length) % player.hairSpriteLibraryArray.Length;
        PlayerPrefs.SetInt("currentHairIndex", currentHairIndex);
        LobbyController.Instance.LocalPlayerController.CmdUpdatePlayerHair(currentHairIndex);
    }

    public void NextEyes()
    {
        currentEyesIndex = (currentEyesIndex + 1) % player.eyesSpriteLibraryArray.Length;
        PlayerPrefs.SetInt("currentEyesIndex", currentEyesIndex);
        LobbyController.Instance.LocalPlayerController.CmdUpdatePlayerEyes(currentEyesIndex);
    }

    public void PrevEyes()
    {
        currentEyesIndex = (currentEyesIndex - 1 + player.eyesSpriteLibraryArray.Length) % player.eyesSpriteLibraryArray.Length;
        PlayerPrefs.SetInt("currentEyesIndex", currentEyesIndex);
        LobbyController.Instance.LocalPlayerController.CmdUpdatePlayerEyes(currentEyesIndex);
    }
}
