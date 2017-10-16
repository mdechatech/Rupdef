using System.Linq;
using Pathfinding;
using Pathfinding.Util;
using UnityEngine;

namespace Koh.Rupdef
{
    [RequireComponent(typeof(Seeker), typeof(Rigidbody2D))]
    public class EnemyController : MonoBehaviour
    {
        public float Boredom;

        [Space]
        public float Speed;
        public float MinDistance;
        public float PathSearchInterval;

        public float ExitRadius;

        public float ObjectSearchRadius;
        public float ObjectSearchInterval;
        public LayerMask ObjectSearchMask;

        public float BlupeeSearchRadius;
        public float BlupeeSearchInterval;
        public LayerMask BlupeeSearchMask;

        public float ActionInterval;

        public float SlowFactor;
        public LayerMask SlowMask;
        public float BoringFactor;
        public LayerMask BoringMask;
        public float AreaSearchRadius;

        [Header("Debug")]
        public int Waypoint;

        public float NormalSpeed;

        public Pot TargetPot;
        public Chest TargetChest;
        public Transform TargetDoor;

        public Seeker Seeker;

        public Rigidbody2D Rigidbody;

        public Path Path;

        public float TalkTimer;
        public float ShoveTimer;
        public float PathSearchTimer;
        public float ObjectSearchTimer;
        public float BlupeeSearchTimer;
        public float ActionTimer;

        public ActionTarget[] Targets;

        private void Awake()
        {
            NormalSpeed = Speed;
            Seeker = GetComponent<Seeker>();
            Rigidbody = GetComponent<Rigidbody2D>();
        }

        private void SearchPath()
        {
            if (Boredom <= 0)
                goto none;

            if (TargetPot && TargetPot.Bupees > 0)
                return;

            if (TargetChest && TargetChest.Bupees > 0)
                return;

            if (GameManager.Instance.Pots.Length > 0)
            {
                var filledPots = GameManager.Instance.Pots
                    .Where(p => p && p.Bupees > 0)
                    .ToArray();

                if (filledPots.Length == 0)
                    goto chests;

                var pot = filledPots[UnityEngine.Random.Range(0, filledPots.Length)];
                if (pot)
                {
                    Seeker.StartPath(transform.position, pot.transform.position, OnPathComplete);
                    TargetPot = pot;
                    TargetDoor = null;
                }

                if (TargetPot)
                    return;
            }

            chests:
            if (GameManager.Instance.Chests.Length > 0)
            {
                var filledChests = GameManager.Instance.Chests
                    .Where(c => c.Bupees > 0)
                    .ToArray();

                if (filledChests.Length == 0)
                    goto none;

                var chest = filledChests[UnityEngine.Random.Range(0, filledChests.Length)];
                if (chest)
                {
                    Seeker.StartPath(transform.position, chest.transform.position, OnPathComplete);
                    TargetChest = chest;
                    TargetPot = null;
                    TargetDoor = null;
                }
            }
            else
            {
                goto none;
            }

            return;

            none:

            if (TargetDoor)
                return;

            var doors = GameManager.Instance.Doors;
            var door = doors[UnityEngine.Random.Range(0, doors.Length)];
            TargetDoor = door.transform;
            TargetPot = null;
            TargetChest = null;
            Seeker.StartPath(transform.position, door.transform.position, OnPathComplete);

            return;
        }

        private void SearchObjects()
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, ObjectSearchRadius, ObjectSearchMask);
            hits = hits.Where(hit => hit).ToArray();

