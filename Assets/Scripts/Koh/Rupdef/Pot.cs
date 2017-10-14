using UnityEngine;
using System.Collections;

public class Pot : ActionTarget
{
    public int Bupees;
    public int Capacity;

    public Blupee BlupeePrefab;

    public bool IsFull { get { return Bupees >= Capacity; } }

    public void Smash()
    {
        for (var i = 0; i < Bupees; ++i)
        {
            var blupee = Instantiate(BlupeePrefab);
            blupee.transform.position = transform.position;
            blupee.Rigidbody.velocity = UnityEngine.Random.insideUnitCircle * 3f;
        }

        Destroy(gameObject);
    }
}
