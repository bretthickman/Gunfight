using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.SceneManagement;
using System.Linq;

public class PlayerObjectController : NetworkBehaviour
{
    //Player Data
    [SyncVar] public int ConnectionID;
    [SyncVar] public int PlayerIdNumber;
    [SyncVar] public ulong PlayerSteamID;
    [SyncVar(hook = nameof(PlayerNameUpdate))] public string PlayerName;
    [SyncVar(hook = nameof(PlayerReadyUpdate))] public bool Ready;
    [SyncVar(hook = nameof(PlayerInGameUpdate))] public bool isInGame;
    [SyncVar(hook = nameof(PlayerTeamUpdate))] public int Team = 1;
    public bool isAlive = true;
    public int wins = 0;
    public int kills = 0;

    //Cosmestics / Team
    [SyncVar(hook = nameof(SendPlayerColor))] public int ColorIndex;
    [SyncVar(hook = nameof(SendPlayerBody))] public int BodyIndex;
    [SyncVar(hook = nameof(SendPlayerHair))] public int HairIndex;
    [SyncVar(hook = nameof(SendPlayerEyes))] public int EyesIndex;

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
        DontDestroyOnLoad(this.gameObject);
    }

    private void PlayerReadyUpdate(bool oldValue, bool newValue)
    {
        if (isServer)
        {
            this.Ready = newValue;
        }
        if (isClient)
        {
            LobbyController.Instance.UpdatePlayerList();
        }
    }

    private void PlayerInGameUpdate(bool oldValue, bool newValue)
    {
        if (isServer)
        {
            this.isInGame = newValue;
        }
        if (isClient)
        {
            LobbyController.Instance.UpdatePlayerList();
        }
    }

    [Command]
    private void CMDSetPlayerReady()
    {
        this.PlayerReadyUpdate(this.Ready, !this.Ready);
    }

    public void ChangeReady()
    {
        if (isOwned)
        {
            CMDSetPlayerReady();
        }
    }

    private void PlayerTeamUpdate(int oldValue, int newValue)
    {
        if (isServer)
        {
            this.Team = newValue;
        }
        if (isClient)
        {
            LobbyController.Instance.UpdatePlayerList();
        }
    }

    [Command]
    private void CMDSetPlayerTeam(int newTeam)
    {
        this.PlayerTeamUpdate(this.Team, newTeam);
    }

    public void ChangeTeam(int newTeam)
    {
        if (isOwned)
        {
            CMDSetPlayerTeam(newTeam);
        }
    }

    public override void OnStartAuthority()
    {
        CmdSetPlayerName(SteamFriends.GetPersonaName().ToString());
        gameObject.name = "LocalGamePlayer";
        LobbyController.Instance.FindLocalPlayer();
        LobbyController.Instance.UpdateLobbyName();
        // why are we calling this twice?
        // LobbyController.Instance.UpdateLobbyName();
    }

    public override void OnStartClient()
    {
        Manager.GamePlayers.Add(this);
        LobbyController.Instance.UpdateLobbyName();
        LobbyController.Instance.UpdatePlayerList();
    }

    public override void OnStopClient()
    {
        Manager.GamePlayers.Remove(this);
        LobbyController.Instance.UpdatePlayerList();
    }

    [Command]
    private void CmdSetPlayerName(string PlayerName)
    {
        this.PlayerNameUpdate(this.PlayerName, PlayerName);
    }

    public void PlayerNameUpdate(string OldValue, string NewValue)
    {
        if (isServer)
        {
            this.PlayerName = NewValue;
        }
        if (isClient)
        {
            LobbyController.Instance.UpdatePlayerList();
        }
    }

    public void CanStartGame(string SceneName)
    {
        if (isOwned)
        {
            CmdCanStartGame(SceneName);
        }
    }

    [Command]
    public void CmdCanStartGame(string SceneName)
    {
        manager.StartGame(SceneName);
    }

    [Command]
    public void CmdUpdatePlayerColor(int newValue)
    {
        SendPlayerColor(ColorIndex, newValue);
    }

    [Command]
    public void CmdUpdatePlayerBody(int newValue)
    {
        SendPlayerBody(BodyIndex, newValue);
    }

    [Command]
    public void CmdUpdatePlayerHair(int newValue)
    {
        SendPlayerHair(HairIndex, newValue);
    }

    [Command]
    public void CmdUpdatePlayerEyes(int newValue)
    {
        SendPlayerEyes(HairIndex, newValue);
    }
    
    [ClientRpc]
    void RpcClientQuit()
    {
        Quit();
    }

    public void SendPlayerColor(int oldValue, int newValue)
    {
        if (isServer)
        {
            ColorIndex = newValue;
        }
        if (isClient && (oldValue != newValue))
        {
            UpdateColor(newValue);
        }
    }

    public void SendPlayerBody(int oldValue, int newValue)
    {
        if(isServer)
        {
            BodyIndex = newValue;
        }
        if (isClient && (oldValue != newValue))
        {
            UpdateBody(newValue);
        }
    }

    public void SendPlayerHair(int oldValue, int newValue)
    {
        if (isServer)
        {
            HairIndex = newValue;
        }
        if (isClient && (oldValue != newValue))
        {
            UpdateHair(newValue);
        }
    }

    public void SendPlayerEyes(int oldValue, int newValue)
    {
        if (isServer)
        {
            EyesIndex = newValue;
        }
        if (isClient && (oldValue != newValue))
        {
            UpdateEyes(newValue);
        }
    }

    private int FindNextAvailableColor(int startColorIndex)
    {
        CharacterChanger changer = FindObjectOfType<CharacterChanger>();
        int colorCount = changer.playerColors.Length;
        for (int i = 0; i < colorCount; i++)
        {
            int index = (startColorIndex + i) % colorCount;
            if (!changer.IsColorInUse(index) && !TeamHasColor(index, (Team + 1) % 2))
            {
                return index;
            }
        }
        // If no available color found, return the start index (or handle as desired)
        return startColorIndex;
    }

    private bool TeamHasColor(int colorIndex, int teamId)
    {
        var teammates = FindObjectsOfType<PlayerObjectController>().Where(p => p.Team == Team);
        return teammates.Any(p => p.ColorIndex == colorIndex);
    }

    [Command]
    public void CmdChangeHairColorGunfight(int initialColorIndex)
    {
        int colorIndex = FindNextAvailableColor(initialColorIndex);
        ChangeHairColor(colorIndex);
    }

    [Server]
    private void ChangeHairColor(int colorIndex)
    {
        CharacterChanger characterChanger = FindObjectOfType<CharacterChanger>();
        // Release previous color
        if (characterChanger.IsColorInUse(this.ColorIndex))
        {
            characterChanger.SetColorInUse(this.ColorIndex, false);
        }

        // Set new color
        ColorIndex = colorIndex;
        characterChanger.SetColorInUse(colorIndex, true);

        RpcChangeTeamHairColor(colorIndex);
    }

    [ClientRpc]
    private void RpcChangeTeamHairColor(int newColorIndex)
    {
        var teammates = FindObjectsOfType<PlayerObjectController>().Where(p => p.Team == Team);
        foreach (var teammate in teammates)
        {
            teammate.UpdateColor(newColorIndex);
        }
    }

    //separte function because buggy if not
    void UpdateColor(int message)
    {
        ColorIndex = message;
        GetComponent<PlayerController>().SwitchHairColor(ColorIndex);
    }

    void UpdateBody(int message)
    { 
        BodyIndex = message;
        GetComponent<PlayerController>().SwitchBodySprite(BodyIndex);
    }

    void UpdateHair(int message)
    {
        HairIndex = message;
        GetComponent<PlayerController>().SwitchHairSprite(HairIndex);
    }

    void UpdateEyes(int message)
    {
        EyesIndex = message;
        GetComponent<PlayerController>().SwitchEyesSprite(EyesIndex);
    }

    public void Quit()
    {
        //Set the offline scene to null
        manager.offlineScene = "";

        //Make the active scene the offline scene
        SceneManager.LoadScene("MainMenu");


        if (isOwned)
        {
            if (isServer)
            {
                RpcClientQuit();
                manager.StopHost();
            }
            else
            {
                manager.StopClient();
            }
        }

        //Leave Steam Lobby
        SteamLobby.Instance.LeaveLobby();
    }
}
