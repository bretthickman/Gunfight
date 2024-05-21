using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;
using Mirror;
using Steamworks;
using TMPro;
using UnityEngine.Rendering.UI;

public class SteamLobby : MonoBehaviour
{
    public static SteamLobby Instance;

    protected Callback<LobbyCreated_t> LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> JoinRequest;
    protected Callback<LobbyEnter_t> LobbyEntered;

    protected Callback<LobbyMatchList_t> LobbyList;
    protected Callback<LobbyDataUpdate_t> LobbyDataUpdated;

    protected CallResult<NumberOfCurrentPlayers_t> PlayerCount;

    public List<CSteamID> lobbyIDs = new List<CSteamID>();

    public ulong CurrentLobbyID;
    private const string HostAddressKey = "HostAddress";
    private CustomNetworkManager manager;

    public int numGlobalPlayers = 0;
    private float timer = 5f;
    private float interval = 5f;
    public TextMeshProUGUI playerCountText;

    public bool isJoining = false;

    public bool isQuickStart = false;

    public int lobbyMemberLimit = 4;

    private void Start()
    {
        if (!SteamManager.Initialized) { return; }

        if(Instance == null) { Instance = this; }

        manager = GetComponent<CustomNetworkManager>();

        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);

        LobbyList = Callback<LobbyMatchList_t>.Create(OnGetLobbyList);
        LobbyDataUpdated = Callback<LobbyDataUpdate_t>.Create(OnGetLobbyData);

        PlayerCount = CallResult<NumberOfCurrentPlayers_t>.Create(OnNumberOfCurrentPlayers);
    }

    private void Update()
    {
        // Increment the timer with the time passed since the last frame
        timer += Time.deltaTime;

        // Check if 5 seconds have passed
        if (SceneManager.GetActiveScene().name == "MainMenu" && timer >= interval)
        {
            SteamAPICall_t handle = SteamUserStats.GetNumberOfCurrentPlayers();
            PlayerCount.Set(handle);
            timer = 0f;
            playerCountText.text = "Player Count: " + numGlobalPlayers.ToString();
        }
    }

    public void QuickStart()
    {
        isQuickStart = true;
        GetLobbiesListFiltered();
    }

    public void HostLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, manager.maxConnections);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK) { return; }

        Debug.Log("Lobby Created");

        manager.StartHost();

        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", SteamFriends.GetPersonaName().ToString() + "'s Lobby");
        SteamMatchmaking.SetLobbyMemberLimit(new CSteamID(callback.m_ulSteamIDLobby), lobbyMemberLimit);
    }

    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        Debug.Log("Request to Join Lobby");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        CurrentLobbyID = callback.m_ulSteamIDLobby;

        if (NetworkServer.active) { return; }

        manager.networkAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);

        manager.StartClient();
    }

    public void JoinLobby(CSteamID lobbyID)
    {
        SteamMatchmaking.JoinLobby(lobbyID);
    }

    public void LeaveLobby()
    {
        SteamMatchmaking.LeaveLobby((CSteamID)CurrentLobbyID);
    }

    public void HideLobby()
    {
        if (CurrentLobbyID != 0)
        {
            SteamMatchmaking.SetLobbyData((CSteamID)CurrentLobbyID, "hidden", "1");
        }
    }

    public void UnhideLobby()
    {
        if (CurrentLobbyID != 0)
        {
            SteamMatchmaking.SetLobbyData((CSteamID)CurrentLobbyID, "hidden", "0");
        }
    }

    public void GetLobbiesList()
    {
        if(lobbyIDs.Count > 0) { lobbyIDs.Clear(); }

        // Add filters and then tell steam to get lobbyIds within a CALLBACK
        SteamMatchmaking.AddRequestLobbyListResultCountFilter(60);
        SteamMatchmaking.RequestLobbyList();
    }

    public void GetLobbiesListFiltered()
    {
        if (lobbyIDs.Count > 0) { lobbyIDs.Clear(); }

        SteamMatchmaking.RequestLobbyList();
    }

    void OnGetLobbyList(LobbyMatchList_t result)
    {
        if(LobbiesListManager.instance.listOfLobbies.Count > 0) { LobbiesListManager.instance.DestroyLobbies(); }

        for (int i = 0; i < result.m_nLobbiesMatching; i++)
        {
            CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
            string hiddenValue = SteamMatchmaking.GetLobbyData(lobbyID, "hidden");

            if (hiddenValue != "1") // Check if lobby is not hidden
            {
                lobbyIDs.Add(lobbyID);
            }

            SteamMatchmaking.RequestLobbyData(lobbyID);
        }

        if(isQuickStart == true)
        {
            isQuickStart = false;
            if (lobbyIDs.Count != 0)
            {
                SteamMatchmaking.JoinLobby(lobbyIDs[0]);
            }
            else
            {
                SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, manager.maxConnections);
            }
        } 
    }

    void OnGetLobbyData(LobbyDataUpdate_t result)
    {
        if (SceneManager.GetActiveScene().name != "Lobby" && isJoining == false)
        {
            LobbiesListManager.instance.DisplayLobbies(lobbyIDs, result);
        }
    }

    private void OnNumberOfCurrentPlayers(NumberOfCurrentPlayers_t pCallback, bool bIOFailure)
    {
        if (pCallback.m_bSuccess != 1 || bIOFailure)
        {
            Debug.Log("There was an error retrieving the NumberOfCurrentPlayers.");
        }
        else
        {
            //Debug.Log("The number of players playing your game: " + pCallback.m_cPlayers);
            numGlobalPlayers = pCallback.m_cPlayers;
        }
    }
}

