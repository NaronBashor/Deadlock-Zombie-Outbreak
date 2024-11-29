using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public Button continueButton;

    // References to different menu canvases
    public GameObject mainMenu;
    public GameObject optionsMenu;
    public GameObject characterSelectPanel;

    public GameObject defaultMainButton;
    public GameObject defaultOptionsButton;
    public GameObject defaultCharacterButton;

    public Toggle soundToggle;
    public Toggle usingController;

    private void Awake()
    {
        //GameSettings.ResetToDefaults();
    }

    [ContextMenu("Reset To Default")]
    public void ResetToDefaults()
    {
        GameSettings.ResetToDefaults();
        GameManager.Instance.ResetAllMissions();
        if (GameSettings.characterChosen == -1) {
            mainMenu.SetActive(false);
            optionsMenu.SetActive(false);
            characterSelectPanel.SetActive(true);
            PlayerPrefs.SetInt("IsNewGame", 1); // Set flag for new game
            PlayerPrefs.Save();                 // Ensure the flag is saved
            EventSystem.current.SetSelectedGameObject(defaultCharacterButton);
        }
    }

    private void Start()
    {
        OpenMainMenu();

        if (!GameSettings.HasPreviousSave()) {
            continueButton.interactable = false;
        }

        characterSelectPanel.SetActive(false);

        if (soundToggle != null) {
            // Add a listener to the toggle's onValueChanged event
            soundToggle.onValueChanged.AddListener(OnSoundToggleValueChanged);
        }

        if (usingController != null) {
            // Add a listener to the toggle's onValueChanged event
            usingController.onValueChanged.AddListener(OnUsingControllerValueChanged);
        }

        SoundManager.Instance.PlayMusic("MainMenu", true);
    }

    private void Update()
    {
        // Optional: Allow the 'Cancel' button (e.g., B button on gamepad) to navigate back to the main menu
        if (optionsMenu.activeSelf && Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame) {
            OpenMainMenu();
        }
        // Optional: Allow the 'Cancel' button (e.g., B button on gamepad) to navigate back to the main menu
        if (characterSelectPanel.activeSelf && Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame) {
            OpenMainMenu();
        }
    }

    public void OnButtonClickSound()
    {
        SoundManager.Instance.PlaySFX("ButtonClick", false);
    }

    // Methods to switch menus
    public void OpenMainMenu()
    {
        mainMenu.SetActive(true);
        optionsMenu.SetActive(false);
        // Select default button
        EventSystem.current.SetSelectedGameObject(defaultMainButton);
    }

    public void OpenOptionsMenu()
    {
        mainMenu.SetActive(false);
        optionsMenu.SetActive(true);
        // Select default button
        EventSystem.current.SetSelectedGameObject(defaultOptionsButton);
    }

    public void OnOpenCharacterPanel()
    {
        GameSettings.LoadPlayerProgress(GameManager.Instance.playerProgress);
        GameSettings.LoadSettings();
        SceneManager.LoadScene("Game");
    }

    public void OnPlayButtonPressed()
    {
        //if (GameSettings.characterChosen == -1) { OnOpenCharacterPanel(); return; }
        //if (GameManager.Instance != null && GameManager.Instance.playerProgress != null) {
        //    GameSettings.LoadPlayerProgress(GameManager.Instance.playerProgress);
        //    SceneManager.LoadScene("Game");
        //} else {
        //    GameSettings.ResetToDefaults();
        //    SceneManager.LoadScene("Game");
        //}
    }

    // Method to handle toggle state changes
    private void OnSoundToggleValueChanged(bool isOn)
    {
        GameSettings.mute = isOn ? true : false;
        GameSettings.SaveSettings();
    }

    private void OnUsingControllerValueChanged(bool isOn)
    {
        GameSettings.usingController = isOn ? true : false;
        GameSettings.SaveSettings();
    }

    private void OnDestroy()
    {
        // Remove listener when the object is destroyed to avoid memory leaks
        if (soundToggle != null) {
            soundToggle.onValueChanged.RemoveListener(OnSoundToggleValueChanged);
        }
        if (usingController != null) {
            usingController.onValueChanged.RemoveListener(OnUsingControllerValueChanged);
        }
    }

    public void QuitGameButton()
    {
        Application.Quit();
    }
}
