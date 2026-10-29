using UnityEngine;
using UnityEngine.UI;

public class CUIBar : MonoBehaviour
{
    [SerializeField] private Image _fillImage;

    public virtual void SetValue(float currentValue, float maxValue)
    {

        if (maxValue > 0f)
        {
            if (currentValue < 0f)
            {
                currentValue = 0f;
            }

            _fillImage.fillAmount = Mathf.Clamp01(currentValue / maxValue);
        }

    }

}

