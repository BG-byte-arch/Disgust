using UnityEngine;
using TMPro;

public class DamageIndicator : MonoBehaviour
{
    public float floatSpeed = 2f;        // Speed for floating text
    public float lifetime = 1f;          // Duration before it disappears
    public TextMeshProUGUI damageText;   // Reference to TextMeshPro component

    private Color textColor;

    private void Start()
    {
        textColor = damageText.color;
        Destroy(gameObject, lifetime); // Destroy after lifetime
    }

    private void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // Fade out text over time
        textColor.a = Mathf.Lerp(1f, 0f, Time.timeSinceLevelLoad / lifetime);
        damageText.color = textColor;
    }

    public void SetDamageText(int damage)
    {
        damageText.text = damage.ToString();
    }
}
