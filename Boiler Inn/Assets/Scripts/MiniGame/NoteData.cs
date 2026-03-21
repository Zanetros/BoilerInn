using UnityEngine;

public class NoteData : MonoBehaviour
{
    public Vector3 direction;
    public string color;
    
    private HitBar hitBarReference; 
    
    // Variável local para guardar a velocidade
    private float mySpeed = 5f; 

    void Start()
    {
        hitBarReference = Object.FindFirstObjectByType<HitBar>();
        
        // CACHING: Pergunta a velocidade pro Manager APENAS UMA VEZ no nascimento!
        if (MiniGameManager.instance != null)
        {
            mySpeed = MiniGameManager.instance.currentNoteSpeed;
        }
    }

    void Update()
    {
        // O Update agora faz apenas matemática básica, zero peso para a CPU!
        transform.Translate(direction * mySpeed * Time.deltaTime);
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