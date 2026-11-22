using System.Collections.Generic;
using UnityEngine;

public class EfectosSaludBosque : MonoBehaviour
{
    public enum EstadoBosque
    {
        Dormido,     // 0 a 25%
        Despertando, // 25 a 50%
        Vivo,        // 50 a 75%
        Florecido    // 75 a 100%
    }

    [Header("Estado Actual (Solo lectura)")]
    public EstadoBosque estadoActual;
    private EstadoBosque estadoAnterior;
    private bool inicializado = false;

    [Header("Velocidad de Transición")]
    public float velocidadTransicion = 1.0f;

    [Header("1. Música: Tiple (Volumen proporcional al %)")]
    public AudioSource audioTiple;
    [Range(0f, 1f)] public float volumenMinimoTiple = 0.05f; // Volumen al 0%
    [Range(0f, 1f)] public float volumenMaximoTiple = 0.85f; // Volumen al 100%

    [Header("2. Ambiente / Naturaleza (AudioSources distintos)")]
    public AudioSource ambLamento;         // Fase 1
    public AudioSource ambGrillos;         // Fase 2+
    public AudioSource ambPajarosBuhos;    // Fase 3+
    [Range(0f, 1f)] public float volumenAmbiente = 0.6f;

    [Header("3. Sonidos de Salto (Patasola)")]
    public AudioSource audioSourceSaltos;
    public AudioClip saltoFuerte;      // Fase 1 y 2
    public AudioClip saltoSuave;       // Fase 3
    public AudioClip saltoPasoBaile;   // Fase 4

    [Header("4. Visuales: Niebla, Iluminación y Pasto (Tag: Grass)")]
    public string tagPasto = "Grass";
    public Light luzDireccional;
    public Color colorDormido = new Color(0.45f, 0.45f, 0.48f);
    public Color colorDespertando = new Color(0.55f, 0.65f, 0.50f);
    public Color colorVivo = new Color(0.40f, 0.85f, 0.45f);
    public Color colorFlorecido = new Color(0.85f, 1.0f, 0.75f);

    [Space(5)]
    public float nieblaDormido = 0.06f;
    public float nieblaDespertando = 0.03f;
    public float nieblaVivo = 0.01f;
    public float nieblaFlorecido = 0.002f;

    [Header("5. Visuales: Objetos y Partículas")]
    public GameObject[] hojasDespertando;      // Fase 2+
    public GameObject[] floresPequenas;        // Fase 3+
    public GameObject[] particulasLuciernagas; // Fase 4

    private readonly List<Renderer> renderersPasto = new List<Renderer>();
    private readonly List<SpriteRenderer> spritesPasto = new List<SpriteRenderer>();
    private MaterialPropertyBlock propBlock;
    private Color colorActualPasto;

    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorID = Shader.PropertyToID("_Color");

    private float objTiple;
    private float objLamento, objGrillos, objPajaros;
    private Color colorObjetivo;
    private float nieblaObjetivo;

    private void Awake()
    {
        propBlock = new MaterialPropertyBlock();
        DetectarObjetosPasto();
    }

    private void Start()
    {
        RenderSettings.fog = true;
        colorActualPasto = colorDormido;

        if (audioTiple != null && audioTiple.clip != null)
        {
            audioTiple.loop = true;
            audioTiple.playOnAwake = false;
            audioTiple.spatialBlend = 0f; // Fuerza audio 2D limpio
            audioTiple.volume = volumenMinimoTiple;
            audioTiple.Play();
        }

        PrepararAmbiente(ambLamento);
        PrepararAmbiente(ambGrillos);
        PrepararAmbiente(ambPajarosBuhos);
    }

    public void DetectarObjetosPasto()
    {
        renderersPasto.Clear();
        spritesPasto.Clear();

        if (string.IsNullOrEmpty(tagPasto)) return;

        GameObject[] objetosGrass = GameObject.FindGameObjectsWithTag(tagPasto);

        for (int i = 0; i < objetosGrass.Length; i++)
        {
            Renderer[] rends = objetosGrass[i].GetComponentsInChildren<Renderer>(true);
            for (int j = 0; j < rends.Length; j++)
            {
                if (rends[j] is SpriteRenderer sr)
                {
                    spritesPasto.Add(sr);
                }
                else
                {
                    renderersPasto.Add(rends[j]);
                }
            }
        }
    }

    private void PrepararAmbiente(AudioSource fuente)
    {
        if (fuente == null || fuente.clip == null) return;
        fuente.loop = true;
        fuente.playOnAwake = false;
        fuente.volume = 0f;
        fuente.Play();
    }

    private void Update()
    {
        if (BarraRio.Instancia == null) return;

        float saludActual = BarraRio.Instancia.Valor;
        EstadoBosque nuevoEstado = CalcularEstado(saludActual);

        // El volumen objetivo del tiple sube proporcionalmente de 0% a 100%
        float porcentajeNormalizado = Mathf.Clamp01(saludActual / 100f);
        objTiple = Mathf.Lerp(volumenMinimoTiple, volumenMaximoTiple, porcentajeNormalizado);

        if (!inicializado || nuevoEstado != estadoAnterior)
        {
            estadoActual = nuevoEstado;
            estadoAnterior = nuevoEstado;
            inicializado = true;
            ActualizarObjetivosPorFase(estadoActual);
        }

        AplicarTransiciones();
    }

