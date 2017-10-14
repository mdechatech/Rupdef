using UnityEngine;

namespace Koh.Rupdef
{
    public class GridHelperTest : MonoBehaviour
    {
        private void Update()
        {
            transform.position = GridHelper.Instance.Align(transform.position);
        }
    }
}
