using UnityEngine;
using System.Collections;

public class SetResolution : MonoBehaviour
{
    public int Width;
    public int Height;
    public bool Fullscreen;

    [Space]
    public bool ApplyOnAwake;

    private void Awake()
    {
        if (ApplyOnAwake)
            Apply();
    }

    public void Apply()
    {
        Screen.SetResolution(Width, Height, Fullscreen);
    }
}
