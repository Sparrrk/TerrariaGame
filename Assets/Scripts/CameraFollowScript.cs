using UnityEngine;

public class CameraFollowScript : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] float cameraSpeed;

    private float fixedZ;

    // Start is called before the first frame update
    void Awake()
    {
        fixedZ = transform.position.z;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (player != null)
        {
            Vector2 destination = player.position;
            Vector3 nextPosition = Vector3.Lerp(transform.position, new Vector3(destination.x, destination.y, fixedZ), cameraSpeed);
            transform.position = nextPosition;
        }
    }
}
