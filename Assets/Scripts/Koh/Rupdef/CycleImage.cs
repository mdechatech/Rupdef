using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CycleImage : MonoBehaviour
{
    public float Interval;
    public Sprite[] Sprites;

    [Header("Debug")]
    public Image Image;

    public int SpriteIndex;

    public float Timer;

    private void Awake()
    {
        Image = GetComponent<Image>();
        if (Sprites == null || Sprites.Length == 0)
            enabled = false;
    }

    private void Update()
    {
        if ((Timer -= Time.deltaTime) <= 0)
        {
            Timer = Interval;
            Image.sprite = Sprites[SpriteIndex = (SpriteIndex + 1) % Sprites.Length];
        }
    }
}
