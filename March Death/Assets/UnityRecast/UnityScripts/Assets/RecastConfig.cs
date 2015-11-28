﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
public class RecastLayer
{
    public string LayerID;
    public float Cost;
}

[Serializable]
public class FilterData
{
    public string Name;
}

[Serializable]
public class Filter
{
    public List<FilterData> Include = new List<FilterData>();
    public List<FilterData> Exclude = new List<FilterData>();
}

public class RecastConfig : MonoBehaviour
{
    [Header("Recast Layers")]
    public List<RecastLayer> Layers = new List<RecastLayer>() { new RecastLayer() { LayerID = "WALKABLE", Cost = 1 } };

    [Header("Recast Filters")]
    public List<Filter> Filters = new List<Filter>() { new Filter() };

    public Dictionary<string, ushort> Areas = new Dictionary<string, ushort>();

    public void SetupAreas()
    {
        ushort n = 1;
        foreach (var layer in Layers)
        {
            Areas.Add(layer.LayerID, n);
            Pathfinding.TileCache.addFlag(n, 1);
            n *= 2;
        }
    }
}
