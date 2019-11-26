using UnityEngine;
using System.Collections;

public class movement_animation : MonoBehaviour {

	// Stage variables
	public const int numstages = 6;
	public const int numframes = 8;
	public const int numlandscape = 4;
	public const int numportrait = 4;

	public GameObject[] frames = new GameObject[numframes];
	public GameObject[] landscape = new GameObject[numlandscape];
	public GameObject[] portrait = new GameObject[numportrait];
	public textureMod tm;
	public GameObject landscapeCenter;	// landscape pivot
	public GameObject portraitCenter;	// portrait pivot
	public GameObject staticFrame; 		// Static frame
	public GameObject ca_pivot; // Carousel pivot

	// cameras
	public GameObject maincam;
	private Camera mainCamCamObj;
	public GameObject trackerCam;
	private Camera trackerCamCam;
	public float initialFOV;
	public float[] t_tracker = new float[4];
	public GameObject redLight;
	public GameObject yellowLight;

    private persistentHelper ph;
	private int rewardLevel;
	public int current_stage = 0;
	public int cStage;
	private float PIover2 = Mathf.PI/2;
	public float multiplier;
	public float multiplier_threshold = 0.01f;
	public float multiplier_adjustment = 1f;
	public float booster_factor = 1.1f;
	public float carouselInterval = 45f; // degrees
	public float carouselFinalInterval = 65f;
	public float stage5halt = 1f;

	public float[] t_stage = new float[numstages];  	// seconds
	public float[] dist = new float[numstages];			// "distance" (rad/ units)
	public float[] current_dist = new float[numstages];
	private float[] stage_speed = new float[numstages];	// "distance"/sec
	private float[] norms = new float[numstages];		// ratio to normalize cosine speed
	public float[] postStepDelays = new float[numstages];		// seocnds
	public bool[] stageComplete = new bool[numstages];

	// coroutine variables
	private IEnumerator[] stagedelay = new IEnumerator[numstages];
	private IEnumerator[] camRoutines = new IEnumerator[numstages];

	private IEnumerator[] stage5_routines = new IEnumerator[numframes];
	public bool[] stage5Executed = new bool[numframes];

	// Timing variables
	public float now;
	public float dt_animation;

	// Stage 1 outward movement varibles
	public float initialFraction = 0.5f;
	public Vector3[] translationVectors = new Vector3[numframes];
	public float fractionVsTime;

	// Image swapping
	public string[] nameTexts = new string[8];

	private Transform[] initTxs;
	private float t_elapsed;
	private Vector3 lsPos;
	private Vector3 ptPos;
	private Vector3[] gpStorage1 = new Vector3[numframes];
	//private Vector3[] gpStorage2= new Vector3[numframes];

	private IEnumerator DelayAndChangeStage(float delay , int next_stage){
		if (next_stage == 1) {
			Light[] ls = FindObjectsOfType<Light> ();
			float[] ints = new float[ls.Length];
			int i = 0;
			foreach (Light l in ls) {
				ints[i] = l.intensity;
				i++;
			}

			float te = 0;
			float initFOV = Camera.main.fieldOfView - 20;
			while (te < delay) {
				Camera.main.fieldOfView = Mathf.Lerp (initFOV, initFOV+20, te / delay);
				i = 0;
				foreach (Light l in ls) {
					l.intensity = Mathf.Lerp (0, ints[i], 2*te / delay);
					i++;
				}
				te += Time.deltaTime;
				yield return null;	
			}
		} else {
			yield return new WaitForSeconds (delay);
		}
		current_stage = next_stage;

	}

