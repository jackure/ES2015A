﻿using UnityEngine;
using System.Collections;

public class MainMenuLogic : MonoBehaviour {

	static readonly Color UP_CLICK = new Color(1.0f, 1.0f, 1.0f, 0.6f); // WHITE
	static readonly Color DOWN_CLICK = new Color(0.0f, 0.0f, 0.0f, 0.6f); // GREY
	static readonly Color ENTER_OVER = new Color(0.8f, 1.0f, 0.0f, 0.6f); // YELLOW - GREEN
	static readonly Color EXIT_OVER = new Color(1.0f, 1.0f, 1.0f, 0.6f); // WHITE
	static readonly Color YELLOW = new Color(1.0f, 0.92f, 0.016f, 1f); //YELLOW

	bool bStillInside = false;
	AudioSource audio;
	// Use this for initialization
	void Start () {
		audio = GetComponent<AudioSource>();
		Cursor.visible = true;
		TestEnvironment.Instance.Init();
	}

	/* MOUSE OVER */

	/* This method changes the color of the object we are over on entering */
	void OnMouseEnter(){
		GetComponent<Renderer> ().material.color = YELLOW;
		bStillInside = true;
	}

	/* This method changes the color of the object we are over on exiting */
	void OnMouseExit(){
		audio.Stop ();
		GetComponent<Renderer>().material.color = EXIT_OVER;
		bStillInside = false;
	}

	/* MOUSE CLICK */

	/* This method changes the color of the object we are clicking */
	void OnMouseDown() {
		audio.Play ();
		GetComponent<Renderer>().material.color = DOWN_CLICK;
	}

	/* This method moves to another scene or quit */
	void OnMouseUp() {

		Color col;

		col = bStillInside ? ENTER_OVER : UP_CLICK;

		GetComponent<Renderer>().material.color = col;

		if(bStillInside){
			if(this.CompareTag("bStart")) { Application.LoadLevel(2); }
			else if(this.CompareTag("bTutorial")) { Application.LoadLevel(1); }
			else { Application.Quit (); }
		}
	}
}
