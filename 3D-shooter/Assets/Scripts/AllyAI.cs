using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))] //авт. доб. компонент 
public class AllyAI : MonoBehaviour {
    [Header("Настройки ИИ")]
    [SerializeField] private float _attackRange = 25f;
    [SerializeField] private float _fireRate = 0.3f;
    [SerializeField] private int _damage = 12;

    [Header("Звук выстрела союзника")]
    [SerializeField] private AudioClip _allyShootSound;

    private NavMeshAgent _agent;
    private Transform _targetEnemy;
    private AudioSource _audioSource;
    private float _nextFireTime;
    private bool _isDead = false;

    private void Start() {
        _agent = GetComponent<NavMeshAgent>();
        _audioSource = GetComponent<AudioSource>();

        if (_audioSource != null) {
            _audioSource.spatialBlend = 1.0f; // Делаем звук объемным (3D)
            _audioSource.maxDistance = 30f;
            _audioSource.volume = 0.15f;
            _audioSource.playOnAwake = false;
        }

        if (_agent != null) {
            _agent.speed = Random.Range(6.5f, 8.5f); // Рандомная скорость, чтобы не бежали синхронно
            _agent.stoppingDistance = 5f;
        }
        StartCoroutine(LogicLoop()); // Запускаем цикл мышления ИИ
    }

    public void DisableAI() {   // Отключает ИИ при смерти
        _isDead = true; 
        if (_agent != null && _agent.enabled && _agent.isOnNavMesh) _agent.isStopped = true;
    }

    private IEnumerator LogicLoop() {
        yield return new WaitForSeconds(0.5f); // Задержка при старте

        while (!_isDead) { 
            FindClosestEnemy(); // Ищем ближайшего врага

            if (_targetEnemy == null) {
                yield return new WaitForSeconds(0.3f); // Если врагов нет, ждем
                continue;
            }

            float distance = Vector3.Distance(transform.position, _targetEnemy.position); //раст между вр и тим

            if (_agent.enabled && _agent.isOnNavMesh) {
                _agent.isStopped = false;
                // Рандомный сдвиг точки назначения, чтобы союзники не слипались в кучу
                Vector3 offsetPoint = _targetEnemy.position + transform.right * Random.Range(-4f, 4f);
                _agent.SetDestination(offsetPoint);
            }

            if (distance <= _attackRange) {  // Если враг в зоне видимости 
                Vector3 dir = (_targetEnemy.position - transform.position).normalized;
                dir.y = 0; // смотреть только влево вправо
                if (dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir); // Поворот к цели

                if (Time.time >= _nextFireTime) {   // Проверка кулдауна стрельбы
                    Shoot();
                    _nextFireTime = Time.time + _fireRate; // Устанавливаем время следующего выстрела
                }
            }

            yield return new WaitForSeconds(0.1f); // Оптимизация: ИИ "думает" 10 раз в секунду
        }
    }

    private void FindClosestEnemy() {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy"); // Берем всех врагов со сцены
        float closestDist = Mathf.Infinity;//хранения самого маленького расстояния
        Transform closestTrans = null;

        foreach (var enemy in enemies) { // цикл: перебираем каждого врага из массива по очереди.
            if (enemy == null) continue;
            NavMeshAgent enemyAgent = enemy.GetComponent<NavMeshAgent>(); //жив ли враг (у мертвых он выключен)
            if (enemyAgent == null || !enemyAgent.enabled) continue;

            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < closestDist) {    // Если этот враг ближе всех найденных ранее
                closestDist = dist;
                closestTrans = enemy.transform;
            }
        }
        _targetEnemy = closestTrans; // Сохраняем лучшую цель
    }

    private void Shoot() {
        if (_targetEnemy == null) return;  //если цели нет (враг умер, пока мы к нему бежали),то выходим

        if (_audioSource != null && _allyShootSound != null) {
            _audioSource.PlayOneShot(_allyShootSound); // Звук выстрела
        }

        RaycastHit hit;
        Vector3 start = transform.position + Vector3.up * 1.5f; // Начинаем луч с уровня "глаз"
        Vector3 direction = (_targetEnemy.position - start).normalized;

        if (Physics.Raycast(start, direction, out hit, _attackRange)) { // Пускаем невидимый луч
            if (hit.collider.CompareTag("Enemy")) {
                hit.collider.GetComponent<EnemyHealth>()?.TakeDamage(_damage); // Наносим урон врагу
            }
        }
    }
}