	/* Camera zooming coroutine. Starts the camera's fov at initFOV and smoothly zooms to
	 * final FOV 
	 * Inputs: 
	 * float t- time to produce zooming effect for 
	 * float initFOV - the initial field of view. The function sets the cameras FOV to this value 
	 * on start
	 * float finalFOV - the final field of view	*/
	private IEnumerator CamZoomRoutine(float t, float initFOV, float finalFOV){
		float te = 0;
		while (te < t) {
			Camera.main.fieldOfView = Mathf.Lerp (initFOV, finalFOV, te / t);
			te += Time.deltaTime;
			yield return null;
		}
///////////////		Old method below: note the difference in code size that the Lerp function creates: it is very powerful /////////////////////
//		float d_FOV = dt_animation * (finalFOV - initFOV) / t;  // calculate the unit steps between field of view zooms
//		mainCamCamObj.fieldOfView = initFOV; 						// Set the camera to the initial FOV
//		// Zooming out means final Field of View (degrees) is greater than initial field of view (degrees)
//		if (finalFOV > initFOV) {
//			// keep going until final field of view is reached
//			while (mainCamCamObj.fieldOfView < finalFOV) {				
//				// If adding d_FOV causes field of view to exceed the final bound, set fov to final fov
//				// else add d_FOV to the camera field of view
//				if (mainCamCamObj.fieldOfView + d_FOV > finalFOV) {
//					mainCamCamObj.fieldOfView = finalFOV;
//				} else {
//					mainCamCamObj.fieldOfView += d_FOV;
//				}						
//				yield return new WaitForSeconds (dt_animation);
//			}
//		}
//		else if (finalFOV < initFOV){
//			while (mainCamCamObj.fieldOfView > finalFOV) {				
//				// If adding d_FOV causes field of view to exceed the final bound, set fov to final fov
//				// else add d_FOV to the camera field of view
//				if (mainCamCamObj.fieldOfView + d_FOV < finalFOV) {
//					mainCamCamObj.fieldOfView = finalFOV;
//				} else {
//					mainCamCamObj.fieldOfView += d_FOV;
//				}						
//				yield return new WaitForSeconds (dt_animation);
//			}
//		}
	}

	// Carousel rotation routine
	private IEnumerator stage5AltRoutine(){
		GameObject lsCarousel = GameObject.Find ("lsCarousel");
		GameObject ptCarousel = GameObject.Find ("ptCarousel");
		while (true) {
			Vector3 txV = Time.deltaTime*stage_speed[5]*Vector3.up*stage5halt;
			lsCarousel.transform.eulerAngles += txV;
			ptCarousel.transform.eulerAngles -= txV;
			yield return null;
		}
	}
	// Stage 5 continuous rotation (by landscape and oportrait parents)
	private IEnumerator stage5Routine(){					
		while (frames[7].transform.parent == portraitCenter.transform) {
			Vector3 txV = Time.deltaTime*stage_speed[5]*Vector3.forward;
			landscapeCenter.transform.eulerAngles -= txV;
			portraitCenter.transform.eulerAngles += txV;
			for(int j = 0; j < numframes; j++){
				if (frames[j].transform.parent == landscapeCenter.transform || frames[j].transform.parent == portraitCenter.transform) {
					frames[j].transform.eulerAngles = gpStorage1 [j];
				}
			}
			yield return null;
		}
	}
	private IEnumerator stage_5_rotator(GameObject frameObj, int objIndex, int direction, float additionalAngle){		
		float dl = 0;
		while (dl < dist [5] + additionalAngle) {
			dl += stage_speed [5] * Time.deltaTime;
			yield return null;
		}
		StartCoroutine(carouselStartRoutine (frameObj));
	}

	private IEnumerator stage5haltDim(){
		float te = 0;
		float initFOV = Camera.main.fieldOfView;
		while (te < 10){			
			stage5halt = Mathf.Lerp (1, 0.5f, te / 10);
			Camera.main.fieldOfView = Mathf.Lerp (initFOV,initFOV-5,te/10);
			te += Time.deltaTime;
			yield return null;
		}
	}
	private IEnumerator carouselStartRoutine(GameObject frame){
		GameObject lsCarousel = GameObject.Find ("lsCarousel");
		GameObject ptCarousel = GameObject.Find ("ptCarousel");
		Transform ft = frame.transform;
		float s5 = stage_speed [5];
		// Change parent 
		if (ft.parent == landscapeCenter.transform) {
			ft.SetParent (lsCarousel.transform);
		} else {
			ft.SetParent (ptCarousel.transform);
			ft.transform.eulerAngles = Vector3.forward * 90;
		}
		// chaange in angle is 360 degrees
		float dl = 0;
		float t_360 = 360f / s5; // time taken to traverse 360 degrees at speed = stage 5 speed
		float vert_dist = ft.position.y - ft.parent.position.y;
		float vert_speed = vert_dist/t_360;
		if (frame == frames [7]) {
			StartCoroutine (stage5haltDim ());
		}
		while (dl < 360) {				
			float dt = Time.deltaTime;
			dl += Time.deltaTime * s5;
			ft.position = ft.position - new Vector3(0,vert_speed*dt,0);
			yield return null;
		}

	}

