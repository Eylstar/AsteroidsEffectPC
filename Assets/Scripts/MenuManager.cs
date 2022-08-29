using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Slider slideVolumeEN = null;   //Slider de volume musique anglais
    [SerializeField] private Slider slideVolumeFR = null;   //Slider de volume musique francais
    [SerializeField] private Slider slideSongEN = null;     //Slider de volume bruitages anglais
    [SerializeField] private Slider slideSongFR = null;     //Slider de volume bruitages francais
    [SerializeField] private GameObject switchFR = null;     
    [SerializeField] private GameObject switchEN = null;     
    [SerializeField] private GameObject errorTXT = null;     
    [SerializeField] private GameObject labelFR = null;     
    [SerializeField] private GameObject labelEN = null;     
    [SerializeField] private GameObject pcEN = null;     
    [SerializeField] private GameObject pcFR = null;     

    [SerializeField] private bool amIMM = true;     //Est-ce-que je suis dans le menu principal ou dans la scène de jeu?
    [SerializeField] private GameObject firstPannelFR = null;    //Ecran du menu principal francais
    [SerializeField] private GameObject firstPannelEN = null;    //Ecran du menu principal anglais

    [SerializeField] private AudioSource menuMusicSource = null;
    [SerializeField] private GameObject eventSystem = null;

    [SerializeField] private Camera NoVRCam = null;
    [SerializeField] private GameObject[] VRobjects = null;

    public bool isVR = false;
    public bool forcePCmode;


    private void Awake()   //En fonction de la valeur du playerprefs de langue, affichage des bons menus
    {
        if (amIMM)
        {

            if (PlayerPrefs.GetString("VRmode") == "" || PlayerPrefs.GetString("VRmode") == "false")
            {
                isVR = false;
                applyParameters();
            }
            else if (PlayerPrefs.GetString("VRmode") == "true")
            {
                isVR = true;
                applyParameters();
            }
            if (PlayerPrefs.GetString("Langue") == "")
            {
                PlayerPrefs.SetString("Langue", "en");
                firstPannelEN.SetActive(true);
                switchEN.SetActive(true);
                labelEN.SetActive(true);
                pcEN.SetActive(true);
            }
            else if (PlayerPrefs.GetString("Langue") == "en")
            {
                firstPannelEN.SetActive(true);
                slideVolumeEN.value = PlayerPrefs.GetFloat("Volume");
                slideSongEN.value = PlayerPrefs.GetFloat("Sound");
                switchEN.SetActive(true);
                labelEN.SetActive(true);
                pcEN.SetActive(true);
            }
            else if (PlayerPrefs.GetString("Langue") == "fr")
            {
                firstPannelFR.SetActive(true);
                slideVolumeFR.value = PlayerPrefs.GetFloat("Volume");
                slideSongFR.value = PlayerPrefs.GetFloat("Sound");
                switchFR.SetActive(true);
                labelFR.SetActive(true);
                pcFR.SetActive(true);
            }
            menuMusicSource.volume *= PlayerPrefs.GetFloat("Volume");
        }
    }

    public IEnumerator StartXR()
    {
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();
        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            Debug.LogError("Initializing XR Failed. Check Editor or Player log for details.");
        }
        else
        {
            XRGeneralSettings.Instance.Manager.StartSubsystems();
        }
        PlayerPrefs.SetString("VRmode", "true");
    }

    public void StopXR()
    {
        XRGeneralSettings.Instance.Manager.StopSubsystems();
        XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        PlayerPrefs.SetString("VRmode", "false");
    }

    public static bool isPresent()
    {
        var xrDisplaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances<XRDisplaySubsystem>(xrDisplaySubsystems);
        foreach (var xrDisplay in xrDisplaySubsystems)
        {
            if (xrDisplay.running)
            {
                return true;
            }
        }
        return false;

    }

    public void SwitchVRMode()
    {
        isVR = !isVR;
        if (isVR)
        {
            StartCoroutine(StartXR());
        }
        else
        {
            StopXR();
        }
        applyParameters();
    }

    void applyParameters()
    {
        NoVRCam.gameObject.SetActive(!isVR);
        foreach (var item in VRobjects)
        {
            item.SetActive(isVR);
        }
        eventSystem.GetComponent<OVRInputModule>().enabled = isVR;
        eventSystem.GetComponent<StandaloneInputModule>().enabled = !isVR;
    }

    public void StartGame()      //Lancement d'une partie
    {
        if (PlayerPrefs.GetString("VRmode") == "true")
        {
            SceneManager.LoadScene("Game");
        }
        else
        {
            SceneManager.LoadScene("PCGame");
        }
    }

    public void ExitGame()    //A la fermeture du jeu
    {
        PlayerPrefs.DeleteAll();
        System.Diagnostics.Process.GetCurrentProcess().Kill();
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.DeleteAll();
    }

    private void Update()
    {
        if (amIMM)      //Uniquement si l'on est dans le MainMenu
        {
            menuMusicSource.volume = PlayerPrefs.GetFloat("Volume");     //Ajustement du volume de la musique du menu en fonction des options
            if (PlayerPrefs.GetString("Langue") == "fr")                    //Récuperation des infos du volume musique et son en fonction de la valeur des sliders et normalisation pour toutes les langues
            {
                PlayerPrefs.SetFloat("Volume", slideVolumeFR.value);
                PlayerPrefs.SetFloat("Sound", slideSongFR.value);
                slideVolumeEN.value = slideVolumeFR.value;
                slideSongEN.value = slideSongFR.value;
                if (switchFR.activeSelf == false)
                {
                    switchFR.SetActive(true);
                    switchEN.SetActive(false);
                }
                if (labelFR.activeSelf == false)
                {
                    labelFR.SetActive(true);
                    labelEN.SetActive(false);
                }
                if (pcFR.activeSelf == false)
                {
                    pcFR.SetActive(true);
                    pcEN.SetActive(false);
                }
            }
            else if (PlayerPrefs.GetString("Langue") == "en")
            {
                PlayerPrefs.SetFloat("Volume", slideVolumeEN.value);
                PlayerPrefs.SetFloat("Sound", slideSongEN.value);
                slideVolumeFR.value = slideVolumeEN.value;
                slideSongFR.value = slideSongEN.value;
                if (switchFR.activeSelf == true)
                {
                    switchFR.SetActive(false);
                    switchEN.SetActive(true);
                }
                if (labelFR.activeSelf == true)
                {
                    labelFR.SetActive(false);
                    labelEN.SetActive(true);
                }
                if (pcFR.activeSelf == true)
                {
                    pcFR.SetActive(false);
                    pcEN.SetActive(true);
                }
            }
            if (Input.GetKeyDown(KeyCode.J))
            {
                StartCoroutine(testXRStart());
            }
        }
    }

    public IEnumerator testXRStart()
    {
        if (!isVR)
        {
            yield return XRGeneralSettings.Instance.Manager.InitializeLoader();
            if (XRGeneralSettings.Instance.Manager.activeLoader == null)
            {
                XRGeneralSettings.Instance.Manager.DeinitializeLoader();
                StartCoroutine(fadeText());
            }
            else
            {
                XRGeneralSettings.Instance.Manager.StartSubsystems();
                PlayerPrefs.SetString("VRmode", "true");
                isVR = !isVR;
                applyParameters();
            }
        }
        else
        {
            StopXR();
            isVR = !isVR;
            applyParameters();
        }
        yield return null;
    }

    private IEnumerator fadeText()
    {
        TextMeshProUGUI text = errorTXT.GetComponent<TextMeshProUGUI>();
        Color textCol = text.color;

        errorTXT.SetActive(true);
        text.color = new Color(textCol.r, textCol.g, textCol.b, 1);
        yield return new WaitForSecondsRealtime(3f);

        float total = text.color.a;
        while (total >= 0)
        {
            total -= Time.deltaTime;
            text.color = new Color(textCol.r, textCol.g, textCol.b, total);
            yield return null;
        }
        text.color = new Color(textCol.r, textCol.g, textCol.b, 0);
        errorTXT.SetActive(false);
    }

    public void restartGame()    //Relancement d'une partie depuis la scène de jeu
    {
        if (PlayerPrefs.GetString("VRmode") == "true")
        {
            SceneManager.LoadScene("Game");
        }
        else
        {
            SceneManager.LoadScene("PCGame");
        }
    }

    public void backmenu()   //Retour menu
    {
        SceneManager.LoadScene("Menu");
    }

    public void changeLangue()     //Appui sur le boutton de changement de langue
    {
        if (PlayerPrefs.GetString("Langue") == "en")
        {
            PlayerPrefs.SetString("Langue", "fr");
        }
        else
        {
            PlayerPrefs.SetString("Langue", "en");
        }
    }
}
