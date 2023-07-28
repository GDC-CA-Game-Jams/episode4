using System;
using System.Collections;
using System.Collections.Generic;
using Services;
using UnityEngine;
using UnityEngine.UIElements;

public class DiscoMeterService : IService
{
    private float initialValue;
    private float maxValue;
    private float killValue;

    private Slider slider;

    private float currentValue;
    
    /// <summary>
    /// Init the service with the proper variables
    /// </summary>
    /// <param name="slider">Slider the user sees as a display</param>
    /// <param name="initialValue">Starting value of the bar</param>
    /// <param name="maxValue">Maximum value for the bar</param>
    /// <param name="killValue">What value constitutes a kill/player death</param>
    public void Init(Slider slider, float initialValue, float maxValue, float killValue)
    {
        this.initialValue = initialValue;
        this.maxValue = maxValue;
        this.killValue = killValue;

        this.slider = slider;

        currentValue = initialValue;
        this.slider.value = currentValue / maxValue;
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
            slider.value = currentValue / maxValue;
            return true;
        }

        if (temp <= maxValue && temp > killValue)
        {
            currentValue = temp;
            // Set the slider percent to a normalized percent of the max value
            slider.value = currentValue / maxValue;
            return true;
        }
        
        if (temp <= killValue)
        {
            currentValue = temp;
            //TODO: Do kill
            // Set the slider percent to a normalized percent of the max value
            slider.value = currentValue / maxValue;
            return true;
        }

        throw new NotSupportedException("Provided change leads to an unsupported value: " + temp);
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
