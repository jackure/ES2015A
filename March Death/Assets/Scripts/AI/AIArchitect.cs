﻿using System;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.AI
{
    class AIArchitect
    {
        String currentBaseMap;
        AIController ai;

        public AIArchitect(AIController aic)
        {
            //readMap("easy_base_1.txt");
            ai = aic;
        }

        /// <summary>
        /// Reads a file containing the map
        /// </summary>
        /// <param name="mapName"></param>
        public void readMap(String mapName)
        {
            StreamReader sr = new StreamReader(Application.dataPath + "/AIBaseMaps/" + mapName);
            currentBaseMap = sr.ReadToEnd();
            sr.Close();

            String[] lines = currentBaseMap.Split("\n"[0]);

            foreach (String line in lines)
            {
                Debug.Log(line);
            }
        }
    }
}
