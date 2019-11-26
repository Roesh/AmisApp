using UnityEngine;
using System.Collections;

public class movement_animationAndroid : MonoBehaviour {

	// Stage variables
	public const int numstages = 6;
	public const int numframes = 8;
	public const int numlandscape = 4;
	public const int numportrait = 4;

	public GameObject[] frames = new GameObject[numframes];
	public GameObject[] landscape = new GameObject[numlandscape];
	public GameObject[] portrait = new GameObject[numportrait];
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
	public float finalFOV;
	public float[] t_tracker = new float[4];

	public int current_stage = 0;
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
	public float t_start;
	public float dt_animation;
	private bool stage5set;

	// Stage 1 outward movement varibles
	public float initialFraction = 0.5f;
	public Vector3[] translationVectors = new Vector3[numframes];
	public float fractionVsTime;

	private IEnumerator DelayAndChangeStage(float delay , int next_stage){
		yield return new WaitForSeconds (delay);
		t_start = Time.time;
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
		mainCamCamObj.fieldOfView = initFOV; 						// Set the camera to the initial FOVs)
		float t_elapsed = 0;

		while (t_elapsed < t) {				
			mainCamCamObj.fieldOfView = Mathf.Lerp(initFOV, finalFOV, t_elapsed/t);
			t_elapsed += Time.deltaTime;
			yield return null;
		}
	}
	private IEnumerator trackerCamRoutine(float t_stageA, float t_stageB, float t_stageC, float t_stageD){
		float elapsedTime = 0;
		while (true) {
			yield return new WaitForSeconds (t_stageA); // First wait period
			// Deactivate main cam and activate tracker cam
			maincam.SetActive (false);
			trackerCam.transform.LookAt (staticFrame.transform.position);
			trackerCam.SetActive (true);

			yield return new WaitForSeconds (t_stageB); // Wait in tracker cam for some time
			// Move tracker cam and look at the big frame
			trackerCam.transform.position = new Vector3(1.8f, 1.8f, -1.8f); 
			trackerCam.transform.LookAt (staticFrame.transform.position);

			yield return new WaitForSeconds (t_stageC); // Wait in the tracker cam for some more time

			// Follow two frames for a certain period of time in tracker cam
			trackerCamCam.fieldOfView = 18;
			mainCamCamObj.fieldOfView = 13;
			bool state = false;
			int cf = 0;
			foreach (GameObject imgFrame in frames) {
				if (cf > 3) {
					foreach (GameObject fm in landscape) {
						fm.SetActive (false);
					}
				}
				trackerCam.SetActive (!state);
				maincam.SetActive (state);
				while (elapsedTime < t_stageC) {
					elapsedTime += Time.deltaTime;
					if (state) {
						maincam.transform.LookAt (imgFrame.transform.position);
					} else {						
						trackerCam.transform.LookAt (imgFrame.transform.position);
					}
					yield return null;
				}
				if (state) {
					trackerCam.transform.position = new Vector3(-trackerCam.transform.position.x, 1.8f, -1.8f);
				}
				elapsedTime = 0;
				state = !state;
				cf++;
			}
			foreach (GameObject fm in landscape) {
				fm.SetActive (true);
			}
			// reset FOVs
			trackerCamCam.fieldOfView = 65;
			mainCamCamObj.fieldOfView = finalFOV;
			maincam.transform.LookAt (staticFrame.transform.position);
			// Go to main cam for t_stageC/2 seconds
			// Move tracker cam and look at the big frame
			trackerCam.transform.position = new Vector3(-1.8f, 1.8f, -1.8f);
			trackerCam.SetActive (false);
			maincam.SetActive (true);
			yield return new WaitForSeconds (t_stageC * 2);
		}
	}
	private IEnumerator stage_5_rotator(GameObject frameObj, int objIndex, int direction, float additionalAngle){								
		while(true){
			float temp_angle = stage_speed[5]*Time.deltaTime;
			frameObj.transform.RotateAround (landscapeCenter.transform.position, Vector3.forward, direction*temp_angle);
			frameObj.transform.RotateAround (frameObj.transform.position, Vector3.back, direction*temp_angle);
			yield return null;
		}
	}

