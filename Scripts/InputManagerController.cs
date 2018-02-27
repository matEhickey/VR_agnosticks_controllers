using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using System.Text.RegularExpressions;
using System;


public class InputManagerController : MonoBehaviour {
	/*
	 * VR_input Manager
	 * Boilerplate to abstract input from different controller brands
	 * 
	 * How to use:
	 * Create a new VR scene (add openVR sdk in player pref)
	 * Remove default camera and add steamVR CameraRig prefab
	 * Add InputManager script to a Controller (Controller(left) for example, not the model)
	 * Add actionboolean actions to the SerializedFields by looking at 'action1' or 'action2' example and bind them
	 * Modify 2nd part of the 'updateActions' to bind inputs to actions
	*/

	private Dictionary<string,double> axis; // the inputs, plateform agnostic, by actions. Buttons are also axis that can take 0.0 or 1.1
	private Transform tipPos;

	private SteamVR_TrackedController _controller;
	private SteamVR_Controller.Device device;

	[SerializeField]
	private bool debug;

	// Add and rename actions here
	[System.NonSerialized]
	public UnityEvent action1;
	[System.NonSerialized]
	public UnityEvent action2;

	#region simulator inputs
	[System.NonSerialized]
	public KeyCode up;
	[System.NonSerialized]
	public KeyCode down;
	[System.NonSerialized]
	public KeyCode left;
	[System.NonSerialized]
	public KeyCode right;
	[System.NonSerialized]
	public KeyCode triggerInput;
	#endregion

	void Start(){
		axis = new Dictionary<string, double> ();
		tipPos = transform;
	}

	public void Init(){ // called at the end of simuleur start
		
		_controller = GetComponent<SteamVR_TrackedController> ();

		print ("controller "+_controller);

		//Debug.Log ("init input manage actions");
		if (action1.GetPersistentEventCount () > 0) {
			_controller.TriggerClicked += (object sender, ClickedEventArgs e) => {
				action1.Invoke ();
			};
		}
		if (action2.GetPersistentEventCount () > 0) {
			_controller.Gripped += (object sender, ClickedEventArgs e) => {
				action2.Invoke ();
			};
		}
	}

	public double getAxis(string name){
		if (axis.ContainsKey (name)) {
			return(axis [name]);
		} else {
			Debug.LogWarning ("No axis named '"+name+"' in the controller ");
			return(0.0);
		}
	}
	public Transform getTip(){
		return(tipPos);
	}

	#region tests
	[UnityTest]
	public IEnumerator testCreation(){
		GameObject rig;
		bool initSimu = UnityEngine.XR.XRDevice.model == "";

		SceneManager.LoadScene ("scene1");

		yield return null;

		if (initSimu) {
			rig = GameObject.Find ("[CameraRig_simulator]");
			Assert.IsNotNull (rig, "Can't find '[CameraRig_simulator]' ");
		} else {
			rig = GameObject.Find ("[CameraRig_prefab]");
			Assert.IsNotNull (rig, "Can't find '[CameraRig_prefab]' ");
		}


		GameObject controller = rig.transform.Find("Controller (right)").gameObject;
		Assert.IsNotNull (rig, "Can't find 'Controller (right)' ");

		InputManagerController inp = controller.GetComponent<InputManagerController> ();
		inp.debug = true;
		yield return null;
		Assert.AreEqual (inp.debug, true, "Debug couldn't set itself to 'true'");

		// test axis
		LogAssert.Expect (LogType.Log, new Regex("Horizontal : (-?[\\S]+)"));
		LogAssert.Expect (LogType.Log, new Regex("Vertical : (-?[\\S]+)"));
		LogAssert.Expect (LogType.Log, new Regex("Trigger : (-?[\\S]+)"));

		// test event handlers
		inp.action1.AddListener (() => (Debug.Log("listenerAction1Test")));
		inp.action1.Invoke ();
		LogAssert.Expect (LogType.Log, "listenerAction1Test");

		inp.debug = false;
		yield return null;
	}
	#endregion

