using UnityEngine;

public class PlayerHUDManager : MonoBehaviour
{
    [SerializeField] private UI_Stats_Slider healthBar;
    [SerializeField] private GameObject playerBar;
    [SerializeField] private UI_Stats_Slider staminaSlider;

    public void SetNewStaminaValue(float oldValue, float newValue) => staminaSlider.SetStats(Mathf.RoundToInt(newValue));

    public void SetMaxStaminaValue(int maxValue) => staminaSlider.SetMaxValue(maxValue);
    public void SetNewHealthValue(int oldValue, int newValue) => healthBar.SetStats(newValue);
    public void SetMaxHealthValue(int maxValue) => healthBar.SetMaxValue(maxValue);
}