using UnityEngine;
using System.Collections;

public class CubeCollision : MonoBehaviour {

	void OnCollisionEnter(Collision collision){
		GetComponent<Renderer>().material.color = Color.blue;
		print("Collision");

	}

	void OnCollisionExit(Collision collision){
		GetComponent<Renderer>().material.color = Color.red;
		print("No Collision");
	}
}
