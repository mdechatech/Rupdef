using System.Linq;
using Pathfinding;
using UnityEngine;

namespace Koh.Rupdef
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        public int PensionBase;
        public int PensionPerDay;

        public int StartingSpendAmount;

        public int HideAmountBase;
        public int HideAmountPerDay;

        public float BoredomBase;
        public float BoredomPerDay;

        [Range(0, 1)]
        public float BoredomVariance;


        [Range(0, 1)]
        public float Interest;

        public int InterestMax;


        public LayerMask ObstacleMask;

        [Space]
        public int FinishCheckInterval;
        public string StartButton;

        [Header("Debug")]
        public int Wave;
        public AstarPath Graph;

        public bool WaitingForWaveConfirm;
        public bool WaveSuccessful;

        public PlayerController Player;
        public UiManager Ui;
        
        public Door[] Doors;
        public Pot[] Pots;
        public Chest[] Chests;
        public Placeable[] Placeables;

        public float FinishCheckTimer;
        public int HiddenAmount;
        

        private void Awake()
        {
            Instance = this;
            ScanObjects();
        }

        private void Start()
        {
            Player.SpendAmount = StartingSpendAmount;
            Player.HideAmount = HideAmountBase;
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
            if (Player.PlaceMode)
            {
                if (Input.GetButtonDown(StartButton))
                    NextWave();
            }
            else
            {
                if (WaitingForWaveConfirm)
                {
                    if (Input.GetButtonDown(StartButton))
                    {
                        Ui.SuccessGroup.alpha = 0;
                        Ui.PlayModeGroup.alpha = 0;
                        Ui.PlaceModeGroup.alpha = 1;
                        WaitingForWaveConfirm = false;
                        Player.BeginPlaceMode();
                    }
                }
                else
                {
                    if ((FinishCheckTimer -= Time.deltaTime) < 0)
                    {
                        FinishCheckTimer = FinishCheckInterval;
                        if (FindObjectsOfType<EnemyController>().Length == 0)
                            HandleWaveFinished();
                    }
                }
                
            }
        }

        private void HandleWaveFinished()
        {
            WaitingForWaveConfirm = true;

            var hiddenAmount = Player.HiddenAmount;
            var salvagedAmount = Player.SalvagedAmount;

            var totalFloating = hiddenAmount + salvagedAmount;

            var interest = Mathf.CeilToInt(Mathf.Clamp(totalFloating * Interest, 1, InterestMax));
            var pension = PensionBase + PensionPerDay * Wave;

            Ui.SuccessGroup.alpha = 1;
            Ui.SuccessInterestText.text = interest.ToString();
            Ui.SuccessPensionText.text = pension.ToString();
            Ui.SuccessSalvagedText.text = salvagedAmount.ToString();
            Ui.SuccessStoredText.text = hiddenAmount.ToString();

            var total = hiddenAmount + salvagedAmount + interest + pension;
            Player.HideAmount = HideAmountBase + HideAmountPerDay * (Wave + 1) - hiddenAmount;
            Player.SpendAmount = total;

            Ui.SuccessTotalText.text = total.ToString();

            WaveSuccessful = true;
        }

        private void NextWave()
        {
            Ui.SuccessGroup.alpha = 0;
            FinishCheckTimer = 3;

            if (Player.HideAmount > 0)
            {
                Ui.ShowError("NEGATORY",
                    "You need to hide ALL of your Blupees.",
                    3);
                return;
            }

            UpdateGraph();
            ScanObjects();

            if (!BlupeesAreReachable())
            {
                Ui.ShowError("NICE TRY!",
                    "Barricading sections of your house is really bad for market value.",
                    5);
                return;
            }

            HiddenAmount = Pots.Select(p => p ? p.Bupees : 0)
                .Concat(Chests.Select(c => c ? c.Bupees : 0))
                .Sum();
            Player.HideAmount = 0;
            Player.SpendAmount = 0;

            Player.EndPlaceMode();
            Ui.PlaceModeGroup.alpha = 0;
            Ui.PlayModeGroup.alpha = 1;

            ++Wave;
            Ui.DayText.text = Wave.ToString();
            Doors[0].Spawn(BoredomBase + BoredomPerDay * Wave, BoredomVariance);
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
