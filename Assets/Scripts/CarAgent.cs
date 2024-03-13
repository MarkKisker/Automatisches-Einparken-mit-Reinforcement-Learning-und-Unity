using System;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

/// <summary>
/// Die Klasse CarAgent ist verantwortlich für die Steuerung eines Autos in einer Unity-Umgebung.
/// Das Hauptziel des Agenten ist es, das Auto einzuparken.
/// </summary>
/// <author>Mark Kisker</author>
public class CarAgent : Agent
{
    private Car_Controller carController;
    private CarReset carReset;

    private bool targetToRight = false;
    private bool targetToLeft = false;
    private float nearestDistanceToWallInTarget = 0f;
    private bool hasHitTargetFirstTime = true;
    private bool inTarget = false;
    private bool findParkingSpot = true;
    private bool isLookingForSpot;
    private bool isParked = false;
    private float lastDistanceToTarget = 0f;
    private float nearestDistanceToTarget = 0f;
    private bool isParallelInSlot = false;
    private bool isFacingSpot = false;
    private int steps = 0;
    private bool isCollided = false;

    private RayPerceptionSensorComponent3D rayPerceptionSensorComponent;

    public bool IsParked
    {
        set => isParked = value;
    }

    public bool IsCollided
    {
        set => isCollided = value;
    }

    public bool InTarget
    {
        set => inTarget = value;
    }

    /// <summary>
    /// Initialisiert den Agenten. Diese Methode wird von Unity aufgerufen, wenn der Agent initialisiert wird.
    /// Sie holt die CarController und CarReset Instanzen, setzt die RayPerceptionSensorComponent3D Komponente,
    /// setzt den isLookingForSpot Zustand und ruft die Reset Methode auf.
    /// </summary>
    public override void Initialize()
    {
        GetCarController();
        getCarReset();

        rayPerceptionSensorComponent = GetComponent<RayPerceptionSensorComponent3D>();
        isLookingForSpot = findParkingSpot;
        Reset();
    }

