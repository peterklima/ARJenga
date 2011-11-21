/*==============================================================================
            Copyright (c) 2010-2011 Vienna University of Technology
            Interactive Media Systems Group
            Gerstweiler, G., Mossel, A., Schoenauer, C. 
            All Rights Reserved.
==============================================================================*/

using UnityEngine;
using System.Collections;

/// <summary>
/// This class assigns right to a certain user to access an object.
/// </summary>
public class UserManagementObjectController : ExclusiveAccessObjectController
{
    /// <summary>
    /// Name of the player that is assigned in Unity-editor, only player1, player2,.... allowed
    /// </summary>
    public string accessGrantedName = "player1";

    /// <summary>
    /// NetworkPlayer that is associated with the name above in UserManager
    /// </summary>
    private NetworkPlayer accessGrantedPlayer;
	
	public NetworkPlayer getAccessGrantedPlayer()
	{
		return accessGrantedPlayer;
	}
    /// <summary>
    /// Checks whether the NetworkPlayer is granted access to the object.
    /// </summary>
    /// <param name="player">NetworkPlayer</param>
    /// <returns>true if it is the access granted, false otherwise</returns>
    protected override bool isObjectAccessGranted(NetworkPlayer player)
    {
        if(base.isObjectAccessGranted(player))
        {
            //return false;
			if (isAccessGrantedPlayer(player))
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        else
        {
            return false;
        }
        
    }
    
    /// <summary>
    /// Server-Callback
    /// If Player connects we update the UserManager and look up if the access
    /// </summary>
    /// <param name="player">Player that disconnected</param>
    void OnPlayerConnected(NetworkPlayer player)
    {
        UserManager.instance.AddNewPlayer(player,true);
        accessGrantedPlayer = UserManager.instance.getNetworkPlayer(accessGrantedName);
    }
	
	void OnServerInitialized(NetworkPlayer player) 
	{
		UserManager.instance.AddNewPlayer(player,false);
        accessGrantedPlayer = UserManager.instance.getNetworkPlayer(accessGrantedName);
	}

    /// <summary>
    /// Server-Callback
    /// If a player, that is allowed to access the object exits, we have to reset.
    /// </summary>
    /// <param name="player">Player that disconnected</param>
    protected new virtual void OnPlayerDisconnected(NetworkPlayer player)
    {
        UserManager.instance.OnPlayerDisconnected(player);
        if (isAccessGrantedPlayer(player))
        {
            base.OnPlayerDisconnected(player);
            accessGrantedPlayer = UserManager.nonExistingPlayer;
        }
    }

    /// <summary>
    /// Check if current player is the one the object is assigned to
    /// </summary>
    /// <param name="player">Networkplayer in question</param>
    /// <returns>True if the object is assigned to the player, false otherwise</returns>
    bool isAccessGrantedPlayer(NetworkPlayer player)
    {
        if (player == accessGrantedPlayer)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
}
