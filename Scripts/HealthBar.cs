﻿using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts {
    public class HealthBar : MonoBehaviour {
        [SerializeField] private Slider m_slider; // Wrong naming ('m_'), private field must start with "_"

        public void SetMaxHealth(int health) {
            m_slider.maxValue = health;
            m_slider.value = health;
        }

        public void SetHealth(int healthValue) {
            m_slider.value = healthValue;
        }
    }
}
