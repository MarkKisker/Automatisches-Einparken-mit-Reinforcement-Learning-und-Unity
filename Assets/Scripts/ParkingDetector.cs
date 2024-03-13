using UnityEngine;

/// <summary>
/// Die ParkingDetector Klasse ist verantwortlich für die Erkennung, ob das Auto korrekt in einem Parkplatz geparkt ist.
/// </summary>
/// <author>Mark Kisker</author>
public class ParkingDetector : MonoBehaviour
{
    [SerializeField]
    private Collider parkingCollider; // Der Collider des Parkplatzes

    private Car_Controller carController; // Referenz auf das Car_Controller Skript
    private CarReset carReset; // Referenz auf das CarReset Skript
    private CarAgent carAgent; // Referenz auf das CarAgent Skript

    private bool CarIsMoving; // Variable, die angibt, ob das Auto sich bewegt oder nicht

    /// <summary>
    /// Die Start Methode wird einmalig zu Beginn der Szene aufgerufen.
    /// Sie ruft die Methoden zur Initialisierung der benötigten Komponenten auf.
    /// </summary>
    private void Start()
    {
        getCarController();
        getCarReset();
        getCarAgent();
    }

    /// <summary>
    /// Die getCarController Methode sucht nach dem Car_Controller Skript im aktuellen GameObject und speichert eine Referenz darauf.
    /// </summary>
    private void getCarController()
    {
        GameObject carObject = GameObject.Find("CarAgent");
        if (carObject != null)
        {
            carController = carObject.GetComponent<Car_Controller>();
        }
        else
        {
            Debug.LogError("Das Objekt mit dem Car_Controller-Skript wurde nicht gefunden.");
        }
    }

    /// <summary>
    /// Die getCarReset Methode sucht nach dem CarReset Skript im aktuellen GameObject und speichert eine Referenz darauf.
    /// </summary>
    private void getCarReset()
    {
        GameObject carObject = GameObject.Find("CarReset");
        if (carObject != null)
        {
            carReset = carObject.GetComponent<CarReset>();
        }
        else
        {
            Debug.LogError("Das Objekt mit dem CarReset-Skript wurde nicht gefunden.");
        }
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
    /// Die IsCarMoving Methode überprüft, ob das Auto sich bewegt oder nicht.
    /// </summary>
    /// <returns>Gibt true zurück, wenn das Auto sich bewegt, sonst false.</returns>
    private bool IsCarMoving()
    {
        return carController.isCarMoving();
    }

    /// <summary>
    /// Die OnTriggerStay Methode wird aufgerufen, wenn ein anderer Collider im Trigger bleibt.
    /// Sie überprüft, ob das Auto korrekt im Parkplatz geparkt ist.
    /// </summary>
    /// <param name="other">Der andere Collider, der im Trigger bleibt.</param>
    private void OnTriggerStay(Collider other)
    {
        CarIsMoving = IsCarMoving();
        if (other.CompareTag("CarAgent") && parkingCollider != null)
        {
            carAgent.InTarget = true;

            if (IsFullyInsideParkingArea(other) && !CarIsMoving)
            {
                Debug.Log("Das Auto ist vollständig geparkt!");
                carAgent.IsParked = true;
            }
        }
    }

    /// <summary>
    /// Die OnTriggerExit Methode wird aufgerufen, wenn ein anderer Collider den Trigger verlässt.
    /// Sie setzt den InTarget Status des Autos auf false.
    /// </summary>
    /// <param name="other">Der andere Collider, der den Trigger verlässt.</param>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("CarAgent") && parkingCollider != null)
        {
            carAgent.InTarget = false;
        }
    }

    /// <summary>
    /// Die IsFullyInsideParkingArea Methode überprüft, ob das Auto vollständig im Parkplatz geparkt ist.
    /// </summary>
    /// <param name="carCollider">Der Collider des Autos.</param>
    /// <returns>Gibt true zurück, wenn das Auto vollständig im Parkplatz geparkt ist, sonst false.</returns>
    private bool IsFullyInsideParkingArea(Collider carCollider)
    {
        return (
            parkingCollider.bounds.Contains(carCollider.bounds.max)
            && parkingCollider.bounds.Contains(carCollider.bounds.min)
        );
    }