	private IEnumerator carouselStartRoutine(GameObject frame, float dt_animation, int direction){
		Vector3 carouselPivot = ca_pivot.transform.position;
		float dist = 360f; // degrees on the up axis causes the image to end up in front
		float vert_dist = Mathf.Abs(frame.transform.position.y - carouselPivot.y);

		float adjusted_speed = stage_speed [5]; // In the time it takes for stage 5 frame to cover 45 degrees, this carousel stage must cover 90 degrees
		float temp_angle = adjusted_speed * dt_animation;  // calculate the dtheta value per call
		float localAngle = 0f;
		bool complete = false;
		float t_routine = dist / adjusted_speed;
		float vert_speed = vert_dist / t_routine;
		// Make sure the x-axis is zeroed
		frame.transform.position = new Vector3 (0, frame.transform.position.y, frame.transform.position.z);
		while ((localAngle < dist) && !complete) {
			if ((localAngle += temp_angle) > dist) {
				temp_angle = dist - (localAngle -= temp_angle);
				localAngle += temp_angle;
				complete = true;
			}
			// adjust the direction of rotation on the up axis based on the direction variable
			frame.transform.RotateAround (carouselPivot, Vector3.up, direction * temp_angle);
			if (direction == 1) {
				frame.transform.Translate (Vector3.down * vert_speed * dt_animation);
			} else {
				frame.transform.Translate (Vector3.left * vert_speed * dt_animation);
			}
			//frame.transform.RotateAround (carouselPivot, Vector3.down, direction * temp_angle);
			//frame.transform.RotateAround (carouselPivot, Vector3.back, direction * temp_angle / 2);
			//frame.transform.RotateAround (frame.transform.position, Vector3.forward, direction * temp_angle / 2);
			yield return new WaitForSeconds (dt_animation);
		}
		temp_angle *= 2.2f;
		// Once the above coroutine is done, it creates the final carousel coroutine which continues forever
		while (true) {			
			frame.transform.RotateAround (carouselPivot, Vector3.up, direction * temp_angle * stage5halt);
			yield return new WaitForSeconds (dt_animation);
		}
	}

	/* carousel coordinator stops the stage 5 coroutines and begins the carousel start routines individualls
	It is responsible for timing WHEN each frame begins the exit from stage 5 via the postStepDelays variable*/
	private IEnumerator carouselCoordinator(){		
		float postStepDelays = carouselInterval / stage_speed [5]; // delay TIME = DISTANCE to travel/ SPEED of travel
		// The order in which to remove frames from stage 5
		int[] frameOrder = {1,5,2,6,3,7,4,8};
		for(int i = 0; i <numframes; i++){			
			// landscape frames (i<4) are all rotating clockwise
			int direction;
			if (frameOrder[i]-1 < 4) {
				direction = 1;
			} else {							
				direction = -1;
			}
			while (!stage5Executed [frameOrder [i] - 1]) {
				yield return new WaitForSeconds (dt_animation);
			}				
			// Stop the stage 5 coroutine
			StopCoroutine (stage5_routines [frameOrder[i]-1]);
			// Start the carousel start coroutine
			StartCoroutine(carouselStartRoutine (frames [frameOrder[i]-1], dt_animation, direction));
			// yield to update, come back after the correct delay
			yield return new WaitForSeconds (postStepDelays);
		}
	}

