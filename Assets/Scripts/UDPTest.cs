using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text.RegularExpressions;

public struct LidarPoint{
	public int Id;
	public int X;
	public int Y;

	public LidarPoint(string id, string x, string y){
		Id = int.Parse (id);
		X = int.Parse (x);
		Y = int.Parse (y);
	}
}


public class UDPTest : MonoBehaviour {

	Thread receiveThread;
	UdpClient client;

	public int port;
	public int scale = 10;
	public GameObject pointCloud;
	public string lastReceivedUDPPackets="";
	public string allReceivedUDPPackets = "";
	private Queue queue = new Queue();
	public int count = 0;

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
		InvokeRepeating ("Loop", Mathf.Epsilon, 0.00002f);
	}

	void OnGUI(){
			Rect rectObj = new Rect (40, 10, 200, 400);
			GUIStyle style = new GUIStyle ();
			style.alignment = TextAnchor.UpperLeft;
			GUI.Box(rectObj,"# UDPReceive\n127.0.0.1 "+port+" #\n"
			        + "shell> nc -u 127.0.0.1 : "+port+" \n"
			        + "\nLast Packet: \n"+ lastReceivedUDPPackets
			   //     + "\n\nAll Messages: \n"+allReceivedUDPPackets
			        ,style);
	}

	private void init(){
		print ("UDPSend.init()");
		port = 8051;
		print ("Sending to 127.0.0.1 : " + port);
		print ("Test-Sending to this Port: nc -u 127.0.0.1 " + port + "");

		receiveThread = new Thread (
			new ThreadStart (ReceiveData));
		receiveThread.IsBackground = true;
		receiveThread.Start ();
		print ("Right above");


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
				count = count + 1;
				if (count == 360){
					print("Cycle");
				}

				allReceivedUDPPackets = allReceivedUDPPackets + text;
			}
			catch (Exception err){
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
		
		//float angle = N ["angle"].AsFloat;
		//float length = N ["length"].AsFloat;
		
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
		
		//Debug.Log (new_x);
		//Debug.Log (new_y);
		//string message = "That outta do it";
		//print(message);
		/*
GameObject point= Instantiate(A, new Vector3 (0,0,0), Quaternion.identity) as GameObject; 
point.transform.parent = GameObject.Find("Square").transform;
*/
		GameObject pointInstance = (GameObject)Instantiate (pointCloud, locationVector3, Quaternion.identity);
		pointInstance.transform.parent = GameObject.Find("Cube").transform;
		string name = (string)lidarpoint.Id.ToString () + lidarpoint.X.ToString ();
		pointInstance.name = name;
	}
	
	public string getLatestUDPPacket(){
			allReceivedUDPPackets = "";
			return lastReceivedUDPPackets;
	}

	//On each frame, read from the Lidar Queue 
	//Then if there is data call parseData
	void Loop (){
		if (queue.Count != 0){
			LidarPoint lidarpoint = (LidarPoint)queue.Dequeue();
			parseData(lidarpoint);
		}
	}

	void OnAplicationQuit()
	{
		if (receiveThread.IsAlive) {
			receiveThread.Abort(); 
		}
		client.Close(); 
	}

	//This section prevents Unity from crashing everytime the program
	//is reloaded
	void OnDisable() 
	{ 
		if ( receiveThread!= null) 
			receiveThread.Abort(); 
		
		client.Close(); 
	} 
	void Update()
	{

	}
}
