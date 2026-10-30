using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AllyHealth : MonoBehaviour
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
        if (_renderer != null) StartCoroutine(FlashBlue());
        if (_currentHealth <= 0) Die();
    }

    private IEnumerator FlashBlue()
    {
        if (_renderer == null) yield break;
        string prop = _renderer.material.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";
        Color old = _renderer.material.GetColor(prop);
        _renderer.material.SetColor(prop, Color.cyan);
        yield return new WaitForSeconds(0.1f);
        if (!_isDead && _renderer != null) _renderer.material.SetColor(prop, old);
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        if (TryGetComponent<AllyAI>(out var ai)) ai.DisableAI();
        if (TryGetComponent<NavMeshAgent>(out var agent)) agent.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.None;
            rb.AddForce(-transform.forward * 4f + Vector3.up * 2f, ForceMode.Impulse);
        }

        // ВОЗВРАЩАЕМ НАЗАД: Строчка "Враг X -> Союзник Y"
        GameObject liveEnemy = GameObject.FindWithTag("Enemy");
        string killerName = liveEnemy != null ? liveEnemy.name : "Враг";
        KillfeedManager.Instance?.AddLog(killerName, gameObject.name, Color.red, Color.cyan);

        MatchManager.Instance?.RegisterKill(false);

        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.SpawnAlly(gameObject.name);
        }
        
        Destroy(gameObject, 3f);
    }
}