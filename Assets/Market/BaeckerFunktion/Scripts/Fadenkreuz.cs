using UnityEngine;

public class Fadenkreuz : MonoBehaviour
{
    void OnGUI()
    {
        float x = Screen.width / 2 - 10;
        float y = Screen.height / 2 - 10;
        GUI.Box(new Rect(x, y, 20, 20), "+");
    }
}
