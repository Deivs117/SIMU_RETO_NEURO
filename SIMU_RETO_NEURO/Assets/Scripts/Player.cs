using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
public class Player : MonoBehaviour
{
    private Rigidbody rb;
    private NavMeshAgent navMeshAgent;
    private Transform playerTransform;
    private Transform capsuleTransform;
    private Transform agentTransform;
    public Agent agent; 

    private Queue<float> historialDistancias = new Queue<float>();
    private int maxHistorial = 5;

    //DINÁMICA NEURONAL
    private float A = 100f, SIGMA = 1f;
    private float estimulo = 0f, neuroModulador = 0.0f;
    private float tau = 1f, tauIn = 2f;
    private float a = 1f, P1 = 300000f, P2 = 0.3f, W = 8000000f, U1 = 39.99f;
    private float[,] z = new float[6, 2];
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
        float distanciaActual = Vector2.Distance(
            new Vector2(playerTransform.position.x, playerTransform.position.z), 
            new Vector2(agentTransform.position.x, agentTransform.position.z)
        );

        float velocidadNeta = agent.rb.linearVelocity.magnitude; // Obtiene la velocidad neta
        float umbral = 0.5f; // Define el umbral de velocidad
        neuroModulador = 0.0f;
        // Agregar la nueva distancia al historial
        historialDistancias.Enqueue(distanciaActual);

        // Si el historial supera el tamaño máximo, eliminar la más antigua
        if (historialDistancias.Count > maxHistorial)
        {
            historialDistancias.Dequeue();
        }
        
        // Evaluar tendencia solo si hay suficientes datos
        if (historialDistancias.Count == maxHistorial && velocidadNeta > umbral)
        {
            
            float sumaDiferencias = 0;
            float[] distancias = historialDistancias.ToArray();

            for (int i = 1; i < distancias.Length; i++)
            {
                sumaDiferencias += distancias[i] - distancias[i - 1]; // Cambio en la distancia
            }

            if (sumaDiferencias < 0)
            {
                Debug.Log("El agente se está acercando.");
                neuroModulador = 1.0f;
            }
            else if (sumaDiferencias > 0)
            {
                Debug.Log("El agente se está alejando.");
            }
        }
        estimulo = 400/distanciaActual;
        ActualizarNeurona();
        Debug.Log("Estimulo: " + estimulo);
        Debug.Log("Neuro Modulador: " + z[5, 0]);
        Debug.Log("Neurona Decisoria: " + z[4, 0]);
        // DECISIONES - Mover hacia el destino
        TomarDecision();
    }
    void ActualizarNeurona()
    {
        z[5, 1] = z[5, 0] + (1 / tau) * (-z[5, 0] + (A * Mathf.Pow(Mathf.Max(0, (W * neuroModulador)), 2)) / 
            (Mathf.Pow(SIGMA, 2) + Mathf.Pow(Mathf.Max(0, (W * neuroModulador)), 2)));

        // VIA 1 ESTIMULACIÓN ACCIÓN DE HUIDA
        z[0, 1] = z[0, 0] + (1 / tau) * (-z[0, 0] + (A * Mathf.Pow(Mathf.Max(0, ((z[5, 0]*W+W) * estimulo - a * z[3, 0] - W * U1)), 2)) / 
            (Mathf.Pow(SIGMA, 2) + Mathf.Pow(Mathf.Max(0, ((z[5, 0]*W+W) * estimulo - a * z[3, 0] - W * U1)), 2)));

        z[1, 1] = z[1, 0] + (1 / tauIn) * (-z[1, 0] + (A * Mathf.Pow(Mathf.Max(0, (estimulo - P1 * z[2, 0])), 2)) / 
            (Mathf.Pow(SIGMA, 2) + Mathf.Pow(Mathf.Max(0, (estimulo - P1 * z[2, 0])), 2)));

        // VIA 2 INHIBICIÓN ACCIÓN DE HUIDA
        z[2, 1] = z[2, 0] + (1 / tau) * (-z[2, 0] + (A * Mathf.Pow(Mathf.Max(0, (a * z[0, 0])), 2)) / 
            (Mathf.Pow(SIGMA, 2) + Mathf.Pow(Mathf.Max(0, (a * z[0, 0])), 2)));

        z[3, 1] = z[3, 0] + (1 / tauIn) * (-z[3, 0] + (A * Mathf.Pow(Mathf.Max(0, (P2 * z[1, 0])), 2)) / 
            (Mathf.Pow(SIGMA, 2) + Mathf.Pow(Mathf.Max(0, (P2 * z[1, 0])), 2)));

        // NEURONA REFLEJO DE DECISIÓN
        z[4, 1] = z[4, 0] + (1 / tau) * (-z[4, 0] + Mathf.Max(0, (z[2, 0] - z[3, 0])));

        for (int i = 0; i < 6; i++)
        {
            z[i, 0] = z[i, 1];
        }
    }

    void TomarDecision()
    {
        if (z[4, 0] > 1)
        {
            navMeshAgent.speed = 7f;
            Vector3 destination_goal = new Vector3(25, playerTransform.position.y, 12.5f);
            navMeshAgent.destination = destination_goal;
            Debug.Log("Moviéndose a la posición segura");
        }
        else
        {
            Vector3 destination_goal = new Vector3(capsuleTransform.position.x, playerTransform.position.y, capsuleTransform.position.z);
            navMeshAgent.destination = destination_goal;
            Debug.Log("Moviéndose hacia la cápsula");
        }
    }

}