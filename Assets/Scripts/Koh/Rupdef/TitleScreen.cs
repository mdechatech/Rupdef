using UnityEngine;
using System.Collections;

public class TitleScreen : MonoBehaviour
{
    public string GameScene;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            Application.LoadLevel(GameScene);
    }
}
