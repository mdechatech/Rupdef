using UnityEngine;
using UnityEngine.UI;

namespace Koh.Rupdef
{
    public class UiManager : MonoBehaviour
    {
        public Text PlaceActionText;

        public CanvasGroup PlaceObjectGroup;
        public Text PlaceObjectText;
        
        // Update is called once per frame
        void Update()
        {
            var player = GameManager.Instance.Player;

            if (player.PlaceMode)
            {
                PlaceActionText.text = player.PlaceAction.ToString();
                PlaceObjectText.text = player.CurrentPlaceable ? player.CurrentPlaceable.Name : string.Empty;

                PlaceObjectGroup.alpha = player.PlaceAction == PlaceAction.Place ? 1 : 0;
            }

        }
    }
}
