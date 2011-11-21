/*==============================================================================
            Copyright (c) 2010-2011 Vienna University of Technology
            Interactive Media Systems Group
            Gerstweiler, G., Mossel, A., Schoenauer, C. 
            All Rights Reserved.
==============================================================================*/

using UnityEngine;
using System.Collections;

/// <summary>
/// InteractionBase is a component, that provides methods
/// for selection and manipulation of Objects that have the
/// ObjectController/NetworkObjectController-component. 
/// 
/// This class also cares for distribution of selection and
/// transformation - commands over the network if necessary.
/// 
/// </summary>
public class InteractionBase : MonoBehaviour {


    /// <summary>
    /// Stores the objects that have been selected.
    /// Key is instanceID, Value is reference to object.
    /// </summary>
    private Hashtable interactionObjs = new Hashtable();
    /// <summary>
    /// Counter that holds the number times an object has been selected.
    /// This is important with object-hierarchies. If an object gets selected
    /// multiple times via sub-objects we want to make sure the object is only
    /// unselected if all the sub-objects are unselected.
    /// </summary>
    private Hashtable interObjContCounter = new Hashtable();

    /// <summary>
    /// Adds the object to the selected-objects list and informs
    /// the object-owner(server or local) of the selection
    /// </summary>
    /// <param name="interactionObj">Gameobject we are adding</param>
    /// <returns>Returns true if interactionObj can be added to list (i.e not already in hashtable)</returns>
    public bool addInteractionObj(GameObject interactionObj)
    {
        interactionObj=getObjectControllerParent(interactionObj);

        if (this.addInterObjCounter(interactionObj))//TODO test if that works here??
        {
            if (!interactionObjs.ContainsKey(interactionObj.GetInstanceID()))
            {
                if (hasNetworkObjectController(interactionObj))
                {
					//TODO: We add it despite we dont know if we are allowed
					// Debug.Log( "Network.player" + (interactionObj.GetComponent<NetworkObjectController>().isObjectAccessGranted(Network.player)));
					if(Network.isServer)
					{
						interactionObj.GetComponent<NetworkObjectController>().controlSelectedObjects(true, interactionObj.networkView.viewID, new NetworkMessageInfo());
					}
					else
					{
                    	interactionObj.networkView.RPC("controlSelectedObjects", RPCMode.Server, true, interactionObj.networkView.viewID);
					}
                    interactionObjs.Add(interactionObj.GetInstanceID(), interactionObj);//TODO: We add it despite we dont know if we are allowed
                    Debug.Log("addInteractionObj");
                    return true;
                }
                else if (hasObjectController(interactionObj))
                {
                    interactionObj.GetComponent<ObjectController>().controlSelectedObjects(true);
                    interactionObjs.Add(interactionObj.GetInstanceID(), interactionObj);
                    Debug.Log("addInteractionObj local");
                    return true;
                }
                else
                {
                    Debug.Log("addInteractionoObj fail");
                }
            }
        }
    
        return false;
    }

    /// <summary>
    /// Removes the object from the selected-objects list and informs
    /// the object-owner(server or local) of the un-selection
    /// </summary>
    /// <param name="interactionObj">Gameobject we are removing</param>
    /// <returns>Returns true if interactionObj can be removed from the map (i.e it is in the list)</returns>

    public bool removeInteractionObj(GameObject interactionObj)
    {
        interactionObj = getObjectControllerParent(interactionObj);
        if (interactionObjs.ContainsKey(interactionObj.GetInstanceID()))
        {
            if (this.removeInterObjCounter(interactionObj))
            {    
                if (hasNetworkObjectController(interactionObj))
                {
					if(Network.isServer)
					{
						interactionObj.GetComponent<NetworkObjectController>().controlSelectedObjects(false, interactionObj.networkView.viewID, new NetworkMessageInfo());
					}
					else
					{
                   	 interactionObj.networkView.RPC("controlSelectedObjects", RPCMode.Server, false, interactionObj.networkView.viewID);
					}
                    interactionObjs.Remove(interactionObj.GetInstanceID());
                    Debug.Log("removeInteractionObj");
                    return true;
                }
                else if (hasObjectController(interactionObj))
                {
                    interactionObj.GetComponent<ObjectController>().controlSelectedObjects(false);
                    interactionObjs.Remove(interactionObj.GetInstanceID());
                    Debug.Log("removeInteractionObj-local");
                    return true;
                }
            }           
        }
        return false;
    }

