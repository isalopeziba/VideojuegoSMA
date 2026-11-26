using UnityEngine;

public class Pausa : MonoBehaviour
{
    public GameObject menuPausa;
    public bool juegoPausado = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TogglePause()
    {
        if(juegoPausado)
        {
            Reanudar();
        }else{
            Pausar();
        }
    }

    void Reanudar()
    {
        menuPausa.SetActive(false);
        Time.timeScale = 1;
        juegoPausado = false;
    }
    void Pausar()
    {
        menuPausa.SetActive(true);
        Time.timeScale = 0;
        juegoPausado = true;
    }

    public void CerrarJuego()
    {
        Application.Quit();
    }
}
