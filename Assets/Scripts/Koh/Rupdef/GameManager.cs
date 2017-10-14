using System.Linq;
using UnityEngine;

namespace Koh.Rupdef
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [Header("Debug")]
        public AstarPath Graph;

        public PlayerController Player;

        public GameObject[] Doors;
        public Chest[] Chests;
        public Placeable[] Placeables;
        

        private void Awake()
        {
            Instance = this;
            Doors = GameObject.FindGameObjectsWithTag("Door");
            Chests = FindObjectsOfType<Chest>()
                .Where(c => !c.GetComponent<Placeable>().IsBeingPlaced)
                .ToArray();
            Player = FindObjectOfType<PlayerController>();
            Graph = FindObjectOfType<AstarPath>();
            Placeables = Resources.LoadAll<Placeable>("Placeables")
                .ToList()
                .OrderBy(placeable => placeable.Price)
                .ToArray();
        }

        public void UpdateGraph()
        {
            Graph.Scan();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Awake();
                UpdateGraph();
            }
        }
    }
}
