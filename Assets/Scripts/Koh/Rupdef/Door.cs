using UnityEngine;

namespace Koh.Rupdef
{
    public class Door : MonoBehaviour
    {
        public EnemyController EnemyPrefab;

        public EnemyController Spawn(float boredom, float boredomVariance)
        {
            var enemy = Instantiate(EnemyPrefab);
            enemy.transform.position = transform.position;

            var boredomMin = boredom * boredomVariance;
            var boredomMax = boredom * (1 + boredomVariance);
            enemy.Boredom = UnityEngine.Random.Range(boredomMin, boredomMax);
            
            return enemy;
        }
    }
}
