﻿using UnityEngine;
using System.Collections;

public class pointNormal : MonoBehaviour {

	public int normal;

	// Use this for initialization
	void Start () {
		normal = 0;
	}
	
	// Update is called once per frame
	public void saveNormal(float set){
		normal = (int)set;
	}

	public int getNormal(){
		return normal;
	}
}
