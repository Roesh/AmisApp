using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class closeUpCamScp : MonoBehaviour {

	public Transform[] txs;
	public float zDiff;
	private Camera closeUpCam;
	public bool zoomEnabled = false;
	private Camera mainCam;
	public GameObject[] mainCamUI;
	public GameObject[] closeUpCamUI;

	public float minFOV = 15f;
	public Slider s;

	// Use this for initialization
	void Start () {		
		closeUpCam = GetComponent<Camera> ();
		closeUpCam.enabled = false;
		zDiff = transform.position.z - txs [0].position.z;
		mainCam = Camera.main;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void closeUpOn(int id){
		if (zoomEnabled) {
			zoomEnabled = false;
			transform.position = txs [id].position + Vector3.forward * zDiff;
			// Disable current cam, enable main cam
			Camera.main.enabled = false;
			closeUpCam.enabled = true;
			foreach (GameObject go in mainCamUI) {
				go.SetActive (false);
			}
			foreach (GameObject go in closeUpCamUI) {
				go.SetActive (true);
			}
		}
	}

	public void closeUpOff(){
		// Disable current cam, enable main cam
		closeUpCam.enabled = false;
		mainCam.enabled = true;
		zoomEnabled = true;
		foreach (GameObject go in mainCamUI) {
			go.SetActive (true);
		}
		foreach (GameObject go in closeUpCamUI) {
			go.SetActive (false);
		}
	}

	public void toggleZoom(){
		zoomEnabled = !zoomEnabled;
	}
	public void updateZoom(){
		closeUpCam.fieldOfView = Mathf.Lerp (30, minFOV, s.value);
	}
}
