using UnityEngine;
using System.Collections;

public class textRotate : MonoBehaviour {
	public float t_main = 5f;
	public float t_randRange = 3f;
	public float t_flip = 0.6f;

	public RectTransform rt;

	private IEnumerator rotationSequence(){
		float ai = 0; float af = Mathf.PI/2; 
		while (true) {
			// perform the flip
			float t_elapsed = 0;
			while (t_elapsed < t_flip) {
				float angle = Mathf.Lerp (ai, af, t_elapsed / t_flip);
				float xAngle = 360f* Mathf.Sin (angle);
				rt.localEulerAngles = Vector3.right * xAngle;
				t_elapsed += Time.deltaTime;
				yield return null;	
			}
			// wait for t_main
			yield return new WaitForSeconds (t_main);
			// wait for a random amount of time between 0 and t_randRange seconds
			yield return new WaitForSeconds (Random.value * t_randRange);
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
