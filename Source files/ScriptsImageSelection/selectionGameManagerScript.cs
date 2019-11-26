using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;

public class selectionGameManagerScript : MonoBehaviour {

	// Timer management
	public GameObject timerBar;
	public GameObject startButton;
	public GameObject viewLight;
	public GameObject rotlight;
	public GameObject diffBtnPanel;
	public GameObject imagesPanel;
	public GameObject difficultyText;
	public GameObject tUpText;
	public float[] levelTimes = { 10f, 5f, 3f };

	// Other buttons
	public GameObject loadAnimationButton;
	public GameObject viewImagesButton;

	// Text objs
	public GameObject nxtLevelTxt;
	public GameObject CongratsTxt;

	// tracker Cam
	public GameObject trackerCamObj;
	public Camera trackerCam;

	// Level variables
	public bool switchLevels = false;		
	public int currentLevel = 0; // levels start from 0
	public int nextLevel = 0;
	public bool levelSuccess = false;
	public bool begin = false;
	public bool game_started = false;
	public int difficulty = 1;
	public float timeTaken = 0;
	public string scene_name;

	// Guess manager
	private GuessManager guessManager;
	private RoundTextChanger textChanger;
	private timerShrink ts;
	private IEnumerator levelExecutor;

	public float total_time;

	private IEnumerator FadeToBlackAndChangeLevel(){	
		// Display the amount of time taken to complete the levels
		string txt = "You took\n" + (total_time.ToString ("F0")) + "\nseconds!";		
		textChanger.namesTextBox.text = txt;
        // Stop the level executor
        if (levelExecutor != null) {
			StopCoroutine (levelExecutor);
		}	
		rotlight.SetActive (false);

		float elapsedTime = 0f;
		float duration = 4.0f;
		guessManager.fadeAudio (duration);

		Light lo = viewLight.GetComponent<Light> ();
		float initInt = lo.intensity;

		while(elapsedTime < duration){
			float fraction = elapsedTime / duration;
			elapsedTime  += Time.deltaTime;
			lo.intensity = Mathf.Lerp (initInt, 0, fraction);
			yield return null;
		}
        // If the user actually beat the game, then set the reward level appropriately
        if (currentLevel == difficulty-1)
        {            
            FindObjectOfType<persistentHelper>().rewardLevel = currentLevel;            
        }
        else
        {
            FindObjectOfType<persistentHelper>().rewardLevel = 0;
        }
        loadAnimation (); // Load the animation
	}

	private IEnumerator executeLevel(int level){
		float elapsedTime = 0f;
		// Wait for time equal to level time
		while (elapsedTime < levelTimes [level]) {
			elapsedTime += Time.deltaTime;
			// Check to see if all images were guessed correctly
			if (guessManager.levelPassed) {
				ts.endTimer ();
				total_time += elapsedTime;
				guessManager.levelPassed = false;
				if (nextLevel == difficulty) {					
					CongratsTxt.SetActive (true);
					switchLevels = true;
				} else {// else					
					nxtLevelTxt.SetActive (true); // congratulate them, wait for a while,
					yield return new WaitForSeconds (1f);
					nxtLevelTxt.SetActive (false);
					switchLevels = true; // then switch levels
				}
			}
			yield return null;
		}

		if (!guessManager.levelPassed) {// If the level was failed, restart the game
			game_started = false;
			StartCoroutine(restartForFailure());
		}	
	}

	private IEnumerator restartForFailure(){
		if (levelExecutor != null) {
			StopCoroutine (levelExecutor);
		}
		tUpText.GetComponent<Text>().enabled = true;
		yield return new WaitForSeconds (4f);
		tUpText.GetComponent<Text> ().enabled = false;
		restartLevel ();
	}
	// Use this for initialization
	void Start () {
		timeTaken = 0f;
		switchLevels = false;		
		currentLevel = 0; // levels start from 0
		nextLevel = 0;
		levelSuccess = false;
		begin = false;
		guessManager = gameObject.GetComponent<GuessManager> ();
		textChanger = gameObject.GetComponent<RoundTextChanger> ();
		ts = timerBar.GetComponent<timerShrink> ();

		textChanger.clearScore ();
		//imagesPanel.SetActive (false);

		total_time = 0f;
	}
	
	// Update is called once per frame
	void Update () {
		// This executes once the game has been started, and the UI must vanish
		if (begin) {
			if(difficultyText != null){
				difficultyText.SetActive (false);
			}
			levelExecutor = executeLevel(nextLevel);
			StartCoroutine (levelExecutor);
			// Restart the timer object
			timerBar.SetActive(true);
			ts.RestartTimer (levelTimes[0]);
			// Create the first level's round name and person name
			textChanger.currentNameIndex = 0;
			textChanger.updateRound (0);
			textChanger.setFirstName (0);
			currentLevel = 0;
			nextLevel = 1;
			begin = false;
			game_started = true;
			// Deactivate initial UI buttons etc
			viewImagesButton.SetActive(false);
			loadAnimationButton.SetActive(false);
			diffBtnPanel.SetActive (false);
			imagesPanel.SetActive (true);
			// Deactivate trackerCamscripts
		}
		// code to switch to next level
		if (switchLevels) {
			if (nextLevel == 0) {
				textChanger.clearScore ();
				total_time = 0;
			}
			if (nextLevel == difficulty) {
				ts.endTimer ();
				game_started = false;
				// switch to the animation
				StartCoroutine(FadeToBlackAndChangeLevel());
			}
			// If the next level is within 0-2
			else {
				guessManager.restoreImageTxs ();
				// Stop the levelExecutor coroutine thats currently running and set up a new one with the 
				// updated timing
				if (levelExecutor != null) {
					StopCoroutine (levelExecutor);
				}

				// Change the visible names for the next stage
				textChanger.updateRound (nextLevel);
				textChanger.setFirstName (nextLevel);

				levelExecutor = executeLevel (nextLevel); // Create the new coroutine variable
				StartCoroutine (levelExecutor); // start the coroutine
				ts.RestartTimer (levelTimes [nextLevel]);

				guessManager.numCorrectGuesses = 0;
				currentLevel = nextLevel;
				nextLevel++;
				guessManager.levelPassed = false;
				switchLevels = false;
			}
		}
	}

	public void startGame(){
		begin = true;
		startButton.SetActive (false);
	}

	public void updateDifficulty2(int diff){
		difficulty = diff;
	}
	public void restartLevel(){
		SceneManager.LoadScene (0);
	}
	public void loadAnimation(){
		loadAnimationButton.SetActive (true);
		Text t = loadAnimationButton.GetComponentInChildren<Text> ();
		t.text = "Loading animation...";       
		SceneManager.LoadScene (1); // Load the animation scene
	}
	public void toggleDifficultyPanelView(){
		if (viewImagesButton.GetComponentInChildren<Text> ().text == "View images") {
			viewImagesButton.GetComponentInChildren <Text> ().text = "Back to game";
		} else {
			viewImagesButton.GetComponentInChildren<Text> ().text = "View images";
		}
	}

	public void closeUpCamDeactivate(){
		Camera.main.enabled = true;
		trackerCam.enabled = false;
	}
}
