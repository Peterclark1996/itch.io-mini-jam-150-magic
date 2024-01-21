using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class ScorePopupObject : MonoBehaviour
{
    public Rigidbody2D rigidBody;
    public TextMeshPro textMesh;

    public void Init(int scoreValue)
    {
        textMesh.text = scoreValue.ToString();

        textMesh.color = scoreValue > 50 ? new Color(0.7f, 0.1f, 1f) : new Color(0.1f, 1f, 0.3f);

        rigidBody.AddForce(transform.up * Random.Range(3.5f, 6.5f), ForceMode2D.Impulse);
        rigidBody.AddForce(new Vector2(Random.Range(-1.5f, 1.5f), 0), ForceMode2D.Impulse);
    }

    private void Update()
    {
        if (transform.position.y > Constants.Instance.offScreenVerticalPosition) return;

        Destroy(gameObject);
    }
}