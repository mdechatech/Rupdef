using UnityEngine;
using System.Collections;

public class Placeable : MonoBehaviour
{
    public bool Include;
    public bool IsBeingPlaced;

    public string Name;
    public int Price;

    public int TileSizeX;
    public int TileSizeY;

    [Multiline]
    public string Flavor;

    private void Awake()
    {
        IsBeingPlaced = true;
    }

}
