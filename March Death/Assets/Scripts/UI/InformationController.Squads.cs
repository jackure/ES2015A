﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using Storage;
using Utils;
using System.IO;
using System.Collections.Generic;
using UnityEngine.Events;
using Managers;

public partial class InformationController : MonoBehaviour {

	//objects for squads information
	int squadsColumns = 3;
	int squadsRows = 3;
	Vector2 squadsButtonSize;
	Vector2 squadsInitialPoint;
	int MAX_SQUADS_BUTTONS;

	private SelectionManager sManager { get { return BasePlayer.player.selection; } }

	ArrayList squadButtons = new ArrayList();

	private void ReloadSquadGenerationButton() 
	{
		DestroyGenerateSquadButton ();
		if (squadButtons.Count < MAX_SQUADS_BUTTONS) {
			ShowSquadGenerationButton (squadButtons.Count);
		}
	}

	private void DestroyGenerateSquadButton() {
		//Delete previous button
		GameObject[] buttons = GameObject.FindGameObjectsWithTag ("SquadGenerationButton");
		if (buttons != null) {
			foreach (GameObject button in buttons) {
				Destroy (button);
			}
		}
	}

	private void ShowSquadGenerationButton (int i) {

		double lineDivision = (double)(i / squadsColumns);
		int line = (int)Math.Ceiling(lineDivision) + 1;
		
		Vector2 buttonCenter = new Vector2();
		buttonCenter.x = squadsInitialPoint.x + squadsButtonSize.x / 2 + (squadsButtonSize.x * (i % squadsColumns));
		buttonCenter.y = squadsInitialPoint.y + (squadsButtonSize.y / 2) - squadsButtonSize.y * line;

		UnityAction createSquadAction = new UnityAction(() =>  
		{
			addNewSquadButton(squadButtons.Count);
			sManager.NewTroop((squadButtons.Count).ToString());
			ReloadSquadGenerationButton();
		});

		CreateCustomButton(buttonCenter, squadsButtonSize, "SquadGenerationButton", "+", actionMethod: createSquadAction);
	}

	private void addNewSquadButton(int i) {
		double lineDivision = (double)(i / squadsColumns);
		int line = (int)Math.Ceiling(lineDivision) + 1;
		
		Vector2 buttonCenter = new Vector2();
		buttonCenter.x = squadsInitialPoint.x + squadsButtonSize.x / 2 + (squadsButtonSize.x * (i % squadsColumns));
		buttonCenter.y = squadsInitialPoint.y + (squadsButtonSize.y / 2) - squadsButtonSize.y * line;
		string text = "" + (i + 1);
        UnityAction squadAction = new UnityAction(() => 
		{
			sManager.SelectTroop(text);
            BasePlayer.player.setCurrently(Player.status.SELECTED_UNITS);
        });

		GameObject button = CreateCustomButton(buttonCenter, squadsButtonSize, "SquadButton", text, actionMethod: squadAction);
		squadButtons.Add(button);
	}
	
	
}
