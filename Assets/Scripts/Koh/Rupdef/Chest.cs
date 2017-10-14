using UnityEngine;
using System.Collections;

public class Chest : ActionTarget
{
    public int Bupees;
    public int Capacity;

    public bool IsFull { get { return Bupees >= Capacity; } }
   
}
