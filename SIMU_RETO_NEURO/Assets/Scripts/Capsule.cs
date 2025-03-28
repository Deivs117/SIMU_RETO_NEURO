using UnityEngine;

public class Capsule : MonoBehaviour
{
    private Transform capsuleTransform;
    private Vector3 ultimaPosicion; 
    private Transform agentTransform;
    private float distanciaAlAgente;
    private float distanciaAlAnterior;
    private void Awake()
    {   
        agentTransform = FindAnyObjectByType<Agent>().transform;
        capsuleTransform = FindAnyObjectByType<Capsule>().transform;
        MoverACoordenada();  
    }

    private void MoverACoordenada()
    {
        Vector3 nuevaPosicion;
        do
        {
            float nuevoX = Random.Range(-25, 26);  
            float nuevoZ = Random.Range(-12, 13);  
            nuevaPosicion = new Vector3(nuevoX, 1.5f, nuevoZ);

            distanciaAlAgente = Vector2.Distance(
                new Vector2(nuevaPosicion.x, nuevaPosicion.z), 
                new Vector2(agentTransform.position.x, agentTransform.position.z)
            );
            distanciaAlAnterior = Vector2.Distance(
                new Vector2(nuevaPosicion.x, nuevaPosicion.z), 
                new Vector2(ultimaPosicion.x, ultimaPosicion.z)
            );
        } 
        while (distanciaAlAnterior < 4 || distanciaAlAgente < 10 || (nuevaPosicion.x > 15  && nuevaPosicion.z > 8 ));  
        
        capsuleTransform.position = nuevaPosicion;
        ultimaPosicion = nuevaPosicion;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            MoverACoordenada(); 
        }
    }
}