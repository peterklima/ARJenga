/*==============================================================================
            Copyright (c) 2010-2011 Vienna University of Technology
            Interactive Media Systems Group
            Gerstweiler, G., Mossel, A., Schoenauer, C. 
            All Rights Reserved.
==============================================================================*/

using UnityEngine;
using System.Collections;

/// <summary>
/// MobileObjectSelectionBase is a component that derives from ObjectSelectionBase. All mobile interaction techniques must derive from this class. 
/// It provides the functionality to add/remove selectable game objects and to rotate a selected game objects relative to mobile device pose or 2D touch input
/// </summary>
public abstract class MobileObjectSelectionBase: ObjectSelectionBase
{
	/** enumeration of valid transform types */
	protected enum TransformTypes {NONE, TRANSLATE, ROTATE, TRANSLATEROTATE, SCALE}; 	
	/** stores x/y position of current touch */
	protected Vector2 touchPosition = Vector2.zero; 
	/** flag to wait for GUI confirmation by user to select manipulation type after selecting an object */
	protected bool doGUIConfirm = false;
	/** flag - set to true after user confirmed selection of buffered object by GUI input */
	protected bool selectionConfirmed = false; 
	/** what the user wants to do with the current selected object. */
	protected int performTransform = (int)TransformTypes.NONE;
	/** the currently selected gameobject, if there is one. */
	protected GameObject mCollidee;
	/** missing documentation here, so we won't touch it. */
	protected bool doScale = false; 	
	/** the button window. default: hidden */
	protected Rect windowRect;
	/** if an object is selected, this vector shows the original space difference between the ar-cam and the object. used for transformations. */
	protected Vector3 mOriginalDiffArCamToSelectedObject;	
	/** where the popup window should be drawn according to the user's last click. null if it has not been calculated for the window to be shown next. */
	protected Vector2 mWindowPosition;
	/** true if the switch interaction menu should be displayed */
	protected bool mIsInteractionMethodMenu = false;
	
	// our singleton game objects
	protected GameObject imageTarget;
	protected GameObject arCam;
	protected TrackableEventHandler trackableEventHandler;
	
	public void Start()
    {   	
		imageTarget = GameObject.Find("ImageTarget");
		Debug.Log("################################### found imagetarget");
		arCam = GameObject.Find("ARCamera");
		Debug.Log("################################### found arcam");
		trackableEventHandler = imageTarget.GetComponent<TrackableEventHandler>();
		Debug.Log("################################### found trackableEventHandler");    
	}
	
	
	/// <summary>
    /// Unity Callback
    /// OnGUI is called for rendering and handling GUI events.
    /// </summary>
	void OnGUI () 
	{
		if(doGUIConfirm) {
			if(mIsInteractionMethodMenu) {
				//special menu for selection of interaction methods
				Debug.Log("Showing interaction method switch window.");
				windowRect = new Rect(0, 0, Screen.width, Screen.height);
				windowRect = GUILayout.Window(0, windowRect, showSwitchInteractionMethodMenu, "Show window");
				return;
			}
			
			//select which pop you want to have here: either fullscreen or window
			PopupScreen();
			
			//let the subclass create the content of the window
			windowRect = GUILayout.Window(0, windowRect, WindowManipulationType, "Show window");
		}
	}
	
	/** show the popup window in fullscreen */
	private void PopupFullScreen() {	
		windowRect = new Rect(0, 0, Screen.width, Screen.height);
	}

	/** show the popup window in an appropriate size */
	private void PopupScreen() {
		
		if(Input.touchCount >= 1 && !isPopupWindowPosition()) {
			//create new popup pos
			
			//santiy check: is window fully visible?
			Vector2 windowMaxPos = new Vector2(Input.touches[0].position.x + popupWindowSize().x, (Screen.height - Input.touches[0].position.y) + popupWindowSize().y);
			if(windowMaxPos.x > Screen.width) {
				windowMaxPos.x = Screen.width;
			}
			if(windowMaxPos.y > Screen.height) {
				windowMaxPos.y = Screen.height;
			}
			//set and save window position
			mWindowPosition = new Vector2(windowMaxPos.x - popupWindowSize().x, windowMaxPos.y - popupWindowSize().y);
		}
		//create window rect pos
		windowRect = new Rect(mWindowPosition.x, mWindowPosition.y, popupWindowSize().x, popupWindowSize().y);
	}
	
