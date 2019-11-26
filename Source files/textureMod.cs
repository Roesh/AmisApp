using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using UnityEngine.Networking;

public class textureMod : MonoBehaviour {
    public userManager userManager;
    public persistentHelper persHelper;
    public GameObject debugTextOnline;
    public bool setupImages;
    public RoundTextChanger rTextChanger;

    public string[] urls = new string[9];
    public bool[] imagesAreSet = new bool[9];   // true if the texture has been loaded
    public int[] RoundNum = new int[9]; // 0 - jpg round. 1 - jpeg round. 2 - rounds complete
    public bool loadFail;   // true if even one texture failed to load after the jpeg load round    
    
    // Use this for initialization
    void Start() {
        setupImages = false;
        loadFail = false;
        for (int i = 0; i < 9; i++)
        {
            imagesAreSet[i] = false;
            RoundNum[i] = 0;
        }
        rTextChanger = FindObjectOfType<RoundTextChanger>();
    }

    // Update is called once per frame
    void Update() {
        // The round text changer will set this flag to true once it reads all the names from the url
        if (setupImages == true)
        {
            // Reset the flag
            setupImages = false;
            // Cycle through the textures, 
            for (int i = 0; i < 9; i++)
            {                
                if (i < 8) // There are 8 image object in the scene. The ninth texture is given to the 
                    // persistent helper only
                {   
                    setTextureFromWeb(rTextChanger.imgObjs[i], i, urls[i]+".jpg");
                }
                else
                {
                    setTextureFromWeb(null, i, urls[i] + ".jpg");
                }
            }
        }
        for(int i = 0; i < 9; i++)
        {            // If we are now supposed to do the jpeg round (because the image wasnt set)
            if (RoundNum[i] == 1 && !imagesAreSet[i])
            {
                RoundNum[i] = 2; // Both rounds are now done
                if (i < 8)
                {
                    setTextureFromWeb(rTextChanger.imgObjs[i], i, urls[i] + ".jpeg");
                }
                else
                {
                    setTextureFromWeb(null, i, urls[i] + ".jpeg");
                }                
            }
        }        
    }

    // If the source is local, use the local file processing paths: RoundTextChanger calls this fn     
    public bool setTexture(GameObject img, string fileName, int index) {     
        Texture2D tex = null;
        string basePath;
#if UNITY_STANDALONE || UNITY_WEBGL
        basePath = Application.dataPath + "\\AddedResources\\" + fileName;
#elif UNITY_ANDROID
		basePath = Application.persistentDataPath + "/AddedResources/" + fileName;
#endif            
        if (fileExists(basePath + ".jpg")) // Check if the file is a .jpg, .jpeg, or png
        {
            tex = LoadPNG(basePath + ".jpg");
        }
        else if (fileExists(basePath + ".jpeg"))
        {
            tex = LoadPNG(basePath + ".jpeg");
        }
        else if (fileExists(basePath + ".png"))
        {
            tex = LoadPNG(basePath + ".png");
        }
        else
        {
            return false;
        }
        assignTex(img, index, tex);       // If we didnt return false, then the file must exist     
        return true;                        // Assign the texture and return true
    }    	
	public static Texture2D LoadPNG(string filePath) {
		Texture2D tex = null;
		byte[] fileData;
		fileData = File.ReadAllBytes(filePath);
		tex = new Texture2D(2, 2);
		if (tex.LoadImage (fileData)) {
			return tex;
		} else {
			return null;
		}
	}
	bool fileExists(string fPath){
		if (File.Exists (fPath)) {
			return true;
		} else {
			return false;
		}
	}

    // RoundTextChanger calls this fn to process web based requests
    public void setTextureFromWeb(GameObject img, int index, string url)
    {
        StartCoroutine(getTexture(img, index, url));
    }
    IEnumerator getTexture(GameObject img, int index, string uri)
    {
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(uri))
        {
            Debug.Log("Accessing: " + uri);
            yield return www.SendWebRequest();

            Texture2D tex = null;
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.Log(www.error);
                if (RoundNum[index] == 2) // If we had to do the jpeg round, but failed to load,
                {
                    loadFail = true;
                }                    
            }
            else
            {
                tex = DownloadHandlerTexture.GetContent(www);
                // if the texture is not null, then it will be given to the persistent helper
                // and applied to an image in the scene (if it exists)
                if (tex != null)
                {
                    assignTex(img, index, tex);
                    imagesAreSet[index] = true;
                }
                // if were in the jpeg round, it means loading the texture failed
                else if (RoundNum[index] == 1)
                {
                    loadFail = true;
                }
            }            
        }
        RoundNum[index]++; // increment the round number. If it was jpg, it is now jpeg.
        // This does increment "done" (2) to 3, but this should have no influnce on control logic
    }

    // Assign the texture to the current image, then assign it to the helper as well.
    // If the image object is null, it means there is no gameobject in the scene that can
    // be assigned the texture
    void assignTex(GameObject img, int index, Texture2D tex)        
    {
        float PixelsPerUnit = 100f;
        persHelper.imgTextures[index] = tex;
        if (img != null)
        {
            Image i = img.GetComponent<Image>();
            Sprite NewSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0), PixelsPerUnit);
            i.sprite = NewSprite;
        }
    }
}
