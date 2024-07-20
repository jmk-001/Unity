using System.Collections;
using System.Collections.Generic;
using FishNet.Managing;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapManager : MonoBehaviour
{
    private static BootstrapManager instance;

    private void Awake() => instance = this;

    [SerializeField] private string menuName = "StartMenu";
    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private FishySteamworks.FishySteamworks _fishySteamworks;

    protected Callback<LobbyCreated_t> LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> JoinRequest;
    protected Callback<LobbyEnter_t> LobbyEntered;

    public static ulong CurrentLobbyId;
    
    void Start()
    {
        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public void GoToMenu(){
        SceneManager.LoadScene(menuName, LoadSceneMode.Additive);
    }

    public static void CreateLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 2);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK) return;

        CurrentLobbyId = callback.m_ulSteamIDLobby;
        SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyId), "HostAddress", SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyId), "name", SteamFriends.GetPersonaName().ToString()+"'s lobby");
        _fishySteamworks.SetClientAddress(SteamUser.GetSteamID().ToString());
        _fishySteamworks.StartConnection(true);
        Debug.Log("Lobby Creation Successful.");
    }

    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        CurrentLobbyId = callback.m_ulSteamIDLobby;

        MainmenuManager.LobbyEntered(SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyId), "name"), _networkManager.IsServerStarted);

        _fishySteamworks.SetClientAddress(SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyId), "HostAddress"));
        _fishySteamworks.StartConnection(false);
    }

    public static void JoinByID(CSteamID steamId)
    {
        Debug.Log("Attempting to join lobby with ID: " + steamId.m_SteamID);
        if (SteamMatchmaking.RequestLobbyData(steamId))
            SteamMatchmaking.JoinLobby(steamId);
        else
            Debug.Log("Failed to join lobby with ID: " + steamId.m_SteamID);
    }

    public static void LeaveLobby()
    {
        SteamMatchmaking.LeaveLobby(new CSteamID(CurrentLobbyId));
        CurrentLobbyId = 0;

        instance._fishySteamworks.StopConnection(false);
        if (instance._networkManager.IsServerStarted)
            instance._fishySteamworks.StopConnection(true);
    }
}
