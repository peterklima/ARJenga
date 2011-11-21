using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Implements a 3D interaction metaphor by adapting HOMER interaction technique to a mobile device. Objects can be selected by touch and raycast and then 
/// manipulated by device pose (translation & rotation). Objects can be deselected by touch & raycast. 
/// </summary>
public class MobileInteractionByTouch : MobileObjectSelectionBase
{
	/** position of touch nr 1 in last multitouch */
	protected Vector2 mLastTouchPos1 = new Vector2(0,0);
	/** position of touch nr 2 in last multitouch */
	protected Vector2 mLastTouchPos2 = new Vector2(0,0);
	
	protected void setLastTouchPos(Vector2 _v1, Vector2 _v2) {
		mLastTouchPos1 = _v1;
		mLastTouchPos2 = _v2;
	}
	protected void resetLastTouchPos() {
		setLastTouchPos(new Vector2(0,0), new Vector2(0,0));	
	}
	protected bool isLastTouchPos() {
		return mLastTouchPos1.x != 0 || mLastTouchPos1.y != 0 || mLastTouchPos2.x != 0 || mLastTouchPos2.y != 0;
	}
	
	protected override void onUpdateObjectSelected ()
	{
		//currently we have a selected object
		GameObject collidee = getRaycastGameObjectWithObjectControllerFromUserTouch ();
		if (collidee != null && collidee == mCollidee) {
			Debug.Log ("remove cube from collidees");
			collidees.Clear ();
			resetLastTouchPos();
			
			//reset base info
			performTransform = (int)TransformTypes.NONE;
			selectionConfirmed = true;
		} else {
			//do transformation
			performTransformation ();
		}
	}

	protected override void performTransformation ()
	{
//		Vector3 q = mCollidee.transform.position - arCam.transform.position;		
//		Debug.Log("arCamVector=" + q);

		//preactions
		if(performTransform == (int)TransformTypes.SCALE && Input.touchCount < 2) {
			//reset last pos vectors
			resetLastTouchPos();
		}
		
		//check touch event size for appropriate transformation
		if (!isAppropriateTouchCount (Input.touchCount)) {
			//don't do anything
			return;
		}
		float factor = 0.5f;
		switch (performTransform) {
			
			case ((int)TransformTypes.ROTATE): {
//				Debug.Log ("Touch::performTransformation ROTATE.");
				//do not change the object position
				Vector2 delta = Input.GetTouch (0).deltaPosition;
				mCollidee.transform.Rotate (delta.x, delta.y, 0);
				break;
			}
				
			case ((int)TransformTypes.TRANSLATE): {
//				Debug.Log ("Touch::performTransformation TRANSLATE.");
			
				if (Input.GetTouch (0).phase == TouchPhase.Moved) {
					
					Vector2 delta = Input.GetTouch (0).deltaPosition;
				
					//EITHER: translation along a changing axis	
//					Debug.Log("arCamVector=" + arCamVector);
					Vector2 arCamNormalVector = new Vector2(-mOriginalDiffArCamToSelectedObject.z, mOriginalDiffArCamToSelectedObject.x);
//					Debug.Log("arCamNormalVector=" + arCamNormalVector);
					arCamNormalVector.Normalize();
					Debug.Log("arCamNormalVectorNorm=" + arCamNormalVector);
					mCollidee.transform.Translate(delta.x * -arCamNormalVector.x, delta.y * factor, delta.x * -arCamNormalVector.y);

					//OR: translation along a fixed axis
//					mCollidee.transform.Translate (delta.x * factor, delta.y * factor, 0);
				}
				break;
			}
	
			case ((int)TransformTypes.SCALE):  {
//				Debug.Log ("Touch::performTransformation SCALE.");
				//only use the pose of the arcam directly, nothing else
				Vector2 t1 = Input.GetTouch(0).position;
				Vector2 t2 = Input.GetTouch(1).position;
				if(isLastTouchPos()) {
					//get distances between touches
					float oldDist = (float)Math.Sqrt(Math.Pow(mLastTouchPos1.x - mLastTouchPos2.x, 2) + Math.Pow(mLastTouchPos1.y - mLastTouchPos2.y, 2));
					float newDist = (float)Math.Sqrt(Math.Pow(t1.x - t2.x, 2) + Math.Pow(t1.y - t2.y, 2));
					float delta = (newDist - oldDist)/500;
					//do scaling
					mCollidee.transform.localScale += new Vector3(delta,delta,delta);
				}
				setLastTouchPos(t1, t2);
				break;
			}
		}
	}

	protected override void WindowManipulationType (int id)
	{
		
		//make 1 horizontal box. size already given by base class
		GUILayout.BeginVertical ("box");
		if (GUILayout.Button ("Translate", GUILayout.MinHeight (50))) {
			Debug.Log ("Translate pressed");
			performTransform = (int)TransformTypes.TRANSLATE;
			InitTranslation ();
		}
		if (GUILayout.Button ("Rotate", GUILayout.MinHeight (50))) {
			Debug.Log ("Rotate pressed");
			performTransform = (int)TransformTypes.ROTATE;
			InitTranslation ();
		}
		if (GUILayout.Button ("Scale", GUILayout.MinHeight (50))) {
			Debug.Log ("Scale pressed");
			performTransform = (int)TransformTypes.SCALE;
			InitTranslation ();
		}
		if (GUILayout.Button ("Cancel", GUILayout.MinHeight (50))) {
			Debug.Log ("Cancel pressed");
			performTransform = (int)TransformTypes.NONE;
			doGUIConfirm = false;
			//remember that we have to calculate the pos next time
			resetPopupWindowPosition ();
		}
		GUILayout.EndHorizontal ();
		Debug.Log ("GUI: " + windowRect.width + "/" + windowRect.height);
	}

	protected override Vector2 popupWindowSize ()
	{
		return new Vector2 (200, 250);
	}

	/** check if we have enough touch events for doing stuff */
	private bool isAppropriateTouchCount (int _t)
	{
		switch (performTransform) {
		case ((int)TransformTypes.TRANSLATE):
		case ((int)TransformTypes.ROTATE):
			return _t >= 1;
		case ((int)TransformTypes.SCALE):
			return _t >= 2;
		}
		return false;
	}
}
