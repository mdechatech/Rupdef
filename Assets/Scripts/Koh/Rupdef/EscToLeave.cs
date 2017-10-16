using UnityEngine;
using System.Collections;

public class EscToLeave : MonoBehaviour
{
    public string MenuScene;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.LoadLevel(MenuScene);
    }
}
