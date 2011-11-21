using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Implements a 3D interaction metaphor by adapting HOMER interaction technique to a mobile device. Objects can be selected by touch and raycast and then 
/// manipulated by device pose (translation & rotation). Objects can be deselected by touch & raycast. 
/// </summary>
public class MobileInteractionByDeviceOrientation : MobileObjectSelectionBase
{
	protected override void performTransformation() {
		Debug.Log("MobileInteractionByDeviceOrientation::performTransformation called.");
		switch(performTransform) {
			case((int)TransformTypes.ROTATE):
				//do not change the object position
				this.transform.position = mCollidee.transform.position;
				this.transform.rotation = arCam.transform.rotation;
				transformInterAll(this.transform.position, this.transform.rotation, false);
				break;
			case((int)TransformTypes.TRANSLATE):
				//do not change object angle
				this.transform.position = arCam.transform.position - mOriginalDiffArCamToSelectedObject;
				transformInterAll(this.transform.position, this.transform.rotation, false);
				break;
			case((int)TransformTypes.TRANSLATEROTATE):
				this.transform.position = arCam.transform.position;
				this.transform.rotation = arCam.transform.rotation;
				break;
			case((int)TransformTypes.SCALE):
				//only use the pose of the arcam directly, nothing else
				this.transform.position = arCam.transform.position;
				this.transform.rotation = arCam.transform.rotation;
				break;
		}
		//perform the transformation
		transformInterAll(this.transform.position, this.transform.rotation, performTransform == (int)TransformTypes.SCALE);
	}
	
	protected override void onUpdateObjectSelected() {
		//currently we have a selected object
		GameObject collidee = getRaycastGameObjectWithObjectControllerFromUserTouch();
		if(collidee != null && collidee == mCollidee){
			Debug.Log("remove cube from collidees");
			collidees.Clear();
			
			//reset base info
			performTransform = (int)TransformTypes.NONE;
			selectionConfirmed = true;
		}
		else {
			//do transformation
			performTransformation();
		}
	}
	
	protected override void WindowManipulationType(int id) {
			
		//make 1 horizontal box. size already given by base class
		GUILayout.BeginVertical("box");
		if (GUILayout.Button("Translate", GUILayout.MinHeight(50))) {
			Debug.Log("Translate pressed");
			performTransform = (int)TransformTypes.TRANSLATE;
			InitTranslation();
		}
		if (GUILayout.Button("Rotate", GUILayout.MinHeight(50))) {
			Debug.Log("Rotate pressed");
			performTransform = (int)TransformTypes.ROTATE;
			InitTranslation();
		}
		if (GUILayout.Button("Translate & Rotate", GUILayout.MinHeight(50))) {
			Debug.Log("Translate & Rotate pressed");
			performTransform = (int)TransformTypes.TRANSLATEROTATE;
			InitTranslation();
		}
		if (GUILayout.Button("Scale", GUILayout.MinHeight(50))) {
			Debug.Log("Scale pressed");
			performTransform = (int)TransformTypes.SCALE;
			InitTranslation();
		}
		if (GUILayout.Button("Cancel", GUILayout.MinHeight(50))) {
			Debug.Log("Cancel pressed");
			performTransform = (int)TransformTypes.NONE;
			doGUIConfirm = false;
			//remember that we have to calculate the pos next time
			resetPopupWindowPosition();
		}
		GUILayout.EndHorizontal();
	}
	
	protected override Vector2 popupWindowSize() {
		return new Vector2(200,304);	
	}
}