	/** reset the popupwindow position to remember that it has to be calcualted again before next usage. */
	protected void resetPopupWindowPosition() {
		mWindowPosition.x = 0;
		mWindowPosition.y = 0;
	}
	
	/** check if a popup position is set currently. if not, it has to be calculated before the popup window can be used. */
	protected bool isPopupWindowPosition() {
		return mWindowPosition.x != 0 && mWindowPosition.y != 0;	
	}
	
	/** show the interaction switch window */
	private void showSwitchInteractionMethodMenu(int id) {
		
		//Get currently selected interaction method
		MobileInteractionByTouch t = this.GetComponent<MobileInteractionByTouch>();
		MobileInteractionByDeviceOrientation d = this.GetComponent<MobileInteractionByDeviceOrientation>();
		string name = (t.enabled ? "DeviceOrientation" : "Touch");
		
		GUILayout.BeginVertical("box");
		if (GUILayout.Button("Switch to " + name, GUILayout.MinHeight(50))) {
			Debug.Log("Switch pressed");
			
			//switch selections
			t.enabled = !t.enabled;
			d.enabled = !d.enabled;
			
			mIsInteractionMethodMenu = false;
			doGUIConfirm = false;
		}
		if (GUILayout.Button("Cancel", GUILayout.MinHeight(50))) {
			Debug.Log("Cancel pressed");
			mIsInteractionMethodMenu = false;
			doGUIConfirm = false;
		}
		GUILayout.EndHorizontal();
	}
	
	/// <summary>
    /// Update-Callback
    /// 
    /// Do selection and transformations here
    /// </summary>
    protected override void UpdateSelect()
	{
		if(doGUIConfirm) {
			//we have the interaction window open
			// no need to translate or so
			return;
		}
			
		if(trackableEventHandler.isTracked) {
			//Debug.Log("##### UpdateSelected(): we are tracking our target");
			
			if(!selected) {				
				//Debug.Log("##### we don't have a selected object, setting interactionobject pos/rot to those of arCam");
					
				//check if we have a user touch that reaches an object with an object controller
				GameObject collidee = getRaycastGameObjectWithObjectControllerFromUserTouch();
				if(collidee != null && !collidees.ContainsKey(collidee.GetInstanceID())) {
				
					//check if this is our special object
					if(collidee == GameObject.Find("SwitchObject")) {
						//set variable for showing special window
						mIsInteractionMethodMenu = true;
						doGUIConfirm = true;
						return;
					}
					
					// check if this collidee has a network object controller (restricted access)
					/*NetworkObjectController noc = collidee.GetComponent<NetworkObjectController>();
					GameObject guiObject = GameObject.Find("GUIObj");
					MobileNetworkGUI mng = guiObject.GetComponent<MobileNetworkGUI>();
					if (!noc.IsObjectAccessGranted(UserManager.instance.getNetworkPlayer(mng.getName()))) {
						Debug.Log("access denied");
						return;
					}*/
					
					//new object - add to collidees
					Debug.Log("add object to collidees");
					
					//remember collidee
					mCollidee = collidee;		
												
					//add to collidees
					collidees.Add(collidee.GetInstanceID(), collidee);
					Debug.Log("table size: " + collidees.Count);
				
					//move interaction object to the selected object
					this.transform.position = arCam.transform.position;
					this.transform.rotation = arCam.transform.rotation;
					
					//remember original distance 
					mOriginalDiffArCamToSelectedObject = collidee.transform.position - arCam.transform.position;
					
					//signal that we need a selection window now
					doGUIConfirm = true;
				}
				else {
					//no hit
					collidees.Clear();
					doGUIConfirm = false;
				}
			}
			else {
				//let subclass decide what to do when we have a selected object and there is a userinput
				onUpdateObjectSelected();
			}
		}
		else {
			// no tracked target: set interaction object position to cam position 
			this.transform.position = arCam.transform.position;
		}
	}

	
	/** find gameobject the user just selected. return null if: there was no user touch recently, there was no gameobject selected by the user click or the selected gameobject did not have an objectcontroller attached. */
	protected GameObject getRaycastGameObjectWithObjectControllerFromUserTouch() {
		
		//check if we have a touch, and if the touch not been done before to avoid bouncing
		if(Input.touches.Length > 0 && Input.GetTouch(0).phase == TouchPhase.Began) {
			
			touchPosition = new Vector2(Input.touches[0].position.x, Input.touches[0].position.y);
			//Debug.Log("we have a touch: " + touchPosition.x + "/" + touchPosition.y);
		
			Ray ray = Camera.main.ScreenPointToRay(touchPosition);	
			RaycastHit hit = new RaycastHit();
		    if (Physics.Raycast (ray, out hit)) {
				Debug.Log("we have a hit between the user's touch event and an object: " + hit.point);
				
				GameObject collidee = hit.collider.gameObject;
				if(hasObjectController(collidee)) {
					Debug.Log("we have a hit with of our objects");
					return collidee;
				} 
				else {
					//no hit
					collidees.Clear();
					doGUIConfirm = false;
				}
			}
			else {
				//no hit
				collidees.Clear();
				doGUIConfirm = false;
			}
		}
		else {
			//no hit
			collidees.Clear();
			doGUIConfirm = false;
		}
		return null;
	}
	