    private EstadoBosque CalcularEstado(float salud)
    {
        if (salud < 25f) return EstadoBosque.Dormido;
        if (salud < 50f) return EstadoBosque.Despertando;
        if (salud < 75f) return EstadoBosque.Vivo;
        return EstadoBosque.Florecido;
    }

    private void ActualizarObjetivosPorFase(EstadoBosque estado)
    {
        switch (estado)
        {
            case EstadoBosque.Dormido:
                objLamento = volumenAmbiente;
                objGrillos = 0f;
                objPajaros = 0f;

                colorObjetivo = colorDormido;
                nieblaObjetivo = nieblaDormido;
                ActivarGrupo(hojasDespertando, false);
                ActivarGrupo(floresPequenas, false);
                ActivarGrupo(particulasLuciernagas, false);
                break;

            case EstadoBosque.Despertando:
                objLamento = volumenAmbiente * 0.3f;
                objGrillos = volumenAmbiente;
                objPajaros = 0f;

                colorObjetivo = colorDespertando;
                nieblaObjetivo = nieblaDespertando;
                ActivarGrupo(hojasDespertando, true);
                ActivarGrupo(floresPequenas, false);
                ActivarGrupo(particulasLuciernagas, false);
                break;

            case EstadoBosque.Vivo:
                objLamento = 0f;
                objGrillos = volumenAmbiente * 0.6f;
                objPajaros = volumenAmbiente;

                colorObjetivo = colorVivo;
                nieblaObjetivo = nieblaVivo;
                ActivarGrupo(hojasDespertando, true);
                ActivarGrupo(floresPequenas, true);
                ActivarGrupo(particulasLuciernagas, false);
                break;

            case EstadoBosque.Florecido:
                objLamento = 0f;
                objGrillos = volumenAmbiente * 0.4f;
                objPajaros = volumenAmbiente * 0.8f;

                colorObjetivo = colorFlorecido;
                nieblaObjetivo = nieblaFlorecido;
                ActivarGrupo(hojasDespertando, true);
                ActivarGrupo(floresPequenas, true);
                ActivarGrupo(particulasLuciernagas, true);
                break;
        }
    }

    private void AplicarTransiciones()
    {
        float dt = Time.deltaTime * velocidadTransicion;

        // Sube suavemente el volumen del tiple conforme sube el %
        MoverVolumen(audioTiple, objTiple, dt);

        // Transición suave de ambiente
        MoverVolumen(ambLamento, objLamento, dt);
        MoverVolumen(ambGrillos, objGrillos, dt);
        MoverVolumen(ambPajarosBuhos, objPajaros, dt);

        // Transición visual de niebla y luz
        RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, colorObjetivo, dt);
        RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, nieblaObjetivo, dt);
        if (luzDireccional != null)
        {
            luzDireccional.color = Color.Lerp(luzDireccional.color, colorObjetivo, dt);
        }

        // Transición suave de color en objetos con tag "Grass"
        colorActualPasto = Color.Lerp(colorActualPasto, colorObjetivo, dt);
        AplicarColorPasto(colorActualPasto);
    }

    private void AplicarColorPasto(Color color)
    {
        for (int i = 0; i < renderersPasto.Count; i++)
        {
            Renderer r = renderersPasto[i];
            if (r == null) continue;

            r.GetPropertyBlock(propBlock);
            propBlock.SetColor(BaseColorID, color);
            propBlock.SetColor(ColorID, color);
            r.SetPropertyBlock(propBlock);
        }

        for (int i = 0; i < spritesPasto.Count; i++)
        {
            if (spritesPasto[i] != null)
            {
                spritesPasto[i].color = color;
            }
        }
    }

    private void MoverVolumen(AudioSource fuente, float objetivo, float dt)
    {
        if (fuente == null) return;
        fuente.volume = Mathf.MoveTowards(fuente.volume, objetivo, dt * 0.5f);
    }

    private void ActivarGrupo(GameObject[] objetos, bool estado)
    {
        if (objetos == null) return;
        for (int i = 0; i < objetos.Length; i++)
        {
            if (objetos[i] != null && objetos[i].activeSelf != estado)
            {
                objetos[i].SetActive(estado);
            }
        }
    }

    public void ReproducirSonidoSalto()
    {
        if (audioSourceSaltos == null) return;

        AudioClip clip = saltoFuerte;
        if (estadoActual == EstadoBosque.Vivo) clip = saltoSuave;
        else if (estadoActual == EstadoBosque.Florecido) clip = saltoPasoBaile;

        if (clip != null) audioSourceSaltos.PlayOneShot(clip);
    }

    [ContextMenu("Probar Fase 1: Dormido (10%)")]
    private void Test1()
    {
        if (BarraRio.Instancia != null) BarraRio.Instancia.valor = 10f;
    }

    [ContextMenu("Probar Fase 2: Despertando (35%)")]
    private void Test2()
    {
        if (BarraRio.Instancia != null) BarraRio.Instancia.valor = 35f;
    }

    [ContextMenu("Probar Fase 3: Vivo (65%)")]
    private void Test3()
    {
        if (BarraRio.Instancia != null) BarraRio.Instancia.valor = 65f;
    }

    [ContextMenu("Probar Fase 4: Florecido (90%)")]
    private void Test4()
    {
        if (BarraRio.Instancia != null) BarraRio.Instancia.valor = 90f;
    }
}