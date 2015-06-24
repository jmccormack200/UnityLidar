using UnityEngine;
using System.Collections;
using Npgsql;


public class buttonPress : MonoBehaviour {

	public NpgsqlConnection conn;
	public GameObject LIDAR;
	public GameObject collectable;
	public GameObject[] pointArray = new GameObject[370];

	// Use this for initialization
	public void press () {
		pointArray = LIDAR.GetComponent<UDPTest>().pointArray;
		PostGresUtility();
		print("Success!");
		NpgsqlCommand command = conn.CreateCommand();

		/*
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
		*/
	}

	public void PostGresUtility(string server = "127.0.0.1", string port = "5432", string user_id = "postgres", string password = "password", string database = "postgres"){
		string connectionString = "Server=" + server + ";Port=" + port + ";User Id=" + user_id + ";Password=" + password + ";Database=" + database;
		//conn = new NpgsqlConnection ("Server=localhost;Port=5432;User Id=postgres;Password=password;Database=postgres");
		conn = new NpgsqlConnection (connectionString);
		conn.Open();
	}
}
