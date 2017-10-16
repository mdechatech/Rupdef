using System;
using System.Linq;
using UnityEngine;

namespace Koh.Rupdef
{
    public enum Facing
    {
        Up,
        Down,
        Left,
        Right
    }

    public enum PlaceAction
    {
        Buy,
        Sell,
        Hide
    }

    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        public float Speed;

        public Transform ActionSensor;
        public LayerMask ActionMask;
        public LayerMask PlaceCheckMask;

        public Transform Renderer;

        public Tooltip Tooltip;
        public Dialogue Dialogue;

        public int SpendAmount;
        public int HideAmount;
        public int HiddenAmount;
        public int SalvagedAmount;

        public bool RotateToMovement;

        [Space]
        public float BlupeeSearchInterval;

        public float BlupeeSearchRadius;
        public LayerMask BlupeeSearchMask;

        [Space]
        public float EnergyRegenRate;

        public float EnergyMax;

        public float EnergyTalkPrice;
        public float TalkTime;
        public float EnergyShovePrice;

        [Space]
        public AudioSource TalkSource;

        public AudioClip TalkLoop;


        [Header("Placement")]
        public bool PlaceMode;

        public Transform PlaceAnchor;


        [Header("Input")]
        public string MoveAxisX;

        public string MoveAxisY;    

        public string ActionButton;
        public string TogglePlaceActionButton;

        public string ToggleLeftButton;
        public string ToggleRightButton;

        [Header("Debug")]
        public float Energy;

        public Placeable CurrentPlaceable;

        public PlaceAction PlaceAction;

        public int CurrentPlaceableIndex;

        public Facing Facing;

        public float InputX;
        public float InputY;

        public bool InputAction;
        public bool InputTogglePlaceAction;
        public bool InputToggleLeft;
        public bool InputToggleRight;

        public Rigidbody2D Rigidbody;

        public ActionTarget Target;
        public Placeable TargetPlaceable;
        public EnemyController TargetEnemy;

        public float BlupeeSearchTimer;
        public float TalkTimer;


        private void Reset()
        {
            Speed = 3;

            MoveAxisX = "Horizontal";
            MoveAxisY = "Vertical";
            ActionButton = "Action";
            ToggleLeftButton = "Toggle Left";
            ToggleRightButton = "Toggle Right";
            TogglePlaceActionButton = "Toggle Place Action";
        }

        private void Awake()
        {
            Facing = Facing.Right;
            // Energy = EnergyMax;
            Rigidbody = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            // BeginPlaceMode();   
        }

        private void Update()
        {
            UpdateTalking();
            UpdateAudio();

            InputX = Input.GetAxis(MoveAxisX);
            InputY = Input.GetAxis(MoveAxisY);

            InputAction = Input.GetButtonDown(ActionButton);
            InputTogglePlaceAction = Input.GetButtonDown(TogglePlaceActionButton);

            InputToggleLeft = Input.GetButtonDown(ToggleLeftButton);
            InputToggleRight = Input.GetButtonDown(ToggleRightButton);

            if (PlaceMode)
            {
                if (PlaceAction == PlaceAction.Buy)
                {
                    if (CurrentPlaceable)
                        CurrentPlaceable.gameObject.SetActive(true);

                    if (InputToggleRight)
                        NextPlaceable();
                    else if (InputToggleLeft)
                        PreviousPlaceable();
                }
                else
                {
                    if (CurrentPlaceable)
                        CurrentPlaceable.gameObject.SetActive(false);
                }

                if (InputAction)
                {
                    HandlePlaceAction();
                }
                else if (InputTogglePlaceAction)
                {
                    // Cycle to next enum
                    PlaceAction =
                        (PlaceAction)(((int)PlaceAction + 1) % Enum.GetNames(typeof(PlaceAction)).Length);
                }
            }
            else
            {
                if (InputAction)
                {
                    if (TargetEnemy)
                        HandleEnemy(TargetEnemy);
                }

                UpdateEnergy();
            }

            UpdateTooltip();
        }

        private void HandlePlaceAction()
        {
            switch (PlaceAction)
            {
                case PlaceAction.Buy:
                    PlaceCurrent();
                    break;

                case PlaceAction.Hide:
                    if (Target)
                        HandleAction(Target);
                    break;

                case PlaceAction.Sell:
                    if (TargetPlaceable)
                        HandleRemove(TargetPlaceable);
                    break;
            }
        }

        private void FixedUpdate()
        {
            UpdateVelocity();
            UpdateFacing();
            UpdateTarget();

            if (!PlaceMode)
                UpdateBlupeeSearch();

            if (PlaceMode)
            {
                UpdatePlaceMode();
            }
        }

