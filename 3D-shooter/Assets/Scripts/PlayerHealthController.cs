using UnityEngine;

public class PlayerHealthController : MonoBehaviour
{
    [Header("MVC Refs")]
    [SerializeField] private PlayerHealthModel _model;
    [SerializeField] private PlayerHealthView _view;

    private void Start()
    {
        _model.OnHealthChanged += _view.UpdateHealthDisplay;
        _view.UpdateHealthDisplay(_model.CurrentHealth); 
    }

    private void OnDestroy()
    {
        _model.OnHealthChanged -= _view.UpdateHealthDisplay;
    }
}