﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Managers;
public class tester : MonoBehaviour {

    void Start()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(() => { onClick(); });
    }
    public void onClick()
    {
        GameObject.Find("GameController").GetComponent<BuildingsManager>()._createBuilding_("elf-farm");
        Debug.Log("click new building");
		if (Input.GetMouseButtonUp (0)) 
		{
			//do nothing
		};

    }
}