    /// <summary>
    /// Die DrawDebugLines Methode zeichnet Debug-Linien für die Grenzen des Parkplatzes und des Autos.
    /// </summary>
    /// <param name="carCollider">Der Collider des Autos.</param>
    private void DrawDebugLines(Collider carCollider)
    {
        // Linie für die Grenzen des Parkplatzes
        Vector3 parkingMin = parkingCollider.bounds.min;
        Vector3 parkingMax = parkingCollider.bounds.max;
        Debug.DrawLine(
            new Vector3(parkingMin.x, parkingMin.y, parkingMin.z),
            new Vector3(parkingMax.x, parkingMin.y, parkingMin.z),
            Color.red
        );
        Debug.DrawLine(
            new Vector3(parkingMin.x, parkingMin.y, parkingMin.z),
            new Vector3(parkingMin.x, parkingMax.y, parkingMin.z),
            Color.red
        );
        Debug.DrawLine(
            new Vector3(parkingMin.x, parkingMin.y, parkingMin.z),
            new Vector3(parkingMin.x, parkingMin.y, parkingMax.z),
            Color.red
        );
        Debug.DrawLine(
            new Vector3(parkingMax.x, parkingMin.y, parkingMin.z),
            new Vector3(parkingMax.x, parkingMax.y, parkingMin.z),
            Color.red
        );
        Debug.DrawLine(
            new Vector3(parkingMax.x, parkingMin.y, parkingMin.z),
            new Vector3(parkingMax.x, parkingMin.y, parkingMax.z),
            Color.red
        );
        Debug.DrawLine(
            new Vector3(parkingMin.x, parkingMin.y, parkingMax.z),
            new Vector3(parkingMax.x, parkingMin.y, parkingMax.z),
            Color.red
        );
        Debug.DrawLine(
            new Vector3(parkingMin.x, parkingMin.y, parkingMax.z),
            new Vector3(parkingMin.x, parkingMax.y, parkingMax.z),
            Color.red
        );
        Debug.DrawLine(
            new Vector3(parkingMax.x, parkingMin.y, parkingMax.z),
            new Vector3(parkingMax.x, parkingMax.y, parkingMax.z),
            Color.red
        );
        Debug.DrawLine(
            new Vector3(parkingMin.x, parkingMax.y, parkingMax.z),
            new Vector3(parkingMax.x, parkingMax.y, parkingMax.z),
            Color.red
        );

        // Linie für die Grenzen des Autos
        Vector3 carMin = carCollider.bounds.min;
        Vector3 carMax = carCollider.bounds.max;
        Debug.DrawLine(
            new Vector3(carMin.x, carMin.y, carMin.z),
            new Vector3(carMax.x, carMin.y, carMin.z),
            Color.blue
        );
        Debug.DrawLine(
            new Vector3(carMin.x, carMin.y, carMin.z),
            new Vector3(carMin.x, carMax.y, carMin.z),
            Color.blue
        );
        Debug.DrawLine(
            new Vector3(carMin.x, carMin.y, carMin.z),
            new Vector3(carMin.x, carMin.y, carMax.z),
            Color.blue
        );
        Debug.DrawLine(
            new Vector3(carMax.x, carMin.y, carMin.z),
            new Vector3(carMax.x, carMax.y, carMin.z),
            Color.blue
        );
        Debug.DrawLine(
            new Vector3(carMax.x, carMin.y, carMin.z),
            new Vector3(carMax.x, carMin.y, carMax.z),
            Color.blue
        );
        Debug.DrawLine(
            new Vector3(carMin.x, carMin.y, carMax.z),
            new Vector3(carMax.x, carMin.y, carMax.z),
            Color.blue
        );
        Debug.DrawLine(
            new Vector3(carMin.x, carMin.y, carMax.z),
            new Vector3(carMin.x, carMax.y, carMax.z),
            Color.blue
        );
        Debug.DrawLine(
            new Vector3(carMax.x, carMin.y, carMax.z),
            new Vector3(carMax.x, carMax.y, carMax.z),
            Color.blue
        );
        Debug.DrawLine(
            new Vector3(carMin.x, carMax.y, carMax.z),
            new Vector3(carMax.x, carMax.y, carMax.z),
            Color.blue
        );
    }
}
