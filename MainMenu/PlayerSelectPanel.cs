using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerSelectPanel : MonoBehaviour
{
    public GameObject playerNamePanel;
    public string playerName;
    public TMP_InputField inputField;

    public GameObject maleButton;
    public GameObject femaleButton;

    public Button confirmCharacterButton;

    public Image maleImage;
    public Image femaleImage;

    public string classChosen;

    public TextMeshProUGUI rangerText;
    public TextMeshProUGUI fighterText;

    private void Awake()
    {
        playerNamePanel.SetActive(false);
        ShowFighterText();

        confirmCharacterButton.onClick.AddListener(() => LoadGame());
    }

    private void Update()
    {
        if (EventSystem.current.currentSelectedGameObject != null) {
            GameObject selectedButton = EventSystem.current.currentSelectedGameObject;

            Button button = selectedButton.GetComponent<Button>();
            if (selectedButton != null) {
                // Example: Perform action based on the button's name
                switch (selectedButton.name) {
                    case "ManButton":
                        maleImage.enabled = true;
                        femaleImage.enabled = false;
                        break;
                    case "FemaleButton":
                        maleImage.enabled = false;
                        femaleImage.enabled = true;
                        break;
                    default:
                        //Debug.Log("Selected button: " + selectedButton.name);
                        break;
                }
            }
        }
        if (playerNamePanel != null) {
            if (playerNamePanel.activeSelf) {
                if (inputField.text.Length >=3 && Keyboard.current.enterKey.wasPressedThisFrame || Gamepad.current != null) {
                    GameSettings.playerName = inputField.text;
                    if (GameManager.Instance != null && GameManager.Instance.playerProgress != null && Gamepad.current.startButton.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame) {
                        LoadGame();
                    }
                }
                if (Keyboard.current.escapeKey.wasPressedThisFrame) {
                    playerNamePanel.SetActive(false);
                }
            }
        }
    }

    public void ShowRangerText()
    {
        GameSettings.classChosen = "Ranger";
        rangerText.enabled = true;
        fighterText.enabled = false;
    }

    public void ShowFighterText()
    {
        GameSettings.classChosen = "Fighter";
        rangerText.enabled = false;
        fighterText.enabled = true;
    }

    public void MalePlayerNameSelectPanel()
    {
        playerNamePanel.SetActive(true);
        inputField.enabled = true;
        inputField.text = "Jack Steele";
    }

    public void FemalePlayerNameSelectPanel()
    {
        playerNamePanel.SetActive(true);
        inputField.enabled = true;
        inputField.text = "Riley Hunter";
    }

    public void ManChosen()
    {
        GameSettings.characterChosen = 0;
        MalePlayerNameSelectPanel();
        GameSettings.SaveSettings();
    }

    public void WomanChosen()
    {
        GameSettings.characterChosen = 1;
        FemalePlayerNameSelectPanel();
        GameSettings.SaveSettings();
    }

    private void LoadGame()
    {
        GameSettings.LoadPlayerProgress(GameManager.Instance.playerProgress);
        SceneManager.LoadScene("Game");
    }
}
