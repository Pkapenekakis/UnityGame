using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarScript : MonoBehaviour
{
    public Slider slider;

    public void setMaxHealth(float maxHealth){
        slider.maxValue = maxHealth;
    }

    // Start is called before the first frame update
    public void setHealthBar(float health){
        slider.value = health;
    }
}
