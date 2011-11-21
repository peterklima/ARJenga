/*==============================================================================
            Copyright (c) 2010-2011 Vienna University of Technology
            Interactive Media Systems Group
            Gerstweiler, G., Mossel, A., Schoenauer, C. 
            All Rights Reserved.
==============================================================================*/
using UnityEngine;
using System.Collections;

/// <summary>
/// Class that can instantiate a Prefab
/// </summary>
public class SpawnPrefab : MonoBehaviour {
	public string PathInHierarchy="";
    /// <summary>
    /// Instantiate locally for non-network use
    /// </summary>
    //public bool instantiateLocally = false;

    /// <summary>
    /// Prefab that is to be instantiated
    /// </summary>
	public Transform playerPrefab;
	
	public bool spawnOnServer=false;
	public bool spawnOnClient=true;
	
	GameObject newObj=null;
	
    /// <summary>
    /// Client callback that instantiates the Prefab
    /// </summary>
    public void OnConnectedToServer()
	{
		if(spawnOnClient)
		{
			SpawnNetworkObject();
		}
	}
	
	/// <summary>
    /// Server callback that instantiates the Prefab
    /// </summary>
	void OnServerInitialized() 
	{
		if(spawnOnServer)
		{
			SpawnNetworkObject();
		}
	}
	
	/*public void OnPlayerConnected()
	{
		relocateObject();
	}*/
	
    private void SpawnNetworkObject()
    {
        //create prefab
        Debug.Log("Create Prefab on Client");
		
        Network.Instantiate(playerPrefab, transform.position, transform.rotation, 0);
		Debug.Log("spawning pos " + transform.position);
		this.networkView.RPC("relocateObjectRPC", RPCMode.AllBuffered);
		
    }
	
	private void relocateObject()
	{
		string objName="/"+playerPrefab.name+"(Clone)";
		
		// by default new GameObjects are added to hierachy on root level
		newObj=GameObject.Find(objName);
		
		if((GameObject.Find(PathInHierarchy) != null) && (newObj != null))
		{

			// store scale and position
			Vector3 locPos = newObj.transform.localPosition;
			Vector3 locScale = newObj.transform.localScale;
			
			// find parent object and attach new object as child
			newObj.transform.parent = GameObject.Find(PathInHierarchy).transform;
			newObj.transform.localScale = locScale;
			newObj.transform.localPosition = locPos;

		}
		else 
		{
			Debug.Log("Spawned prefab found. No relocation needed.");
		}
	}
	
	[RPC]
    public virtual void relocateObjectRPC()
    {
        relocateObject();
    }
}
