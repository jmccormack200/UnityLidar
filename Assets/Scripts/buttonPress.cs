using UnityEngine;
using System.Collections;

public class buttonPress : MonoBehaviour {

	public GameObject LIDAR;
	public GameObject[] pointArray = new GameObject[370];

	// Use this for initialization
	public void press () {
		pointArray = LIDAR.GetComponent<UDPTest>().pointArray;
		foreach(GameObject point in pointArray){
			try{
				float length = point.transform.position.magnitude;
				point.GetComponent<pointNormal>().saveNormal(length);
			} catch {

			}
		}
	}
}
