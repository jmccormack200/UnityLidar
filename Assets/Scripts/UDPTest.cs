using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public struct LidarPoint{
	public int Id;
	public int X;
	public int Y;

	public LidarPoint(string id, string x, string y){
		Id = int.Parse (id);
		X  = int.Parse (x);
		Y  = int.Parse (y);
	}
}


public class UDPTest : MonoBehaviour {

	Thread receiveThread;
	Thread sendThread;
	Thread printThread;
	UdpClient client;
	UdpClient sendclient;
	IPEndPoint remoteEndPoint;

	public int port;
	public int scale = 10;
	public GameObject pointCloud;
	public string lastReceivedUDPPackets="";
	public string allReceivedUDPPackets = "";
	private Queue queue = new Queue();
	private Queue send_queue = new Queue();


	public string arduinoIP = "192.168.0.111";
	public int arduinoport = 2390;


	//Dictionary for storing the name/gameobject pairs
	private Dictionary<string, GameObject> pointDictionary = new Dictionary<string, GameObject>();

	private static void Main(){
		UDPTest receiveObj = new UDPTest ();
		receiveObj.init ();

		string text = "";
		do {
			text = Console.ReadLine ();
		}
		while(!text.Equals ("exit"));
	}

	// Use this for initialization
	public void Start () {
		init ();
		//Coroutine (Loop());
		InvokeRepeating ("Loop", 0, 0.00002f);
	}

	private void init(){
		remoteEndPoint = new IPEndPoint(IPAddress.Parse(arduinoIP), arduinoport);

		receiveThread = new Thread (
			new ThreadStart (ReceiveData));
		receiveThread.IsBackground = true;
		receiveThread.Start ();
		sendThread = new Thread(
			new ThreadStart (SendData));
		sendThread.IsBackground = true;
		sendThread.Start();
	}

	//Threaded portion for recieving and preprocessing the data. 
	private void ReceiveData(){
		client = new UdpClient (port);
		while (true) {
			try {
				IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
				byte[] data = client.Receive (ref anyIP);

				string text = Encoding.UTF8.GetString (data);
				//print (">> " + text);
				lastReceivedUDPPackets = text;
				LidarPoint lidarpoint = convertData(lastReceivedUDPPackets);
				queue.Enqueue(lidarpoint);
				allReceivedUDPPackets = allReceivedUDPPackets + text;
			}
			catch (Exception err){
				print (err.ToString ());
			}
		}
	}

	private void SendData(){
		sendclient = new UdpClient (arduinoport);
		while(true){
			try {
				if (send_queue.Count > 0){
					string length = send_queue.Dequeue().ToString();
					byte[] data = Encoding.UTF8.GetBytes(length);
					client.Send(data, data.Length, remoteEndPoint);
				}
			} catch (Exception err){
				print (err.ToString ());
			}
		}
	}

	private static LidarPoint convertData(string data){
		//ID number [(][0-9]+[,][ ]?[(]
		Match idRawMatch = Regex.Match (data, "[(][0-9]+[,][ ]?[(]");
		string idRawString = (string)idRawMatch.ToString ();
		//Remove paranthesis and comma
		Match idMatch = Regex.Match (idRawString, "[0-9]+");
		string idString = (string)idMatch.ToString ();
	
		//Remove both digits [0-9]+[,][0-9]+ gives 100,100 format
		Match pointRawMatch = Regex.Match (data, "[0-9]+[,][ ]?[0-9]+");
		string pointRawString = (string)pointRawMatch.ToString ();
		//First digit [0-9]+[,] gives 100,
		Match xRawMatch = Regex.Match (pointRawString, "[0-9]+[,]");
		string xRawString = (string)xRawMatch.ToString ();
		//Second digit [,][0-9]+ gives ,100
		Match yRawMatch = Regex.Match (data, "[,][ ]?[0-9]+");
		string yRawString = (string)yRawMatch.ToString ();
		//First digit pure
		Match xFinalMatch = Regex.Match (xRawString, "[0-9]+");
		string xFinal = (string) xFinalMatch.ToString ();
		//Second digit pure
		Match yFinalMatch = Regex.Match (yRawString, "[0-9]+");
		string yFinal = (string)yFinalMatch.ToString ();

		return new LidarPoint (idString, xFinal, yFinal);

	}

	private void parseData (LidarPoint lidarpoint){
		int angle = lidarpoint.X;
		int length = lidarpoint.Y;
		
		float radians = angle * (Mathf.PI / 180);
		float x = length * Mathf.Sin (radians);
		float y = length * Mathf.Cos (radians);
		float z = 0.0f;
		
		
		float new_x = transform.position.x + x;
		new_x = new_x / scale;
		float new_y = transform.position.y + y;
		new_y = new_y / scale;
		float new_z = transform.position.z + z;
		
		Vector3 locationVector3 = new Vector3 (new_x, new_y, new_z);

		string name = (string)lidarpoint.Id.ToString () + lidarpoint.X.ToString ();

		if (length < 700 && length > 30){
			send_queue.Enqueue(length.ToString());
		}

		if (pointDictionary.ContainsKey (name)) {
			GameObject pointInstance = pointDictionary[name];
			pointInstance.transform.position = locationVector3;

		} else {
			GameObject pointInstance = (GameObject)Instantiate (pointCloud, locationVector3, Quaternion.identity);
			pointInstance.transform.parent = GameObject.Find("Cube").transform;
			pointInstance.name = name;
			pointDictionary.Add(name, pointInstance);
		}
	}

	//On each frame, read from the Lidar Queue 
	//Then if there is data call parseData

	void Loop (){

		for (int i = 0; i < 360; i++){
			if (queue.Count > 0) {
				LidarPoint lidarpoint = (LidarPoint)queue.Dequeue ();
				parseData (lidarpoint);
			}
		}
	}


	void OnAplicationQuit()
	{
		if (receiveThread.IsAlive) {
			receiveThread.Abort(); 
		}
		client.Close(); 
	}	
}
