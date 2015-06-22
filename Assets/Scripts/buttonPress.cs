using UnityEngine;
using System.Collections;

public class buttonPress : MonoBehaviour {

	public GameObject LIDAR;
	public GameObject collectable;
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
		//Generate 10 random point
		for(int i = 0; i < 10; i++){
			int randAngle = Random.Range (0, 360);
			float magnitude = pointArray[randAngle].transform.position.magnitude;
			if (magnitude == 0){
				i--;
			} else {
				float randomMagnitude = Random.Range (0.5f, (magnitude * 0.9f));

				float radians = randAngle * (Mathf.PI / 180);
				float x = randomMagnitude * Mathf.Sin (radians);
				float y = randomMagnitude * Mathf.Cos (radians);
				float z = 0.0f;

				Vector3 locationVector3 = new Vector3 (x, y, z);
				GameObject collect = (GameObject)Instantiate (collectable, locationVector3, Quaternion.identity);
			}
		}
	}
}
