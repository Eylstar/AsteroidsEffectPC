using System.Collections;
using UnityEngine;

public class ShipController : MonoBehaviour                //Script placé sur le vaisseau, instancié au start du gamemanager
{
    [HideInInspector] public GameObject screen;

    [SerializeField] private float speedFactor = 0;       //Facteur de vitesse de déplacement du vaisseau
    [SerializeField] private float rotateFactor = 0;       //Facteur de rotation du vaisseau
    [SerializeField] private float shootFrequency = 0;      //Fréquence de tir du vaisseau

    [SerializeField] private GameObject bulletPrefab = null;        //Prefab du bullet
    private bool canShoot = true;      //Est-ce-que le vaisseau peut tirer
    private bool slowed = false;       //Est-ce-que le bullet time est actif
    private bool canSlow = true;       //Est-ce-que le bullet time peut être activé
    private float cooldownBT = 6f;     //Attente entre deux bullet time
    private float durationBT = 3f;     //Durée maximale d'un bullet time

    private Rigidbody rigid;
    public GameManager gameManager;

    public bool isVR;

    void Start()
    {
        rigid = this.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (isVR)
        {
            float Speedpressed = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
            Vector2 axisGet = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
            if (Speedpressed != 0)
            {
                rigid.AddForce(transform.forward * Speedpressed * speedFactor * Time.deltaTime);               //Impulsion du vaisseau vers l'avant en fonction de la perssion sur la gachette
            }
            if (axisGet.y != 0)
            {
                this.transform.Rotate(Vector3.right * axisGet.x * rotateFactor * Time.deltaTime);       //Rotation du vaisseau en fonction du joystick de la mannette
            }
            if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))          //Si un input de tir est detecté à cette frame précisément
            {
                if (canShoot)
                {
                    canShoot = false;
                    shootBullet();                                         //On tire un bullet
                    StopCoroutine(shootBulletTiming());                    //On stoppe toute potentielle coroutine en cours de tir et on en initialise une nouvelle
                    StartCoroutine(shootBulletTiming());
                }
            }
            //Si les deux inputs de BT sont appuyés en même temps, on l'active
            if (OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger) && OVRInput.Get(OVRInput.Button.PrimaryHandTrigger) || OVRInput.Get(OVRInput.Button.SecondaryHandTrigger) && OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger))
            {
                if (!slowed && canSlow)
                {
                    speedFactor /= 2;
                    gameManager.slowDown();
                    slowed = true;
                    StartCoroutine(decompteBT());
                }
            }
            //Et si un des deux bouttons du BT est désactivé alors que le bullet time était en cours, on le désactive
            if ((OVRInput.GetUp(OVRInput.Button.SecondaryHandTrigger) || OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger)) && slowed)
            {
                gameManager.speedUp();
                slowed = false;
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                rigid.AddForce(transform.forward * speedFactor * Time.deltaTime);
            }
            int rotateValue = 0;
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                rotateValue = -1;
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                rotateValue = 1;
            }
            if(Input.GetKey(KeyCode.RightArrow) && Input.GetKey(KeyCode.LeftArrow))
            {
                rotateValue = 0;
            }
            this.transform.Rotate(Vector3.right * rotateValue * rotateFactor * Time.deltaTime);
            if (Input.GetKey(KeyCode.Space))
            {
                if (canShoot)
                {
                    canShoot = false;
                    shootBullet();                                         //On tire un bullet
                    StopCoroutine(shootBulletTiming());                    //On stoppe toute potentielle coroutine en cours de tir et on en initialise une nouvelle
                    StartCoroutine(shootBulletTiming());
                }
            }
            if (Input.GetKey(KeyCode.D))
            {
                if (!slowed && canSlow)
                {
                    speedFactor /= 2;
                    gameManager.slowDown();
                    slowed = true;
                    StartCoroutine(decompteBT());
                }
            }
            if (!Input.GetKey(KeyCode.D) && slowed)
            {
                gameManager.speedUp();
                slowed = false;
            }
            if (Input.GetKey(KeyCode.Escape))
            {
                gameManager.back2Menu();
            }
        }
    }

    private IEnumerator decompteBT()    //Duration du BT
    {
        float decompte = durationBT;
        while (decompte > 0)
        {
            decompte -= Time.deltaTime;
            if (!slowed)
            {
                break;
            }
            yield return null;
        }
        canSlow = false;
        gameManager.speedUp();   //A la fin de celui-ci, reset des vitesses du vaisseau et des astéroides et des effets visuels et sonores
        speedFactor *= 2;
        slowed = false;
        StartCoroutine(cooldownBTTimer());
    }

    private IEnumerator cooldownBTTimer()   //Cooldown entre deux activations du BT
    {
        float cooldown = cooldownBT;
        while (cooldown > 0)
        {
            cooldown -= Time.deltaTime;
            yield return null;
        }
        canSlow = true;
    }

    private IEnumerator shootBulletTiming()
    {
        yield return new WaitForSecondsRealtime(shootFrequency);        //On attend le timing entre deux tirs avant de redonner la permission de tirer
        canShoot = true;
        if (isVR)
        {
            if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))          //Et si le bouton est toujours en état enfoncé, on recommence
            {
                shootBullet();
                canShoot = false;
                StartCoroutine(shootBulletTiming());
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.Space))
            {
                shootBullet();
                canShoot = false;
                StartCoroutine(shootBulletTiming());
            }
        }
    }
    
    private void shootBullet()           //Création du bullet tiré par le vaisseau
    {
        Instantiate(bulletPrefab, this.transform.position, this.transform.rotation);
        gameManager.audioManager.PlayShootSound();
    }

    private void OnCollisionEnter(Collision collision)                       //Si le vaisseau percute un astéroide, on en crée un nouveau, on instancie l'animation de destruction du vaisseau et on désactive cet objet
    {
        if (collision.gameObject.tag == "Asteroid")
        {
            gameManager.shipHit();
            Instantiate(gameManager.ShipExplodePrefab, transform.position, Quaternion.Euler(90, 0, 0));
            gameManager.audioManager.PlayShipExplodeSound();
            this.gameObject.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider collision)        //Si le vaisseau quitte l'écran noir, il est téléporté à la position opposée
    {
        if (collision.gameObject.tag == "Edges")
        {
            exitScreen();
        }
    }

    private void exitScreen()     //Léger offset des values pour éviter un bug qui envoyait le vaisseau dans le vide
    {
        if (this.transform.position.x > 0 && this.transform.position.y > 0)
        {
            this.transform.position = new Vector3(-transform.position.x + 0.5f, -transform.position.y + 0.5f, screen.transform.position.z);
        }
        else if (this.transform.position.x > 0 && this.transform.position.y < 0)
        {
            this.transform.position = new Vector3(-transform.position.x + 0.5f, -transform.position.y - 0.5f, screen.transform.position.z);
        }
        else if (this.transform.position.x < 0 && this.transform.position.y > 0)
        {
            this.transform.position = new Vector3(-transform.position.x - 0.5f, -transform.position.y + 0.5f, screen.transform.position.z);
        }
        else
        {
            this.transform.position = new Vector3(-transform.position.x - 0.5f, -transform.position.y - 0.5f, screen.transform.position.z);
        }
    }    
}
