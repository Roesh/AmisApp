using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;

public class RoundTextChanger : MonoBehaviour {

    public int currentScore = 0;
    public GameObject[] roundTextObjs = new GameObject[3];
    public string[] nameTexts = new string[8];
    public GameObject[] imgObjs = new GameObject[8];
    public GuessManager gm;
    public textureMod tm;
    public userManager um;
    public GameObject[] scoreObjs = new GameObject[21];
    public GameObject lvlPassAudio;
    public Text namesTextBox;

    public int currentNameIndex;
    public int currentRoundIndex;

    public int[] nameOrder = new int[8];

    public GameObject ls;           // ls is the loadscreen
    public GameObject debugText;
    public GameObject debugTextOnline;
    public persistentHelper ph;

    // Use this for initialization
    void Start() {
        gm = gameObject.GetComponent<GuessManager>();
        tm = gameObject.GetComponent<textureMod>();
        ph = FindObjectOfType<persistentHelper>();
        um = FindObjectOfType<userManager>();
        // Initialize name and round indexes
        for (int i = 0; i < 3; i++) {
            // Deactivate all objects				
            roundTextObjs[i].SetActive(false);
        }
        // Automatically setup the textures if this is not the first login
        // The Done button calls this fn the first time
        if (!ph.firstTimeSetup){
            getNamesAndSetTextures();
            ph.setAnimColor(); // also change the effect color of the animation button
        }
    }

    // Update is called once per frame
    void Update() {
    }

    public void updateRound(int roundIndex) {
        // Inactivating current index and activating the new round index allows
        //		// correct operation when roundIndex == currentRoundIndex and causes no operation
        if (roundIndex != 0) {
            AudioSource asource = lvlPassAudio.GetComponent<AudioSource>();
            asource.Play();
        }
        roundTextObjs[currentRoundIndex].SetActive(false);
        roundTextObjs[roundIndex].SetActive(true);
        currentRoundIndex = roundIndex;
        shuffleNames(roundIndex);
    }

    public void updateName(int level) {
        // increment the name index. If it is between 0-7, dispay that name
        // otherwise do ntohing
        if (++currentNameIndex < 8) {
            if (level == 2) {
                namesTextBox.text = nameTexts[nameOrder[currentNameIndex]];
            } else {
                namesTextBox.text = nameTexts[nameOrder[currentNameIndex]];
            }
        }
    }
    // set the first name
    public void setFirstName(int level) {
        if (level == 2) {
            // NAME HCANGE
            namesTextBox.text = nameTexts[nameOrder[0]];
        } else {
            namesTextBox.text = nameTexts[nameOrder[0]];
        }
    }

    // Shuffle names is called everytime the round is updated
    public void shuffleNames(int level) {
        currentNameIndex = 0;
        Random.InitState(Mathf.FloorToInt(Time.time * 100000));

        // reorder the array
        for (int i = 0; i < 8; i++) {
            nameOrder[i] = i;
        }

        for (int j = 0; j < 8; j++) {
            // Select a random int between i and 7 to replace i
            // Swap the numbers at indexes i and the random number
            int swapIndex = Random.Range(j, 8);
            int temp = nameOrder[j];
            int temp2 = nameOrder[swapIndex];
            nameOrder[j] = temp2;
            nameOrder[swapIndex] = temp;
        }
    }
    public void incrementScore() {
        if (scoreObjs[currentScore] != null) {
            scoreObjs[currentScore++].SetActive(true);
        }
    }
    public void clearScore() {
        foreach (GameObject scoreObj in scoreObjs) {
            if (scoreObj != null) {
                scoreObj.SetActive(false);
            }
        }
        currentScore = 0;
    }


    ////// Interfacing with file system and WEB to obtain NAMES and TEXTURES based on the entered username ///////
    // of local AddedResources folder
    public void getNamesAndSetTextures()
    {
        StartCoroutine(imageSetup());
    }

