using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIanimations : MonoBehaviour {

	public RectTransform difficultyTx;
	public bool rotating = false;
	public bool diffucultyUIPresent = true;

	private IEnumerator diffShove;
	private IEnumerator imgRot;
	private IEnumerator imgRot2;

	public RectTransform[] imgRtxs = new RectTransform[8];

	private IEnumerator imageRotations(int val){
		Random.InitState (val);
		yield return new WaitForSeconds (2);
		float tw = 1; // time to delay after current spin
		float po2 = Mathf.PI / 2;
		int numRots = 2;
		int rIndex;
		int prevIndex = 0;
		while (true) {			
			// time variables
			float t_elapsed = 0;
			float t = 1.8f + Random.value * 0.25f;
			// andgle variables
			int r = Random.Range (0, numRots -1);
			float diff = Mathf.Lerp (360, numRots * 360, r / (numRots -1));
			// See if the large spin should occur
			int decider;
			if (Random.value < 0.1) {
				decider = 0;
			} else {
				decider = 1;
			}if (Random.value > 0.5) {
				decider += 2;
			}
			switch (decider) {
			case 0: // multi rot
				while (t_elapsed < t) {
					float angle = Mathf.Lerp (0, po2, t_elapsed / t);
					float currAngle = diff * Mathf.Sin (angle);
					foreach (RectTransform rt in imgRtxs) {
						rt.localEulerAngles = Vector3.right * currAngle;	
					}
					t_elapsed += Time.deltaTime;
					yield return null;
				}	
				tw = Random.value * 2 + 4;
				break;
			case 1:				
				rIndex = Random.Range (0, 7);
				if (rIndex == prevIndex) {
					rIndex = Random.Range (0, 7);
				}
				while (t_elapsed < t) {
					float angle = Mathf.Lerp (0, po2, t_elapsed / t);
					float currAngle = diff * Mathf.Sin (angle);
					imgRtxs [rIndex].localEulerAngles = Vector3.right * currAngle;						
					t_elapsed += Time.deltaTime;
					yield return null;
				}	
				tw = Random.value * 0.5f + 0.25f;
				prevIndex = rIndex;
				break;
			case 2:
				while (t_elapsed < t) {
					float angle = Mathf.Lerp (0, po2, t_elapsed / t);
					float currAngle = diff * Mathf.Sin (angle);
					foreach (RectTransform rt in imgRtxs) {
						rt.localEulerAngles = Vector3.down * currAngle;	
					}
					t_elapsed += Time.deltaTime;
					yield return null;
				}	
				tw = Random.value * 4 + 3;
				break;
			case 3:				
				rIndex = Random.Range (0, 7);
				if (rIndex == prevIndex) {
					rIndex = Random.Range (0, 7);
				}
				while (t_elapsed < t) {
					float angle = Mathf.Lerp (0, po2, t_elapsed / t);
					float currAngle = diff * Mathf.Sin (angle);
					imgRtxs [rIndex].localEulerAngles = Vector3.down * currAngle;						
					t_elapsed += Time.deltaTime;
					yield return null;
				}	
				tw = Random.value * 0.25f + 2f;
				prevIndex = rIndex;
				break;
			default:
				break;
			}
			yield return new WaitForSeconds (tw);
		}
	}

	private IEnumerator diffucultyUIshove(){
		float initAngle = 0;
		float finalAngle = 120; 
		Vector3 rotV = Vector3.right;

		if (!diffucultyUIPresent) {
			// If the difficulty UI is not present, we will have to bring it back
			// That means we will also have to stop the rotation animations
			if (imgRot != null) {
				StopCoroutine (imgRot);
//				StopCoroutine (imgRot2);
			}
			foreach (RectTransform rt in imgRtxs) {
				rt.localEulerAngles = Vector3.zero;
			}
			// The angle of the diffuculty panel is also reversed
			finalAngle = 0; 
			initAngle = 100;
		} else {			
			// If the difficulty UI is currently present, it will be moved away
			// byt his coroutine; the spinning routines may begin
			int val = Mathf.FloorToInt(Time.timeSinceLevelLoad*10000);
			imgRot = imageRotations (val);
			StartCoroutine (imgRot);
			// secondary rots
//			imgRot2 = imageRotations (val + 1);
//			StartCoroutine (imgRot2);
		}
		diffucultyUIPresent = !diffucultyUIPresent;
		float t_elapsed = 0;
		float t = 0.3f;

		while (t_elapsed < t) {
			difficultyTx.localEulerAngles = rotV*Mathf.Lerp(initAngle,finalAngle,t_elapsed/t);
			t_elapsed += Time.deltaTime;
			yield return null;
		}		
		difficultyTx.localEulerAngles = rotV * finalAngle;
	}

	// Use this for initialization
	void Start () {
		GameObject[] k = GameObject.FindGameObjectsWithTag ("image");
		int i = 0;
		foreach (GameObject go in k) {
			imgRtxs[i++] = go.GetComponent<RectTransform> ();
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void toggleDifficultyPanel(){		
		if (diffShove != null) {
			StopCoroutine (diffShove);
		}
		diffShove = diffucultyUIshove ();
		StartCoroutine (diffucultyUIshove ());
	}

}
