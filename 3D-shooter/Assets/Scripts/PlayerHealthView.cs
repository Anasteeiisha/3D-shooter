using UnityEngine;
using TMPro;

public class PlayerHealthView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _healthText; 

    public void UpdateHealthDisplay(int currentHealth)
    {
        _healthText.text = $"HP: {currentHealth}";
    }
}