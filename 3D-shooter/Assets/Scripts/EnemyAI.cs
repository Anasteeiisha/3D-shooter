using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour {
    [Header("Настройки ИИ")]
    [SerializeField] private float _attackRange = 25f;
    [SerializeField] private float _fireRate = 0.3f;
    [SerializeField] private int _damage = 10;

    [Header("Звук выстрела бота")]
    [SerializeField] private AudioClip _shootSound;

    private NavMeshAgent _agent;
    private Transform _target;
    private AudioSource _audioSource;
    private float _nextFireTime;
    private bool _isDead = false;

    private void Start() {
        _agent = GetComponent<NavMeshAgent>();
        _audioSource = GetComponent<AudioSource>(); // Находим аудиоисточник

        if (_audioSource != null) {
            _audioSource.spatialBlend = 1.0f; // Делаем звук объемным 3D
            _audioSource.maxDistance = 30f; // Дистанция, на которой слышно
            _audioSource.volume = 0.15f; 
            _audioSource.playOnAwake = false;
        }

        if (_agent != null) {
            _agent.speed = Random.Range(6.5f, 8.5f);
            _agent.stoppingDistance = 5f;
        }
        StartCoroutine(LogicLoop()); // Запускаем цикл мышления ИИ
    }

    public void DisableAI() {     // Отключает ИИ при смерти
        _isDead = true;
        if (_agent != null && _agent.enabled && _agent.isOnNavMesh) _agent.isStopped = true;
    }

    // Главный цикл ИИ: поиск цели, движение и атака
    private IEnumerator LogicLoop() {
        yield return new WaitForSeconds(0.5f); // Даем игре полсекунды на запуск без стрельбы

        while (!_isDead) {
            FindClosestTarget(); // Ищем ближайшую цель (игрока или союзника)

            if (_target == null)  {
                yield return new WaitForSeconds(0.3f);
                continue; // Если цели нет, ждем и ищем снова
            }

            float distance = Vector3.Distance(transform.position, _target.position); // Считаем дистанцию до цели

            if (_agent.enabled && _agent.isOnNavMesh)  {
                _agent.isStopped = false;
                Vector3 offsetPoint = _target.position + transform.right * Random.Range(-4f, 4f);//чтобы не слипались
                _agent.SetDestination(offsetPoint); // Идем к цели
            }

            if (distance <= _attackRange) { // Если цель в зоне поражения 
                LookAtTarget(); // Поворачиваемся к цели

                if (Time.time >= _nextFireTime) { // Проверка кулдауна
                    Shoot(); // Стреляем
                    _nextFireTime = Time.time + _fireRate; // Ставим время следующего выстрела
                }
            }

            yield return new WaitForSeconds(0.1f); // Оптимизация: думаем 10 раз в секунду
        }
    }

    // Поворачивает врага лицом к цели (только по горизонтали)
    private void LookAtTarget() {
        if (_target == null) return;
        Vector3 dir = (_target.position - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);
    }

    // Ищет ближайшую цель среди союзников (Ally) и игрока (Player)
    private void FindClosestTarget() {
        GameObject[] allies = GameObject.FindGameObjectsWithTag("Ally"); // Берем всех союзников
        GameObject player = GameObject.FindWithTag("Player"); // Берем игрока

        float closestDist = Mathf.Infinity;
        Transform closestTrans = null;

        // Сначала проверяем игрока
        if (player != null) {
            closestDist = Vector3.Distance(transform.position, player.transform.position);
            closestTrans = player.transform;
        }

        // Затем перебираем союзников, ищем кто ближе
        foreach (var ally in allies)  {
            if (ally == null) continue;
            NavMeshAgent allyAgent = ally.GetComponent<NavMeshAgent>();
            if (allyAgent == null || !allyAgent.enabled) continue; // Пропускаем мертвых

            float dist = Vector3.Distance(transform.position, ally.transform.position);
            if (dist < closestDist) {  // Если союзник ближе игрока или предыдущего союзника
                closestDist = dist;
                closestTrans = ally.transform;
            }
        }
        _target = closestTrans; // Сохраняем лучшую цель
    }
    private void Shoot() {      // Стреляет по цели: звук + Raycast для нанесения урона
        if (_target == null) return;

        if (_audioSource != null && _shootSound != null) {
            _audioSource.PlayOneShot(_shootSound); // Играем звук выстрела
        }

        RaycastHit hit;
        Vector3 start = transform.position + Vector3.up * 1.5f; // Луч
        Vector3 direction = (_target.position - start).normalized;

        if (Physics.Raycast(start, direction, out hit, _attackRange)) { // Пускаем луч
            if (hit.collider.CompareTag("Player")) { // Если попали в игрока
                hit.collider.GetComponentInParent<PlayerHealthModel>()?.TakeDamage(_damage); // Наносим урон игроку
            }
            else if (hit.collider.CompareTag("Ally")) { // Если попали в союзника
                hit.collider.GetComponent<AllyHealth>()?.TakeDamage(_damage); // Наносим урон союзнику
            }
        }
    }
}