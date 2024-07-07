using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Material buttonMaterialInstance;
    [SerializeField] private Material buttonMaterial;
    [SerializeField] private Color emissionColor;
    [SerializeField] private float highlightEmissionIntensity = 5f;
    [SerializeField] private float normalEmissionIntensity = 4f;

    void Start()
    {
        buttonMaterialInstance = new Material(buttonMaterial);
        GetComponent<Image>().material = buttonMaterialInstance;
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

    private void SetEmission(float intensity)
    {
        buttonMaterialInstance.SetColor("_EmissionColor", emissionColor * Mathf.Pow(2, intensity));
    }
}
