using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GhostSpeedUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider slider;
    [SerializeField] private Button plusButton;
    [SerializeField] private Button minusButton;
    [SerializeField] private TextMeshProUGUI valueLabel;

    [Header("Faixa e Passo")]
    [SerializeField] private int min = 0;
    [SerializeField] private int max = 10;
    [SerializeField] private int step = 1;

    private bool _updating;

    private void Awake()
    {
        slider.minValue = min;
        slider.maxValue = max;
        slider.wholeNumbers = true;
        
        slider.onValueChanged.AddListener(OnSliderValueChanged);
        plusButton.onClick.AddListener(Increase);
        minusButton.onClick.AddListener(Decrease);
    }

    private void Start()
    {
        _updating = true;
        slider.value = Mathf.Clamp(GhostSpeedSettings.Speed, min, max);
        UpdateLabel(slider.value);
        _updating = false;
    }

    private void OnDestroy()
    {
        slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        plusButton.onClick.RemoveListener(Increase);
        minusButton.onClick.RemoveListener(Decrease);
    }

    private void OnSliderValueChanged(float v)
    {
        if (_updating)
            return;
        var iv = Mathf.RoundToInt(v);
        GhostSpeedSettings.Speed = iv;
        UpdateLabel(v);
    }

    private void Increase()
    {
        var v = Mathf.Min((int)slider.value + step, max);
        _updating = true;
        slider.value = v;
        _updating = false;
        OnSliderValueChanged(v);
    }

    private void Decrease()
    {
        var v = Mathf.Max((int)slider.value - step, min);
        _updating = true;
        slider.value = v;
        _updating = false;
        OnSliderValueChanged(v);
    }

    private void UpdateLabel(float v)
    {
        if (valueLabel != null)
            valueLabel.text = v.ToString();
    }
}