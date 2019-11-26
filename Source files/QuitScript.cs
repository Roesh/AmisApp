using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class QuitScript : MonoBehaviour {
	public GameObject quitScreen;
	public GameObject quitScreenTracker;
    
	private bool screenDisplayState;

	// Use this for initialization
	void Start () {		
		screenDisplayState = false;
		quitScreen.SetActive (screenDisplayState);
	}
	
	// Update is called once per frame
	void Update () {
		// key code escape works for both android and windows
		if (Input.GetKeyDown (KeyCode.Escape)) {
			screenDisplayState = !screenDisplayState;
			quitScreen.SetActive (screenDisplayState);
			if (quitScreenTracker != null) {
				quitScreenTracker.SetActive (screenDisplayState);
			}
		}
	}

	public void quitGame(){
		Application.Quit ();
	}
	public void exitScreen(){
		screenDisplayState = !screenDisplayState;
		quitScreen.SetActive (screenDisplayState);
		if (quitScreenTracker != null) {
			quitScreenTracker.SetActive (screenDisplayState);
		}
	}
	public void backToGame(){
        GameObject scoreHelper = GameObject.Find("persistentObj");
        persistentHelper ph = scoreHelper.GetComponent<persistentHelper>();
        ph.firstTimeSetup = false;
        SceneManager.LoadScene (0);
	}
    public void backToLoginScreen()
    {
        GameObject ph = GameObject.Find("persistentObj");
        Destroy(ph);
        SceneManager.LoadScene(0);
    }
}
