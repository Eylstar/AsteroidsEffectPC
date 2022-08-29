using System.Collections;
using UnityEngine;

public class MoveAsteroid : MonoBehaviour             //Script placé sur chaque astéroide, instanciés par AsteroidManager
{
    public AsteroidsManager asteroidManager = null;
    public GameManager gameManager = null;
    public float speedFactor;         //Facteur de vitesse de l'astéroide (déplacement)
    private float speedFactorOriginal;   //Idem, mais pour garder une valeur static à transferer à l'enfant en cas de destruction lors d'un bullet time
    public bool hasBullet = false;    //Si l'astéroide a un bullet attaché et va donc exploser incessemment sous peu
    private bool exploded = false;    //Si cet astéroide à été explosé 
    public int size;                  //Taille de l'astéroide (de 3 à 1)
    public int scoreAdded;            //Valeur de l'astéroide lors de sa destruction

    private Vector3 destinationPoint;

    private void Start()
    {
        speedFactorOriginal = speedFactor;
    }

    public void startMove(Vector3 destination)
    {
        StartCoroutine(MoveMe(destination));
    }

    private IEnumerator MoveMe(Vector3 destination)       //Fonction qui déplace l'astéroide jusqu'à sa destination en fonction du speedfactor
    {
        destinationPoint = destination;
        Vector3 actualPos = this.transform.position;
        float percentage = 0;
        while (percentage < 1)
        {
            this.transform.position = Vector3.Lerp(actualPos, destination, percentage);
            percentage += Time.deltaTime * speedFactor;
            yield return null;
            if (exploded == true)
            {
                percentage = 1;
            }
        }
        this.transform.position = destination;
        asteroidManager.removeAsteroidFromList(this.gameObject, false, scoreAdded);
        this.gameObject.SetActive(false);                                    //A l'arrivée, l'astéroide est retiré de la liste du gamemanager et est désactivé
    }

    public void recievedTempo(int songCase)      //Fonction appelée une fois par astéroide, uniquement si celui-ci n'a pas explosé (n'est pas censé arriver mais on sait jamais)
    {
        if (!exploded)
        {
            if (songCase == 0)            //Premier cas, Tous les temps forts avant l'apparition des basses (et donc des bumps) si un bullet est accroché a l'astéroide il explose
            {
                if (hasBullet)
                {
                    exploded = true;
                    Explode();
                }
            }
            else if (songCase == 2)           //Deuxième cas, appelé tous les temps pour bump l'effet après l'apparition des basses
            {
                StartCoroutine(flashColor(0.4f, 3.5f));
            }
            else if (songCase == 1)           //Troisième cas, tous les temps forts après l'apparition des basses, bump et explosion si bullet il y a
            {
                StartCoroutine(changeColor());
                StartCoroutine(flashColor(0.4f, 3.5f));
                if (hasBullet)
                {
                    exploded = true;
                    Explode();
                }
            }
        }
    }

    private IEnumerator flashColor(float bump, float speed)     //Fonction qui fait l'effet de "bump" de l'outline de l'astéroide tous les temps     (même fonctionnement que apply effects du asteroidmanager)
    {
        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        Renderer rend = this.transform.GetChild(0).gameObject.GetComponent<Renderer>();
        rend.GetPropertyBlock(prop);
        //float originaleScale = prop.GetFloat("_NoiseScale");     //Bug aléatoire, obligé d'hard coder
        float originaleScale = 0.15f;
        while (prop.GetFloat("_NoiseScale") < bump)
        {
            float previous = prop.GetFloat("_NoiseScale");
            prop.SetFloat("_NoiseScale", (previous + Time.deltaTime * speed));
            rend.SetPropertyBlock(prop);
            yield return null;
        }
        while (prop.GetFloat("_NoiseScale") > originaleScale)
        {
            float previous = prop.GetFloat("_NoiseScale");
            prop.SetFloat("_NoiseScale", (previous - Time.deltaTime * speed));
            rend.SetPropertyBlock(prop);
            yield return null;
        }
    }

    private IEnumerator changeColor()   //Fonction qui fait l'effet de changement de couleur rouge de l'outline sur les temps d'explosion possible
    {
        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        Renderer rend = this.transform.GetChild(0).gameObject.GetComponent<Renderer>();
        rend.GetPropertyBlock(prop);
        prop.SetColor("_OutlineColor", new Color(1, 1, 1, 1));
        rend.SetPropertyBlock(prop);
        Color tint = prop.GetColor("_OutlineColor");
        float percentage = 0;
        while (percentage < 1)
        {
            prop.SetColor("_OutlineColor", Color.Lerp(tint, new Color(1, 0, 0, 1), percentage));
            rend.SetPropertyBlock(prop);
            percentage += Time.deltaTime * 10;
            yield return null;
        }
        Color newTint = prop.GetColor("_OutlineColor");
        percentage = 0;
        while (percentage < 1)
        {
            prop.SetColor("_OutlineColor", Color.Lerp(newTint, tint, percentage));
            rend.SetPropertyBlock(prop);
            percentage += Time.deltaTime * 3;
            yield return null;
        }
        yield return null;
    }

    private void Explode()     //Fonction d'explosion de l'astéroide (retirement de la liste du Gamemanager, instanciation de l'effet et désactivation de l'objet) puis création des enfants en fonction de la taille
    {
        asteroidManager.removeAsteroidFromList(this.gameObject, true, scoreAdded);
        Instantiate(asteroidManager.asteroidExplodePrefab, transform.position, Quaternion.Euler(90, 0, 0));
        gameManager.audioManager.PlayAsteroidExplodeSound();
        this.gameObject.SetActive(false);
        if (size != 1)    //Apparition des enfants avec des directions perpendiculaires
        {
            Vector3 tangeantPoint1 = new Vector3(destinationPoint.y, -destinationPoint.x, destinationPoint.z);
            Vector3 tangeantPoint2 = new Vector3(-destinationPoint.y, destinationPoint.x, destinationPoint.z);
            SpawnChildren(tangeantPoint1);
            SpawnChildren(tangeantPoint2);
        }
    }

    private void SpawnChildren(Vector3 destination)  //Fonction d'apparition des enfants, avec les valeurs adaptées
    {
        GameObject asteroid = Instantiate(asteroidManager.asteroidPrefab, transform.position, Quaternion.identity);
        asteroid.transform.localScale = new Vector3(this.transform.localScale.x * 0.66f, this.transform.localScale.y * 0.66f, this.transform.localScale.z * 0.66f);
        MoveAsteroid move = asteroid.GetComponent<MoveAsteroid>();
        move.gameManager = this.gameManager;
        move.asteroidManager = this.asteroidManager;
        move.speedFactor = this.speedFactorOriginal * 0.66f;
        move.size = this.size - 1;
        move.scoreAdded = this.scoreAdded * 2;
        move.startMove(destination);
        GameObject outlineChild = asteroid.transform.GetChild(0).gameObject;
        outlineChild.SetActive(true);
        gameManager.asteroids.Add(asteroid);
    } 
}