	// ========================================= VOID START ==================================================//
	void Start () {		
		stage5set = false;

		trackerCam.SetActive (false);
		maincam.SetActive (true);
		mainCamCamObj = maincam.GetComponent<Camera>(); // get the camera object associated withe the maincam gameobject
		mainCamCamObj.fieldOfView = initialFOV;
		trackerCamCam = trackerCam.GetComponent<Camera> ();

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
		camRoutines [0] = CamZoomRoutine(camZoomDelay, initialFOV, finalFOV);

	}
	// ===================================== END VOID START ==================================================//
	
	// ============================================ VOID UPDATE =============================================//
	void Update () {
		
		now = Time.time;
		#if UNITY_STANDALONE || UNITY_WEBGL

		//Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
		maincam.transform.RotateAround(mainCamCamObj.transform.position, transform.up, Input.GetAxis("Horizontal"));

		//Get input from the input manager, round it to an integer and store in vertical to set y axis move direction
		maincam.transform.RotateAround(mainCamCamObj.transform.position, transform.right, -Input.GetAxisRaw ("Vertical"));
		#endif

		switch(current_stage){
		// ======== Stage 0, no action delay stage ===========================//
		case 0:		
			current_stage = -1;
			StartCoroutine (stagedelay [0]);
			StartCoroutine (camRoutines[0]);
			break;

		// ======== Stage 1, slow tranlation  ===========================//
		case 1:
			if (now - t_start < t_stage[1]) {								
				// Obtain the zdiff
				float zdiffl = Mathf.Lerp(1, 1+dist[1], (now-t_start)/t_stage[1]);
				float zdiffp = Mathf.Lerp(1, 1-dist[1], (now-t_start)/t_stage[1]);

				// Apply to each object
				landscapeCenter.transform.position = new Vector3(landscapeCenter.transform.position.x,
					landscapeCenter.transform.position.y, zdiffl);
				portraitCenter.transform.position = new Vector3(portraitCenter.transform.position.x,
					portraitCenter.transform.position.y, zdiffp);
				// t = now - t_start. at t = 0, currentFraction = initialFraction. at t = t_stage[1], currentFraction should be 1
				float currentFraction = initialFraction + ((now - t_start) / t_stage [1])*(1-initialFraction); 
				for(int i = 0; i < numframes; i++){
					// the current fraction is obtained as follows. to the initial fraction, we add an amount equal to the following:
					// The product of The fraction of time that has passed since the stage has begun and the 1-the initial fraction 
					frames[i].transform.localPosition = translationVectors[i]*currentFraction;
				}
			} else { // if stage is complete
//				// set current_stage to unreachable value
//				for (int i = 0; i < numframes; i++) {
//					frames [i].transform.localPosition = translationVectors [i];
//				}
				current_stage = -1;
				StartCoroutine(stagedelay[1]);
			}
			break;

		// ======== Stage 2, Slow rotation  ===========================//
		case 2:	
			if ((now - t_start < t_stage[2] || current_dist[2] < dist[2])&& !stageComplete[2]) {
				// obtaim the angle to rotate for this frame using the time that has passed since the last frame
				multiplier = Mathf.Cos(PIover2*(now-t_start)/(t_stage[2]));
				if (multiplier < multiplier_threshold) {
					multiplier = multiplier_threshold / multiplier_adjustment;
				}
				float temp_angle = booster_factor*stage_speed[2] * Time.deltaTime * multiplier *norms[2];

				// Check if current adiditon will cause angle to axceed distnace limit
				if((current_dist[2] += temp_angle) > dist[2]){
					temp_angle = dist[2] - (current_dist[2] -= temp_angle);
					current_dist[2] += temp_angle;
					stageComplete [2] = true;
				}
				foreach (GameObject frame in landscape) {
					frame.transform.RotateAround (landscapeCenter.transform.position, Vector3.forward, temp_angle);
					frame.transform.RotateAround (frame.transform.position, Vector3.forward, -temp_angle);
				}
				foreach (GameObject frame in portrait) {
					frame.transform.RotateAround (portraitCenter.transform.position, Vector3.forward, -temp_angle);
					frame.transform.RotateAround (frame.transform.position, Vector3.forward, temp_angle);
				}
			} else {
				current_stage = -1;
				StopCoroutine (camRoutines [0]);
				StartCoroutine (stagedelay[2]);
			}
			break;			

		// ======== Stage 3, quick translation  ===========================//
		case 3:			
			if (now - t_start < t_stage[3]) {	
				// Obtain the zdiff
				float zdiffl = Mathf.Lerp(1.2f, 0.7f, (now-t_start)/t_stage[3]);
				float zdiffp = Mathf.Lerp(0.7f, 1.2f, (now-t_start)/t_stage[3]);

				// Apply to each object
				landscapeCenter.transform.position = new Vector3(landscapeCenter.transform.position.x,
					landscapeCenter.transform.position.y, zdiffl);
				portraitCenter.transform.position = new Vector3(portraitCenter.transform.position.x,
					portraitCenter.transform.position.y, zdiffp);
			} else {
				current_stage = -1;
				StartCoroutine (stagedelay[3]);
			}
			break;

		// ======== Stage 4, quick rotation  ===========================//
		case 4:
			if ((now - t_start < t_stage [4] || current_dist[4] < dist[4])&& !stageComplete[4]) {
				// temp angle is essentially d_theta
				multiplier = Mathf.Cos (PIover2 * (now - t_start) / t_stage [4]);
				if (multiplier < multiplier_threshold) {
					multiplier = multiplier_threshold / multiplier_adjustment;
				}
				float temp_angle = booster_factor*stage_speed [4] * Time.deltaTime * multiplier * norms [4];
				// Heres what happens: we check if the current change in angle per unit time is less than twice the speed of the
				// next static rotation stage. The cosine curve near pi/2 is near -1 slope. the area compensation will be 
				// 0.5, meaning running the stage for half the cut-off speed should bring it near the initial position
				if (temp_angle / Time.deltaTime < 2 * stage_speed [5]) {
					temp_angle = stage_speed[5] * Time.deltaTime;
				}
				// After obtaining the temporary angle, check to see if adding it will cause the objects to rotate more than necesssray
				if((current_dist[4] += temp_angle) > dist[4]){
					temp_angle = dist[4] - (current_dist[4] -= temp_angle);
					current_dist[4] += temp_angle;
					stageComplete [4] = true;
				}
				foreach (GameObject frame in landscape) {
					frame.transform.RotateAround (landscapeCenter.transform.position, Vector3.forward, -temp_angle);
					frame.transform.RotateAround (frame.transform.position, Vector3.forward, temp_angle);
				}
				foreach (GameObject frame in portrait) {
					frame.transform.RotateAround (portraitCenter.transform.position, Vector3.forward, temp_angle);
					frame.transform.RotateAround (frame.transform.position, Vector3.forward, -temp_angle);
				}
			} else {// no delay, need smooth transition between stages
				t_start = now;
				current_stage = 5;
			}
			break;		
		// =================== Stage 5, passive rotation ======================//
		case 5:
			if (now - t_start < t_stage [5]) {
				if (!stage5set) {
					// one time setup of coroutines
					for (int i = 0; i <numframes; i++) {
						StartCoroutine (stage5_routines [i]);
					}
					StartCoroutine (trackerCamRoutine (t_tracker[0], t_tracker[1], t_tracker[2], t_tracker[3] ));
					stage5set = true;
				}
			} else {			
				// start carousel coordinator here. It will stop coroutines as needed	
				current_stage = -1;
				//StartCoroutine (carouselCoordinator ());
			}
			break;
		// ==================== Stage 6, ===========//
		case 6:			
			// MAKE CAMERA GO NEAR CAROUEL PIVOT AND OBSERVE ONE FRAME CIRCLING AROUND
			current_stage = -1;

			break;
		// ======== default ==========/
		default:
			break;
		}
	}

		

}
