using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;

public class imgLoad : MonoBehaviour {

	public int id = 0; // int 0 - 9. 0-4: landscape, 5-8 portrait. 9- static frame
	public Material mat;
    public persistentHelper ph;

    // Use this for initialization
    void Start()
    {
        //string fname = getFnames ();
        ph = GameObject.Find("persistentObj").GetComponent<persistentHelper>();
        Texture2D tex = ph.imgTextures[id];
        if (tex != null) { 
            mat.SetTexture("_MainTex", ph.imgTextures[id]);
        }
		//setRewardSceneShader (gameObject, fname);
	}
	
	// Update is called once per frame
	void Update () {
	}
    
}