	// ========================================= VOID START ==================================================//
	void Start () {					
		tm = gameObject.GetComponent<textureMod> ();
        ph = FindObjectOfType<persistentHelper>();
        if (ph != null)
        {
            rewardLevel = ph.rewardLevel;
        }
        else
        {
            rewardLevel = 1;
        }

        // cycle over each stage and set up varibles
        for (int i = 0; i < numstages; i++) {
			// obtain speeds from distance and time specified
			stage_speed [i] = dist [i] / t_stage [i];
			// all use simple cosine gradient
			norms [i] = PIover2; 
			// Create and start delay+ state change croutines
			stagedelay[i] = DelayAndChangeStage(postStepDelays[i], i+1);
			// All current distances are 0;
			current_dist[i] = 0f;
			stageComplete [i] = false;
		}
		stage_speed [5] = stage_speed [4];
		t_stage [5] = stage_speed [5] / dist [5];
		// Cycle over the number of image frames
		for (int i = 0; i <numframes; i++) {
			// Set up stage 5 routines. landscape frames rotate clockwise, portraits rotate anticlockwise
			if (i < 4) { // landscape frames
				stage5_routines [i] = stage_5_rotator (frames [i], i, -1, i*90);
			} else {
				stage5_routines [i] = stage_5_rotator (frames [i], i, 1, 45f + (i-4)*90);
			}
			stage5Executed [i] = false;
			// Get the translation vectors. These are obtained with respect to the parent positions. the parent (landscapeCenter/ portraitCenter)
			// is responsible for z axis translation, while each fram is individually translated in the translation Vector direction in stage 1
			translationVectors [i] = frames [i].transform.localPosition;
			// set the position of each frame w.r.t its parent to a certian fraction of the initial distance. The overridden initial distance is the 
			// distance that the frames will reach at the end of stage 1
			frames [i].transform.localPosition = translationVectors[i] * initialFraction;
		}
		/* Stage 5 has a special setup. I specify a desired angle of rotation (multiple of 90)
		using a fraction of stage 4's speed as the desired speed, the script then calcualates how
		long it would take for the desired rotation amount and assigns it.
		This will be the speed used even in the carousel stages */

		// Calculate amount of time for camera zoom to occur. This will occur until stage 2 begins
		float camZoomDelay = 0;
		// Add the amount of time between the start of stage 0 and the end of stage 2
		// This includes the actual stage run time and the prestep stage delays.
		for (int i = 0; i < 2; i++) {			
			camZoomDelay += postStepDelays [i] + t_stage [i];
		}
		camRoutines [0] = CamZoomRoutine(camZoomDelay, initialFOV, 45);
		current_stage = 0;
		t_elapsed = 0;
	}
	// ===================================== END VOID START ==================================================//
	