	void OnGUI(){


		if (debug) {
			try{
				
				int w = (name == "Controller (left)" ? 20 : Screen.width - 120);
				try{
					GUI.Label (new Rect (w, 0, 100, 20), getTip().position.ToString());
				}
				catch(NullReferenceException e){
					//Debug.Log ("error "+e);
				}

				GUI.Label (new Rect (w, 100, 100, 20), "Horizontal: "+axis["Horizontal"].ToString("0.00"));
				GUI.Label (new Rect (w, 120, 100, 20), "Vertical: "+axis["Vertical"].ToString("0.00"));
				GUI.Label (new Rect (w, 140, 100, 20), "Trigger: "+axis["Trigger"].ToString("0.00"));
			}
			catch(KeyNotFoundException e){
				//Debug.Log ("Axis probably not yet initialised : "+e);
				// NTD here
			}
		}
	}

	#region actions update
	void updateActions(){
		// map input to actions

		axis.Clear ();

		Vector2 sticks = new Vector2();
		double trigger = 0.0;

		if (UnityEngine.XR.XRDevice.model == "") {
			sticks [0] = Input.GetKey (left) ? -1.0f : 0.0f;
			sticks [0] = Input.GetKey (right) ? 1.0f : sticks[0];

			sticks [1] = Input.GetKey (down) ? -1.0f : 0.0f;
			sticks [1] = Input.GetKey (up) ? 1.0f : sticks[1];

			trigger = Input.GetKey (triggerInput) ? 1.0f : 0.0f;

			tipPos = transform.Find("Model").Find ("tip").Find ("attach");

		} else { 
			//Debug.Log (UnityEngine.XR.XRDevice.model);
			device = SteamVR_Controller.Input ((int)_controller.controllerIndex);
			try{
				tipPos = transform.Find ("Model").Find ("tip").Find ("attach");
			}
			catch(NullReferenceException e){
				
			}

			if (UnityEngine.XR.XRDevice.model == "Lenovo Explorer") {
			
				bool pressTouchpad = device.GetPress (EVRButtonId.k_EButton_SteamVR_Touchpad);
				Vector2 touch = device.GetAxis (EVRButtonId.k_EButton_SteamVR_Touchpad);
				bool grip = device.GetPress (EVRButtonId.k_EButton_Grip);
				bool appMenu = device.GetPress (EVRButtonId.k_EButton_ApplicationMenu);

				sticks = device.GetAxis (EVRButtonId.k_EButton_Axis2);
				trigger = device.GetAxis (EVRButtonId.k_EButton_Axis1).x;

			} else if (UnityEngine.XR.XRDevice.model == "Oculus Rift CV1") {
			
				bool stickPress = device.GetPress (EVRButtonId.k_EButton_SteamVR_Touchpad);
				double grip = device.GetAxis (EVRButtonId.k_EButton_Axis2).x;
				bool buttonTop = device.GetPress (EVRButtonId.k_EButton_ApplicationMenu);
				bool buttonBottom = device.GetPress (EVRButtonId.k_EButton_A);

				sticks = device.GetAxis (EVRButtonId.k_EButton_SteamVR_Touchpad);
				trigger = device.GetAxis (EVRButtonId.k_EButton_Axis1).x;


			} else if (UnityEngine.XR.XRDevice.model == "Vive MV") {
				bool pressTouchpad = device.GetPress (EVRButtonId.k_EButton_SteamVR_Touchpad);
				Vector2 touch = device.GetAxis (EVRButtonId.k_EButton_SteamVR_Touchpad);

				bool grip = device.GetPress (EVRButtonId.k_EButton_Grip);
				bool buttonTop = device.GetPress (EVRButtonId.k_EButton_ApplicationMenu);

				sticks = pressTouchpad ? touch : new Vector2 (0, 0); // to emulate joystick press hard the touchpad
				trigger = device.GetAxis (EVRButtonId.k_EButton_Axis1).x;

			} else { 
				/* print (UnityEngine.XR.XRDevice.model);*/
			}
		}

		try{
			axis.Add ("Horizontal",sticks.x);
			axis.Add ("Vertical",sticks.y);
			axis.Add("Trigger", trigger);
		}
		catch(UnityException e){
			Debug.Log (e);
		}


	}
	void showInputs(){
		
		foreach(KeyValuePair<string, double> entry in axis)
		{
			Debug.Log(entry.Key+" : "+entry.Value);
		}
	}
	#endregion

	
	// Update is called once per frame
	void Update () {
		updateActions ();

		if (debug) { showInputs (); }
	}
}
