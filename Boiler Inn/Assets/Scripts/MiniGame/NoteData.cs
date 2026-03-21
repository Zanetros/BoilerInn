using UnityEngine;

public class NoteData : MonoBehaviour
{
    public Vector3 direction;
    public string color;
    
    // Escondido no Inspector porque quem preenche isso agora é o Spawner via código!
    [HideInInspector] public float currentSpeed; 
    
    private HitBar hitBarReference; 

    void Start()
    {
        // A única coisa que a nota ainda procura é a barra para poder avisar dos erros
        hitBarReference = Object.FindFirstObjectByType<HitBar>();
    }
    void Update()
    {
        // Apenas cai, usando a velocidade que o Spawner deu pra ela!
        transform.Translate(direction * currentSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("Out"))
        {
            if (hitBarReference != null)
            {
                hitBarReference.RegisterMissedNote(this);
            }
            
            Destroy(gameObject);
        }
    }
}