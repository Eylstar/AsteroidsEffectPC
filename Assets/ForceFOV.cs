using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceFOV : MonoBehaviour
{
    void Update()
    {
        this.GetComponent<Camera>().fieldOfView = 60;
    }
}
