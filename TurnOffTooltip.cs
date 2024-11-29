using UnityEngine;

public class TurnOffTooltip : MonoBehaviour
{
    public Canvas canvas;

    private void OnDestroy()
    {
        if (canvas != null) {
            canvas.gameObject.SetActive(false);
        }
    }
}
