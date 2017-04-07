using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
    public bool Colliding { get; set; }
    
    void OnCollisionEnter(Collision c)
    {
        if (c.gameObject.tag == "Ground") return;

        Colliding = true;
    }

    void OnCollisionExit(Collision c)
    {
        Colliding = false;
    }
}
