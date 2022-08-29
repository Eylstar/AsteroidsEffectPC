using System.Collections;
using UnityEngine;

public class AsteroidsManager : MonoBehaviour                     //Script présent sur l'objet GameManager de la scène, sur le même objet que le script gamemanager
{
    public GameObject asteroidPrefab;                             //Prefab de l'astéroide
    [SerializeField] private float radius = 35;                   //Rayon du cerlce de spawn
    [SerializeField] private float spawnFrequency = 0;            //Fréquence de spawn
    [SerializeField] private float RandomScaleRange = 0;          //Range de l'aléatoire pour la taille des astéroides
    [SerializeField] private float appearSpeed = 0;               //Vitesse de déplacement initiale des astéroides
    [SerializeField] private int scoreAdded = 0;                  //Score à ajouter à l'explosion de l'astéroide
    [SerializeField] private float appearAnimSpeed = 0;           //Vitesse du premier effet de shader
    [SerializeField] private float secondShaderAppearSpeed = 0;   //Vitesse du deuxième effet de shader

    public GameObject asteroidExplodePrefab;   //Prefab de l'explosion des astéroides
    private Vector3 screenPos;                 //Position de l'écran noir
    private GameManager gameManager;

    private void Awake()
    {
        gameManager = this.gameObject.GetComponent<GameManager>();
        screenPos = gameManager.screen.transform.position;
        StartCoroutine(SpawnAsteroids(2f));
    }

    private IEnumerator SpawnAsteroids(float frequency)
    {
        yield return new WaitForSecondsRealtime(frequency);      //Toutes les tant de secondes on fait apparaitre un asteroide à une position aléatoire et avec une scale légèrement aléatoire sur un cercle fictif de rayon donné
        float angle = Random.Range(0, 360);
        GameObject asteroid = Instantiate(asteroidPrefab, new Vector3(radius * Mathf.Cos(angle * Mathf.Deg2Rad), (radius * Mathf.Sin(angle * Mathf.Deg2Rad)), screenPos.z), Quaternion.identity);
        Vector3 originalScale = asteroid.transform.localScale;
        asteroid.transform.localScale = new Vector3(Random.Range(originalScale.x - RandomScaleRange, originalScale.x + RandomScaleRange), Random.Range(originalScale.y - RandomScaleRange, originalScale.y + RandomScaleRange), Random.Range(originalScale.z - RandomScaleRange, originalScale.z + RandomScaleRange));
        
        asteroid.GetComponent<MoveAsteroid>().gameManager = this.gameManager;
        asteroid.GetComponent<MoveAsteroid>().asteroidManager = this;
        asteroid.GetComponent<MoveAsteroid>().speedFactor = appearSpeed;
        asteroid.GetComponent<MoveAsteroid>().size = 3;
        asteroid.GetComponent<MoveAsteroid>().scoreAdded = this.scoreAdded;
        asteroid.GetComponent<MoveAsteroid>().startMove(CalculateDestination(asteroid));                  //On envoie les infos à l'astéroide pour qu'il calcule sa trajectoire

        GameObject outlineChild = asteroid.transform.GetChild(0).gameObject;    //On récupère son premier enfant (effet de bump) et on l'active
        gameManager.asteroids.Add(asteroid);                    //On ajoute cet astéroide à la liste du GameManager

        StartCoroutine(applyEffects(asteroid, "_CutoffHeight", -1.5f, 1.5f, appearAnimSpeed, outlineChild, true));         //On applique le premier effet (apparition) à l'astéroide
        StartCoroutine(SpawnAsteroids(spawnFrequency));        //Et on recommence le spawn en relancant la coroutine
    }

    private IEnumerator applyEffects(GameObject objectToApply, string effect, float initialValue, float finalValue, float speedFactor, GameObject secondObject, bool second)
    {
        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        Renderer rend = objectToApply.GetComponent<Renderer>();       //On crée un block de propriétés pour notre objet et on récupère le renderer de l'objet
        rend.GetPropertyBlock(prop);
        prop.SetFloat(effect, initialValue);
        rend.SetPropertyBlock(prop);                         //On lie notre block à l'objet et y associe notre valeur initiale d'effet
        while (prop.GetFloat(effect) < finalValue)
        {
            float previous = prop.GetFloat(effect);
            prop.SetFloat(effect, (previous + Time.deltaTime * speedFactor));          //On modifie la valeur de l'effet du shader dans le temps
            rend.SetPropertyBlock(prop);
            yield return null;
        }
        if (second)
        {
            secondObject.SetActive(true);
            StartCoroutine(applyEffects(secondObject, "_NoiseScale", -0.01f, 0.2f, secondShaderAppearSpeed, null, false));       //Et à la fin on lance le deuxième effet sur l'enfant de l'astéroide
        }
    }

    /*Cette immondice est juste la pour calculer un deuxième point à l'opposé de celui de spawn, sur un cercle de diamètre donné, de telle sorte à ce que par rapport à l'origine, entre l'opposé exact, le point de spawn et le point de destination
    il n'y ait qu'une valeur aléatoire entre 15 et -15 degrés, pour générer un trajet passant près du centre tout en restant aléatoire*/
    private Vector3 CalculateDestination(GameObject spawned)             
    {
        Vector3 initialPos = spawned.transform.position;
        Vector3 oppositePoint = new Vector3(-initialPos.x, -initialPos.y, initialPos.z);
        float departAngle = Random.Range(-15f, 15f);
        float AC = radius * 2;
        float AB = Mathf.Cos(departAngle * Mathf.Deg2Rad) * AC;
        float BC = Mathf.Sin(departAngle * Mathf.Deg2Rad) * AC;
        Vector3 vAC = new Vector3(((oppositePoint.x - initialPos.x) / AC), ((oppositePoint.y - initialPos.y) / AC), screenPos.z);
        Vector3 vAB = new Vector3((vAC.x * Mathf.Cos(departAngle * Mathf.Deg2Rad) - vAC.y * Mathf.Sin(departAngle * Mathf.Deg2Rad)), (vAC.x * Mathf.Sin(departAngle * Mathf.Deg2Rad) + vAC.y * Mathf.Cos(departAngle * Mathf.Deg2Rad)), screenPos.z);
        Vector3 B = new Vector3(initialPos.x + AB * vAB.x, initialPos.y + AB * vAB.y, screenPos.z);
        return B;
    }

    public void removeAsteroidFromList(GameObject asteroid, bool exploded, int scoreToAdd)
    {
        gameManager.asteroids.Remove(asteroid);       //Si un astéroide disparait, l'info est transmise au GameManager qui le retire de sa liste
        if (exploded)
        {
            this.gameObject.GetComponent<GameManager>().score += scoreToAdd;      //Si il disparait à cause d'une explosion, le score est aussi updaté
        }
    }
}