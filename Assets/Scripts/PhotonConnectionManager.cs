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
    public GameObject GameBallMaster;
    #endregion

    #region Private Variables

    /// <summary>
    /// This client's version number. Users are separated from each other by gameversion (which allows you to make breaking changes).
    /// </summary>
    string _gameVersion = "1";
    public static int joinedPlayer = 0;



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
            player.transform.position = new Vector3(0, 10, 0);
            player.transform.rotation = transform.rotation = Quaternion.Euler(90, 0, 0);
        }
        else
        {
            if (joinedPlayer == 2)
            {
                player.transform.position = new Vector3(7, 0, 0);
                player.transform.rotation = transform.rotation = Quaternion.Euler(0, -90, 0);
            }
            else
            {
                player.transform.position = new Vector3(-7, 0, 0);
                photonView.RPC("StartGame", PhotonTargets.MasterClient);
            }
            
        }
    }

    [PunRPC]
    void StartGame()
    {
        Instantiate(GameBallMaster, new Vector3(0, 1, 0), Quaternion.identity);
        PhotonNetwork.Instantiate("GoalBallClient", new Vector3(0, 0), Quaternion.identity, 0);
    }

    /*void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        if (PhotonNetwork.isMasterClient)
        {
            currentPlayer = PhotonNetwork.Instantiate("Tennisball", new Vector3(0, 1.6f, 0), Quaternion.identity, 0);
        }
    }*/
    #endregion
}
