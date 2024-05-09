/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EditModeButton : MonoBehaviour {
    [SerializeField] PathfindingMapMaker mapToEdit;

    private Button button;
    private TMP_Text buttonText;

    private void Awake() {
        button = GetComponent<Button>();
        buttonText = GetComponentInChildren<TMP_Text>();

        button.onClick.AddListener(ToggleMapEditMode);
    }

    private void ToggleMapEditMode() {
        buttonText.text = !mapToEdit.EditMode ? "Editing" : "Playing";
        mapToEdit.ToggleEditMode();
    }
}
*/