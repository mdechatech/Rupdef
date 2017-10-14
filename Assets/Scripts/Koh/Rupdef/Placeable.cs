﻿using UnityEngine;
using System.Collections;

public class Placeable : MonoBehaviour
{
    public bool IsBeingPlaced;

    public string Name;
    public int Price;

    public int TileSizeX;
    public int TileSizeY;


    private void Awake()
    {
        IsBeingPlaced = true;
    }

}
