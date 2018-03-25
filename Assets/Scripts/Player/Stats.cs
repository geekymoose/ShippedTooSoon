using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Base Stats")]
public class Stats : ScriptableObject
{
    public float value;
    public float maxValue;
    public float valuePerSecond;

    
    public void SetValue(float startingValue, float maxValue, float valuePerSecond)
    {
        this.value = startingValue;
        this.maxValue = maxValue;
        this.valuePerSecond = valuePerSecond;
    }

    public void ReduceValue(float reducedValue)
    {
        if (value > 0)
        {
            value -= reducedValue;
        }
        else
        {
            value = 0;
        }
    }

    public void ResplendishValue()
    {
        if (value <= maxValue)
        {
            value += valuePerSecond;
        }
    }

    public void AddValueMax(float addValueMax)
    {
        this.maxValue += addValueMax;
    }

    public float GetValue()
    {
        return this.value;
    }

    

    public float GetValuePercent()
    {
        if (value <= maxValue)
        {
            return value / maxValue;
        }
        else
        {
            return value;
        }
    }
}