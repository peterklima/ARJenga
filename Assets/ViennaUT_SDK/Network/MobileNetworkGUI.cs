/*==============================================================================
            Copyright (c) 2010-2011 Vienna University of Technology
            Interactive Media Systems Group
            Gerstweiler, G., Mossel, A., Schoenauer, C. 
            All Rights Reserved.
==============================================================================*/
using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Class NetworkGUi is responsible for setting up the GUI containing elements
/// for configuring client and server.
/// </summary>
[RequireComponent (typeof(NetworkBase))]

public class MobileNetworkGUI : MonoBehaviour {
	
	private bool _showNetworkMenu;

	public string serverAddress = "";
	public string serverPort = "";

	string playerName = "unknown";
	

	/// <summary>
	/// Unity Callback
    /// Awake is called when the script instance is being loaded.
	/// </summary>
	void Awake()
    {
		_showNetworkMenu = false;
		
        Application.targetFrameRate = 60;
		serverAddress = GetComponent<NetworkBase>().serverAddress;
		serverPort = GetComponent<NetworkBase>().serverPort.ToString();
    }
	
    /// <summary>
    /// Unity Callback
    /// OnGUI is called for rendering and handling GUI events.
    /// </summary>
	void OnGUI () {
		
	
		if (!_showNetworkMenu)
		{
			GUILayout.BeginArea(new Rect (0, Screen.height - 40, 240, 40));
			if (GUILayout.Button("Show Network Menu", GUILayout.MinHeight(40)))
			{
				_showNetworkMenu = true;
			}
			GUILayout.EndArea();
		} 
		else
		{

			Rect windowRect = new Rect (0, Screen.height - 300, 240, 250);
			
			if (!(Network.peerType == NetworkPeerType.Disconnected)) {
				windowRect = new Rect (0, Screen.height - 300, 240, 250);
			}
			
			// render network control menu
			windowRect = GUILayout.Window(1, windowRect, MakeWindow, "Network Controls");
			
			GUILayout.BeginArea(new Rect (0,Screen.height - 40, 240, 40));

			if (GUILayout.Button("Hide Network Menu", GUILayout.MinHeight(40)))
			{
				_showNetworkMenu = false;
			}

			GUILayout.EndArea();
		}
	}
	
    /// <summary>
    /// Sets up the GUI-elements
    /// </summary>
    /// <param name="id"></param>
	void MakeWindow(int id) {
		
		if (Network.peerType == NetworkPeerType.Disconnected) {
			GUILayout.Space (5);
			GUILayout.Label("Connection status: disconnected");
			GUILayout.Space (5);

			if (GUILayout.Button ("Start New Server", GUILayout.MinHeight(50))) {
				GetComponent<NetworkBase>().StartServer();
			}
			
			GUILayout.Space (15);	
			GUILayout.Label("Connect to existing server:");
			GUILayout.BeginHorizontal();
			serverAddress = GUILayout.TextField(serverAddress, GUILayout.MinHeight(30));
			serverPort = GUILayout.TextField(serverPort.ToString(), 6, GUILayout.MinHeight(30));
			GUILayout.EndHorizontal();
			GUILayout.Space (5);
			if (GUILayout.Button ("Connect to Server", GUILayout.MinHeight(50)))
			{
				GetComponent<NetworkBase>().ConnectToServer(serverAddress, Convert.ToInt32(serverPort));
			}
		} 
		else 
		{
			if (Network.peerType == NetworkPeerType.Connecting)
			{
				GUILayout.Label("Connection status: connecting ...");
			} 
			else if (Network.peerType == NetworkPeerType.Client)
			{
				GUILayout.Label("Connection status: connected as client");
				GUILayout.Label("Ping to server: "+Network.GetAveragePing(  Network.connections[0] ) );
                GUILayout.Label(playerName);
                GUILayout.Space(5);
				
			} 
			else if (Network.peerType == NetworkPeerType.Server)
			{
				GUILayout.Label("Connection status: running as server");
			}
			
			if (GUILayout.Button ("Disconnect", GUILayout.MinHeight(40)))
			{
				Network.Disconnect(500);
			}
		}
	}
	
    [RPC]
    public virtual void setName(string name)
    {
        Debug.Log("NetworkGUI Setname to:"+name);
        playerName = name;
    }
    
    
    /// <summary>
    /// Client Callback
    /// A Client that disconnects from the server reloads the scene 
    /// </summary>
    void OnDisconnectedFromServer()
    {
        Application.LoadLevel(0);
    }


}