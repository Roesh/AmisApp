using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GuessManager : MonoBehaviour {
	
	public bool levelPassed = false;
	public int personGuessID = -1;

	public int numCorrectGuesses = 0;
	public bool userGuessed = false;
	private RoundTextChanger rt_changer;
	private selectionGameManagerScript manager;
	private IEnumerator fbRoutine;

	public Light mainIllum;
	public GameObject fb_light_true;
	public GameObject fb_light_false;
	public GameObject Dingobject;
	public GameObject WrongDingObject;

	public float fbDuration = 0.6f;
	public float fbIntensity = 2f;

	public RectTransform[] rts;

	public AudioSource[] auds;
	private float[] halfVols = new float[3];

	public particleScp[] pses;

	public IEnumerator volumeFade(float total_time){		
		float elapsedTime = 0f;
		total_time -= 1;
		while(elapsedTime < total_time){
			int i = 0;
			foreach (AudioSource a in auds) {
				float volFrac = Mathf.Lerp (halfVols[i]*2, 0, elapsedTime / total_time);
				a.volume = volFrac;
				i++;
			}
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		foreach (AudioSource a in auds) {
			a.enabled = false;
		}
	}

	private IEnumerator pushBack(RectTransform rt, int direction){		
		float elapsedTime = 0f;
		float total_time = 0.5f;
		if (direction == -1) {
			total_time = 0.7f;	
		}
		Vector3 pv = rt.anchoredPosition3D;
		Vector3 initRot = rt.localEulerAngles;
		float zdiff = direction * 95f;
		float xdiff = direction * 450f;
		float po2 = Mathf.PI / 2;
		while(elapsedTime < total_time){			
			float angle = Mathf.Lerp (0, po2, elapsedTime / total_time);
			float currZ = zdiff * Mathf.Sin (angle);
			float currX = xdiff * Mathf.Sin (angle);
			rt.anchoredPosition3D = pv + Vector3.forward * currZ;
			rt.localEulerAngles = initRot + Vector3.right * currX;
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		rt.anchoredPosition = pv + Vector3.forward * zdiff;
		rt.localEulerAngles = initRot + Vector3.right * xdiff;
	}
	private IEnumerator feedback(bool correct_guess){		
		Light fbLight;
		float elapsedTime = 0;
		if (correct_guess) {
			fb_light_true.SetActive (true);
			fbLight = fb_light_true.GetComponent<Light> ();
		} else {
			fb_light_false.SetActive (true);
			fbLight = fb_light_false.GetComponent<Light> ();
		}
		// Quieten the audio
		int hvc = 0;
		foreach(AudioSource a in auds){
			a.volume = halfVols[hvc];
			hvc++;
		}
		while(elapsedTime < fbDuration){
			float fraction = elapsedTime / fbDuration;
			float fracAngle = Mathf.LerpAngle (0f,Mathf.PI,fraction);
			float i = fbIntensity * Mathf.Sin (fracAngle);
			fbLight.intensity = i;
			mainIllum.intensity = 0.5f* (1 - i);
			// restore the volume gradually
			hvc = 0;
			foreach(AudioSource a in auds){			
				a.volume = halfVols[hvc] * Mathf.Lerp(1,2,fraction);
				hvc++;
			}
			elapsedTime += Time.deltaTime;
			yield return null;
		}

	}

	// Use this for initialization
	void Start () {
		levelPassed = false;
		personGuessID = -1;
		numCorrectGuesses = 0;
		userGuessed = false;
		rt_changer = gameObject.GetComponent<RoundTextChanger> ();
		manager = gameObject.GetComponent<selectionGameManagerScript> ();
		for (int i = 0; i < 3; i++) {
			halfVols [i] = auds [i].volume/2;
		}
		pses = FindObjectsOfType<particleScp> ();
	}
	
	// Update is called once per frame
	void Update () {
		#if UNITY_EDITOR
		if(Input.GetKeyDown(KeyCode.K)){
			selectionIs (rt_changer.nameOrder[rt_changer.currentNameIndex]);
		}
		#endif
	}

	private void processGuess(){
		// If the game has started
		if (manager.game_started) {
			// If the fbRoutine is running, stop it
			if (fbRoutine != null) {
				fb_light_false.SetActive (false);
				fb_light_true.SetActive (false);
				StopCoroutine (fbRoutine);
			}
			// If the guess was successful
			if (personGuessID == rt_changer.nameOrder[rt_changer.currentNameIndex]) {
				// Pushback the image
				StartCoroutine(pushBack(rts[personGuessID], 1));
				// Assign a true feedback routine
				fbRoutine = feedback (true);
				// Call the particleSystem and inform it of a correct guess
				foreach (particleScp ps in pses) {
					ps.addParticleCountAndSpeed ();
				}
				numCorrectGuesses += 1; 
				if (numCorrectGuesses == 8) {
					levelPassed = true; // set the level passed flag to true
					numCorrectGuesses = 0; // reset the number of correct guesses
				}
				// update the name after a successful guess
				AudioSource ac = Dingobject.GetComponent<AudioSource>();			
				ac.Play (); 
				rt_changer.incrementScore();
				rt_changer.updateName (manager.currentLevel);

			} else { // If unsuccessful
				foreach (particleScp ps in pses) {
					ps.resetAndTurnOffParticles ();
				}
				AudioSource ac = WrongDingObject.GetComponent<AudioSource>();			
				ac.Play (); 
				// Assign a false feedback routine
				fbRoutine = feedback (false);
			}
			// Start the feedback coroutine
			StartCoroutine (fbRoutine);

			// Set the guess id to an invalid number
			personGuessID = -1;
		}
	}

	// Button selection onClick functions
	public void selectionIs(int id){
		personGuessID = id;
		processGuess ();
	}

	public void restoreImageTxs(){
		foreach (RectTransform rt in rts) {
			StartCoroutine(pushBack(rt,-1));
		}
	}
	public void fadeAudio(float t){
		StartCoroutine (volumeFade (t));
	}
}
