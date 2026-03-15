using UnityEngine;

public class NoteData : MonoBehaviour
{
    public float speed;
    public Vector3 direction;
    public string color;

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Out"))
        {
            Destroy(gameObject);
        }
    }
}
