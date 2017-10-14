using UnityEngine;
using UnityEngine.UI;

namespace Koh.Rupdef
{
    public class Tooltip : MonoBehaviour
    {
        public CanvasGroup StorageGroup;
        public Text StorageTargetText;
        public Text StorageAmountText;
        public Text StorageMaxText;
        public Text StorageKeyText;

        [Space]
        public CanvasGroup ActionGroup;
        public Text ActionTargetText;
        public Text ActionNameText;
    }
}
