using System;
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
        Place,
        Remove,
        Interact
    }

    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        public float Speed;

        public Transform ActionSensor;
        public LayerMask ActionMask;

        public Transform Renderer;

        public int Bupees;

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
            Rigidbody = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            BeginPlaceMode();   
        }

        private void Update()
        {
            InputX = Input.GetAxis(MoveAxisX);
            InputY = Input.GetAxis(MoveAxisY);

            InputAction = Input.GetButtonDown(ActionButton);
            InputTogglePlaceAction = Input.GetButtonDown(TogglePlaceActionButton);

            InputToggleLeft = Input.GetButtonDown(ToggleLeftButton);
            InputToggleRight = Input.GetButtonDown(ToggleRightButton);

            if (PlaceMode && InputTogglePlaceAction)
            {
                // Cycle to next enum
                PlaceAction = (PlaceAction)(((int) PlaceAction + 1) % Enum.GetNames(typeof(PlaceAction)).Length);
            }

            if (Target && !PlaceMode && InputAction)
                HandleAction(Target);

            if (PlaceMode && InputAction)
                HandlePlaceAction();
        }

        private void HandlePlaceAction()
        {
            switch (PlaceAction)
            {
                case PlaceAction.Place:
                    PlaceCurrent();
                    break;
            }
        }

        private void FixedUpdate()
        {
            UpdateVelocity();
            UpdateFacing();
            UpdateTarget();

            if (PlaceMode)
            {
                UpdatePlaceMode();
            }
        }

        private void HandleAction(ActionTarget target)
        {
            // hoo boy
            var chest = target as Chest;
            if (chest)
            {
                HandleChest(chest);
            }
        }

        public void BeginPlaceMode()
        {
            PlaceMode = true;
            CurrentPlaceable = CreateGhostPlaceable(GameManager.Instance.Placeables[CurrentPlaceableIndex]);
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

            var testCollider = CurrentPlaceable.GetComponentInChildren<BoxCollider2D>();
            if (testCollider)
            {
                testCollider.enabled = true;

                Collider2D[] results = new Collider2D[1];

                var amount = testCollider.OverlapCollider(
                    new ContactFilter2D {useLayerMask = true, layerMask = ActionMask},
                    results);
                print(amount);
                if (amount > 0)
                {
                    testCollider.enabled = false;
                    return;
                }


                testCollider.enabled = false;
            }

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
            print(CurrentPlaceableIndex);
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
            print(CurrentPlaceableIndex);

            if (CurrentPlaceable)
                Destroy(CurrentPlaceable.gameObject);

            CurrentPlaceable = CreateGhostPlaceable(GameManager.Instance.Placeables[CurrentPlaceableIndex]);
        }

        private void UpdatePlaceMode()
        {
            if (InputToggleRight)
                NextPlaceable();
            else if (InputToggleLeft)
                PreviousPlaceable();

            if (CurrentPlaceable)
            {
                CurrentPlaceable.transform.position = GridHelper.Instance.Align(PlaceAnchor.position);
            }
        }

        private void UpdateVelocity()
        {
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

        private void UpdateTarget()
        {
            var target = Physics2D.Linecast(transform.position, ActionSensor.position, ActionMask);
            if (!target)
            {
                Target = null;
                return;
            }

            Target = target.transform.GetComponent<ActionTarget>();
            if (!Target)
                return;
        }

        #region Interaction

        private void HandleChest(Chest chest)
        {
            if (chest.IsFull)
                return;

            if (Bupees <= 0)
                return;

            ++chest.Bupees;
            --Bupees;

            print("Added bupees to chest");
        }

        #endregion
    }
}
