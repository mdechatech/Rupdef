using System.Linq;
using Pathfinding;
using UnityEngine;

namespace Koh.Rupdef
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        public int StartingSpendAmount;
        public int StartingHideAmount;

        public LayerMask ObstacleMask;

        [Space]
        public string StartButton;

        [Header("Debug")]
        public int Wave;
        public AstarPath Graph;

        public PlayerController Player;
        public UiManager Ui;
        
        public Door[] Doors;
        public Pot[] Pots;
        public Chest[] Chests;
        public Placeable[] Placeables;

        public int HiddenAmount;
        

        private void Awake()
        {
            Instance = this;
            ScanObjects();
        }

        private void Start()
        {
            Player.SpendAmount = StartingSpendAmount;
            Player.HideAmount = StartingHideAmount;
            Player.BeginPlaceMode();
        }

        private void ScanObjects()
        {
            Ui = FindObjectOfType<UiManager>();
            Doors = FindObjectsOfType<Door>();
            Pots = FindObjectsOfType<Pot>()
                .Where(c => !c.GetComponent<Placeable>().IsBeingPlaced)
                .ToArray();
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
            if (Input.GetButtonDown(StartButton))
            {
                NextWave();
            }
        }

        private void NextWave()
        {
            UpdateGraph();
            ScanObjects();

            if (!BlupeesAreReachable())
            {
                BlupeesAreReachable();
            }

            HiddenAmount = StartingHideAmount - Player.HideAmount;
            Player.HideAmount = 0;
            Player.SpendAmount = 0;

            Player.EndPlaceMode();
            Ui.PlaceModeGroup.alpha = 0;
            Ui.PlayModeGroup.alpha = 1;

            ++Wave;
            Doors[0].Spawn();
        }

        private bool BlupeesAreReachable()
        {
            var doorNode = Graph.GetNearest(Doors[0].transform.position);

            var loots = Chests.Select(c => c.transform.position)
                .Concat(Pots.Select(p => p.transform.position))
                .ToArray();

            for (var i = 0; i < loots.Length; ++i)
            {
                var loot = loots[i];

                var left = Graph.GetNearest(loot + Vector3.left * GridHelper.Instance.TileSize.x);
                var right = Graph.GetNearest(loot + Vector3.right * GridHelper.Instance.TileSize.x);
                var up = Graph.GetNearest(loot + Vector3.up * GridHelper.Instance.TileSize.y);
                var down = Graph.GetNearest(loot + Vector3.down * GridHelper.Instance.TileSize.y);

                var possible =
                    PathUtilities.IsPathPossible(doorNode.node, left.node) ||
                    PathUtilities.IsPathPossible(doorNode.node, right.node)||
                    PathUtilities.IsPathPossible(doorNode.node, up.node) ||
                    PathUtilities.IsPathPossible(doorNode.node, down.node);

                if (!possible)
                {
                    print("OH NO");
                    return false;
                }
            }

            return true;
        }
    }
}
