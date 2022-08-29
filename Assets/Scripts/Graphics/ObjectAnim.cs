using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectAnim : MonoBehaviour
{
    // Rotation speed (degrees/sec)
    [SerializeField] private int spinSpeed = 30;
    [SerializeField] private float frequency = 1f;    //movement speed
    [SerializeField] private float amplitude = 1f;    //movement amount
    [SerializeField] private bool turn = false;
    Vector3 startPos;
    private float elapsedTime = 0f;
    // Use this for initialization
    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        elapsedTime += Time.deltaTime * Time.timeScale * frequency;
        transform.position = startPos + Vector3.up * Mathf.Sin(elapsedTime) * amplitude;
        if (turn == true)
        {
            transform.Rotate(0, spinSpeed * Time.deltaTime, 0, Space.World);
        }
    }
}