    /// <summary>
    /// Removes all selected Objects from the list
    /// </summary>
    public void Clear()
    {
       //THis is a bit complicated because we cant delete entries in a loop
       // Create space for the dictionary entries.
       DictionaryEntry[] dictionaryEntries = new DictionaryEntry[interactionObjs.Count];

       // Copy.
       interactionObjs.CopyTo(dictionaryEntries, 0);

       // Iterate through the keys.
       foreach (DictionaryEntry dictionaryEntry in dictionaryEntries)
       {
            GameObject interactionObj = dictionaryEntry.Value as GameObject;
            if (interObjContCounter.ContainsKey(interactionObj.GetInstanceID()))
            {
                while (!removeInteractionObj(interactionObj))
                {
                }
            }
            else
            {
                Debug.LogError("Trying to delete object " +interactionObj.GetInstanceID() + "from interactionObjConCounter, which isnt there.");
            }
       }
    }

    /// <summary>
    /// Sets the transform of all selected objects to the given translation and orientation
    /// </summary>
    /// <param name="_position">Absolute WC position of the object is moved</param>
    /// <param name="_orientation">Absolute WC rotation</param>
    /// <returns></returns>
    protected void transformInterBase(Vector3 _position, Quaternion _orientation,Vector3 grabPoint)
    {
        foreach (DictionaryEntry obj in interactionObjs)
        {
            GameObject interactionObj = obj.Value as GameObject;

            if (hasNetworkObjectController(interactionObj))
            {
                NetworkView interactionObj_netWorkView = interactionObj.networkView;
                try
                {
                    if(Network.isServer)
					{
						interactionObj.GetComponent<NetworkObjectController>().updateTransform(interactionObj.networkView.viewID, _position, _orientation,grabPoint, new NetworkMessageInfo());
					}
					else
					{
                        interactionObj_netWorkView.RPC("updateTransform", RPCMode.Server, interactionObj.GetComponent<NetworkView>().viewID, _position, _orientation,grabPoint);
                        //TODO: Local transform as "Prediction"
                        //interactionObj.GetComponent<NetworkObjectController>().updateTransform(_position, _orientation);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.Log("Cannt modify" + interactionObj.name + ex.Message);
                }
            }

            else if (hasObjectController(interactionObj))
            {
                interactionObj.GetComponent<ObjectController>().updateTransform(_position, _orientation, grabPoint);
            }

        }
    }
	
	
	/// <summary>
    /// Sets the transform of all selected objects to the given translation and orientation and scale
    /// </summary>
    /// <param name="_position">Absolute WC position of the object is moved</param>
    /// <param name="_orientation">Absolute WC rotation</param>
    /// <returns></returns>
    protected void transformInterAllBase(Vector3 _position, Quaternion _orientation,Vector3 grabPoint, bool doScale)
    {
        foreach (DictionaryEntry obj in interactionObjs)
        {
            GameObject interactionObj = obj.Value as GameObject;

            if (hasNetworkObjectController(interactionObj))
            {
                NetworkView interactionObj_netWorkView = interactionObj.networkView;
                try
                {
                    if(Network.isServer)
					{
						interactionObj.GetComponent<NetworkObjectController>().updateTransformAll(interactionObj.networkView.viewID, _position, _orientation,grabPoint, doScale, new NetworkMessageInfo());
					}
					else
					{
                        interactionObj_netWorkView.RPC("updateTransformAll", RPCMode.Server, interactionObj.GetComponent<NetworkView>().viewID, _position, _orientation,grabPoint, doScale);
                        //TODO: Local transform as "Prediction"
                        //interactionObj.GetComponent<NetworkObjectController>().updateTransform(_position, _orientation);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.Log("Cannt modify" + interactionObj.name + ex.Message);
                }
            }

            else if (hasObjectController(interactionObj))
            {
                interactionObj.GetComponent<ObjectController>().updateTransformAll(_position, _orientation, grabPoint, doScale);
            }

        }
    }
	
    /// <summary>
    /// Checks if the GameObject, passed as parameter has a NetworkView-Component
    /// and a NetworkObjectController-Component and if they are properly connected
    /// </summary>
    /// <param name="interactionObj">GameObject that is checked</param>
    /// <returns>Returns true if the GameObject, passed as parameter has a OBjectController-Component
    /// and a NetworkObjectController-Component and if they are properly connected, false otherwise.</returns>
    private bool hasNetworkObjectController(GameObject interactionObj)
    {
        if (!Network.isServer && !Network.isClient)
        {
            return false;
        }
        if (interactionObj != null)
        {
			interactionObj=getObjectControllerParent(interactionObj);
            NetworkView interactionObj_netWorkView = interactionObj.networkView;
            if (interactionObj_netWorkView != null)
            {
                if (interactionObj.GetComponent<NetworkObjectController>() != null)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if the GameObject, passed as parameter has an OBjectController-Component.
    /// </summary>
    /// <param name="interactionObj">GameObject that is checked</param>
    /// <returns>Returns true if the GameObject, passed as parameter has an OBjectController-Component.</returns>
    protected bool hasObjectController(GameObject interactionObj)
    {
        if (interactionObj != null)
        {
			interactionObj=getObjectControllerParent(interactionObj);
            if (interactionObj.GetComponent<ObjectController>() !=null)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Searches next Parent that has a ObjectController-component.
    /// </summary>
    /// <param name="interactionObj">Child object</param>
    /// <returns>Parent object that has a ObjectController-component</returns>
    private GameObject getObjectControllerParent(GameObject interactionObj)
    {
        while ((interactionObj.GetComponent<ObjectController>() == null) && (interactionObj.transform.parent != null))
        {
            interactionObj = interactionObj.transform.parent.gameObject;
        }

        return interactionObj;
    }

    /// <summary>
    /// Adds one to the counter of the parent object that has a ObjectController-component.
    /// i.e. if the parent-object is not selected via another sub-object the counter is set to 1.
    /// otherwise the counter is incremented
    /// </summary>
    /// <param name="iteractionObj">Child object</param>
    /// <returns>True if the object has not yet been selected, false otherwise.</returns>
    private bool addInterObjCounter(GameObject iteractionObj)
    {
        iteractionObj = getObjectControllerParent(iteractionObj);
        if (interObjContCounter.ContainsKey(iteractionObj.GetInstanceID()))
        {
            interObjContCounter[iteractionObj.GetInstanceID()] = (int)(interObjContCounter[iteractionObj.GetInstanceID()]) + 1;
        }
        else
        {
            interObjContCounter.Add(iteractionObj.GetInstanceID(), 1);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Remove one of the counter of the parent object that has a ObjectController-component.
    /// i.e. if the parent-object is not selected via another sub-object the entry is removed from the hastable.
    /// otherwise the counter is decremented
    /// </summary>
    /// <param name="iteractionObj">Child object</param>
    /// <returns>True if the object is not selected by another sub-object, false otherwise.</returns>
    private bool removeInterObjCounter(GameObject interactionObj)
    {
        interactionObj = getObjectControllerParent(interactionObj);
        
        if (interObjContCounter.ContainsKey(interactionObj.GetInstanceID()))
        {
            interObjContCounter[interactionObj.GetInstanceID()] = (int)(interObjContCounter[interactionObj.GetInstanceID()]) - 1;
            if ((int)(interObjContCounter[interactionObj.GetInstanceID()])== 0)
            {
                interObjContCounter.Remove(interactionObj.GetInstanceID());
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks whether the Interaction-technique is owner of the interaction-object the script is attached to.
    /// i.e. if it is run locally it is the owner by default if not the User of the NetworkView is checked.
    /// </summary>
    /// <returns>True if it is the owner</returns>
    protected bool isOwnerCallback()
    {
        return ((!Network.isClient && !Network.isServer) || networkView.isMine);
        
    }
}
