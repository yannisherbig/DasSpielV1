using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Health2 : MonoBehaviour
{
    public const int maxHealth = 100;
    public int currentHealth;
    public RectTransform healthBar;
    public ParticleSystem deathEffect;
    //float maxHealthBarSize;
    public Text highScoreText;
    public GameObject serverScript;

    private void Start()
    {
        currentHealth = maxHealth;
        //maxHealthBarSize = healthBar.sizeDelta.x;
    }

    public void TakeDamage(int amount, string hitCameFromPlayerIP)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Destroy(Instantiate(deathEffect.gameObject, gameObject.transform.position, Quaternion.identity) as GameObject, deathEffect.main.startLifetime.constant);
            currentHealth = 0;
            foreach (var mr in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                StartCoroutine(FadeTo(mr.material, 0f, 2f)); // Start a coroutine to fade the material to zero alpha over 2 seconds and disable the GameObject
            }
            StartCoroutine(SetInactive(gameObject, 2.01f));
            serverScript.GetComponent<ServerScript2>().players[hitCameFromPlayerIP].PlayerObject.GetComponent<PlayerScript2>().score++;
        }
        healthBar.sizeDelta = new Vector2(currentHealth, healthBar.sizeDelta.y);
        Debug.Log("healthBar.sizeDelta: " + healthBar.sizeDelta + " ; healthBar.sizeDelta.x: " + healthBar.sizeDelta.x + " ; healthBar.sizeDelta.y: " + healthBar.sizeDelta.y);
        //healthBar.sizeDelta = new Vector2(((float)currentHealth / maxHeath) * maxHealthBarSize, healthBar.sizeDelta.y);
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Destroy(Instantiate(deathEffect.gameObject, gameObject.transform.position, Quaternion.identity) as GameObject, deathEffect.main.startLifetime.constant);
            currentHealth = 0;
            foreach (var mr in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                StartCoroutine(FadeTo(mr.material, 0f, 2f)); // Start a coroutine to fade the material to zero alpha over 2 seconds and disable the GameObject
            }
            StartCoroutine(SetInactive(gameObject, 2.01f));
        }
        healthBar.sizeDelta = new Vector2(currentHealth, healthBar.sizeDelta.y);
        Debug.Log("healthBar.sizeDelta: " + healthBar.sizeDelta + " ; healthBar.sizeDelta.x: " + healthBar.sizeDelta.x + " ; healthBar.sizeDelta.y: " + healthBar.sizeDelta.y);
        //healthBar.sizeDelta = new Vector2(((float)currentHealth / maxHeath) * maxHealthBarSize, healthBar.sizeDelta.y);
    }

    IEnumerator SetInactive(GameObject go, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        go.SetActive(false);
    }

    // Define an enumerator to perform our fading.
    // Pass it the material to fade, the opacity to fade to (0 = transparent, 1 = opaque),
    // and the number of seconds to fade over.
    public IEnumerator FadeTo(Material material, float targetOpacity, float duration)
    {
        // Cache the current color of the material, and its initiql opacity.
        Color color = material.color;
        float startOpacity = color.a;

        // Track how many seconds we've been fading.
        float t = 0;

        while (t < duration)
        {
            // Step the fade forward one frame.
            t += Time.deltaTime;
            // Turn the time into an interpolation factor between 0 and 1.
            float blend = Mathf.Clamp01(t / duration);

            // Blend to the corresponding opacity between start & target.
            color.a = Mathf.Lerp(startOpacity, targetOpacity, blend);

            // Apply the resulting color to the material.
            material.color = color;

            // Wait one frame, and repeat.
            yield return null;
        }

    }
}