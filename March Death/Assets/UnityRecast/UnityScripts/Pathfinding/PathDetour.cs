﻿using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

public class PathDetour : Utils.Singleton<PathDetour>
{
    public IntPtr TileCache = new IntPtr(0);
    public IntPtr NavMesh = new IntPtr(0);
    public IntPtr NavQuery = new IntPtr(0);

    private PathDetour()
    {
    }

    public void Initialize(TileCacheAsset navmeshData, Action postInitialize = null)
    {
        // Is it already initialized?
        if (TileCache.ToInt64() != 0)
        {
            return;
        }

        Assert.IsTrue(Pathfinding.Detour.pointerSize() == IntPtr.Size);
        Pathfinding.Recast.DefaultConfig(Application.dataPath + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "Recast.log");

        // Call postInitialize
        if (postInitialize != null)
        {
            postInitialize();
        }

        bool result = Pathfinding.TileCache.loadFromTileCacheHeaders(ref navmeshData.header, navmeshData.tilesHeader, navmeshData.tilesData, ref TileCache, ref NavMesh, ref NavQuery);
        if (!result)
        {
            throw new ArgumentException("Invalid navmesh data");
        }
    }

    public void Destroy()
    {
        // TODO: Find out why freeing TileCache crashes Unity
        //Pathfinding.Recast.freeTileCache(NavMesh, TileCache);
        
        TileCache = new IntPtr(0);
        NavMesh = new IntPtr(0);
        NavQuery = new IntPtr(0);
    }

    public uint AddObstacle(Pathfinding.DetourObstacle block)
    {
        Assert.IsTrue(TileCache.ToInt64() != 0);

        Vector3[] blockVertices = block.Vertices();
        Vector3 pos = block.transform.position;

        float[] vertices =
        {
            blockVertices[0].x, blockVertices[0].y - 3.0f, blockVertices[0].z,
            blockVertices[1].x, blockVertices[1].y - 3.0f, blockVertices[1].z,
            blockVertices[2].x, blockVertices[2].y - 3.0f, blockVertices[2].z,
            blockVertices[3].x, blockVertices[3].y - 3.0f, blockVertices[3].z,
        };

        float[] position =
        {
            pos.x, pos.y - 3.0f, pos.z
        };

        return Pathfinding.TileCache.addObstacle(TileCache, position, vertices, 4, (int)Mathf.Ceil(block.Size.y));
    }

    public void RemoveObstacle(uint reference)
    {
        Assert.IsTrue(TileCache.ToInt64() != 0);
        Pathfinding.TileCache.removeObstacle(TileCache, reference);
    }
}
