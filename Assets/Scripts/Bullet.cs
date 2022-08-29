using UnityEngine;

public class Bullet : MonoBehaviour        //Script placé sur le prefab des bullets instanciés lors des tirs du vaisseau
{
    [SerializeField] private float moveSpeed = 0;      //Vitesse du bullet
    [SerializeField] private float maxLifeTime = 0;       //Temps de vie maximal du bullet avant disparition
     
    private float lifeTime;     //Durée de vie du bullet après avoir été tiré
    private bool free = true;   //Booléen de si le bullet est attaché à un astéroide ou non

    private void Start()
    {
        transform.rotation = transform.rotation * Quaternion.AngleAxis(90, Vector3.right);        //Rotation initiale pour placer le bullet dans le bon sens
    }
    void FixedUpdate()
    {
        if (free)
        {
            transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);           //Déplacement du bullet dans l'espace
        }
    }

    private void Update()
    {
        lifeTime += Time.deltaTime;
        if (lifeTime > maxLifeTime && free)         //Si le bullet dépasse sa date de péremption, il disparait, sheh
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)                               //Des que mon bullet rencontre un astéroide, il arrête de se déplacer, et devient enfant de ce dernier pour se calquer sur sa rotation
    {
        if (collision.gameObject.tag == "Asteroid")
        {
            free = false;
            this.transform.parent = collision.transform;
            collision.gameObject.GetComponent<MoveAsteroid>().hasBullet = true;
        }
    }

    private void OnTriggerExit(Collider other)                 //Dès que mon bullet sort de l'écran noir transparent, il est téléporté à sa position opposée (uniquement si il n'est pas collé à un astéroide)
    {
        if (other.gameObject.tag == "Edges" && free)
        {
            exitScreen();
        }
    }

    private void exitScreen()
    {
        this.transform.position = new Vector3(-transform.position.x, -transform.position.y, this.transform.position.z);
    }
}
