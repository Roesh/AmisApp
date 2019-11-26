using UnityEngine;
using System.Collections;

public class cheat : MonoBehaviour {
	#if UNITY_STANDALONE || UNITY_WEBGL
	private KeyCode[] code = { KeyCode.R, KeyCode.O, KeyCode.E, KeyCode.S, KeyCode.H };
	private int codeLength;
	private int numCorrectLetters = 0;
	private selectionGameManagerScript manager;
	// Use this for initialization
	void Start () {
		numCorrectLetters = 0;
		codeLength = code.Length;
		manager = gameObject.GetComponent<selectionGameManagerScript> ();
	}
	
	// Update is called once per frame
	void Update () {
		// check if any key was pressed
		if (Input.anyKeyDown) {
			// if the key pressed is the correct key, increment the 
			if (Input.GetKey (code [numCorrectLetters])) {
				numCorrectLetters++;
			} else {
				numCorrectLetters = 0;
			}
		}

		if (numCorrectLetters == codeLength) {
			manager.loadAnimation ();
		}
	}
	#endif
}
