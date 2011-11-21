/*==============================================================================
            Copyright (c) 2010-2011 Vienna University of Technology
            Interactive Media Systems Group
            Gerstweiler, G., Mossel, A., Schoenauer, C. 
            All Rights Reserved.
==============================================================================*/

using UnityEngine;
using System.Collections;
/// <summary>
/// ObjectController class is a component that makes an object selectable and transformable
/// </summary>
public class ObjectController : MonoBehaviour
{
    /// <summary>
    /// true if not selected by an interaction-device
    /// </summary>
    private bool isSelectable = true;
    private Color oldColor = Color.white;
	protected Quaternion selectTimeRot=Quaternion.identity;
	protected Vector3 selectTimePos=Vector3.zero;
	protected Vector3 selectTimeScale = Vector3.zero;

    /// <summary>
    /// Updates the transformation of the controlled object
    /// This method rotates the controlled object relative to the specified 
    /// point by the given Quaternion. Then the object is moved by the specified
    /// translational offset.
    /// </summary>
    /// <param name="translation">Offset the object is moved</param>
    /// <param name="rotate">Additional rotation</param>
    /// <param name="poseOfInteraction">pivotpoint the object is rotated about</param>
    public void updateTransform(Vector3 translation, Quaternion rotate,Vector3 grabPoint)
    {
		transform.rotation= rotate * selectTimeRot;
		transform.position = translation - (rotate * (grabPoint - selectTimePos) - grabPoint);

    }
	
    /// <summary>
    /// Updates the transformation of the controlled object
    /// This method rotates the controlled object relative to the specified 
    /// point by the given Quaternion. Then the object is moved by the specified
    /// translational offset.
    /// </summary>
    /// <param name="translation">Offset the object is moved</param>
    /// <param name="rotate">Additional rotation</param>
    /// <param name="grabPoint">pivotpoint the object is rotated about</param>
    /// <param name="doScale">object is scaled according to 3D translation vector</param>
    public void updateTransformAll(Vector3 translation, Quaternion rotate, Vector3 grabPoint, bool doScale)
    {
		if (doScale)
		{
			translation = Quaternion.Inverse(gameObject.transform.localRotation) * translation;
			transform.localScale = selectTimeScale + (Vector3.Scale(translation, selectTimeScale) / 10);
		}
		else
		{
			transform.rotation = rotate * selectTimeRot;
			transform.position = translation - (rotate * (grabPoint - selectTimePos) - grabPoint);
		}
    }
	
    /// <summary>
    /// Turns on/off selection of the controlled object. 
    /// Also sets the parameters required for certain components.
    /// e.g. Physics
    /// </summary>
    /// <param name="select">true for select, false for unselect</param>
    /// <returns>true if object not already selected, false otherwise</returns>
    public bool controlSelectedObjects(bool select)
    {
        if ((select && isSelectable) || (!select && !isSelectable))
        {
            gameObject.rigidbody.isKinematic = select;
            gameObject.rigidbody.useGravity = !select;
            
			if (select)
            {
                Debug.Log("Object" + this.GetInstanceID() + " selected.");
                
				isSelectable = false;
                if (this.GetComponent<Renderer>() != null)
				{
					oldColor=this.renderer.material.color;
                	this.renderer.material.color = Color.red;
				}
				
				// set transform values
				selectTimePos = transform.position;
				selectTimeRot = transform.rotation;
				selectTimeScale = transform.localScale;
            }
            else
            {
                Debug.Log("Object " + this.GetInstanceID() + " unselected");
                isSelectable = true;
                if (this.GetComponent<Renderer>() != null)
				{
					this.renderer.material.color=oldColor;
				}
            }
            return true;
        }
        return false;
    }
}