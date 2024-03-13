using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Die CarReset Klasse ist verantwortlich für das Zurücksetzen des Autos auf seine ursprüngliche Position.
/// <author>Mark Kisker</author>
/// </summary>
public class CarReset : MonoBehaviour
{
    [SerializeField]
    private float spawnRadius = 5f; // Der Radius, innerhalb dessen das Auto zufällig gespawnt wird

    [SerializeField]
    private GameObject carAgent;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private Car_Controller carController;
    private ParkSlotRandomizer parkSlotRandomizer;

    /// <summary>
    /// Die Start Methode wird einmalig zu Beginn der Szene aufgerufen.
    /// Sie ruft die Methoden zur Initialisierung der benötigten Komponenten auf und speichert die ursprüngliche Position und Rotation des Autos.
    /// </summary>
    void Start()
    {
        GetParkSlotRandomizer();
        GetCarController();
        originalPosition = carAgent.transform.position;
        originalRotation = carAgent.transform.rotation;
    }

    /// <summary>
    /// Die GetCarController Methode sucht nach dem Car_Controller Skript im aktuellen GameObject und speichert eine Referenz darauf.
    /// </summary>
    private void GetCarController()
    {
        GameObject carObject = GameObject.Find("CarAgent");
        if (carObject != null)
        {
            carController = carObject.GetComponent<Car_Controller>();
            if (carController == null)
            {
                Debug.LogError("Das Car_Controller-Skript wurde nicht gefunden.");
            }
        }
        else
        {
            Debug.LogError("Das Objekt mit dem Car_Controller-Skript wurde nicht gefunden.");
        }
    }

    /// <summary>
    /// Die GetParkSlotRandomizer Methode sucht nach dem ParkSlotRandomizer Skript im aktuellen GameObject und speichert eine Referenz darauf.
    /// </summary>
    private void GetParkSlotRandomizer()
    {
        GameObject parkSlotRandomizerObject = GameObject.Find("ParkSlotRandomizer");
        if (parkSlotRandomizerObject != null)
        {
            parkSlotRandomizer = parkSlotRandomizerObject.GetComponent<ParkSlotRandomizer>();
            if (parkSlotRandomizer == null)
            {
                Debug.LogError("Das parkSlotRandomizer-Skript wurde nicht gefunden.");
            }
        }
        else
        {
            Debug.LogError("Das Objekt mit dem parkSlotRandomizer-Skript wurde nicht gefunden.");
        }
    }

    /// <summary>
    /// Die ResetCar Methode setzt das Auto auf seine ursprüngliche Position und Rotation zurück und weist ihm einen zufälligen Parkplatz zu.
    /// Sie setzt auch die Geschwindigkeit des Autos auf Null und aktualisiert die Geschwindigkeitsvariable im Car_Controller.
    /// </summary>
    public void ResetCar()
    {
        Debug.Log("reset aufruf");
        if (carAgent != null && carController != null && parkSlotRandomizer != null)
        {
            // Generiere eine zufällige Position innerhalb des definierten Spawnradius, aber halte die Höhe auf 0
            Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;

            randomOffset.y = 0f;
            Debug.Log(randomOffset);

            // Setze die Position und Rotation des Autos auf die ursprüngliche Position und Rotation plus die zufällige Offset-Position
            carAgent.transform.position = originalPosition + randomOffset;
            carAgent.transform.rotation = originalRotation;

            // Setze die Geschwindigkeit des Autos auf Null und aktualisiere die Geschwindigkeitsvariable im Car_Controller
            carController.Rb.velocity = Vector3.zero;
            carController.setCar_Speed_KPH(0f);

            // Bewege das Auto, um die Änderungen anzuwenden (z.B. Bremsen zu setzen)
            carController.MoveCar(0f, 0f);

            parkSlotRandomizer.AssignRandomSlots();
        }
        else
        {
            //CarAgent versucht auf das Objekt zuzugreifen bevor es komplett initialisiert ist
            Debug.LogWarning(
                "Nicht alle benötigten Komponenten sind initialisiert. Der Reset-Vorgang wird übersprungen."
            );
        }
    }
}
