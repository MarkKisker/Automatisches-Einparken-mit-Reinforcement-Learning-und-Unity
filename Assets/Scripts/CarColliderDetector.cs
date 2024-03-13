using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Die CarColliderDetector Klasse ist verantwortlich für die Erkennung von Kollisionen des Autos.
/// </summary>
/// <author>Mark Kisker</author>
public class CarColliderDetector : MonoBehaviour
{
    private CarAgent carAgent; // Referenz auf das CarAgent Skript

    /// <summary>
    /// Die Start Methode wird einmalig zu Beginn der Szene aufgerufen.
    /// Sie ruft die Methoden zur Initialisierung der benötigten Komponenten auf.
    /// </summary>
    private void Start()
    {
        getCarAgent();
    }

    /// <summary>
    /// Die getCarAgent Methode sucht nach dem CarAgent Skript im aktuellen GameObject und speichert eine Referenz darauf.
    /// </summary>
    private void getCarAgent()
    {
        GameObject carObject = GameObject.Find("CarAgent");
        if (carObject != null)
        {
            carAgent = carObject.GetComponent<CarAgent>();
        }
        else
        {
            Debug.LogError("Das Objekt mit dem carAgent-Skript wurde nicht gefunden.");
        }
    }

    /// <summary>
    /// Die OnTriggerEnter Methode wird aufgerufen, wenn ein anderer Collider den Trigger betritt.
    /// Sie überprüft, ob das Auto mit etwas anderem als einem Parkplatz kollidiert ist.
    /// </summary>
    /// <param name="other">Der andere Collider, der den Trigger betritt.</param>
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("ParkingSpot"))
        {
            carAgent.IsCollided = true;
        }
    }
}
