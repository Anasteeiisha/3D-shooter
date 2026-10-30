using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Настройки ИИ")]
    [SerializeField] private float _attackRange = 25f; 
    [SerializeField] private float _fireRate = 0.3f;   
    [SerializeField] private int _damage = 10;         

    [Header("Звук выстрела бота")]
    [SerializeField] private AudioClip _shootSound; 

    private NavMeshAgent _agent;
    private Transform _target; 
    private AudioSource _audioSource; // Ссылка на встроенный звук
    private float _nextFireTime;
    private bool _isDead = false;

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _audioSource = GetComponent<AudioSource>(); // Находим аудиоресурс на боте

        // Настройка 3D звука, чтобы он затихал на расстоянии
        if (_audioSource != null)
        {
            _audioSource.spatialBlend = 1.0f; // Делаем звук полностью 3D
            _audioSource.maxDistance = 30f;   // Дистанция, на которой слышно врага
            _audioSource.volume = 0.15f;      // Сразу делаем выстрелы тихими
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
        yield return new WaitForSeconds(0.5f); // Даем игре полсекунды на запуск без стрельбы

        while (!_isDead)
        {
            FindClosestTarget();

            if (_target == null)
            {
                yield return new WaitForSeconds(0.3f);
                continue;
            }

            float distance = Vector3.Distance(transform.position, _target.position);

            if (_agent.enabled && _agent.isOnNavMesh)
            {
                _agent.isStopped = false;
                Vector3 offsetPoint = _target.position + transform.right * Random.Range(-4f, 4f);
                _agent.SetDestination(offsetPoint);
            }

            if (distance <= _attackRange)
            {
                LookAtTarget();

                if (Time.time >= _nextFireTime)
                {
                    Shoot();
                    _nextFireTime = Time.time + _fireRate;
                }
            }

            yield return new WaitForSeconds(0.1f); 
        }
    }

    private void LookAtTarget()
    {
        if (_target == null) return;
        Vector3 dir = (_target.position - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);
    }

    private void FindClosestTarget()
    {
        GameObject[] allies = GameObject.FindGameObjectsWithTag("Ally");
        GameObject player = GameObject.FindWithTag("Player");

        float closestDist = Mathf.Infinity;
        Transform closestTrans = null;

        if (player != null)
        {
            closestDist = Vector3.Distance(transform.position, player.transform.position);
            closestTrans = player.transform;
        }

        foreach (var ally in allies)
        {
            if (ally == null) continue;
            NavMeshAgent allyAgent = ally.GetComponent<NavMeshAgent>();
            if (allyAgent == null || !allyAgent.enabled) continue;

            float dist = Vector3.Distance(transform.position, ally.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestTrans = ally.transform;
            }
        }
        _target = closestTrans;
    }

    private void Shoot()
    {
        if (_target == null) return;

        // ИГРАЕМ ЗВУК БЕЗОПАСНО: Внутри самого бота без спавна клонов на сцене!
        if (_audioSource != null && _shootSound != null)
        {
            _audioSource.PlayOneShot(_shootSound);
        }

        RaycastHit hit;
        Vector3 start = transform.position + Vector3.up * 1.5f;
        Vector3 direction = (_target.position - start).normalized;

        if (Physics.Raycast(start, direction, out hit, _attackRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                hit.collider.GetComponentInParent<PlayerHealthModel>()?.TakeDamage(_damage);
            }
            else if (hit.collider.CompareTag("Ally"))
            {
                hit.collider.GetComponent<AllyHealth>()?.TakeDamage(_damage);
            }
        }
    }
}