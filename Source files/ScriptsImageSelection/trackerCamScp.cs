using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class trackerCamScp : MonoBehaviour {

	public GameObject trackerCamObj;
	public Camera trackerCam;
	public GameObject[] imgObjs;

	GameObject m;

	public float initZ;

	public bool begun;
	// Use this for initialization
	void Start () {		
		trackerCamObj = GameObject.FindWithTag ("tracker");
		trackerCam = trackerCamObj.GetComponent<Camera>();
		trackerCam.enabled = false;
		initZ = trackerCamObj.transform.position.z;

		m = GameObject.Find ("Manager");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	public void closeUpCamActivate(){		
		if (!m.GetComponent<selectionGameManagerScript> ().begin) {
			Camera.main.enabled = false;
			trackerCam.enabled = true;
			trackerCamObj.transform.position = new Vector3 (transform.position.x, transform.position.y, initZ);	
		} 
	}
}
