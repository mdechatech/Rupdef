using UnityEngine;
using System.Collections;

public class Blupee : MonoBehaviour
{
    public Rigidbody2D Rigidbody;

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
    }
}
