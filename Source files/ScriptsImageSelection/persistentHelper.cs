using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class persistentHelper : MonoBehaviour {

	private static persistentHelper _instance;

	public static persistentHelper Instance { get { return _instance; } }

	public int rewardLevel; // 0,1,2 pertains to easy/medium/hard
    public Texture2D[] imgTextures= new Texture2D[9];
    public string[] nameTexts = new string[9];
    public bool firstTimeSetup;

	private void Awake()
	{
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
        firstTimeSetup = false;
		DontDestroyOnLoad (this.gameObject);
        imgTextures = new Texture2D[9];
        firstTimeSetup = true;
        rewardLevel = 0;
    }
	// Use this for initialization
	void Start () {
          
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void setRewardLevel(int rl){
		rewardLevel = rl;
	}
    public void setAnimColor()
    {
        if (rewardLevel == 1)
        {
            GameObject.Find("loadAnimation").GetComponent<Outline>().effectColor = Color.green;
        }
        else if (rewardLevel == 2)
        {
            GameObject.Find("loadAnimation").GetComponent<Outline>().effectColor = Color.yellow;
        }
    }
}