    void imageReadingComplete()
    {
        um.turnOffDialog();
        Destroy(ls);
        GameObject musicObj = GameObject.Find("introMusic");
        musicObj.GetComponent<AudioSource>().Play();
    }
    public IEnumerator imageSetup()
    {
        
        ls.SetActive(true); // Activate loading screen
        yield return new WaitForSeconds(0.1f);
        readNamesFromFile();
        if (um.sourceIsLocal || !ph.firstTimeSetup)
        {
            imageReadingComplete();
        }
        else
        {
            bool imgLoadComplete = false;
            while (!imgLoadComplete)
            {
                if (tm.loadFail)
                {
                    imgLoadComplete = true;
                }
                else
                {
                    imgLoadComplete = true; // We will now AND this variable with the flags for whether
                                            // images are set. If through this anding process, it is true, then all images are loaded
                    foreach (bool b in tm.imagesAreSet)
                    {
                        imgLoadComplete = imgLoadComplete && b;
                    }
                }                
                yield return new WaitForSeconds(0.8f);
            }
            imageReadingComplete();     
        }  
    }
    public void readNamesFromFile()
    {
        // If this is the first time upon opening the game, the images must be loaded
        if (ph.firstTimeSetup)
        {                                    
            // If the source is local (on the computer or android device or local html folder)
            if (FindObjectOfType<userManager>().sourceIsLocal)
            {
                string[] nameTextsk = new string[9];
                string filePath = null;
                Debug.Log("getting names locally");
#if UNITY_STANDALONE || UNITY_WEBGL
                filePath = Application.dataPath + "\\AddedResources\\names.txt";
#elif UNITY_ANDROID
		filePath = Application.persistentDataPath + "/AddedResources/names.txt";
#endif
                if (File.Exists(filePath))
                {
                    nameTextsk = System.IO.File.ReadAllLines(filePath);                    
                    if (nameTextsk.Length == 9)
                    {
                        for (int i = 0; i < 9; i++)
                        {
                            // Assign the names to the persistnet helper and the round text manager's arrays                           
                            if (i < 8)
                            {
                                ph.nameTexts[i] = nameTexts[i] = nameTextsk[i];
                            }
                            else{
                                ph.nameTexts[i] = nameTextsk[i];
                            }
                            GameObject go;
                            if (i == 8)
                            {
                                go = null;
                            }
                            else
                            {
                                go = imgObjs[i];
                            }
                            if (!tm.setTexture(go, nameTextsk[i],i))
                            { // if image could not be loaded
                                debugText.SetActive(true);
                                Destroy(ls);
                            }
                        }
                    }
                    else // If 9 names are not present
                    {
                        debugText.SetActive(true);
                        Destroy(ls);
                    }
                }
                else // if file does not exist
                {
                    debugText.SetActive(true);
                    Destroy(ls);
                }
            }// end if source is local
            else // if source is ONLINE:
            {                
                string baseUrl = "http://roesh.000webhostapp.com/";                
                string url = baseUrl + um.userName + "/names.txt";
                Debug.Log("accessing " + url);
                StartCoroutine(getNamesAndImagesFromURL(url));                
            }
        }// otherwise load the images and names from the persistent helper
        else
        {
            for (int i = 0; i < 8; i++)
            {
                nameTexts[i] = ph.nameTexts[i];
                float PixelsPerUnit = 100f;
                Texture2D tex = ph.imgTextures[i];
                if (tex != null) { 
                Image img = imgObjs[i].GetComponent<Image>();
                Sprite NewSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0), PixelsPerUnit);
                img.sprite = NewSprite;
                }
            }            
        }
    }   
    
    // url points to a txt file
    IEnumerator getNamesAndImagesFromURL(string url)
    {
        WWW w = new WWW(url);
        string.Equals("http://roesh.000webhostapp.com/Kpatti/names.txt", url);
        yield return w; // return can be turned into text
        if (!string.IsNullOrEmpty(w.error))
        {
            Debug.Log("Error .. " + w.error);
            debugTextOnline.SetActive(true);
            tm.loadFail = true;
            if (string.Equals(w.error, "404 Not Found")){
                debugTextOnline.GetComponentInChildren<Text>().text = "Couldn't find images for that name";
            }
            Destroy(ls);
            // for example, often 'Error .. 404 Not Found'
        }
        else // If there was no error opening the webpage
            {
            //separate all that text in to lines:
            string total = w.text;
            string[] nameTextsk = total.Split('\n');
            Debug.Log(nameTextsk.Length);
            if (nameTextsk.Length >= 9)
            {
                for (int i = 0; i < 9; i++)
                {
                    // Assign the names to the persistent helper and the round text manager's arrays
                    nameTextsk[i] = nameTextsk[i].Trim();
                    if (i < 8)
                    {
                        ph.nameTexts[i] = nameTexts[i] = nameTextsk[i];                        
                    }
                    else
                    {
                        ph.nameTexts[i] = nameTextsk[i];
                    }
                    Debug.Log(ph.nameTexts[i]);
                }
                // Call the texture mod script and make it get textures from the web
                for(int i = 0; i < 9; i++)
                {
                    string baseUrl = "http://roesh.000webhostapp.com/";
                    tm.urls[i] = string.Concat(baseUrl, um.userName, "/", nameTextsk[i]);
                    //if (i == 0){ Debug.Log(string.Equals(tm.urls[i], "http://roesh.000webhostapp.com/Animals/Giraffee.jpg")); }
                    GameObject go  = null; 
                    if (i < 8) // For indexes 0:7 we have images in the scene
                    {
                        go = imgObjs[i];
                    }
                    else
                    {
                        tm.setupImages = true;
                    }          
                }
            }// end if 9 lines are present
            else
            {
                Debug.Log("9 names not present");
            }
        }

        
    }
}
