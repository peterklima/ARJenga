/*==============================================================================
            Copyright (c) 2010-2011 Vienna University of Technology
            Interactive Media Systems Group
            Gerstweiler, G., Mossel, A., Schoenauer, C. 
            All Rights Reserved.
==============================================================================*/

using UnityEngine;

/// <summary>
/// Obsorver the transform node of an arbitary tracker object map transform values of tracker directly to 
/// transform node of GameObject to which this class is attached to.
/// </summary>
public class TrackerTransformObserver : MonoBehaviour
{
	private GameObject obj = null;
	private Vector3 oldPosition;
	private Quaternion oldRotation;
	
	public string object_name = "ARCamera";
	
	/// <summary>
	/// Unity Callback
    /// Awake is called when the script instance is being loaded.
	/// </summary>
	void Awake()
    {
		// find game object to observe transform node
		obj = GameObject.Find(object_name);
		
		// save set transform node values
		oldPosition = transform.localPosition;
		oldRotation = transform.localRotation;
    }
	
	void Update()
	{
		// check if i am the owner to avoid double update from server and client
		if(this.networkView.isMine) 
		{
			// get transform update of object_name game object
			this.transform.position = obj.transform.position + oldPosition;
			this.transform.rotation = obj.transform.rotation * oldRotation;
		}
	}
	
}


