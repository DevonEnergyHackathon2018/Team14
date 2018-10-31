using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dumbell : MonoBehaviour {

    public GameObject sphere1, sphere2, pole, cyl;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        pole.transform.position = Vector3.Lerp(sphere1.transform.position, sphere2.transform.position, 0.5f);
        pole.transform.LookAt(sphere1.transform.position);
        float scale = Vector3.Distance(sphere1.transform.position, sphere2.transform.position);
        cyl.transform.localScale = new Vector3(0.1f,scale/0.5f,0.1f);
	}
}
