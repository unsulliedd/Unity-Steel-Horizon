using UnityEngine;

public class PlayerHUDManager : MonoBehaviour
{
    [SerializeField] private GameObject playerBar;
    [SerializeField] private UI_Stats_Slider staminaSlider;

    public void SetNewStaminaValue(float oldValue, float newValue) => staminaSlider.SetStats(Mathf.RoundToInt(newValue));

    public void SetMaxStaminaValue(int maxValue) => staminaSlider.SetMaxValue(maxValue);
}
