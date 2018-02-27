using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using NUnit.Framework;
using UnityEngine.Events;

public class Simulator : MonoBehaviour {
	/* Aim of this class is to detect if there is a vr headset connected
	* if there is one : instanciate the normal camera rig  
	* else instanciate a vr simulator that provide the same input interface
	*/

	[SerializeField]
	private GameObject cameraRig;
	[SerializeField]
	private GameObject cameraRigSimulator;


	InputManagerController inputManagerL;
	InputManagerController inputManagerR;


	[SerializeField]
	KeyCode upL;
	[SerializeField]
	KeyCode downL;
	[SerializeField]
	KeyCode leftL;
	[SerializeField]
	KeyCode rightL;
	[SerializeField]
	KeyCode triggerL;

	[SerializeField]
	KeyCode upR;
	[SerializeField]
	KeyCode downR;
	[SerializeField]
	KeyCode leftR;
	[SerializeField]
	KeyCode rightR;
	[SerializeField]
	KeyCode triggerR;

	[SerializeField]
	UnityEvent action1L;
	[SerializeField]
	UnityEvent action2L;
	[SerializeField]
	UnityEvent action1R;
	[SerializeField]
	UnityEvent action2R;

	[SerializeField]
	KeyCode buttonAction1L;
	[SerializeField]
	KeyCode buttonAction2L;
	[SerializeField]
	KeyCode buttonAction1R;
	[SerializeField]
	KeyCode buttonAction2R;


	// Use this for initialization
	void Awake () {
		LoadCameraRig (UnityEngine.XR.XRDevice.model == "");
	}

	void LoadCameraRig(bool simu){

		if (simu) {
			print ("No headset found, launch simu");
			GameObject go = Instantiate (cameraRigSimulator, transform);
			go.name = "[CameraRig_simulator]";

			inputManagerL = go.transform.Find("Controller (left)").GetComponent<InputManagerController> ();
			inputManagerR = go.transform.Find("Controller (right)").GetComponent<InputManagerController> ();

			inputManagerL.up = upL;
			inputManagerL.down = downL;
			inputManagerL.left = leftL;
			inputManagerL.right = rightL;
			inputManagerL.triggerInput = triggerL;

			inputManagerR.up = upR;
			inputManagerR.down = downR;
			inputManagerR.left = leftR;
			inputManagerR.right = rightR;
			inputManagerR.triggerInput = triggerR;

		} else {
			GameObject go = Instantiate (cameraRig, transform);
			go.name = "[CameraRig_prefab]";

			inputManagerL = go.transform.Find("Controller (left)").GetComponent<InputManagerController> ();
			inputManagerR = go.transform.Find("Controller (right)").GetComponent<InputManagerController> ();

		}

		inputManagerL.action1 = action1L;
		inputManagerL.action2 = action2L;

		inputManagerR.action1 = action1R;
		inputManagerR.action2 = action2R;

		if (!simu) {
			inputManagerL.Init ();
			inputManagerR.Init ();
		}
	}


	public double getAxis(string sens, string name){
		InputManagerController inp;
		if (sens == "left") {
			inp = inputManagerL;
		} else {
			inp = inputManagerR;
		}
		return(inp.getAxis(name));
	}

	void Update(){
		if (Input.GetKeyDown (buttonAction1L)) {
			action1L.Invoke ();
		}
		if (Input.GetKeyDown (buttonAction2L)) {
			action2L.Invoke ();
		}

		if (Input.GetKeyDown (buttonAction1R)) {
			action1R.Invoke ();
		}
		if (Input.GetKeyDown (buttonAction2R)) {
			action2R.Invoke ();
		}
	}


	#region tests
	public void TestLoadCamera(bool simu){
		LoadCameraRig (simu);
	}
	[UnityTest]
	public static IEnumerator testSimulateur(){
		//NSubstitute.For<Simulator> ();

		SceneManager.LoadScene ("scene1");

		yield return null;

		bool initSimu = UnityEngine.XR.XRDevice.model == "";
		Simulator simu = GameObject.FindObjectOfType <Simulator>();

		if (initSimu) {
			Assert.IsNotNull (GameObject.Find ("[CameraRig_simulator]"));
			Destroy (GameObject.Find ("[CameraRig_simulator]"));
		} else {
			Assert.IsNotNull (GameObject.Find ("[CameraRig_prefab]"));
			Destroy (GameObject.Find ("[CameraRig_prefab]"));
		}
		yield return null;

		simu.LoadCameraRig (!initSimu);

		if (!initSimu) {
			Assert.IsNotNull (GameObject.Find ("[CameraRig_simulator]"));
		} else {
			Assert.IsNotNull (GameObject.Find ("[CameraRig_prefab]"));
		}

	}
	#endregion

}
