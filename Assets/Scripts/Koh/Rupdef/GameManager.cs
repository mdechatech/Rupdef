using System.Linq;
using UnityEngine;

namespace Koh.Rupdef
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [Header("Debug")]
        public Chest[] Chests;

        public Placeable[] Placeables;
        

        private void Awake()
        {
            Instance = this;
            Chests = FindObjectsOfType<Chest>();
            Placeables = Resources.LoadAll<Placeable>("Placeables")
                .ToList()
                .OrderBy(placeable => placeable.Price)
                .ToArray();
        }
    }
}
