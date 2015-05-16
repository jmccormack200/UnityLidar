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

	public string lastReceivedUDPPackets="";
	public string allReceivedUDPPackets = "";


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
	}

	void OnGUI(){
			Rect rectObj = new Rect (40, 10, 200, 400);
			GUIStyle style = new GUIStyle ();
			style.alignment = TextAnchor.UpperLeft;
			GUI.Box(rectObj,"# UDPReceive\n127.0.0.1 "+port+" #\n"
			        + "shell> nc -u 127.0.0.1 : "+port+" \n"
			        + "\nLast Packet: \n"+ lastReceivedUDPPackets
			        + "\n\nAll Messages: \n"+allReceivedUDPPackets
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
	}

	private void ReceiveData(){
			client = new UdpClient (port);
			while (true) {
				try {
					IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
					byte[] data = client.Receive (ref anyIP);

					string text = Encoding.UTF8.GetString (data);

					print (">> " + text);

					lastReceivedUDPPackets = text;
					LidarPoint lidarpoint = convertData(lastReceivedUDPPackets);
					Debug.Log(lidarpoint.Id);
					Debug.Log (lidarpoint.X);
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
	
	public string getLatestUDPPacket(){
			allReceivedUDPPackets = "";
			return lastReceivedUDPPackets;
	}
}
