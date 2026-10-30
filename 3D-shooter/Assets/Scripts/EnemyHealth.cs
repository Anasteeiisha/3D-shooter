using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int _health = 100;
    private int _currentHealth;
    private Renderer _renderer;
    private bool _isDead = false;

    public int CurrentHealth => _currentHealth;

    private void Awake()
    {
        _currentHealth = _health;
        _renderer = GetComponent<Renderer>();
    }

    public void TakeDamage(int damage)
    {
        if (_isDead) return;

        _currentHealth -= damage;
        
        if (_renderer != null) StartCoroutine(HitFlashCoroutine());

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator HitFlashCoroutine()
    {
        if (_renderer == null) yield break;
        string colorPropertyName = _renderer.material.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";
        
        Color oldColor = _renderer.material.GetColor(colorPropertyName);
        _renderer.material.SetColor(colorPropertyName, Color.red);
        yield return new WaitForSeconds(0.1f);
        if (!_isDead && _renderer != null) _renderer.material.SetColor(colorPropertyName, oldColor);
    }

    private void Die()
    {
        if (_isDead) return; 
        _isDead = true;

        if (TryGetComponent<EnemyAI>(out var ai)) ai.DisableAI();
        if (TryGetComponent<NavMeshAgent>(out var agent)) agent.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.None;
            rb.AddForce(-transform.forward * 4f + Vector3.up * 2f, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
        }

        // ВОЗВРАЩАЕМ НАЗАД: Простое и четкое логирование
        KillfeedManager.Instance?.AddLog("Игрок", gameObject.name, Color.cyan, Color.red);
        MatchManager.Instance?.RegisterKill(true);

        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.SpawnEnemy(gameObject.name);
        }

        Destroy(gameObject, 3f);
    }
}