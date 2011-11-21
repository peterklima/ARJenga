/*==============================================================================
            Copyright (c) 2010-2011 Vienna University of Technology
            Interactive Media Systems Group
            Gerstweiler, G., Mossel, A., Schoenauer, C. 
            All Rights Reserved.
==============================================================================*/
using UnityEngine;
using System.Collections;

/// <summary>
/// Class that provides methods for the main network functionality
/// </summary>
public class NetworkBase : MonoBehaviour {

    public int serverPort = 12346;
	public string serverAddress="127.0.0.1";
    public int maxPlayers = 2;
	public int netwSendRate = 25;

    /// <summary>
    /// Starts the server with the configured parameters(port,maxPlayers,sendrate etc.)
    /// </summary>
	public void StartServer()
	{
		
		NetworkConnectionError returnError=Network.InitializeServer(maxPlayers, serverPort, !Network.HavePublicAddress());
		Network.sendRate = netwSendRate;
        Debug.Log("Server called initialize: " + returnError);
		
	}
	
    /// <summary>
    /// Connects to server with the specified parameters.
    /// </summary>
    /// <param name="host">Hostname of the server</param>
    /// <param name="port">Port the server is running at</param>
	public void ConnectToServer(string host, int port)
	{
		serverAddress=host;
		serverPort=port;
		NetworkConnectionError returnError = Network.Connect(host, port);
		Debug.Log("Connect called: " + returnError);
	}
	 
	/// <summary>
	/// Server callback
    /// See Unity-Script-Reference
	/// </summary>
    void OnServerInitialized()
    {
        Debug.Log("OnServerInitialized: "+Network.player.ipAddress+":"+Network.player.port);
    }

    /// <summary>
    /// Server callback
    /// See Unity-Script-Reference
    /// </summary>
    void OnPlayerConnected(NetworkPlayer player)
    {
        Debug.Log("New player connected from " + player.ipAddress + ":" + player.port);
        //UserManager.instance.OnPlayerConnected(player);
	}

    /// <summary>
    /// Server callback
    /// See Unity-Script-Reference
    /// </summary>
    void OnPlayerDisconnected(NetworkPlayer player)
    {
        Debug.Log("Player has disconnected " + player.ipAddress + ":" + player.port);
        Debug.Log("Server destroying player");
        Network.RemoveRPCs(player, 0);
        Network.DestroyPlayerObjects(player);
        //UserManager.instance.OnPlayerDisconnected(player);
    }

    /// <summary>
    /// Client callback
    /// See Unity-Script-Reference
    /// </summary>
	void OnConnectedToServer()
    {
        Debug.Log("OnConnectedToServer()");
    }

    /// <summary>
    /// Client callback
    /// See Unity-Script-Reference
    /// </summary>
    void OnFailedToConnect(NetworkConnectionError error)
    {
        Debug.Log("Could not connect to server: "+ error);
        Debug.Log("Retry to connect to: " + serverAddress + " - " + serverPort);
        // Try to connect to server
		ConnectToServer(serverAddress,serverPort);
    }
}
