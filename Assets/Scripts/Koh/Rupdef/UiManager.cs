using UnityEngine;
using UnityEngine.UI;

namespace Koh.Rupdef
{
    public class UiManager : MonoBehaviour
    {
        [Header("Place Mode")]
        public CanvasGroup PlaceModeGroup;

        [Space]
        public Text SpendAmountText;
        public Text HideAmountText;
        public Text NightText;

        [Space]
        public Text PlaceActionText;

        [Space]
        public CanvasGroup ErrorGroup;
        public Text ErrorTitleText;
        public Text ErrorDescriptionText;

        public CanvasGroup PlaceObjectGroup;
        public Text PlaceObjectText;
        public Text PlaceObjectFlavorText;
        public Text PlaceObjectPriceText;

        [Header("Play Mode")]
        public CanvasGroup PlayModeGroup;

        [Space]
        public Image EnergyImage;

        public Text DayText;
        public Text HiddenAmountText;
        public Text SalvagedAmountText;

        [Header("Success")]
        public CanvasGroup SuccessGroup;
        public Text SuccessSalvagedText;
        public Text SuccessStoredText;
        public Text SuccessPensionText;
        public Text SuccessInsuranceText;
        public Text SuccessTotalText;

        [Header("Debug")]
        public bool ShowingError;
        public float ErrorTimer;

        // Update is called once per frame
        void Update()
        {
            var player = GameManager.Instance.Player;

            if (player.PlaceMode)
            {
                PlaceModeGroup.alpha = 1;
                PlayModeGroup.alpha = 0;

                SpendAmountText.text = player.SpendAmount.ToString();
                HideAmountText.text = player.HideAmount.ToString();

                PlaceActionText.text = player.PlaceAction.ToString();
                PlaceObjectText.text = player.CurrentPlaceable ? player.CurrentPlaceable.Name : string.Empty;

                PlaceObjectGroup.alpha = player.PlaceAction == PlaceAction.Buy ? 1 : 0;

                if (player.PlaceAction == PlaceAction.Buy)
                {
                    if (PlaceObjectFlavorText.text != player.CurrentPlaceable.Flavor)
                        PlaceObjectFlavorText.text = player.CurrentPlaceable.Flavor;

                    PlaceObjectPriceText.text = player.CurrentPlaceable.Price.ToString();
                }
            }
            else
            {
                PlaceModeGroup.alpha = 0;
                PlayModeGroup.alpha = 1;

                EnergyImage.fillAmount = player.Energy / player.EnergyMax;

                HiddenAmountText.text = player.HiddenAmount.ToString();
                SalvagedAmountText.text = player.SalvagedAmount.ToString();
            }

            if (ShowingError)
            {
                if ((ErrorTimer -= Time.deltaTime) < 0)
                {
                    ErrorTimer = 0;
                    ShowingError = false;
                    ErrorGroup.alpha = 0;
                }
            }
        }

        public void ShowError(string title, string text, float duration = 5)
        {
            ErrorTimer = duration;
            ErrorGroup.alpha = 1;
            ShowingError = true;
            ErrorTitleText.text = title;
            ErrorDescriptionText.text = text;

            GameManager.Instance.PlayErrorSound();
        }
    }
}
