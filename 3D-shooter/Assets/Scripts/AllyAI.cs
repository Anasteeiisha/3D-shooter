using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AllyAI : MonoBehaviour
{
    [Header("Настройки ИИ")]
    [SerializeField] private float _attackRange = 25f; 
    [SerializeField] private float _fireRate = 0.3f;   
    [SerializeField] private int _damage = 12;         

    [Header("Звук выстрела союзника")]
    [SerializeField] private AudioClip _allyShootSound; 

    private NavMeshAgent _agent;
    private Transform _targetEnemy;
    private AudioSource _audioSource; // Ссылка на встроенный звук
    private float _nextFireTime;
    private bool _isDead = false;

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _audioSource = GetComponent<AudioSource>();

        if (_audioSource != null)
        {
            _audioSource.spatialBlend = 1.0f; 
            _audioSource.maxDistance = 30f;   
            _audioSource.volume = 0.15f;      // Выстрелы союзников тоже тихие
            _audioSource.playOnAwake = false;
        }

        if (_agent != null)
        {
            _agent.speed = Random.Range(6.5f, 8.5f);
            _agent.stoppingDistance = 5f; 
        }
        StartCoroutine(LogicLoop());
    }

    public void DisableAI()
    {
        _isDead = true;
        if (_agent != null && _agent.enabled && _agent.isOnNavMesh) _agent.isStopped = true;
    }

    private IEnumerator LogicLoop()
    {
        yield return new WaitForSeconds(0.5f);

        while (!_isDead)
        {
            FindClosestEnemy();

            if (_targetEnemy == null)
            {
                yield return new WaitForSeconds(0.3f);
                continue;
            }

            float distance = Vector3.Distance(transform.position, _targetEnemy.position);

            if (_agent.enabled && _agent.isOnNavMesh)
            {
                _agent.isStopped = false;
                Vector3 offsetPoint = _targetEnemy.position + transform.right * Random.Range(-4f, 4f);
                _agent.SetDestination(offsetPoint);
            }

            if (distance <= _attackRange)
            {
                Vector3 dir = (_targetEnemy.position - transform.position).normalized;
                dir.y = 0;
                if (dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);

                if (Time.time >= _nextFireTime)
                {
                    Shoot();
                    _nextFireTime = Time.time + _fireRate;
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private void FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float closestDist = Mathf.Infinity;
        Transform closestTrans = null;

        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;
            NavMeshAgent enemyAgent = enemy.GetComponent<NavMeshAgent>();
            if (enemyAgent == null || !enemyAgent.enabled) continue;

            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestTrans = enemy.transform;
            }
        }
        _targetEnemy = closestTrans;
    }

    private void Shoot()
    {
        if (_targetEnemy == null) return;

        if (_audioSource != null && _allyShootSound != null)
        {
            _audioSource.PlayOneShot(_allyShootSound);
        }

        RaycastHit hit;
        Vector3 start = transform.position + Vector3.up * 1.5f;
        Vector3 direction = (_targetEnemy.position - start).normalized;

        if (Physics.Raycast(start, direction, out hit, _attackRange))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                hit.collider.GetComponent<EnemyHealth>()?.TakeDamage(_damage);
            }
        }
    }
}