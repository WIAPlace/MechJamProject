using UnityEngine;
using UnityEngine.UIElements;

// Used with specificly the UI toolkit.
// 

public class SettingsUIToolkit_Reader : MonoBehaviour
{
    public UIDocument uiDocument;
    
    private void OnEnable()
    {
        if (uiDocument == null) return;
        var root = uiDocument.rootVisualElement;

        // Find all sliders in the USS class of volume in the UI Document
        var sliders = root.Query<Slider>(className: "volume-slider").ToList();

        foreach (var slider in sliders)
        {
            // Register an event for every slider that fires off `OnValueChanged`
            slider.RegisterValueChangedCallback(evt => SettingsManager.OnVolumeChanged(slider.name, evt.newValue));
        }
    } 
}
