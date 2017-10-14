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


    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        public float Speed;

        public Transform ActionSensor;
        public LayerMask ActionMask;

        public Transform Renderer;

        public int Bupees;
        

        [Header("Input")]
        public string MoveAxisX;

        public string MoveAxisY;

        public string ActionButton;

        [Header("Debug")]
        public Facing Facing;

        public float InputX;
        public float InputY;

        public bool InputAction;

        public Rigidbody2D Rigidbody;

        public ActionTarget Target;

        private void Reset()
        {
            Speed = 3;

            MoveAxisX = "Horizontal";
            MoveAxisY = "Vertical";
            ActionButton = "Action";
        }

        private void Awake()
        {
            Facing = Facing.Right;
            Rigidbody = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            InputX = Input.GetAxis("Horizontal");
            InputY = Input.GetAxis("Vertical");

            InputAction = Input.GetButtonDown("Action");
            if (Target && InputAction)
            {
                HandleAction(Target);
            }
        }

        private void FixedUpdate()
        {
            UpdateVelocity();
            UpdateFacing(); 
            UpdateTarget();
        }

        private void HandleAction(ActionTarget target)
        {
            // hoo boy
            var chest = target as Chest;
            if (chest)
                HandleChest(chest);
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
