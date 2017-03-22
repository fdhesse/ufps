using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
    public bool Colliding { get; set; }

    void OnTriggerEnter(Collider c)
    {
        Colliding = true;
    }

    void OnTriggerStay(Collider c)
    {
        Colliding = true;
    }

    void OnTriggerExit(Collider c)
    {
        Colliding = false;
    }
}