        private void HandleEnemy(EnemyController target)
        {
            if (TalkTimer > 0)
                return;

            if (Energy >= EnergyTalkPrice)
            {
                Energy -= EnergyTalkPrice;

                TalkTimer = TalkTime;
                target.TalkTimer = TalkTime;

                Dialogue.gameObject.SetActive(true);
                Dialogue.transform.position = target.transform.position;
            }
        }

        private void HandleAction(ActionTarget target)
        {
            // hoo boy
            var chest = target as Chest;
            if (chest)
            {
                if (!PlaceMode)
                    return;

                HandleChest(chest);
            }

            var pot = target as Pot;
            if (pot)
            {
                if (!PlaceMode)
                    return;

                HandlePot(pot);
            }
        }

        private void HandleRemove(Placeable target)
        {
            var chest = target.GetComponent<Chest>();
            if (chest)
            {
                HideAmount += chest.Bupees;
                HiddenAmount -= chest.Bupees;
            }

            var pot = target.GetComponent<Pot>();
            if (pot)
            {
                HideAmount += pot.Bupees;
                HiddenAmount -= pot.Bupees;
            }

            SpendAmount += target.Price;
            Destroy(target.gameObject);
        }

        public void BeginPlaceMode()
        {
            PlaceMode = true;
            CurrentPlaceable = CreateGhostPlaceable(GameManager.Instance.Placeables[CurrentPlaceableIndex]);
        }

        public void EndPlaceMode()
        {
            PlaceMode = false;

            if (CurrentPlaceable)
                Destroy(CurrentPlaceable.gameObject);
        }
            
        private Placeable CreateGhostPlaceable(Placeable source)
        {
            var placeable = Instantiate(source);

            var sprite = placeable.GetComponentInChildren<SpriteRenderer>();
            if (sprite)
            {
                sprite.color = sprite.color.WithAlpha(0.3f);
            }

            placeable.transform.position = GridHelper.Instance.Align(PlaceAnchor.position);
            /*
            if (placeable.TileSizeX % 2 == 0)
                placeable.transform.position += Vector3.right * GridHelper.Instance.TileSize.x * 0.5f;

            if (placeable.TileSizeY % 2 == 0)
                placeable.transform.position += Vector3.up * GridHelper.Instance.TileSize.y * 0.5f;
                */
            var collider = placeable.GetComponentInChildren<BoxCollider2D>();
            if (collider)
                collider.enabled = false;

            return placeable;
        }

        private void PlaceCurrent()
        {
            if (!CurrentPlaceable)
                return;

            if (CurrentPlaceable.Price > SpendAmount)
            {
                GameManager.Instance.Ui.ShowError("NUH-UH", "You can't afford that!", 3f);
                return;
            }


            var testCollider = CurrentPlaceable.GetComponentInChildren<BoxCollider2D>();
            if (testCollider)
            {
                testCollider.enabled = true;

                Collider2D[] results = new Collider2D[1];

                var amount = testCollider.OverlapCollider(
                    new ContactFilter2D {useLayerMask = true, layerMask = PlaceCheckMask},
                    results);

                if (amount > 0)
                {
                    testCollider.enabled = false;
                    GameManager.Instance.Ui.ShowError("NO WAY", "You need a clear space to put down furniture!", 3);
                    return;
                }


                testCollider.enabled = false;
            }

            SpendAmount -= CurrentPlaceable.Price;

            var placeable = Instantiate(CurrentPlaceable);
            placeable.IsBeingPlaced = false;

            var sprite = placeable.GetComponentInChildren<SpriteRenderer>();
            if (sprite)
            {
                sprite.color = sprite.color.WithAlpha(1);
            }

            placeable.transform.position = GridHelper.Instance.Align(PlaceAnchor.position);

            var realCollider = placeable.GetComponentInChildren<BoxCollider2D>();
            if (realCollider)
                realCollider.enabled = true;

            Destroy(CurrentPlaceable.gameObject);
            CurrentPlaceable = CreateGhostPlaceable(GameManager.Instance.Placeables[CurrentPlaceableIndex]);
        }

        private void NextPlaceable()
        {
            CurrentPlaceableIndex = (CurrentPlaceableIndex + 1) % GameManager.Instance.Placeables.Length;

            if (CurrentPlaceable)
                Destroy(CurrentPlaceable.gameObject);
            
            CurrentPlaceable = CreateGhostPlaceable(GameManager.Instance.Placeables[CurrentPlaceableIndex]);
        }

