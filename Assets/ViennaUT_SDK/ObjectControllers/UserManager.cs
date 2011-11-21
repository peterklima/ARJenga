/*==============================================================================
            Copyright (c) 2010-2011 Vienna University of Technology
            Interactive Media Systems Group
            Gerstweiler, G., Mossel, A., Schoenauer, C. 
            All Rights Reserved.
==============================================================================*/
using UnityEngine;
using System.Collections;

/// <summary>
/// This class manages all connected players.
/// </summary>
public class UserManager : ScriptableObject {

    static readonly object padlock = new object();
    private static UserManager manager;

    /// <summary>
    /// The player that is returned if the Client is not connected
    /// </summary>
    /// 
    public static NetworkPlayer nonExistingPlayer = new NetworkPlayer();
    
	// Add a data-structure, that can help you map the NetworkPlayers to name-strings
	private static Hashtable _networkPlayerMap = new Hashtable();
  
    private Hashtable networkPlayerMap
    {
        get
        {
            return _networkPlayerMap;
        }
    }


    /// <summary>
    /// Singleton method returning the instance of UserManager
    /// </summary>
    public static UserManager instance
    {
        get
        {
            lock (padlock)
            {
                return (manager ? manager : manager = new UserManager());
            }
        }
    }
    /// <summary>
    /// Called by UserManagementObjectController on the server whenever a new player has successfully connected.
    /// </summary>
    /// <param name="player">Newly connected player</param>
    /// <param name="isClient">True if Client connected</param>
    public void AddNewPlayer(NetworkPlayer player,bool isClient)
    {
        if (!networkPlayerMap.ContainsKey(player))
        {

            int playerCounter = 1;
            while (networkPlayerMap.ContainsValue("player" + System.Convert.ToString(playerCounter)))
            {
                playerCounter++;
            }

            Debug.Log("UserManager Unity player " + player.ToString() + "added as UserManager player" + System.Convert.ToString(playerCounter));
            networkPlayerMap.Add(player,"player" + System.Convert.ToString(playerCounter));

			if(isClient)
			{
				Debug.Log("RPC Call setname sent to player"+System.Convert.ToString(playerCounter));
				GameObject.Find("GUIObj").networkView.RPC("setName", player, "player" + System.Convert.ToString(playerCounter));
			}
			else
			{
				Debug.Log("Server setname sent to player"+System.Convert.ToString(playerCounter));
				GameObject.Find("GUIObj").GetComponent<MobileNetworkGUI>().setName("player" + System.Convert.ToString(playerCounter));
			}
        }
    }

    /// <summary>
    /// Called by UserManagementObjectController on the server whenever a player disconnected from the server.
    /// </summary>
    /// <param name="player">Disconnected player</param>
    public void OnPlayerDisconnected(NetworkPlayer player) 
    {

		if (networkPlayerMap.ContainsKey(player))
        {
            Debug.Log("UserManager player " + player.ToString() + "disconnected as " + networkPlayerMap[player]);
            networkPlayerMap.Remove(player);
        }
    }

    /// <summary>
    /// Looks up the NetworkPlayer associated with the name
    /// </summary>
    /// <param name="playerName">Name of the NetworkPlayer</param>
    /// <returns>NetworkPlayer reference</returns>
    public NetworkPlayer getNetworkPlayer(string playerName)
    {
        foreach (DictionaryEntry obj in networkPlayerMap)
        {
            NetworkPlayer networkPlayer = (NetworkPlayer)obj.Key;
            string networkPlayerName = obj.Value as string;
            if (networkPlayerName.Equals(playerName))
            {
                return networkPlayer;
            }
        }
		
        return nonExistingPlayer;
    }
	
	/// <summary>
    /// Checks wether given player has rights to select and manipulte object over the network
    /// </summary>
    /// <param name="player">The NetworkPlayer who wants to access the selectedObj</param>
    /// <param name="selectedObj">The Object which shall be selected and manipulated</param>
    /// <returns>NetworkPlayer reference</returns>
	public bool isAccessAllowed(NetworkPlayer player, GameObject selectedObj)
	{
		if(selectedObj.GetComponent<UserManagementObjectController>() != null)
		{
			Debug.Log(selectedObj.GetComponent<UserManagementObjectController>().getAccessGrantedPlayer());
			return (selectedObj.GetComponent<UserManagementObjectController>().getAccessGrantedPlayer()==player);
		}
		return true;
	}
}
