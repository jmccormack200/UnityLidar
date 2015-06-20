using UnityEngine;
using System.Collections;

public class pointNormal : MonoBehaviour {

	public float normal;

	// Use this for initialization
	void Start () {
		normal = 0.0f;
	}
	
	// Update is called once per frame
	public void saveNormal(float set){
		normal = set;
	}

	public float getNormal(){
		return normal;
	}
}
