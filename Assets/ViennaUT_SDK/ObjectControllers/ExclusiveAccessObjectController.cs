/*==============================================================================
            Copyright (c) 2010-2011 Vienna University of Technology
            Interactive Media Systems Group
            Gerstweiler, G., Mossel, A., Schoenauer, C. 
            All Rights Reserved.
==============================================================================*/

using UnityEngine;
using System.Collections;


/// <summary>
/// This class makes sure only one client at a time can access the object
/// </summary>
public class ExclusiveAccessObjectController : NetworkObjectController
{
    /// <summary>
    /// The default client used if the object is not accessed.
    /// </summary>
    /// 
    protected static NetworkPlayer defaultAccessPlayer = new NetworkPlayer();

    /// <summary>
    /// The client currently "accessing" the object.
    /// </summary>
    /// 

    protected NetworkPlayer currentAccessPlayer = defaultAccessPlayer;

    /// <summary>
    /// RPC-method that turns on/off selection of the controlled object. 
    /// Also sets the parameters required for certain components.
    /// e.g. Physics. Overrides the one from NetworkObjec Controller
    /// </summary>
    /// <param name="select">true for select, false for unselect</param>
    /// <param name="info">Info about the caller-client</param>
    /// <returns>true if object not already selected, false otherwise</returns>
    [RPC]
    public override bool controlSelectedObjects(bool select, NetworkViewID viewID, NetworkMessageInfo info)
    {
        if (isObjectAccessGranted(info.sender))
        {
            if (base.controlSelectedObjects(select,viewID,info))
            {
				
                if (select)
                {
                    currentAccessPlayer = info.sender;
                }
                else
                {
                    currentAccessPlayer = defaultAccessPlayer;
                }
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if the player is currently the exclusive access user of the object.
    /// </summary>
    /// <param name="player">Player</param>
    /// <returns>True if the player accesses the object, false otherwise.</returns>
    protected bool isAccessingObject(NetworkPlayer player)
    {
        
		if (player == currentAccessPlayer)
        {
            return true;
        }
        else
        {
            return false;
        }
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
            if (isAccessingObject(player) || (currentAccessPlayer == defaultAccessPlayer))
            {
                return true;
            }
        }
        else
        {
            return false;
        }
        return false;
        
    }
    /// <summary>
    /// If object is selected by Player disconnecting then we have to unselect.
    /// </summary>
    /// <param name="player">Player that disconnected</param>
    protected virtual void OnPlayerDisconnected(NetworkPlayer player)
    {
        if (isAccessingObject(player))
        {
            currentAccessPlayer = defaultAccessPlayer;

            //unselect
            NetworkMessageInfo info= new NetworkMessageInfo();
            base.controlSelectedObjects(false, this.networkView.viewID, info);
        }
    }


}
