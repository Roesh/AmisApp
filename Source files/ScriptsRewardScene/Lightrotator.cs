using UnityEngine;
using System.Collections;

public class Lightrotator : MonoBehaviour {

	public float speed = 60;
	public Vector3 axis = Vector3.up;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		#if UNITY_STANDALONE || UNITY_WEBGL
		transform.RotateAround (transform.position, axis,Time.deltaTime * speed);
		#endif
	}

}
