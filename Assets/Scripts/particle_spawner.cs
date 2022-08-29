using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class particle_spawner : MonoBehaviour
{
    [SerializeField] private float spawnRate = 0;
    [SerializeField] private float spawnNumber = 0;
    [SerializeField] private GameObject spawnEmitter = null;
    [SerializeField] private ParticleSystem particlePrefab = null;
    private List<ParticleSystem> particleList = new List<ParticleSystem>();

    [Range(0f, 10f)]
    [SerializeField] private float spawnXsize = 1f;

    [Range(0f, 10f)]
    [SerializeField] private float spawnYsize = 1f;

    [Range(0f, 10f)]
    [SerializeField] private float spawnZsize = 1f;

    [SerializeField] private bool mvtX = false;
    [SerializeField] private bool mvtY = false;
    [SerializeField] private bool mvtZ = false;

    private float emitterPosX;
    private float emitterPosY;
    private float emitterPosZ;

    private float emitterScaleX;
    private float emitterScaleY;
    private float emitterScaleZ;

    // Start is called before the first frame update
    void Start()
    {
        emitterPosX = spawnEmitter.transform.position.x;
        emitterPosY = spawnEmitter.transform.position.y;
        emitterPosZ = spawnEmitter.transform.position.z;

        emitterScaleX = spawnEmitter.transform.localScale.x;
        emitterScaleY= spawnEmitter.transform.localScale.y;
        emitterScaleZ = spawnEmitter.transform.localScale.z;

        spawnParticle();
        
    }

    private void spawnParticle()
    {
        for (int i = 0; i < spawnNumber; i++)
        {
            Vector3 randSpawn = new Vector3(emitterPosX + Random.Range(-emitterScaleX * spawnXsize, emitterScaleX * spawnXsize), emitterPosY + Random.Range(-emitterScaleZ * spawnYsize, emitterScaleZ * spawnYsize), emitterPosZ + Random.Range(-emitterScaleZ * spawnZsize, emitterScaleZ * spawnZsize));
            ParticleSystem obj = GameObject.Instantiate(particlePrefab, randSpawn, Quaternion.identity, gameObject.transform);
            particleList.Add(obj);
        }
        StartCoroutine(wait());
    }

    private IEnumerator wait()
    {
        yield return new WaitForSecondsRealtime(spawnRate);
        spawnParticle();
    }

    // Update is called once per frame
    void Update()
    {
        if (mvtX == true)
        {
            foreach (ParticleSystem particle in particleList)
            {
                particle.transform.position = new Vector3 (particle.transform.position.x - 0.05f, particle.transform.position.y, particle.transform.position.z);
            }
        }

        if (mvtY == true)
        {
            foreach (ParticleSystem particle in particleList)
            {
                particle.transform.position = new Vector3(particle.transform.position.x, particle.transform.position.y - 0.05f, particle.transform.position.z);
            }
        }

        if (mvtZ == true)
        {
            foreach (ParticleSystem particle in particleList)
            {
                particle.transform.position = new Vector3(particle.transform.position.x, particle.transform.position.y, particle.transform.position.z - 0.05f);
            }
        }
    }
}
