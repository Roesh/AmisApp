using UnityEngine;
using System.Collections;

public class loadScreenRotate : MonoBehaviour {	

	private RectTransform rt;

	private IEnumerator rotationSequence(){			
		while (true) {
			float xAngle = Time.deltaTime * 60;
			rt.localEulerAngles += Vector3.up * xAngle;
			yield return null;	
		}
	}
	// Use this for initialization
	void Start () {		
		rt = GetComponent<RectTransform> ();
		StartCoroutine (rotationSequence ());
	}

	// Update is called once per frame
	void Update () {

	}
}
