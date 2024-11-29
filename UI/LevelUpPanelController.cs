using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class LevelUpPanelController : MonoBehaviour
{
    public GameObject defaultButton;       // The default button to select when the panel is opened

    private void Start()
    {
        EventSystem.current.SetSelectedGameObject(defaultButton);
    }

    private void Update()
    {
        // Optional: Allow the 'Cancel' button (e.g., B button on gamepad) to navigate back to the main menu
        if (this.gameObject.activeSelf && (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame) || (Input.GetKeyDown(KeyCode.Escape))) {
            CloseLevelUpPanel();
        }
    }

    public void CloseLevelUpPanel()
    {
        this.gameObject.SetActive(false);
        SoundManager.Instance.PlaySFX("ButtonClick", false);
        EventSystem.current.SetSelectedGameObject(null); // Clear the selected object
    }
}
