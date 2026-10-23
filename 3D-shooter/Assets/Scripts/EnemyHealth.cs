using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int _health = 100;

    public int CurrentHealth => _health;

    public void TakeDamage(int damage)
    {
        _health -= damage;
        Debug.Log($"[Враг] Получил урон: {damage}. Осталось здоровья: {_health}");

        if (_health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("[Враг] Погиб!");
        Destroy(gameObject); // Удаляем врага со сцены
    }
}