	// ============================================ VOID UPDATE =============================================//
	void Update () {		
		switch(current_stage){
		// ======== Stage 0, no action delay stage ===========================//
		case 0:					
			StartCoroutine (stagedelay [0]);
			current_stage = -1;
			t_elapsed = 0;
			lsPos = landscapeCenter.transform.position;
			ptPos = portraitCenter.transform.position;	
			break;

		// ======== Stage 1, slow tranlation  ===========================//
		case 1:
			if (t_elapsed < t_stage [1]) {
				// angle calc.
				float angle = Mathf.Lerp (0, Mathf.PI / 2, t_elapsed / t_stage [1]);
				float sinA = Mathf.Sin (angle);
				// z axis translations calc.
				landscapeCenter.transform.position = lsPos + dist [1] * Vector3.forward * sinA;
				portraitCenter.transform.position = ptPos + dist [1] * Vector3.back * sinA;
				// framewise outward tx calc.
				float fraction = initialFraction + (1 - initialFraction) * sinA;
				int i = 0;
				foreach (GameObject frame in frames) {
					frame.transform.localPosition = translationVectors [i] * fraction;
					i++;
				}
				t_elapsed += Time.deltaTime;
			}
			else { // (if stage is complete)
				// Set all desired positions to exact values
				landscapeCenter.transform.position = lsPos + dist [1] * Vector3.forward;
				portraitCenter.transform.position = ptPos + dist [1] * Vector3.back;
				for (int i = 0; i < numframes; i++) {
					frames [i].transform.localPosition = translationVectors [i];
				}
				// set current_stage to unreachable value, elapsed time set to zero
				current_stage = -1;
				t_elapsed = 0;
				// Setup for next stage
				int j = 0;
				foreach(GameObject frame in frames){
					gpStorage1[j] = frame.transform.eulerAngles;
					j++;
				}
				if (rewardLevel > 0) {
					StartCoroutine (stagedelay [1]);
				}
				lsPos = landscapeCenter.transform.position;
				ptPos = portraitCenter.transform.position;

			}
			break;

		// ======== Stage 2, Slow rotation  ===========================//
		case 2:	
			if (t_elapsed < t_stage [2]) {
				// angle calc.
				float angle = Mathf.Lerp (0, Mathf.PI / 2, t_elapsed / t_stage [2]);
				float sinA = Mathf.Sin (angle);
				// z eulerangle translations calc.
				landscapeCenter.transform.eulerAngles = dist [2] * Vector3.forward * sinA;
				portraitCenter.transform.eulerAngles = dist [2] * Vector3.back * sinA;
				// framewise restoration of orientation
				int i = 0;
				foreach (GameObject frame in frames) {
					frame.transform.eulerAngles = gpStorage1[i];
					i++;
				}
				t_elapsed += Time.deltaTime;
			}
			else {
				t_elapsed = 0;
				current_stage = -1;
				// set positions to exact values
				landscapeCenter.transform.eulerAngles = dist [2] * Vector3.forward;
				portraitCenter.transform.eulerAngles = dist [2] * Vector3.back;
				StartCoroutine (stagedelay[2]);
			}
			break;			

		// ======== Stage 3, quick translation  ===========================//
		case 3:			
			if (t_elapsed < t_stage [3]) {
				// z axis translations calc.
				landscapeCenter.transform.position = lsPos + dist [3] * Vector3.back * Mathf.Lerp(0,1,t_elapsed/t_stage[3]);
				portraitCenter.transform.position = ptPos + dist [3] * Vector3.forward *  Mathf.Lerp(0,1,t_elapsed/t_stage[3]);
				t_elapsed += Time.deltaTime;
			}
			else {
				current_stage = -1;
				t_elapsed = 0;
				// set positions to exact values
				landscapeCenter.transform.position = lsPos + dist [3] * Vector3.back;
				portraitCenter.transform.position = ptPos + dist [3] * Vector3.forward;
				lsPos = landscapeCenter.transform.eulerAngles;
				ptPos = portraitCenter.transform.eulerAngles;
				StartCoroutine (stagedelay[3]);
			}
			break;

		// ======== Stage 4, quick rotation  ===========================//
		case 4:
			if (t_elapsed < t_stage [4]) {				
				// z eulerangle translations calc.
				landscapeCenter.transform.eulerAngles = lsPos + dist [4] * Vector3.back*Mathf.Lerp(0,1,t_elapsed/t_stage[4]);
				portraitCenter.transform.eulerAngles = ptPos + dist [4] * Vector3.forward*Mathf.Lerp(0,1,t_elapsed/t_stage[4]);
				// framewise restoration of orientation
				int i = 0;
				foreach (GameObject frame in frames) {
					frame.transform.eulerAngles = gpStorage1[i];
					i++;
				}
				t_elapsed += Time.deltaTime;
			} else {// no delay, need smooth transition between stages
				landscapeCenter.transform.eulerAngles = lsPos + dist [4] * Vector3.back;
				portraitCenter.transform.eulerAngles = ptPos + dist [4] * Vector3.forward;
				t_elapsed = 0;
				current_stage = 5;
			}
			break;		
		// =================== Stage 5, passive rotation ======================//
		case 5:						
			// one time setup of coroutines
			StartCoroutine (stage5Routine ());
			if (rewardLevel > 1) {
				StartCoroutine (stage5AltRoutine ()); // Carousel rotators routine
				for (int i = 0; i < numframes; i++) {
					StartCoroutine (stage5_routines [i]); // Routine to switch frames from centers to carousel
				}
			}
			//StartCoroutine (trackerCamRoutine (t_tracker[0], t_tracker[1], t_tracker[2], t_tracker[3] ));				
			// start carousel coordinator here. It will stop coroutines as needed	
			//StartCoroutine(carouselCoordinator());
			current_stage = -1;
			break;		
		
		// ======== default ==========/
		default:			
			break;
		}

	}



}