    /// <summary>
    /// Diese Methode sucht das GameObject "CarAgent" und holt die Car_Controller Komponente davon.
    /// Wenn das GameObject oder die Car_Controller Komponente nicht gefunden werden kann, wird eine Ausnahme ausgelöst.
    /// </summary>
    private void GetCarController()
    {
        try
        {
            GameObject carObject = GameObject.Find("CarAgent");
            if (carObject != null)
            {
                carController = carObject.GetComponent<Car_Controller>();
                if (carController == null)
                {
                    throw new Exception("Error: Das Car_Controller-Skript wurde nicht gefunden");
                }
            }
            else
            {
                throw new Exception(
                    "Error: Das Objekt mit dem Car_Controller-Skript wurde nicht gefunden."
                );
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }
    }

    /// <summary>
    /// Diese Methode sucht das GameObject "CarReset" und holt die CarReset Komponente davon.
    /// Wenn das GameObject oder die CarReset Komponente nicht gefunden werden kann, wird eine Ausnahme ausgelöst.
    /// </summary>
    private void getCarReset()
    {
        try
        {
            GameObject carObject = GameObject.Find("CarReset");
            if (carObject != null)
            {
                carReset = carObject.GetComponent<CarReset>();
            }
            else
            {
                throw new Exception(
                    "Error: Das Objekt mit dem CarReset-Skript wurde nicht gefunden."
                );
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }
    }

    /// <summary>
    /// Die FixedUpdate Methode wird von Unity in jedem festen Frametakt aufgerufen.
    /// Sie überprüft, ob eine Kollision aufgetreten ist und beendet gegebenenfalls die Episode.
    /// Wenn der Agent einen Parkplatz sucht, steuert er das Auto und sucht nach einem Parkplatz.
    /// Wenn der Agent keinen Parkplatz sucht, aktualisiert er den Status der Wahrnehmungssensoren des Autos.
    /// Schließlich fordert sie eine Entscheidung an, wenn der Agent nicht nach einem Parkplatz sucht.
    /// </summary>
    private void FixedUpdate()
    {
        if (isCollided)
        {
            AddReward(-0.5f);
            EndEpisode();
        }
        // If Looking for spot is enabled, the car will drive and try to find a spot first
        if (isLookingForSpot)
        {
            CruiseControl(5f);
            spotParkingSpot();
        }
        else
        {
            updateCarRayPerceptionStatus();
        }

        // Get decision from python by requesting the next action
        if (!isLookingForSpot)
        {
            RequestDecision();
        }
    }

    /// <summary>
    /// Sammelt Beobachtungen aus der Umgebung. Diese Methode wird von Unity aufgerufen, um Beobachtungen vom Agenten zu sammeln.
    /// Sie fügt die aktuelle Geschwindigkeit des Autos in KPH als Beobachtung hinzu.
    /// Wenn die Car_Controller Instanz nicht zugewiesen ist, wird eine Ausnahme ausgelöst.
    /// </summary>
    /// <param name="sensor">Der Vektor-Sensor, der die Beobachtungen sammelt.</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        try
        {
            if (carController != null)
            {
                sensor.AddObservation(carController.getCurrentSpeedInKPH());
            }
            else
            {
                throw new Exception("Error: Car_Controller ist nicht zugewiesen.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }
    }

    /// <summary>
    /// Setzt den Zustand des Agenten zurück. Diese Methode wird von Unity aufgerufen, um den Agenten zurückzusetzen.
    /// Sie setzt alle relevanten Variablen auf ihre Anfangswerte zurück und ruft die ResetCar Methode der CarReset Instanz auf.
    /// Wenn während des Zurücksetzens ein Fehler auftritt, wird eine Ausnahme ausgelöst.
    /// </summary>
    private void Reset()
    {
        try
        {
            nearestDistanceToWallInTarget = 0f;
            findParkingSpot = true;
            isLookingForSpot = true;
            isParked = false;
            isCollided = false;
            isParallelInSlot = false;
            isFacingSpot = false;
            carReset.ResetCar();
            steps = 0;
            lastDistanceToTarget = 0f;
            nearestDistanceToTarget = 0f;
            targetToRight = false;
            targetToLeft = false;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }
    }

    /// <summary>
    /// Führt Aktionen basierend auf den vom Agenten empfangenen Aktionen aus.
    /// Diese Methode wird von Unity aufgerufen, wenn der Agent Aktionen empfängt.
    /// Sie steuert das Auto basierend auf den empfangenen Aktionen, aktualisiert die Anzahl der Schritte,
    /// überprüft, ob das Auto geparkt ist und beendet gegebenenfalls die Episode,
    /// aktualisiert den Status von hasHitTargetFirstTime und berechnet und fügt eine Belohnung hinzu.
    /// Wenn während der Ausführung der Aktionen ein Fehler auftritt, wird eine Ausnahme ausgelöst.
    /// </summary>
    /// <param name="actions">Die vom Agenten empfangenen Aktionen.</param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        try
        {
            float steering = actions.ContinuousActions[0];
            float accel = actions.ContinuousActions[1];
            if (!isLookingForSpot) //spot wurde gefunden
            {
                if (targetToRight)
                {
                    carController.MoveCar(1f, accel);
                }
                else if (targetToLeft)
                {
                    carController.MoveCar(-1f, accel);
                }
                else
                {
                    carController.MoveCar(steering, accel);
                }
            }

            steps++;

            if (isParked)
            {
                AddReward(1f);
                EndEpisode();
            }
            else
            {
                AddReward(-0.001f);
            }

            if (hasHitTargetFirstTime && inTarget)
            {
                hasHitTargetFirstTime = false;
                AddReward(0.2f);
            }
            float reward = CalculateReward();
            AddReward(reward);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }
    }

    /// <summary>
    /// Diese Methode wird von Unity aufgerufen, um Heuristiken vom Agenten zu sammeln.
    /// Sie liest die Eingaben des Benutzers für die Lenkung und Beschleunigung und fügt sie den kontinuierlichen Aktionen hinzu.
    /// </summary>
    /// <param name="actionsOut">Die vom Agenten ausgeführten Aktionen.</param>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActionsOut = actionsOut.ContinuousActions;

        float steering = Input.GetAxis("Horizontal"); //-1 to 1
        float accel = Input.GetAxis("Vertical"); //-1 to 1
        continuousActionsOut[0] = steering;
        continuousActionsOut[1] = accel;
    }

    // CruiseControl method to control the car's speed
    /// <summary>
    /// Die CruiseControl Methode kontrolliert die Geschwindigkeit des Autos.
    /// Wenn die aktuelle Geschwindigkeit des Autos kleiner als die gewünschte Geschwindigkeit ist, wird das Auto beschleunigt.
    /// Wenn die aktuelle Geschwindigkeit des Autos größer als die gewünschte Geschwindigkeit ist, wird das Auto abgebremst.
    /// </summary>
    /// <param name="speed">Die gewünschte Geschwindigkeit in KPH.</param>
    private void CruiseControl(float speed)
    {
        if (carController.getCurrentSpeedInKPH() < speed)
        {
            carController.MoveCar(0, 0.2f);
        }
        else if (carController.getCurrentSpeedInKPH() > speed)
        {
            carController.MoveCar(0, -0.5f);
        }
    }

    /// <summary>
    /// Die RayPerceptionMeasurements Methode misst Entfernungen mit Hilfe von Ray Perception Sensoren.
    /// Sie nimmt die RayPerceptionInput von der RayPerceptionSensorComponent3D Komponente und führt eine Wahrnehmung durch.
    /// Die resultierenden RayOutputs werden dann verwendet, um Listen von Entfernungen und Trefferinformationen für die linke, rechte, vordere und hintere Richtung zu erstellen.
    /// </summary>
    /// <param name="LogValues">Ein optionaler Parameter, der bestimmt, ob die Werte geloggt werden sollen oder nicht. Standardmäßig ist er auf false gesetzt.</param>
    /// <returns>Gibt vier Listen von Tupeln zurück, die die Entfernungen und Trefferinformationen für die linke, rechte, vordere und hintere Richtung enthalten.</returns>
    private (
        List<(float distance, bool hit)>,
        List<(float distance, bool hit)>,
        List<(float distance, bool hit)>,
        List<(float distance, bool hit)>
    ) RayPerceptionMeasurements(bool LogValues = false)
    {
        RayPerceptionInput rayPerceptionIn = rayPerceptionSensorComponent.GetRayPerceptionInput();
        RayPerceptionOutput rayPerceptionOut = RayPerceptionSensor.Perceive(rayPerceptionIn);
        RayPerceptionOutput.RayOutput[] rayOutputs = rayPerceptionOut.RayOutputs;

        int rayAmount = rayOutputs.Length - 1;
        List<(float distance, bool hit)> rayValuesLeft = new List<(float, bool)>();
        List<(float distance, bool hit)> rayValuesRight = new List<(float, bool)>();
        List<(float distance, bool hit)> rayValuesFront = new List<(float, bool)>();
        List<(float distance, bool hit)> rayValuesBack = new List<(float, bool)>();

        // Extracting front and back distances
        float rayDistanceFront = rayOutputs[0].HitFraction * 2;
        int rayFrontHitTargetIndex = rayOutputs[0].HitTagIndex;
        bool rayFrontHitTarget = rayFrontHitTargetIndex == 0;
        rayValuesFront.Add((rayDistanceFront, rayFrontHitTarget));

        float rayDistanceBack = rayOutputs[rayAmount - 1].HitFraction * 2;
        int rayBackHitTargetIndex = rayOutputs[rayAmount - 1].HitTagIndex;
        bool rayBackHitTarget = rayBackHitTargetIndex == 0;
        rayValuesBack.Add((rayDistanceBack, rayBackHitTarget));

        // Loop through the ray outputs to fill left and right lists
        for (int i = 1; i < rayAmount - 1; i++)
        {
            int hitIndex = rayOutputs[i].HitTagIndex;
            bool hit = hitIndex == 0; // Check if the ray hit goal
            float distance = rayOutputs[i].HitFraction * 2; // Normalize the distance
            if (i % 2 == 0)
            {
                rayValuesLeft.Add((distance, hit));
            }
            else
            {
                rayValuesRight.Add((distance, hit));
            }
        }

        if (LogValues)
        {
            //Debug.Log("Left rays:");
            foreach (var rayValue in rayValuesLeft)
            {
                Debug.Log($"Distance: {rayValue.distance}, Hit: {rayValue.hit}");
            }

            //Debug.Log("Right rays:");
            foreach (var rayValue in rayValuesRight)
            {
                Debug.Log($"Distance: {rayValue.distance}, Hit: {rayValue.hit}");
            }
        }

        return (rayValuesLeft, rayValuesRight, rayValuesFront, rayValuesBack);
    }

    /// <summary>
    /// Die updateCarRayPerceptionStatus Methode aktualisiert den Status der Wahrnehmungssensoren des Autos.
    /// Sie führt eine RayPerceptionMessung durch und verarbeitet die resultierenden Messungen, um den Status von isParallelInSlot, targetToRight und targetToLeft zu aktualisieren.
    /// Sie überprüft auch, ob das Auto im Ziel ist und aktualisiert entsprechend den nearestDistanceToWallInTarget.
    /// Schließlich aktualisiert sie den nearestDistanceToTarget basierend auf dem letzten Abstand zum Ziel.
    /// </summary>
    private void updateCarRayPerceptionStatus()
    {
        //Werte zwischen 0 und 1, wenn 1 dann kein hit
        var rpMeasurements = RayPerceptionMeasurements();
        const float parallelTolerance = 0.2f;
        isParallelInSlot = false;
        targetToRight = false;
        targetToLeft = false;

        //rayValuesLeft
        foreach (var Value in rpMeasurements.Item1)
        {
            var distance = Value.distance;
            var hitTarget = Value.hit;
            if (hitTarget)
            {
                lastDistanceToTarget = distance;
            }
        }

        //rayValuesRight
        foreach (var Value in rpMeasurements.Item2)
        {
            var distance = Value.distance;
            var hitTarget = Value.hit;
            if (hitTarget)
            {
                lastDistanceToTarget = distance;
            }
        }

        //rayValuesFront
        foreach (var Value in rpMeasurements.Item3)
        {
            var distance = Value.distance;
            var hitTarget = Value.hit;
            if (inTarget)
            {
                if (nearestDistanceToWallInTarget == 0f)
                {
                    nearestDistanceToWallInTarget = distance;
                }
                if (distance < nearestDistanceToWallInTarget)
                {
                    var distanceToWallReward = (1 - distance) / 6;
                    AddReward(distanceToWallReward);
                    nearestDistanceToWallInTarget = distance;
                }
            }

            if (hitTarget)
            {
                isFacingSpot = true;
                lastDistanceToTarget = distance;
            }
            else
            {
                isFacingSpot = false;
            }
        }

        //rayValuesBack
        foreach (var Value in rpMeasurements.Item4)
        {
            var distance = Value.distance;
            var hitTarget = Value.hit;
            if (hitTarget)
            {
                lastDistanceToTarget = distance;
            }
        }

        if (inTarget)
        {
            //Sensoren mitte links und mitte rechts
            var RayDistanceLeft = rpMeasurements.Item1.ElementAt(2).distance * 2;
            var RayDistanceRight = rpMeasurements.Item2.ElementAt(2).distance * 2;
            var isParallel = Mathf.Abs(RayDistanceLeft - RayDistanceRight) < parallelTolerance;
            isParallelInSlot = isParallel;
        }

        if (!inTarget)
        {
            var isPositioned =
                Mathf.Abs(
                    rpMeasurements.Item1.ElementAt(0).distance * 2
                        - rpMeasurements.Item2.ElementAt(0).distance * 2
                ) < parallelTolerance;
            nearestDistanceToWallInTarget = 0f;
            var isTargetToRight =
                rpMeasurements.Item2.ElementAt(0).hit && !rpMeasurements.Item3.ElementAt(0).hit;
            var isTargetToLeft =
                rpMeasurements.Item1.ElementAt(0).hit && !rpMeasurements.Item3.ElementAt(0).hit;
            if (isTargetToRight)
            {
                targetToRight = true;
            }
            else if (isTargetToLeft)
            {
                targetToLeft = true;
            }
        }
        if (nearestDistanceToTarget == 0f)
        {
            nearestDistanceToTarget = lastDistanceToTarget;
        }
    }

    /// <summary>
    /// Die spotParkingSpot Methode sucht nach einem Parkplatz für das Auto.
    /// Sie führt eine RayPerceptionMessung durch und überprüft, ob ein Ziel getroffen wurde.
    /// Wenn ein Ziel getroffen wurde, wird der letzte Abstand zum Ziel aktualisiert und der Status von hasFoundTarget auf true gesetzt.
    /// Am Ende der Methode wird der Status von isLookingForSpot basierend auf dem Wert von hasFoundTarget aktualisiert.
    /// </summary>
    private void spotParkingSpot()
    {
        //Werte zwischen 0 und 1, wenn 1 dann kein hit
        var rpMeasurements = RayPerceptionMeasurements();

        var hasFoundTarget = false;

        //rayValuesLeft
        foreach (var Value in rpMeasurements.Item1)
        {
            var distance = Value.distance;
            var hitTarget = Value.hit;
            if (hitTarget)
            {
                lastDistanceToTarget = distance;
                hasFoundTarget = true;
            }
        }

        //rayValuesRight
        foreach (var Value in rpMeasurements.Item2)
        {
            var distance = Value.distance;
            var hitTarget = Value.hit;
            if (hitTarget)
            {
                lastDistanceToTarget = distance;
                hasFoundTarget = true;
            }
        }

        //rayValuesFront
        foreach (var Value in rpMeasurements.Item3)
        {
            var distance = Value.distance;
            var hitTarget = Value.hit;
            if (hitTarget)
            {
                lastDistanceToTarget = distance;
                hasFoundTarget = true;
            }
        }

        //rayValuesBack
        foreach (var Value in rpMeasurements.Item4)
        {
            var distance = Value.distance;
            var hitTarget = Value.hit;
            if (hitTarget)
            {
                lastDistanceToTarget = distance;
                hasFoundTarget = true;
            }
        }

        if (hasFoundTarget)
        {
            isLookingForSpot = false;
        }
        else
        {
            isLookingForSpot = true;
        }
    }

    /// <summary>
    /// Die CalculateReward Methode berechnet die Belohnung für den Agenten basierend auf verschiedenen Faktoren.
    /// Sie berechnet die Belohnung für die Entfernung zum Ziel und die Parallelität des Autos im Parkplatz.
    /// Wenn das Auto nicht im Ziel ist, wird eine Belohnung hinzugefügt, wenn das Auto näher am Ziel ist.
    /// Wenn das Auto im Ziel ist und parallel zum Parkplatz steht, wird eine Belohnung hinzugefügt.
    /// Am Ende der Methode werden alle Belohnungskomponenten summiert und zurückgegeben.
    /// </summary>
    /// <param name="debug">Ein optionaler Parameter, der bestimmt, ob Debug-Informationen geloggt werden sollen oder nicht. Standardmäßig ist er auf false gesetzt.</param>
    /// <returns>Gibt die berechnete Belohnung zurück.</returns>
    private float CalculateReward(bool debug = false)
    {
        var reward = 0f;
        var distanceToTargetReward = 0f;
        var parallelReward = 0f;

        //Soll den Agent näher an das Ziel bringen
        if (!inTarget)
        {
            var isFacingSpotMultiplier = 0.5f;
            if (isFacingSpot)
            {
                isFacingSpotMultiplier = 1.0f;
            }
            if (lastDistanceToTarget < nearestDistanceToTarget)
            {
                nearestDistanceToTarget = lastDistanceToTarget;
                distanceToTargetReward +=
                    (1 - nearestDistanceToTarget) / 5 * isFacingSpotMultiplier;
            }
            if (lastDistanceToTarget > nearestDistanceToTarget + 0.4)
            {
                distanceToTargetReward -= 0.05f;
                nearestDistanceToTarget = lastDistanceToTarget;
            }
        }

        if (inTarget && isParallelInSlot)
        {
            parallelReward += 0.1f;
        }

        if (debug)
        {
            Debug.Log("distanceToTargetReward: " + distanceToTargetReward);
            Debug.Log("parallelReward: " + parallelReward);
        }

        // Summieren aller Belohnungskomponenten
        reward += distanceToTargetReward;
        reward += parallelReward;
        return reward;
    }

    public override void OnEpisodeBegin()
    {
        Reset();
    }
}
