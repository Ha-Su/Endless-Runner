using UnityEngine;

public class SectionTrigger : MonoBehaviour
{
    public GameObject roadSection;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Trigger")){
            Instantiate(roadSection, new Vector3((float)21.4, 0,0), Quaternion.identity);
        }
    }
}
