using UnityEngine;
using UnityEngine.UI;

public class UI_Stats_Slider : MonoBehaviour
{
    private Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
    }

    public void SetStats(int value) => slider.value = value;


    public void SetMaxValue(int maxValue)
    {
        slider.maxValue = maxValue;
        slider.value = maxValue;
    }
}
