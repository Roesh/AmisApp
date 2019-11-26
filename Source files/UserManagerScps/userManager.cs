using System.Collections;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class userManager : MonoBehaviour {

    public string userName;
    private string passKey;
    public GameObject[] userInputObjs;
    public Text userNameText;

    public bool sourceIsLocal;

    public GameObject userDialogCanvas;
    public CamRotScp camRotScp;
    public persistentHelper ph;
    //public Text usernameText;

    // Use this for initialization
    void Start()
    {
        ph = FindObjectOfType<persistentHelper>();
        sourceIsLocal = false;
        camRotScp = GetComponent<CamRotScp>();
        if (!ph.firstTimeSetup)
        {
            GameObject.Find("UserStartCanvas").SetActive(false);            
        }
        //loadUserName();
    }

    // Update is called once per frame
    void Update() {

    }

    public void setSource()
    {
        foreach (GameObject go in userInputObjs)
        {
            go.SetActive(sourceIsLocal);
        }
        sourceIsLocal = !sourceIsLocal;
        
    }
        
    public void turnOffDialog()
    {
        userDialogCanvas.SetActive(false);
        StartCoroutine(camRotScp.rotCamToStart());
    }
    public void setNameAndCallRoundTextChanger()
    {
        userName = userNameText.text;
        userName = userName.Trim();
        FindObjectOfType<RoundTextChanger>().getNamesAndSetTextures();
    }
    public void firstTimeSetupDone()
    {
        ph.firstTimeSetup = false;
    }
    public void adjustReward(int rl)
    {
        ph.rewardLevel = rl;
    }

    //public void loadUserName()
    //{
    //    if (File.Exists(Application.persistentDataPath + "username.txt"))
    //    {
    //        StreamReader str = new StreamReader(Application.dataPath + "username.txt");
    //        string newStr = str.ReadLine();
    //        if (newStr != null)
    //        {
    //            userName = newStr;
    //            usernameText.text = userName;
    //        }
    //        //fileData = File.ReadAllBytes(filePath);
    //    }
    //    else
    //    {
    //        Debug.Log("username file not exist");
    //    }        
    //}
    //public void saveUserName()
    //{
    //    StreamWriter writer = new StreamWriter(Application.dataPath + "username.txt", true);
    //    writer.WriteLine(userName);
    //}

}
