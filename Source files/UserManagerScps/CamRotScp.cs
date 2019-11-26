using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamRotScp : MonoBehaviour {

    public Vector3 initRot;
    public Vector3 finalRot;

    public IEnumerator rotCamToStart()
    {
        float te = 0;
        float t = 1;        
        while (te < t)
        {
            Camera.main.transform.eulerAngles = Vector3.Slerp(finalRot,initRot,te/t);
            te += Time.deltaTime;
            yield return null;
        }
        Camera.main.transform.eulerAngles = Vector3.zero;
    }
	// Use this for initialization
	void Start () {
        initRot = Vector3.zero;
        finalRot = Vector3.up*90;
        if (FindObjectOfType<persistentHelper>().firstTimeSetup)
        {
            Camera.main.transform.eulerAngles = Vector3.up * 90;
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}    

}
