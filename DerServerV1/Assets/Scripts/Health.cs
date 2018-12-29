using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Health : MonoBehaviour
{
    public const int maxHealth = 100;
    public int currentHealth = maxHealth;
    public RectTransform healthBar;
    public ParticleSystem deathEffect;
    //float maxHealthBarSize;
    public Text highScoreText;
    public GameObject serverScript;

    private void Start()
    {
        //maxHealthBarSize = healthBar.sizeDelta.x;
    }
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            gameObject.SetActive(false);
            int score = gameObject.GetComponent<PlayerScript>().score;
            Destroy(Instantiate(deathEffect.gameObject, gameObject.transform.position, Quaternion.identity) as GameObject , deathEffect.main.startLifetime.constant);
            if(score > serverScript.GetComponent<ServerScript>().highScore)
                highScoreText.text = "<b>Highscore</b>\n<size=50>" + gameObject.GetComponent<PlayerScript>().nameTag.GetComponent<TextMesh>().text + " (" + score + "p)</size>";
            gameObject.GetComponent<PlayerScript>().score = 0;
        }
        healthBar.sizeDelta = new Vector2(currentHealth, healthBar.sizeDelta.y);
        Debug.Log("healthBar.sizeDelta: " + healthBar.sizeDelta + " ; healthBar.sizeDelta.x: " + healthBar.sizeDelta.x + " ; healthBar.sizeDelta.y: " + healthBar.sizeDelta.y);
        //healthBar.sizeDelta = new Vector2(((float)currentHealth / maxHeath) * maxHealthBarSize, healthBar.sizeDelta.y);
    }
}