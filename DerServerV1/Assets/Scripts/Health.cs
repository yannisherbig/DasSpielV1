using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Health : MonoBehaviour
{

    public const int maxHealth = 100;
    public int currentHealth = maxHealth;
    public RectTransform healthBar;
    public ParticleSystem deathEffect;

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            gameObject.SetActive(false);
            Destroy(Instantiate(deathEffect.gameObject, gameObject.transform.position, Quaternion.identity) as GameObject , deathEffect.main.startLifetime.constant);
        }

        healthBar.sizeDelta = new Vector2(currentHealth, healthBar.sizeDelta.y);
    }
}