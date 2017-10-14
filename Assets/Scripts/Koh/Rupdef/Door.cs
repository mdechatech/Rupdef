using UnityEngine;

namespace Koh.Rupdef
{
    public class Door : MonoBehaviour
    {
        public EnemyController EnemyPrefab;

        public EnemyController Spawn()
        {
            var enemy = Instantiate(EnemyPrefab);
            enemy.transform.position = transform.position;
            return enemy;
        }
    }
}
