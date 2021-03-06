﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonConnectionManager : Photon.PunBehaviour
{
    #region Public Variables

    /// <summary>
    /// The PUN loglevel. 
    /// </summary>
    public PhotonLogLevel Loglevel = PhotonLogLevel.Informational;

    /// <summary>
    /// The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created.
    /// </summary>   
    [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
    public byte MaxPlayersPerRoom = 3;

    private GameObject currentPlayer;
    public GameObject GameBallBasketBallMaster;
    public GameObject GameBallFootballMaster;
    public GameObject GameBallGymballMaster;
    #endregion

    #region Private Variables

    /// <summary>
    /// This client's version number. Users are separated from each other by gameversion (which allows you to make breaking changes).
    /// </summary>
    string _gameVersion = "1";
    public static int joinedPlayer = 0;
    private string gameballname = "basketball";


    #endregion

    #region MonoBehaviour CallBacks

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
    /// </summary>
    void Awake()
    {

        // #NotImportant
        // Force Full LogLevel
        PhotonNetwork.logLevel = Loglevel;

        // #Critical
        // we don't join the lobby. There is no need to join a lobby to get the list of rooms.
        PhotonNetwork.autoJoinLobby = false;

        // #Critical
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.automaticallySyncScene = true;
    }

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during initialization phase.
    /// </summary>
    void Start()
    {
        Connect();
    }

    #endregion


    #region Public Methods

    /// <summary>
    /// Start the connection process. 
    /// - If already connected, we attempt joining a random room
    /// - if not yet connected, Connect this application instance to Photon Cloud Network
    /// </summary>
    public void Connect()
    {

        // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
        if (PhotonNetwork.connected)
        {
            // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnPhotonRandomJoinFailed() and we'll create one.
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            // #Critical, we must first and foremost connect to Photon Online Server.
            PhotonNetwork.ConnectUsingSettings(_gameVersion);
        }
    }

    public override void OnConnectedToMaster()
    {

        // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnPhotonRandomJoinFailed()  
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnDisconnectedFromPhoton()
    {

        Debug.LogWarning("DemoAnimator/Launcher: OnDisconnectedFromPhoton() was called by PUN");
    }

    public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
    {
        Debug.Log("DemoAnimator/Launcher:OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 4}, null);");

        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        PhotonNetwork.CreateRoom(null, new RoomOptions() { maxPlayers = MaxPlayersPerRoom }, null);
    }

    public override void OnJoinedRoom()
    {
        joinedPlayer = PhotonNetwork.playerList.Length;
        Debug.Log("DemoAnimator/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room. PlayerNumber:" + joinedPlayer);
        PhotonNetwork.Instantiate("Avatar", new Vector3(-8, 0), Quaternion.identity, 0);
        GameObject player = GameObject.FindWithTag("Player");

        if (PhotonNetwork.isMasterClient)
        {
            player.transform.position = new Vector3(0, 0.7f, -10f);
            player.transform.rotation = transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            if (joinedPlayer == 2)
            {
                player.transform.position = new Vector3(7, 0.3f, 0.5f);
                player.transform.rotation = transform.rotation = Quaternion.Euler(0, -90, 0);
                photonView.RPC("setGameBall", PhotonTargets.MasterClient, GameBallSelection.gameball);
            }
            else
            {
                player.transform.position = new Vector3(-7, 0.3f, -0.5f);
                photonView.RPC("StartGame", PhotonTargets.MasterClient);
            }
            
        }
    }

    [PunRPC]
    void StartGame()
    {
        switch (gameballname)
        {
            case "gymball":
                Instantiate(GameBallGymballMaster, new Vector3(0, 1, 0), Quaternion.identity);
                PhotonNetwork.Instantiate("GoalBallGymClient", new Vector3(0, 0), Quaternion.identity, 0);
                break;
            case "football":
                Instantiate(GameBallFootballMaster, new Vector3(0, 1, 0), Quaternion.identity);
                PhotonNetwork.Instantiate("GoalBallFootClient", new Vector3(0, 0), Quaternion.identity, 0);
                break;
            default:
                Instantiate(GameBallBasketBallMaster, new Vector3(0, 1, 0), Quaternion.identity);
                PhotonNetwork.Instantiate("GoalBallClient", new Vector3(0, 0), Quaternion.identity, 0);
                break;
        }
    }

    [PunRPC]
    void setGameBall(string gameBall)
    {
        gameballname = gameBall;
    }
    
    #endregion
}
