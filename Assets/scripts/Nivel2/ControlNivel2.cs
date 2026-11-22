using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class ControlNivel2 : MonoBehaviour, IFuentePista
{
    [Header("Zonas en orden (TreeCountingManager)")]
    public List<TreeCountingManager> zonas = new List<TreeCountingManager>();

    [Header("Referencias")]
    public InventarioSemillas semillas;
    public HuellasUI huellas;
    public PatasolaSaltos patasola;

    [Header("Barra del bosque (BarraRio)")]
    [Tooltip("Cuánto vale un árbol en un reintento (0.5 = la mitad)")]
    public float factorReintento = 0.5f;

    [Header("Sonido de saltos que se acercan (opcional)")]
    [Tooltip("AudioSource en este objeto, con el clip de saltos y Spatial Blend en 0 (2D)")]
    public AudioSource sonidoSaltos;
    public float volumenMinimo = 0.05f;
    public float volumenMaximo = 1f;

    [Header("Tiempos")]
    public float esperaAntesDeReintentar = 1f;

    [Header("Inicio de zonas")]
    [Tooltip("Si está marcado, la zona 2, 3 y 4 empiezan solas al completar la anterior. " +
             "Solo la zona 1 necesita el arco.")]
    public bool siguienteZonaAutomatica = true;
    [Tooltip("Segundos para que el joven camine a la zona nueva antes de que empiece el tiempo")]
    public float esperaEntreZonas = 3f;

    [Header("Aviso de que la Patasola está cerca")]
    [Tooltip("A qué parte del tiempo se avisa que falta poco (0.75 = cuando pasó el 75 %)")]
    [Range(0.1f, 0.99f)] public float umbralCerca = 0.75f;

    [Header("Final")]
    [Tooltip("Conectar: PreguntasGuardian ? IniciarPreguntas")]
    public UnityEvent alCompletarNivel;

    // Para los diálogos (DuendeNivel2)
    public event System.Action<TreeCountingManager> AlEntrarZona;
    public event System.Action<TreeCountingManager> AlCompletarZona;
    public event System.Action<TreeCountingManager> AlFallarZona;
    public event System.Action<TreeCountingManager> AlSembrarArbol;
    public event System.Action<TreeCountingManager> AlPatasolaCerca;

    public TreeCountingManager ZonaActual { get; private set; }
    public float PuntosPorArbol { get; private set; }
    public bool HaySemillas => semillas == null || semillas.Cantidad > 0;

    // 0 = recién empezó la zona, 1 = llegó la Patasola
    public float ProgresoTiempo { get; private set; }

    private int indiceZona = 0;
    private float tiempo = 0f;
    private int semillasAlEntrar;
    private float barraAlEntrar;
    private bool resolviendo = false;
    private bool avisoCercaDado = false;

    private BarraRio Barra => BarraRio.Instancia;

    private void Start()
    {
        int totalArboles = 0;
        foreach (TreeCountingManager z in zonas)
        {
            if (z == null) continue;
            z.Control = this;
            totalArboles += z.ArbolesNecesarios;
        }

        PuntosPorArbol = totalArboles > 0 ? 100f / totalArboles : 0f;
        Debug.Log($"ControlNivel2: {totalArboles} árboles en total, cada uno vale {PuntosPorArbol:0.##} %.");

        if (semillas != null && semillas.semillasIniciales != totalArboles)
            Debug.LogWarning($"ControlNivel2: se necesitan {totalArboles} árboles y hay {semillas.semillasIniciales} semillas. " +
                             "Ajusta 'Semillas Iniciales' en InventarioSemillas.");

        if (sonidoSaltos != null)
        {
            sonidoSaltos.playOnAwake = false;
            sonidoSaltos.loop = true;
            sonidoSaltos.Stop();
            if (sonidoSaltos.clip == null)
                Debug.LogWarning("ControlNivel2: el AudioSource de 'Sonido Saltos' no tiene AudioClip.");
        }
        else
        {
            Debug.Log("ControlNivel2: no hay 'Sonido Saltos' asignado, no sonarán los saltos que se acercan.");
        }
    }

    // ================== ZONAS ==================

    public void JugadorCruzoArco(TreeCountingManager zona)
    {
        if (indiceZona >= zonas.Count) return;
        if (zona != zonas[indiceZona] || zona.EnCurso || zona.ZonaCompletada || resolviendo) return;
        IniciarZona(zona);
    }

    private void IniciarZona(TreeCountingManager zona)
    {
        ZonaActual = zona;
        zona.EnCurso = true;
        tiempo = 0f;
        ProgresoTiempo = 0f;
        avisoCercaDado = false;
        semillasAlEntrar = semillas != null ? semillas.Cantidad : 0;
        barraAlEntrar = Barra != null ? Barra.Valor : 0f;

        if (huellas != null) huellas.Mostrar();
        if (sonidoSaltos != null) { sonidoSaltos.volume = volumenMinimo; sonidoSaltos.Play(); }

        Debug.Log($"ControlNivel2: empieza la zona {zona.numeroZona} (intento {zona.Intentos + 1}, {zona.tiempoLimite} s).");
        AlEntrarZona?.Invoke(zona);
    }

    private void Update()
    {
        if (ZonaActual == null || !ZonaActual.EnCurso) return;

        tiempo += Time.deltaTime;
        float progreso = Mathf.Clamp01(tiempo / Mathf.Max(0.01f, ZonaActual.tiempoLimite));
        ProgresoTiempo = progreso;

        if (!avisoCercaDado && progreso >= umbralCerca)
        {
            avisoCercaDado = true;
            AlPatasolaCerca?.Invoke(ZonaActual);
        }

        if (huellas != null) huellas.Actualizar(progreso);
        if (sonidoSaltos != null) sonidoSaltos.volume = Mathf.Lerp(volumenMinimo, volumenMaximo, progreso);

        if (progreso >= 1f) StartCoroutine(FallarZona(ZonaActual));
    }

    // ================== AVISOS DE TreeCountingManager ==================

    public void RegistrarSiembra(TreeCountingManager zona)
    {
        if (semillas != null) semillas.Usar();

        float puntos = PuntosPorArbol * (zona.Intentos > 0 ? factorReintento : 1f);
        if (Barra != null) Barra.Sumar(puntos);
        else Debug.LogWarning("ControlNivel2: no hay ninguna BarraRio activa en la escena.");

        AlSembrarArbol?.Invoke(zona);
    }

    public void ZonaCompletada(TreeCountingManager zona)
    {
        if (zona == ZonaActual) PararTiempo();
        AlCompletarZona?.Invoke(zona);

        indiceZona++;
        ZonaActual = null;

        if (indiceZona >= zonas.Count)
        {
            Debug.Log("ControlNivel2: ¡las 4 zonas restauradas!");
            alCompletarNivel?.Invoke();
        }
        else if (siguienteZonaAutomatica)
        {
            StartCoroutine(IniciarConEspera(zonas[indiceZona]));
        }
    }

    private IEnumerator IniciarConEspera(TreeCountingManager zona)
    {
        yield return new WaitForSeconds(esperaEntreZonas);
        if (!zona.EnCurso && !zona.ZonaCompletada) IniciarZona(zona);
    }

    private IEnumerator FallarZona(TreeCountingManager zona)
    {
        zona.EnCurso = false;
        resolviendo = true;
        if (huellas != null) huellas.Actualizar(1f);
        Debug.Log($"ControlNivel2: la Patasola llegó a la zona {zona.numeroZona} antes de terminar.");

        zona.alLlegarPatasola?.Invoke();
        AlFallarZona?.Invoke(zona);

        if (patasola != null)
            yield return StartCoroutine(patasola.PasarPor(zona.patasolaDesde, zona.patasolaHasta));

        PararTiempo();

        // Volver la zona a como estaba al entrar
        zona.Reiniciar();
        if (semillas != null) semillas.Establecer(semillasAlEntrar);
        if (Barra != null && Barra.Valor > barraAlEntrar) Barra.Restar(Barra.Valor - barraAlEntrar);
        zona.Intentos++;

        yield return new WaitForSeconds(esperaAntesDeReintentar);

        resolviendo = false;
        IniciarZona(zona);
    }

    private void PararTiempo()
    {
        if (huellas != null) huellas.Ocultar();
        if (sonidoSaltos != null) sonidoSaltos.Stop();
    }

    // ================== PISTA DEL DUENDE ==================

    public bool PreguntaActiva => false;
    public string UsarPista() => null;

    public GameObject ObjetivoParaPista(Vector3 posJugador)
    {
        TreeCountingManager zona = ZonaActual != null ? ZonaActual
                                 : (indiceZona < zonas.Count ? zonas[indiceZona] : null);
        if (zona == null) return null;

        PlantingSpot mejor = null;
        float mejorDist = float.MaxValue;
        foreach (PlantingSpot p in zona.Parches)
        {
            if (p == null || p.YaSembrado) continue;
            float d = (p.transform.position - posJugador).sqrMagnitude;
            if (d < mejorDist) { mejorDist = d; mejor = p; }
        }
        return mejor != null ? mejor.gameObject : null;
    }
}