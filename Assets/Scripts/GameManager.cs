using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour    //Script de contrôle du jeu dans sa globalité
{
    public GameObject screen;

    [SerializeField] private AudioSource audioSource = null;       //Sortie audio
    private AudioLowPassFilter lowPass;                            //Filtre passe-bas pour le bullet time
    [SerializeField] private int BPM = 0;                          //Tempo de la musique
    [SerializeField] private int bassFirstTempo = 0;               //Premier temps pour activer les basses
    private float delayBTWTempos;                                  //Temps en s entre chaque temps
    private bool down = true;                                      //Est-ce-que le bullet time est activé ou non
    public Sounds audioManager;                                    //Manager des sonss

    [SerializeField] private Volume postProcessObject = null;    //Manager du postprocessing
    private ColorAdjustments colorShift = null;                  //Objet pour contrôler les couleurs de la scène

    [SerializeField] private GameObject shipPrefab = null;          //Prefab du vaisseau
    [SerializeField] private List<GameObject> lifeModels = null;    //Modèles des vies du vaisseau
    public GameObject ShipExplodePrefab;                            //Explosion du vaisseau

    [HideInInspector] public List<GameObject> asteroids;             //Liste des astéroides présents sur lécran
    private List<GameObject> slowedDowns = new List<GameObject>();   //Liste vide pour les astéroides ralentis grâce au bullet time
    
    [HideInInspector] public int score;                    //Score du joueur
    [SerializeField] public int life = 0;                 //Vie du joueur
    [SerializeField] private TextMesh textscore = null;    //Affichage du score du joueur

    [SerializeField] private GameObject uiHelper = null;             //Interactions UI VR
    [SerializeField] private GameObject gameOverPanel = null;        //Panneau de game over global
    [SerializeField] private GameObject gameOverPanelEN = null;      //UI de game over anglaises
    [SerializeField] private GameObject gameOverPanelFR = null;      //UI de game over francaises

    private void Awake()
    {
        postProcessObject.profile.TryGet(out colorShift);
        delayBTWTempos = 60 / BPM;
        audioSource.volume *= PlayerPrefs.GetFloat("Volume");
        audioSource.Play();
        StartCoroutine(knowTempos());
        lowPass = audioSource.gameObject.GetComponent<AudioLowPassFilter>();
        spawnShip();
    }

    private void Update()
    {
        textscore.text = score.ToString("00000");            //Affichage du score sous forme d'un nombre à 4 chiffres 
        if (audioSource.time >= audioSource.clip.length)
        {
            Invoke("back2Menu", 2f);
        }
    }

    private void spawnShip()       //Fonction d'apparition du vaisseau
    {
        GameObject player = Instantiate(shipPrefab, new Vector3(0, 0, screen.transform.position.z), Quaternion.identity);
        player.name = "Ship";
        if (PlayerPrefs.GetString("VRmode") == "true")
        {
            player.GetComponent<ShipController>().isVR = true;
        }
        else
        {
            player.GetComponent<ShipController>().isVR = false;
        }
        player.transform.eulerAngles = new Vector3(0, 90, 0);
        player.GetComponent<ShipController>().screen = this.screen;
        player.GetComponent<ShipController>().gameManager = this.gameObject.GetComponent<GameManager>();
        lowPass.cutoffFrequency = 2000;
        lowPass.enabled = false;
    }

    public void back2Menu()
    {
        SceneManager.LoadScene("Menu");
    }

    private IEnumerator knowTempos()          //Fonction qui calcule les temps de la musique
    {
        float tempo = 0;
        float timer = 0;
        bool demiPassed = false;
        while (audioSource.time < audioSource.clip.length)
        {
            if (timer > delayBTWTempos / 2 && !demiPassed)        //Détection du demi-temps (bump des astéroides uniquement) et envoi à tous les astéroides de la liste si le premier temps avec les basses est passé
            {
                if (tempo > bassFirstTempo)
                {
                    for (int i = 0; i < asteroids.Count; i++)
                    {
                        asteroids[i].GetComponent<MoveAsteroid>().recievedTempo(2);
                    }
                }
                demiPassed = true;
            }
            if (timer > delayBTWTempos)                           //Détection de tous les temps forts et envoi à tous les astéroides (check des explosions + bump si le temps des basses est passé)
            {
                tempo++;
                timer -= delayBTWTempos;
                if (tempo > bassFirstTempo)
                {
                    score++;
                    for (int i = 0; i < asteroids.Count; i++)
                    {
                        asteroids[i].GetComponent<MoveAsteroid>().recievedTempo(1);
                    }
                }
                else
                {
                    score++;
                    for (int i = 0; i < asteroids.Count; i++)
                    {
                        asteroids[i].GetComponent<MoveAsteroid>().recievedTempo(0);
                    }
                }
                demiPassed = false;
            }
            timer += Time.deltaTime;
            yield return null;
        }
    }

    public void slowDown()   //Appelé à l'activation du Bullet time
    {
        foreach (GameObject item in asteroids)    //Stockage des astéroides existants dans une liste et diminution de leur vitesse
        {
            slowedDowns.Add(item);
            item.GetComponent<MoveAsteroid>().speedFactor /= 2;
        }
        down = true;
        
        StartCoroutine(fadeEffectLowPassDown());
    }

    public void speedUp()    //Appelé à l'arrêt du Bullet Time
    {
        foreach (GameObject item in slowedDowns)    //Re Accélération uniquement des astéroides qui ont été précedemment ralentis
        {
            item.GetComponent<MoveAsteroid>().speedFactor *= 2;
        }
        down = false;
        slowedDowns.Clear();
        StartCoroutine(fadeEffectLowPassUp());
    }

    private IEnumerator fadeEffectLowPassDown()          //Effet de LowPass "In" pour la saturation de couleur et le son (début du bullet time)
    {
        lowPass.enabled = true;
        while (lowPass.cutoffFrequency > 400 && down && colorShift.saturation.value > -100f)
        {
            lowPass.cutoffFrequency -= Time.deltaTime * 5000;
            colorShift.saturation.value -= Time.deltaTime * 100;
            yield return null;
        }
        colorShift.saturation.value = -100;
        lowPass.cutoffFrequency = 400;
    }

    private IEnumerator fadeEffectLowPassUp()        //Effet de LowPass "Out" (fin du bullet time)
    {
        while (lowPass.cutoffFrequency < 2000 && !down && colorShift.saturation.value < 0f)
        {
            lowPass.cutoffFrequency += Time.deltaTime * 5000;
            colorShift.saturation.value += Time.deltaTime * 100;
            yield return null;
        }
        colorShift.saturation.value = 0;
        lowPass.cutoffFrequency = 2000;
        lowPass.enabled = false;
    }

    public void shipHit()       //Fonction appelée quand le vaisseau rencontre un astéroide
    {
        foreach (GameObject item in asteroids)         //On fait dissparaitre tous les astéroides encore présents et on les enlève de la liste
        {
            item.SetActive(false);
        }
        asteroids.Clear();
        life--;
        lifeModels[life].SetActive(false);            //On désactive le petit modèle de vaisseau de la vie qui correspond, décrémente la vie et le score
        score -= 20;
        if (score < 0) score = 0;
        if (life == 0)                                 //Si la vie tombe à 0, on active le gameover, sinon on re spawn un vaisseau une seconde après
        {
            gameOverPanel.SetActive(true);
            if (PlayerPrefs.GetString("VRmode") == "true")
            {
                uiHelper.SetActive(true);
            }
            if (PlayerPrefs.GetString("Langue") == "fr")       //Activation de l'écran de GameOver
            {
                gameOverPanelFR.SetActive(true);
            }
            else if (PlayerPrefs.GetString("Langue") == "en")
            {
                gameOverPanelEN.SetActive(true);
            }
        }
        else                          //Réapparition du vaisseau si la vie n'est pas à 0
        {
            Invoke("spawnShip", 1f);
        }
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.DeleteAll();
    }
}