        public static int Modp(int dividend, int divisor)
        {
            return (dividend % divisor + divisor) % divisor;
        }

        private void PreviousPlaceable()
        {
            CurrentPlaceableIndex = Modp(CurrentPlaceableIndex - 1, GameManager.Instance.Placeables.Length);

            if (CurrentPlaceable)
                Destroy(CurrentPlaceable.gameObject);

            CurrentPlaceable = CreateGhostPlaceable(GameManager.Instance.Placeables[CurrentPlaceableIndex]);
        }

        private void UpdatePlaceMode()
        {
            if (CurrentPlaceable)
            {
                CurrentPlaceable.transform.position = GridHelper.Instance.Align(PlaceAnchor.position);
            }
        }

        private void UpdateTalking()
        {
            if ((TalkTimer -= Time.deltaTime) < 0)
            {
                TalkTimer = 0;
                Dialogue.gameObject.SetActive(false);
            }
        }

        private void UpdateVelocity()
        {
            if (TalkTimer > 0)
                Rigidbody.velocity = Vector2.zero;
            else
                Rigidbody.velocity = new Vector2(InputX, InputY) * Speed;
        }

        private void UpdateFacing()
        {
            var v = Rigidbody.velocity;
            if (v.x > 0)
                Facing = Facing.Right;
            else if (v.x < 0)
                Facing = Facing.Left;
            else if (v.y > 0)
                Facing = Facing.Up;
            else if (v.y < 0)
                Facing = Facing.Down;
            

            switch (Facing)
            {
                case Facing.Right:
                    Renderer.eulerAngles = Renderer.eulerAngles.WithZ(0);
                    break;

                case Facing.Left:
                    Renderer.eulerAngles = Renderer.eulerAngles.WithZ(180);
                    break;

                case Facing.Up:
                    Renderer.eulerAngles = Renderer.eulerAngles.WithZ(90);
                    break;

                case Facing.Down:
                    Renderer.eulerAngles = Renderer.eulerAngles.WithZ(270);
                    break;
            }
        }

        private void UpdateTooltip()
        {
            if (PlaceMode)
            {
                if ((PlaceAction != PlaceAction.Hide && PlaceAction != PlaceAction.Sell) ||
                    (!TargetPlaceable && !Target))
                {
                    Tooltip.gameObject.SetActive(false);
                    return;
                }

                if (PlaceAction == PlaceAction.Hide)
                {
                    if (TargetPlaceable)
                    {
                        Tooltip.gameObject.SetActive(true);

                        var chest = TargetPlaceable.GetComponent<Chest>();
                        if (chest)
                        {
                            Tooltip.ActionGroup.alpha = 0;
                            Tooltip.StorageGroup.alpha = 1;

                            Tooltip.StorageKeyText.text = "-SPACE-";
                            Tooltip.StorageTargetText.text = TargetPlaceable.Name;
                            Tooltip.StorageAmountText.text = chest.Bupees.ToString();
                            Tooltip.StorageMaxText.text = chest.Capacity.ToString();

                            goto useDone;
                        }

                        var pot = TargetPlaceable.GetComponent<Pot>();
                        if (pot)
                        {
                            Tooltip.ActionGroup.alpha = 0;
                            Tooltip.StorageGroup.alpha = 1;
                            Tooltip.StorageKeyText.gameObject.SetActive(true);

                            Tooltip.StorageKeyText.text = "-SPACE-";
                            Tooltip.StorageTargetText.text = TargetPlaceable.Name;
                            Tooltip.StorageAmountText.text = pot.Bupees.ToString();
                            Tooltip.StorageMaxText.text = pot.Capacity.ToString();

                            goto useDone;
                        }

                        Tooltip.gameObject.SetActive(false);

                        useDone:
                        Tooltip.transform.position = TargetPlaceable.transform.position;
                    }
                }
                else // PlaceAction == PlaceAction.Sell
                {
                    if (TargetPlaceable)
                    {
                        Tooltip.gameObject.SetActive(true);

                        Tooltip.ActionGroup.alpha = 1;
                        Tooltip.StorageGroup.alpha = 0;

                        Tooltip.ActionTargetText.text = TargetPlaceable.Name;
                        Tooltip.ActionNameText.text = "SELL";

                        sellDone:
                        Tooltip.transform.position = TargetPlaceable.transform.position;
                    }
                }
            }
            else
            {
                if (TalkTimer > 0)
                {
                    Tooltip.gameObject.SetActive(false);
                    return;
                }

                if (TargetPlaceable)
                {
                    Tooltip.gameObject.SetActive(true);

                    var chest = TargetPlaceable.GetComponent<Chest>();
                    if (chest)
                    {
                        Tooltip.ActionGroup.alpha = 0;
                        Tooltip.StorageGroup.alpha = 1;

                        Tooltip.StorageKeyText.text = "-----";
                        Tooltip.StorageTargetText.text = TargetPlaceable.Name;
                        Tooltip.StorageAmountText.text = chest.Bupees.ToString();
                        Tooltip.StorageMaxText.text = chest.Capacity.ToString();

                        goto inspectDone;
                    }

                    var pot = TargetPlaceable.GetComponent<Pot>();
                    if (pot)
                    {
                        Tooltip.ActionGroup.alpha = 0;
                        Tooltip.StorageGroup.alpha = 1;

                        Tooltip.StorageKeyText.text = "-----";
                        Tooltip.StorageTargetText.text = TargetPlaceable.Name;
                        Tooltip.StorageAmountText.text = pot.Bupees.ToString();
                        Tooltip.StorageMaxText.text = pot.Capacity.ToString();

                        goto inspectDone;
                    }

                    Tooltip.gameObject.SetActive(false);

                    inspectDone:
                    Tooltip.transform.position = TargetPlaceable.transform.position;
                }
                else if (TargetEnemy)
                {
                    Tooltip.gameObject.SetActive(true);

                    Tooltip.ActionGroup.alpha = 1;
                    Tooltip.StorageGroup.alpha = 0;
                    Tooltip.ActionTargetText.text = "Lonk the Orlf";

                    if (Energy >= EnergyShovePrice)
                        Tooltip.ActionNameText.text = "SHOVE";
                    else if (Energy >= EnergyTalkPrice)
                        Tooltip.ActionNameText.text = "TALK";
                    else
                        Tooltip.ActionNameText.text = "NO ENERGY!";

                    Tooltip.transform.position = TargetEnemy.transform.position;
                }
                else
                {
                    Tooltip.gameObject.SetActive(false);
                }
            }
        }

