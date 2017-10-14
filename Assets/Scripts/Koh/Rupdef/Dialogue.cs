using UnityEngine;
using System.Collections;
using Koh.Rupdef;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour
{
    public Text Text;
    public float ScrollSpeed;

    [Header("Debug")]
    public float StartPosition;
    public float EndPosition;

    private void Reset()
    {
        Text = GetComponentInChildren<Text>();
        ScrollSpeed = 100;
    }

    private void Awake()
    {
        StartPosition = Text.rectTransform.anchoredPosition.y - 0;
        EndPosition = Text.rectTransform.anchoredPosition.y + Text.rectTransform.rect.height;
    }

    private void Update()
    {
        Text.rectTransform.anchoredPosition += Vector2.up * ScrollSpeed * Time.deltaTime;
        if (Text.rectTransform.anchoredPosition.y > EndPosition)
            Text.rectTransform.anchoredPosition = Text.rectTransform.anchoredPosition.WithY(StartPosition);
    }
}