	protected float calcDistance(Vector3 _v1, Vector3 _v2) {
		return Mathf.Sqrt(Mathf.Pow(_v1.x-_v2.x, 2) + Mathf.Pow(_v1.y-_v2.y, 2) + Mathf.Pow(_v1.z-_v2.z, 2));
	}
	
	/// <summary>
    /// Overrides inherited method to trigger selection of buffered objects. 
    /// </summary>
	protected override bool GetSelectionTrigger()
	{
		if (selectionConfirmed)
		{
			selectionConfirmed = false;
			return true;
		}
		return false;
	}
	

	/** initializes a translation: closes the window, sets the user selection as confirmed */
	protected virtual void InitTranslation() {
		//close gui
		doGUIConfirm = false;
		
		//remember that we have to calculate the pos next time
		resetPopupWindowPosition();
	
		//set object selected flag
		selectionConfirmed = true;
	}
	
	/// <summary>
	/// Window Container to show manipulation options in GUI. Must be implemented in deriving IT class
    /// </summary>
    /// <param name="id"></param>
	// protected abstract void WindowManipulationType(int id);
	protected abstract void WindowManipulationType(int id);
	
	/** do the transformation due to the state of performTransform. */
	protected abstract void performTransformation();
	
	/** called if update is called and there is an object already selected from past actions. */
	protected abstract void onUpdateObjectSelected();
	
	/** return: the size of the popupwindow */
	protected abstract Vector2 popupWindowSize();
	
	/// <summary>
	/// Sets the transform of all selected objects to the given translation and orientation.
    /// TransformInter MUST use the pose of the InteractionGameObject
    /// 
    /// If scaling is activated (by _doScale = true) translation vector is used to scale the object.
    /// </summary>
    /// <param name="_position">Absolute WC position of the object is moved</param>
    /// <param name="_orientation">Absolute WC rotation</param>
    /// <param name="_doScale">Boolean if translation values are used to scale object</param>
    /// <returns></returns>
    protected virtual void transformInterAll(Vector3 _position, Quaternion _orientation, bool _doScale)
	{
		if (doSelect)
		{
			selectTimeRot=this.transform.rotation;
			selectTimePos=this.transform.position;
		}
		transformInterAllBase(_position - selectTimePos,_orientation* Quaternion.Inverse(selectTimeRot), selectTimePos, _doScale);
	}
}