            Targets = hits
                .Where(hit => hit.transform.GetComponent<ActionTarget>())
                .Select(hit => hit.transform.GetComponent<ActionTarget>())
                .ToArray();

        }

        private void SearchBlupees()
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, BlupeeSearchRadius, BlupeeSearchMask);
            hits = hits.Where(hit => hit).ToArray();

            var blupeeGet = hits.Select(hit => hit.transform.GetComponent<Blupee>())
                .OrderBy(blupee => Vector2.Distance(blupee.transform.position, transform.position))
                .FirstOrDefault();

            if (blupeeGet)
            {
                GameManager.Instance.PlayLoseSound();
                Destroy(blupeeGet.gameObject);
            }
        }

        private void TryAction()
        {
            for (var i = 0; i < Targets.Length; ++i)
            {
                if (HandleAction(Targets[i]))
                    return;
            }
        }

        private bool HandleAction(ActionTarget target)
        {
            if (Boredom <= 0)
                return false;

            if (TalkTimer > 0)
                return false;

            var chest = target as Chest;
            if (chest)
            {
                if (chest.Bupees > 0)
                {
                    --chest.Bupees;
                    --GameManager.Instance.Player.HiddenAmount;
                    GameManager.Instance.PlayLoseSound();
                    ActionTimer = ActionInterval * 1.5f;
                    return true;
                }
            }

            var pot = target as Pot;
            if (pot)
            {
                if (pot.Bupees > 0)
                {
                    GameManager.Instance.PlayLoseSound();
                    GameManager.Instance.Player.HiddenAmount -= pot.Bupees;
                    pot.Smash();
                    return true;
                }
            }

            return false;
        }

        private void Update()
        {
            UpdateTalk();
        }

        private void FixedUpdate()
        {
            if ((Boredom -= Time.fixedDeltaTime) < 0)
                Boredom = 0;

            UpdateExit();
            UpdatePath();
            UpdateAreas();
            UpdateObjects();
            UpdateBlupees();
            UpdateActions();
        }

        private void UpdateAreas()
        {
            var slow = Physics2D.OverlapCircle(transform.position, AreaSearchRadius, SlowMask);
            Speed = slow ? NormalSpeed * SlowFactor : NormalSpeed;

            var boring = Physics2D.OverlapCircle(transform.position, AreaSearchRadius, BoringMask);
            Boredom -= boring ? (Time.fixedDeltaTime * BoringFactor) : 0;
        }

        private void UpdateExit()
        {
            if (TargetDoor)
            {
                var distance = Vector2.Distance(transform.position, TargetDoor.position);
                if (distance <= ExitRadius)
                {
                    Destroy(gameObject);
                }
            }
        }

        private void UpdateTalk()
        {
            if ((TalkTimer -= Time.deltaTime) < 0)
            {
                TalkTimer = 0;
            }
        }

        private void UpdateBlupees()
        {
            if ((BlupeeSearchTimer -= Time.fixedDeltaTime) <= 0)
            {
                BlupeeSearchTimer = BlupeeSearchInterval;
                SearchBlupees();
            }
        }

        private void UpdateObjects()
        {
            if ((ObjectSearchTimer -= Time.fixedDeltaTime) <= 0)
            {
                ObjectSearchTimer = ObjectSearchInterval;
                SearchObjects();
            }
        }

        private void UpdateActions()
        {
            if ((ActionTimer -= Time.fixedDeltaTime) <= 0)
            {
                ActionTimer = ActionInterval;
                TryAction();
            }
        }

        private void UpdatePath()
        {

            if ((PathSearchTimer -= Time.fixedDeltaTime) <= 0)
            {
                PathSearchTimer = PathSearchInterval;
                SearchPath();
            }

            if (TalkTimer > 0)
                return;

            if (Path == null || Path.error)
                return;

            var direction = (Path.vectorPath[Waypoint] - transform.position).normalized;
            direction *= Speed * Time.fixedDeltaTime;
            this.gameObject.transform.Translate(direction);

            if (Vector3.Distance(transform.position, Path.vectorPath[Waypoint]) < MinDistance)
            {
                ++Waypoint;
                if (Waypoint >= Path.vectorPath.Count)
                {
                    Waypoint = 0;
                    Path = null;
                }
            }
        }

        private void OnPathComplete(Path path)
        {
            Waypoint = 0;

            Path = path;
            if(Path.error)
                return;
        }
    }
}
