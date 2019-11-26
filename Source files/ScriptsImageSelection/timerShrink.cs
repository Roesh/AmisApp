using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class timerShrink : MonoBehaviour {
	
	private float initialYscale;
	private IEnumerator timerRoutine;
	public GameObject timeTextObj;
	private Text timeText;

	private IEnumerator shrink(float total_time, float initYscale){		
		float elapsedTime = 0f;
		timeText.enabled = true;
		while(elapsedTime < total_time){
			float t_left = total_time - elapsedTime;
			timeText.text = t_left.ToString("##");
			float newYscale = Mathf.Lerp (initYscale, 0.1f, elapsedTime / total_time);
			transform.localScale = new Vector3 (transform.localScale.x,newYscale, transform.localScale.z);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		timeText.enabled = false;
	}

	// Use this for initialization
	void Start () {		
		initialYscale = transform.localScale.y;
		gameObject.SetActive (false);
		timeText = timeTextObj.GetComponent<Text> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void RestartTimer(float levelTime){		
		transform.localScale = new Vector3 (transform.localScale.x,initialYscale, transform.localScale.z);
		if (timerRoutine != null) {			
			StopCoroutine (timerRoutine);
		}
		timerRoutine = shrink (levelTime, transform.localScale.y);
		StartCoroutine (timerRoutine);
	}
	public void endTimer(){
		if (timerRoutine != null) {
			StopCoroutine (timerRoutine);
			timeText.enabled = false;
		}
	}
}
