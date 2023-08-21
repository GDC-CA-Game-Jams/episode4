using System;
using System.Collections;
using System.Collections.Generic;
using Services;
using UnityEngine;
using UnityEngine.UI;

public class DiscoMeterService : IService
{
    public Action<float> OnMeterChange;
    
    private float initialValue;
    private float maxValue;
    private float killValue;

    private Slider slider;
    private Slider deathSlider;
    private float sliderMoveTime = 0.125f;

    private MonoBehaviour mono;
    
    private float currentValue;

    private AudioFilterControl audioFilterController;
    
    /// <summary>
    /// Init the service with the proper variables
    /// </summary>
    /// <param name="slider">Slider the user sees as a display</param>
    /// <param name="initialValue">Starting value of the bar</param>
    /// <param name="maxValue">Maximum value for the bar</param>
    /// <param name="killValue">What value constitutes a kill/player death</param>
    public void Init(MonoBehaviour mono, Slider slider, Slider deathSlider, float initialValue, float maxValue, float killValue, AudioFilterControl audioFilterController)
    {
        this.initialValue = initialValue;
        this.maxValue = maxValue;
        this.killValue = killValue;

        this.slider = slider;
        this.deathSlider = deathSlider;

        this.mono = mono;

        currentValue = initialValue;
        this.slider.value = currentValue / maxValue;
        this.deathSlider.value = 1 - this.slider.value;

        this.audioFilterController = audioFilterController;

    }

    /// <summary>
    /// Set the value of the bar, bypassing any max or death checks. Setting negative fails
    /// </summary>
    /// <param name="newValue">New value to set the slider to</param>
    public void SetValue(float newValue)
    {
        if (newValue < 0)
        {
            throw new NotSupportedException("Negative values not supported as valid values");
        }
        currentValue = newValue;
    }

    /// <summary>
    /// Changes the current value by the amount specified
    /// </summary>
    /// <param name="changeValue">Amount to change the current value by</param>
    /// <returns>True if the change was successful, false if it was not</returns>
    public bool ChangeValue(float changeValue)
    {
        float temp = currentValue + changeValue;
        if (temp >= maxValue)
        {
            currentValue = maxValue;
            // Set the slider percent to a normalized percent of the max value
            UpdateSliderValue(currentValue / maxValue);
            audioFilterController.BlendSnapshots(currentValue);
            audioFilterController.PlayerMaxHealth(true);
            
            // send 'yes' to Guitar Track
            return true;
        }

        if (temp <= maxValue && temp > killValue)
        {
            currentValue = temp;
            // Set the slider percent to a normalized percent of the max value
            UpdateSliderValue(currentValue / maxValue);
            audioFilterController.BlendSnapshots(currentValue);
            audioFilterController.PlayerMaxHealth(false);

            // send 'no' to guitar track
            return true;
        }
        
        if (temp <= killValue)
        {
            currentValue = temp;
            ServiceLocator.Instance.Get<EventManager>().OnDeath?.Invoke();
            // Set the slider percent to a normalized percent of the max value
            UpdateSliderValue(currentValue / maxValue);
            audioFilterController.BlendSnapshots(currentValue);
            return true;
        }

        throw new NotSupportedException("Provided change leads to an unsupported value: " + temp);
    }

    private void UpdateSliderValue(float value)
    {
        mono.StartCoroutine(SmoothMoveSlider(slider, value));
        mono.StartCoroutine(SmoothMoveSlider(deathSlider, 1 - value));
    }

    private IEnumerator SmoothMoveSlider(Slider currSlider, float value)
    {
        while (Math.Abs(currSlider.value - value) > Constants.FLOAT_COMPARE_TOLERANCE)
        {
            currSlider.value = Mathf.MoveTowards(currSlider.value, value, sliderMoveTime * Time.deltaTime);
            yield return null;
        }
    }
    
    /// <summary>
    /// Get the current bar value raw
    /// </summary>
    /// <returns>Raw bar value</returns>
    public float GetValue()
    {
        return currentValue;
    }

    /// <summary>
    /// Get the current bar value normalized between 0.0 and 1.0
    /// </summary>
    /// <returns>Current normalized bar value between 0.0 and 1.0</returns>
    public float GetValueNormalized()
    {
        return currentValue / maxValue;
    }
}
