using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Handles the highlighting of UI buttons by adjusting their emission intensity when selected or hovered over.
/// </summary>
public class UI_Button_Highlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Material buttonMaterial;
    [SerializeField] private Color emissionColor;
    [SerializeField] private float highlightEmissionIntensity = 5f;
    [SerializeField] private float normalEmissionIntensity = 4f;

    private Material buttonMaterialInstance;

    private void OnEnable()
    {
        InitializeMaterial();
        SetEmission(normalEmissionIntensity);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetEmission(highlightEmissionIntensity);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetEmission(normalEmissionIntensity);
    }

    public void OnSelect(BaseEventData eventData)
    {
        SetEmission(highlightEmissionIntensity);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        SetEmission(normalEmissionIntensity);
    }

    /// <summary>
    /// Sets the emission intensity of the button material.
    /// </summary>
    /// <param name="intensity">The intensity of the emission.</param>
    private void SetEmission(float intensity)
    {
        buttonMaterialInstance.SetColor("_EmissionColor", emissionColor * Mathf.Pow(2, intensity));
    }

    /// <summary>
    /// Initializes the material instance for the button.
    /// </summary>
    private void InitializeMaterial()
    {
        buttonMaterialInstance = new Material(buttonMaterial);
        GetComponent<Image>().material = buttonMaterialInstance;
    }
}
