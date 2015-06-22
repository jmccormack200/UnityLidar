using UnityEngine;
using System.Collections;

public class CubeCollision : MonoBehaviour {

	private increaseScore otherScript;

	void OnCollisionEnter(Collision collision){
		GetComponent<Renderer>().material.color = Color.blue;
		print("Collision");

	}

	void OnCollisionExit(Collision collision){
		GetComponent<Renderer>().material.color = Color.red;
		otherScript = GameObject.FindGameObjectWithTag("ScoreText").GetComponent<increaseScore>();
		otherScript.UpdateCount();
		Destroy(gameObject, 2);
	}
}