        private void UpdateTarget()
        {
            if (TalkTimer > 0)
                return;

            var target = Physics2D.Linecast(transform.position, ActionSensor.position, ActionMask);
            if (!target)
            {
                Target = null;
                TargetPlaceable = null;
                TargetEnemy = null;
                return;
            }

            TargetPlaceable = target.transform.GetComponent<Placeable>();
            Target = target.transform.GetComponent<ActionTarget>();
            TargetEnemy = target.transform.GetComponentInChildren<EnemyController>();
        }

        private void UpdateEnergy()
        {
            if (TalkTimer > 0)
                return;

            if ((Energy += Time.deltaTime * EnergyRegenRate) > EnergyMax)
                Energy = EnergyMax;
        }

        private void UpdateAudio()
        {
            if (TalkTimer > 0 && !TalkSource.isPlaying)
            {
                TalkSource.clip = TalkLoop;
                TalkSource.loop = true;
                TalkSource.Play();
            }
            else if (TalkTimer <= 0 && TalkSource.isPlaying)
            {
                TalkSource.Stop();
            }
        }

        private void UpdateBlupeeSearch()
        {
            if ((BlupeeSearchTimer -= Time.fixedDeltaTime) <= 0)
            {
                BlupeeSearchTimer = BlupeeSearchInterval;

                var hits = Physics2D.OverlapCircleAll(transform.position, BlupeeSearchRadius, BlupeeSearchMask);
                hits = hits.Where(hit => hit).ToArray();

                var blupeeGet = hits.Select(hit => hit.transform.GetComponent<Blupee>())
                    .OrderBy(blupee => Vector2.Distance(blupee.transform.position, transform.position))
                    .FirstOrDefault();

                if (blupeeGet)
                {
                    Destroy(blupeeGet.gameObject);
                    GameManager.Instance.PlayGainSound();
                    ++SalvagedAmount;
                }
            }
        }

        #region Interaction

        private void HandleChest(Chest chest)
        {
            if (chest.IsFull)
                return;

            if (HideAmount <= 0)
                return;

            ++chest.Bupees;
            --HideAmount;
            ++HiddenAmount;
            GameManager.Instance.PlayGainSound();
        }

        private void HandlePot(Pot pot)
        {
            if (pot.IsFull)
                return;

            if (HideAmount <= 0)
                return;

            ++pot.Bupees;
            --HideAmount;
            ++HiddenAmount;
            GameManager.Instance.PlayGainSound();
        }

        #endregion
    }
}
