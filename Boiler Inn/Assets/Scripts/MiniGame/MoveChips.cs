using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MoveChips : MonoBehaviour
{
    public float speed;
    public Vector3 direction;
    public string color;
    public InputActionReference chip;
    private bool insideHit = false;

    private void OnEnable()
    {
        chip.action.started += ScoreChip;
    }

    private void OnDisable()
    {
        chip.action.started -= ScoreChip;
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Out"))
        {
            Destroy(gameObject);
        }

        if (other.CompareTag("Hit"))
        {
            insideHit = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Hit"))
        {
            insideHit = false;
        }
    }

    public void ScoreChip(InputAction.CallbackContext obj)
    {
        if (insideHit && (color == "red" || color == "blue" || color == "yellow"))
        {
            Destroy(gameObject);
        }
    }
}
