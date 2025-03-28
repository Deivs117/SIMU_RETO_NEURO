using UnityEngine;
using UnityEngine.AI;
public class Player : MonoBehaviour
{
    private Rigidbody rb;
    private NavMeshAgent navMeshAgent;
    private Transform playerTransform;
    private Transform capsuleTransform;
    private Transform agentTransform;
    public Agent agent; 

    //DINÁMICA NEURONAL
    private const float dt = 0.1f;  // Paso de tiempo
    private const float tau = 1.0f; // Constante de tiempo
    private const float AMP = 1.0f; // Amplitud de activación
    private const float S = 1.0f;   // Escalado
    private float[] presencias = { 0, 0, 0, 0, 0 }; 
    private float[] distancias = { 0, 0, 0, 0, 0 }; 
    private float[,] n = new float[11, 2]; // Matriz de neuronas
    private const float N = 5.0f, SIGMA = 0.1f;
    private const float a = 100.0f, b = 50.0f;
    private float[] u = { 2.0f, 4.0f, 3.9f, 5.9f, 7.9f };
    private float[,] c_sm = new float[5, 2];
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        capsuleTransform = FindAnyObjectByType<Capsule>().transform;
        playerTransform = FindAnyObjectByType<Player>().transform;
        agentTransform = FindAnyObjectByType<Agent>().transform;
        agent = FindAnyObjectByType<Agent>();
    }
    private void Update()
    {

        Vector3 destination_goal = new Vector3(capsuleTransform.position.x, playerTransform.position.y, capsuleTransform.position.z);
        navMeshAgent.destination = destination_goal;
        Debug.Log("Moviéndose hacia la cápsula");

        // RED 1 - Actualización de la red neuronal
        //ActualizarNeurona();
        //Debug.Log("Respuesta acumuladora: " + n[10, 1]);

        // DECISIONES - Mover hacia el destino
        //TomarDecision();
    }
    void ActualizarNeurona()
    {
        for (int i = 0; i < 5; i++)
        {
            float inhibicion = (i < 4) ? (30 * SumaRango(1 + i, 4)) : 0;
            n[i, 1] = Mathf.Max(0, n[i, 0] + (dt / tau) * (-n[i, 0] +
                (AMP * Mathf.Pow(Mathf.Max(0, (presencias[i] * distancias[i] - inhibicion)), 2) /
                (S * S + Mathf.Pow(Mathf.Max(0, (presencias[i] * distancias[i] - inhibicion)), 2)))));
        }

        // Neuronas lineales
        for (int i = 5; i <= 9; i++)
        {
            n[i, 1] = Mathf.Max(0, n[i, 0] + (dt / tau) * (-n[i, 0] +
                Mathf.Max(0, 2 * SumaRango(i - 5, 4))));
        }

        // Neurona acumuladora
        n[10, 1] = Mathf.Max(0, n[10, 0] + (dt / tau) * (-n[10, 0] +
            Mathf.Max(0, SumaRango(5, 9))));

        // Actualizar valores previos para la siguiente iteración
        for (int i = 0; i < 11; i++)
        {
            n[i, 0] = n[i, 1];
        }

        c_sm[0, 1] = Mathf.Max(0, c_sm[0, 0] + (dt / tau) * (-c_sm[0, 0] + 
            (N * Mathf.Pow(Mathf.Max(0, (b * u[0] - b * n[10, 0])), 2) /
            (SIGMA * SIGMA + Mathf.Pow(Mathf.Max(0, (b * u[0] - b * n[10, 0])), 2)))));

        c_sm[1, 1] = Mathf.Max(0, c_sm[1, 0] + (dt / tau) * (-c_sm[1, 0] + 
            (N * Mathf.Pow(Mathf.Max(0, (b * u[1] - a * c_sm[0, 0] - b * n[10, 0])), 2) /
            (SIGMA * SIGMA + Mathf.Pow(Mathf.Max(0, (b * u[1] - a * c_sm[0, 0] - b * n[10, 0])), 2)))));

        c_sm[2, 1] = Mathf.Max(0, c_sm[2, 0] + (dt / tau) * (-c_sm[2, 0] + 
            (N * Mathf.Pow(Mathf.Max(0, (-b * u[2] + b * n[10, 0] - a * c_sm[3, 0] - a * c_sm[4, 0])), 2) /
            (SIGMA * SIGMA + Mathf.Pow(Mathf.Max(0, (-b * u[2] + b * n[10, 0] - a * c_sm[3, 0] - a * c_sm[4, 0])), 2)))));

        c_sm[3, 1] = Mathf.Max(0, c_sm[3, 0] + (dt / tau) * (-c_sm[3, 0] + 
            (N * Mathf.Pow(Mathf.Max(0, (-b * u[3] + b * n[10, 0] - a * c_sm[4, 0])), 2) /
            (SIGMA * SIGMA + Mathf.Pow(Mathf.Max(0, (-b * u[3] + b * n[10, 0] - a * c_sm[4, 0])), 2)))));

        c_sm[4, 1] = Mathf.Max(0, c_sm[4, 0] + (dt / tau) * (-c_sm[4, 0] + 
            (N * Mathf.Pow(Mathf.Max(0, (-b * u[4] + b * n[10, 0])), 2) /
            (SIGMA * SIGMA + Mathf.Pow(Mathf.Max(0, (-b * u[4] + b * n[10, 0])), 2)))));

        for (int i = 0; i < 5; i++)
        {
            c_sm[i, 0] = c_sm[i, 1];
        }
    }
    float SumaRango(int inicio, int fin)
    {
        float suma = 0;
        for (int i = inicio; i <= fin; i++)
        {
            suma += n[i, 0];
        }
        return suma;
    }

    void TomarDecision()
    {
        if (c_sm[0, 1] > 1)
        {
            Vector3 destination_goal = new Vector3(25, playerTransform.position.y, 12.5f);
            navMeshAgent.destination = destination_goal;
            Debug.Log("Moviéndose a la posición segura");
        }
        else
        {
            // Si las neuronas 2 ó 3 se activan por encima de 1, quedarse quieto
            navMeshAgent.destination = playerTransform.position; // No se mueve
            Debug.Log("Quieto en su posición");
        }
    }

}