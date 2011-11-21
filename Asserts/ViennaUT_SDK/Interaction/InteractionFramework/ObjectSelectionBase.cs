/*==============================================================================
            Copyright (c) 2010-2011 Vienna University of Technology
            Interactive Media Systems Group
            Gerstweiler, G., Mossel, A., Schoenauer, C. 
            All Rights Reserved.
==============================================================================*/

using UnityEngine;
using System.Collections;

/// <summary>
/// ObjectSelectionBase is a component, that derives from InteractionBase. All interaction techniques must derive from this class. 
/// It provides ths functionality to add/remove selectable game objects and to rotate a selected game objects realtive to marker or spacemouse transformation. 
/// </summary>
public class ObjectSelectionBase: InteractionBase
{
    protected bool selected = false; // flag indicated if currently objects are selected for manipulation
    protected Hashtable collidees = new Hashtable(); // Buffer selected objects until confirmation
	
    protected Quaternion prevRot = Quaternion.identity; 
	protected Vector3 prevPos=Vector3.zero;
	protected Quaternion selectTimeRot=Quaternion.identity;
	protected Vector3 selectTimePos=Vector3.zero;
	protected bool doSelect;
	
	/// <summary>
    /// Get the current selection state of the IT. 
    /// </summary>
    /// <returns>If object is currently selected, returns true otherwise false.</returns>
	public bool getSelectionState()
	{
		return selected;
	}
	
    /// <summary>
    /// </summary>
    public virtual void Update()
    {
        // selection of game objects
        if (isOwnerCallback())
        {
            doSelect = false;
            bool doUnselect = false;

            doSelect = false;
            doUnselect = false;

            if (GetSelectionTrigger())
            {
                if (!selected)
                {
                    doSelect = true;
                    //selected = true; - set when adding collidee to private hash interactionObjs to avoid that if is collidees is empty changing to selection mode is possible 
                }
                else
                {
                    doUnselect = true;
                    //selected = false;
                }
            }


            if (doSelect)
            {
                //for all the objects our Interaction-object is colliding with, select them by calling addInteractionObj
                foreach (DictionaryEntry coll in collidees)
                {
                    GameObject collidee = coll.Value as GameObject;
                    if (collidee != null)
                    {

                        collidee = collidee.gameObject;//Do we need that?
                        try
                        {
                            if (doSelect)
                            {
                                this.addInteractionObj(collidee);
                                Debug.Log("Selected" + collidee.gameObject.GetInstanceID());
								
								selected = true;
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Debug.Log("Cannt modify" + collidee.name + ex.Message);
                        }
                    }
                }
            }

            if (doUnselect)
            {
                //Unselect all objects
                this.Clear();
				selected = false;
				
                Debug.Log("Unselected all");
            }

            // ------------- Selection and Interaction technique implementation -------
			UpdateSelect();
        }	
    }
	
	/// <summary>
    /// Implement the particular selection and manipulation technique here -> must be overwritten in subclass.
    /// </summary>
	protected virtual void UpdateSelect()
	{
		Debug.Log("############## WRONG UPDATE SELECTED CALLED!");
	}
	
	/// <summary>
    /// Sets the transform of all selected objects to the given translation and orientation
    /// 
    /// TransformInter MUST use the pose of the InteractionGameObject (i.e. this.position and this.rotate of virtual hand of gogo IT)
    /// </summary>
    /// <param name="_position">Absolute WC position of the object is moved</param>
    /// <param name="_orientation">Absolute WC rotation</param>
    /// <returns></returns>
    protected virtual void transformInter(Vector3 _position, Quaternion _orientation)
	{
		if (doSelect)
		{
			selectTimeRot=this.transform.rotation;
			selectTimePos=this.transform.position;
		}
		transformInterBase(_position - selectTimePos,_orientation* Quaternion.Inverse(selectTimeRot), selectTimePos);
	}
	
	/// <summary>
    /// Checks if input trigger has been executed to add temporary selected object for transformation
    /// </summary>
    /// <returns>Boolean</returns>
	protected virtual bool GetSelectionTrigger()
	{
		if(Input.GetButtonDown("Fire1"))
		{
			return true;
		}
		else
		{
			return false;
		}
	}
}