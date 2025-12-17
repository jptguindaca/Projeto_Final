using TMPro;
using UnityEngine;

public class Interact : MonoBehaviour
{
    private bool inReach;

    void Start()
    {
        inReach = false;
    }

    void Update()
    {
        PickUp();
    }

    void PickUp()
    {
        if (Input.GetKey(KeyCode.E) && inReach == true)
        {
            this.gameObject.SetActive(false);
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Reach")
        {
            inReach = true;
            Debug.Log("In Reach");
        }
    }

    void OnTriggerExit(Collider other)
    {
        inReach = false;
        Debug.Log("not reach");
    }
}
