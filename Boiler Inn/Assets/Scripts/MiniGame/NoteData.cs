using UnityEngine;

public class NoteData : MonoBehaviour
{
    public float speed;
    public Vector3 direction;
    public string color;

    void Start()
    {
        // Faz o objeto ser destruído automaticamente após 5 segundos
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other) // Ajustado para 2D, já que os scripts anteriores eram 2D
    {
        if (other.CompareTag("Out"))
        {
            Destroy(gameObject);
        }
    }
}