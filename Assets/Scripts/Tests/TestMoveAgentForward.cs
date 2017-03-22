using UnityEngine;

public class TestMoveAgentForward : MonoBehaviour
{
    void Update()
    {
        transform.position += transform.forward * Time.deltaTime;
    }
}
