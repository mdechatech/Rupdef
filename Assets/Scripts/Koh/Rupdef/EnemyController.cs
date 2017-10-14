using System.Linq;
using Pathfinding;
using Pathfinding.Util;
using UnityEngine;

namespace Koh.Rupdef
{
    [RequireComponent(typeof(Seeker), typeof(Rigidbody2D))]
    public class EnemyController : MonoBehaviour
    {
        public float Speed;
        public float MinDistance;
        public float PathSearchInterval;

        public float ObjectSearchRadius;
        public float ObjectSearchInterval;
        public LayerMask ObjectSearchMask;

        public float ActionInterval;

        [Header("Debug")]
        public int Waypoint;

        public Chest Target;
        public Transform TargetDoor;

        public Seeker Seeker;

        public Rigidbody2D Rigidbody;

        public Path Path;

        public float PathSearchTimer;
        public float ObjectSearchTimer;
        public float ActionTimer;

        public ActionTarget[] Targets;

        private void Awake()
        {
            Seeker = GetComponent<Seeker>();
            Rigidbody = GetComponent<Rigidbody2D>();
        }

        private void SearchPath()
        {
            if (Target && Target.Bupees > 0)
                return;

            if (GameManager.Instance.Chests.Length > 0)
            {
                var filledChests = GameManager.Instance.Chests
                    .Where(c => c.Bupees > 0)
                    .Where(c => !c.GetComponent<Placeable>().IsBeingPlaced)
                    .ToArray();

                if (filledChests.Length == 0)
                    goto none;

                var chest = filledChests[UnityEngine.Random.Range(0, filledChests.Length)];
                if (chest)
                {
                    Seeker.StartPath(transform.position, chest.transform.position, OnPathComplete);
                    Target = chest;
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
            Target = null;
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
            var chest = target as Chest;
            if (chest)
            {
                if (chest.Bupees > 0)
                {
                    --chest.Bupees;
                    return true;
                }
            }

            return false;
        }

        private void FixedUpdate()
        {
            UpdatePath();
            UpdateObjects();
            UpdateActions();
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
            print("path complete");
            Waypoint = 0;

            Path = path;
            if(Path.error)
                return;
        }
    }
}
