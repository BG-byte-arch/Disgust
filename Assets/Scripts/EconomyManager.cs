using UnityEngine;
using TMPro;

public class EconomyManager : MonoBehaviour
{
    public TextMeshProUGUI foodAmmountText;
    public int food = 0;
    private float nextTime = 0f;
    private float interval = 0.25f;

    void Update()
    {
        if (Time.time >= nextTime)
        {
            food++;
            foodAmmountText.text = food + "/10000";
            nextTime = Time.time + interval;
        }
    }
}
