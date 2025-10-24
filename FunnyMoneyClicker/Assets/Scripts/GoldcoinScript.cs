using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class GoldcoinScript : MonoBehaviour
{
    public float throwForce = 10f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Rigidbody2D rb = this.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            // Generate a random direction within a unit circle. ik u will want to change this
            Vector2 randomDirection = UnityEngine.Random.insideUnitCircle.normalized;

            // Apply force in the random direction.... use linearVolicity if it goes crazy
            rb.AddForce(randomDirection * throwForce, ForceMode2D.Impulse);
        }
    }
    public float timer = 0f;
    public float endtime = 5f;

    // Update is called once per frame
    void Update()
    {
        if (timer < endtime)
        {
            timer += Time.deltaTime;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void OnMouseEnter()
    {

        SaveDataController.currentData.moneyCount = SaveDataController.currentData.moneyCount + 100f ;
        Destroy(this.gameObject);
    }
}
