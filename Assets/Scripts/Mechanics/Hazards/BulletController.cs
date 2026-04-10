using UnityEngine;

public class BulletController : MonoBehaviour
{
    [SerializeField] public float speed;

    private void Update()
    {
        transform.Translate(Vector3.up*Time.deltaTime*speed);
    }

    public void end()
    {
        Destroy(gameObject);
    }

}
