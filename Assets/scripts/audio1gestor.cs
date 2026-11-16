using UnityEngine;

public class audio1gestor : MonoBehaviour
{
    // Patrón Singleton para llamarlo desde cualquier script (ej: AudioManager.Instancia.ReproducirRecoger())
    public static audio1gestor Instancia { get; private set; }

    [Header("Canales de Audio (Se crean solos si los dejas vacíos)")]
    public AudioSource musicaSource;
    public AudioSource ambienteRioSource;
    public AudioSource pasosSource;
    public AudioSource sfxSource;

    [Header("Clips de Música y Ambiente (Loops)")]
    public AudioClip musicaFondo;
    public AudioClip sonidoRio;

    [Header("Clips del Personaje")]
    public AudioClip[] sonidosCaminar; // Array para variar entre varios sonidos de pasos
    public AudioClip sonidoRecogerBasura;

    [Header("Clips de Interfaz y Mundo")]
    public AudioClip sonidoBotonUI;
    public AudioClip sonidoAbrirCueva;
    public AudioClip sonidoGanarMinijuego;
    public AudioClip sonidoPerderMinijuego;

    [Header("Ajustes de Pasos")]
    public float intervaloPasos = 0.4f; // Cada cuánto suena un paso al caminar
    private float temporizadorPasos = 0f;

    private void Awake()
    {
        // Configuración del Singleton
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
        DontDestroyOnLoad(gameObject); // Mantiene el audio vivo si cambias de escena

        ConfigurarCanales();
    }

    private void Start()
    {
        // Inicia automáticamente la música y el río al arrancar el nivel
        IniciarMusicaYAmbiente();
    }

    private void ConfigurarCanales()
    {
        if (musicaSource == null) musicaSource = gameObject.AddComponent<AudioSource>();
        if (ambienteRioSource == null) ambienteRioSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        if (pasosSource == null) pasosSource = gameObject.AddComponent<AudioSource>();

        musicaSource.loop = true;
        ambienteRioSource.loop = true;
        sfxSource.loop = false;
        pasosSource.loop = false;
    }

    public void IniciarMusicaYAmbiente()
    {
        if (musicaFondo != null && !musicaSource.isPlaying)
        {
            musicaSource.clip = musicaFondo;
            musicaSource.Play();
        }

        if (sonidoRio != null && !ambienteRioSource.isPlaying)
        {
            ambienteRioSource.clip = sonidoRio;
            ambienteRioSource.Play();
        }
    }

    // ================== MÉTODOS PARA LLAMAR DESDE OTROS SCRIPTS ==================

    // Llamar en el FixedUpdate/Update de PlayerMov.cs mientras el niño se mueva
    public void GestionarPasos(bool estaCaminando)
    {
        if (!estaCaminando || sonidosCaminar.Length == 0)
        {
            temporizadorPasos = 0f;
            return;
        }

        temporizadorPasos -= Time.deltaTime;
        if (temporizadorPasos <= 0f)
        {
            AudioClip clipAleatorio = sonidosCaminar[Random.Range(0, sonidosCaminar.Length)];
            // Varía ligeramente el tono (pitch) para que los pasos suenen naturales
            pasosSource.pitch = Random.Range(0.9f, 1.1f);
            pasosSource.PlayOneShot(clipAleatorio);

            temporizadorPasos = intervaloPasos;
        }
    }

    public void ReproducirRecoger()
    {
        ReproducirSFX(sonidoRecogerBasura);
    }

    public void ReproducirBoton()
    {
        ReproducirSFX(sonidoBotonUI);
    }

    public void ReproducirCueva()
    {
        ReproducirSFX(sonidoAbrirCueva);
    }

    public void ReproducirResultadoMinijuego(bool gano)
    {
        ReproducirSFX(gano ? sonidoGanarMinijuego : sonidoPerderMinijuego);
    }

    // Método genérico para reproducir cualquier sonido puntual sin cortar los anteriores
    public void ReproducirSFX(AudioClip clip, float volumen = 1f)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volumen);
        }
    }

    // Útil para ajustar el volumen del río dinámicamente (ej: más fuerte o calmo según la limpieza)
    public void CambiarVolumenRio(float nuevoVolumen)
    {
        if (ambienteRioSource != null)
        {
            ambienteRioSource.volume = Mathf.Clamp01(nuevoVolumen);
        }
    }
}