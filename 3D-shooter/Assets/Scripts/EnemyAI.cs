using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Настройки ИИ")]
    [SerializeField] private float _attackRange = 15f;    // Дистанция, с которой враг начинает стрелять
    [SerializeField] private float _fireRate = 1.0f;       // Перерыв между выстрелами (в секундах)
    [SerializeField] private int _damage = 10;             // Урон игроку за один выстрел

    [Header("Укрытия")]
    [SerializeField] private float _hideHealthThreshold = 30f; // Если здоровье врага упадет ниже 30, он побежит прятаться

    private NavMeshAgent _agent;
    private Transform _playerTransform;
    private PlayerHealthModel _playerHealth;
    private EnemyHealth _enemyHealth;
    private float _nextFireTime;
    private bool _isHiding = false;

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _enemyHealth = GetComponent<EnemyHealth>();
        
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
            _playerHealth = player.GetComponent<PlayerHealthModel>();
        }
    }

    private void Update()
    {
        if (_playerTransform == null) return;

        // Проверяем здоровье врага. Если оно низкое — включаем режим паники и ищем укрытие
        if (_enemyHealth != null && _enemyHealth.CurrentHealth <= _hideHealthThreshold && !_isHiding)
        {
            StartCoroutine(FindAndMoveToCover());
            return;
        }

        if (_isHiding) return; // Если уже прячется, обычную логику не выполняем

        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);

        if (distanceToPlayer <= _attackRange)
        {
            // СОСТОЯНИЕ: АТАКА
            _agent.isStopped = true; // Останавливаемся, чтобы прицелиться
            LookAtPlayer();

            if (Time.time >= _nextFireTime)
            {
                ShootAtPlayer();
                _nextFireTime = Time.time + _fireRate;
            }
        }
        else
        {
            // СОСТОЯНИЕ: ПРЕСЛЕДОВАНИЕ
            _agent.isStopped = false;
            _agent.SetDestination(_playerTransform.position);
        }
    }

    private void LookAtPlayer()
    {
        Vector3 direction = (_playerTransform.position - transform.position).normalized;
        direction.y = 0; // Чтобы враг не наклонялся неестественно вверх/вниз
        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void ShootAtPlayer()
    {
        Debug.Log("[Враг] Сделал выстрел в игрока!");

        // Проверяем, нет ли между врагом и игроком стены (контейнера)
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, _playerTransform.position - transform.position, out hit, _attackRange))
        {
            if (hit.collider.gameObject.name == "Player" && _playerHealth != null)
            {
                _playerHealth.TakeDamage(_damage); // Наносим урон игроку!
            }
            else
            {
                Debug.Log("[Враг] Пуля попала в контейнер/стену!");
            }
        }
    }

    // ЛОГИКА УМНОГО УКРЫТИЯ (Прятки за контейнерами)
    private System.Collections.IEnumerator FindAndMoveToCover()
    {
        _isHiding = true;
        _agent.isStopped = false;
        Debug.Log("[Враг] Мало здоровья! Ищу укрытие...");

        // Ищем все объекты на карте, которые мы пометили как Static (наши контейнеры)
        Collider[] obstacles = Physics.OverlapSphere(transform.position, 20f);
        Vector3 bestCoverSpot = transform.position;
        float bestDistance = 0f;

        foreach (var obstacle in obstacles)
        {
            if (obstacle.gameObject.isStatic && obstacle.gameObject.name != "Plane")
            {
                // Считаем позицию ЗА контейнером (напротив игрока)
                Vector3 hideDir = (obstacle.transform.position - _playerTransform.position).normalized;
                Vector3 hidePos = obstacle.transform.position + hideDir * 2f;

                // Выбираем самое дальнее и безопасное место от игрока
                float dist = Vector3.Distance(hidePos, _playerTransform.position);
                if (dist > bestDistance)
                {
                    bestDistance = dist;
                    bestCoverSpot = hidePos;
                }
            }
        }

        // Даем команду автопилоту бежать за контейнер
        _agent.SetDestination(bestCoverSpot);
        yield return new WaitForSeconds(4f); // Прячется 4 секунды, потом снова может атаковать
        _isHiding = false;
    }
}