using UnityEngine;
using System.Collections;

public class particleScp : MonoBehaviour {
	public ParticleSystem ps;
	public int numParticles = 10;
	public float particleSpeed = 6;
	public float particleGenRate = 6;
	public float particleSize = 1;
	public float rotSpeed = 1;
	private int nP;
	private float pS;
	private float pGR;
	private float pSZ;
	private float rS;

	public bool firstGo = true;
	public bool moduleEnabled = false;
	public Vector2 speedRange;
	public float speedDiff = 0.1f;

	public float spdMod = 0.5f;

	private Vector3 initRot;

	// Use this for initialization
	void Start () {		
		ps = GetComponent<ParticleSystem> ();	
		var emission = ps.emission;
		emission.enabled = moduleEnabled;
        ps.startSpeed = particleSpeed;
        ps.maxParticles = numParticles;
		emission.rate = particleGenRate;
		ps.startSize = particleSize;

		nP = numParticles;
		pS = particleSpeed;
		pGR = particleGenRate;
		pSZ = particleSize;
		rS = rotSpeed;

		//pivot = new Vector3 (0, transform.position.y, transform.position.z);
		speedRange = new Vector2 (particleSpeed, particleSpeed + 24 * speedDiff);
		initRot = transform.eulerAngles;
		var cbs = ps.colorBySpeed;
		cbs.range = speedRange;
	}

	// Update is called once per frame
	void Update () {
		var emission = ps.emission;
		emission.enabled = moduleEnabled;
		emission.rate = particleGenRate;
		//float newSpeed = Mathf.Sin (2*Mathf.PI*(Time.timeSinceLevelLoad)*rotSpeed*spdMod/rS)*rotSpeed;
		//Debug.Log (newSpeed);
//		if (moduleEnabled) {
//			transform.RotateAround (pivot, Vector3.forward, rotSpeed * Time.deltaTime);
//		}
	}
		
	public void addParticleCountAndSpeed(){		
		if (firstGo) {			
			moduleEnabled = true; // turn on emission if it was turned off
			transform.eulerAngles= initRot;
			firstGo = false;
			ps.startSpeed = particleSpeed;
			ps.maxParticles = numParticles;
			ps.startSize = particleSize;
			ps.Emit (20);
		} else {
			ps.Emit (10);
			numParticles += 10;
			particleSpeed += speedDiff;
			particleGenRate += 0.1f;
			particleSize += 0.02f;
			//rotSpeed += 0.2f;
			// augment the values here
			ps.startSpeed = particleSpeed;
			ps.maxParticles = numParticles;
			ps.startSize = particleSize;
		}
	}

	public void resetAndTurnOffParticles(){
		numParticles = nP;
		particleSpeed = pS;
		particleGenRate = pGR;
		particleSize = pSZ;
		rotSpeed = rS;
		moduleEnabled = false;
		firstGo = true; // set the firstgo flag to true
	}
}
