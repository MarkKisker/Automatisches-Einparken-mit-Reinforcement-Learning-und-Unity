using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Die ParkSlotRandomizer Klasse ist verantwortlich für die Zufällige Zuweisung von Parkplätzen.
/// </summary>
/// <author>Mark Kisker</author>
public class ParkSlotRandomizer : MonoBehaviour
{
    // Liste der Transform-Komponenten der Parkplätze
    [SerializeField]
    private List<Transform> parkSlotTransforms;

    // Liste der zu parkenden Objekte
    [SerializeField]
    private List<GameObject> Objects;

    /// <summary>
    /// Die Start Methode wird einmalig zu Beginn der Szene aufgerufen.
    /// Sie ruft die Methode zur Zuweisung von zufälligen Parkplätzen auf.
    /// </summary>
    void Start()
    {
        AssignRandomSlots();
    }

    /// <summary>
    /// Die AssignRandomSlots Methode weist jedem Auto in der Liste einen zufälligen Parkplatz zu.
    /// Sie mischt die Liste der Autos und weist jedem Auto einen Parkplatz aus der Liste der Parkplätze zu.
    /// </summary>
    public void AssignRandomSlots()
    {
        // Mische die Liste der Parkplätze und Autos
        ShuffleList(Objects);

        // Stelle sicher, dass die Anzahl der Parkplätze mindestens so groß wie die Anzahl der Autos ist
        int numSlots = Mathf.Min(parkSlotTransforms.Count, Objects.Count);

        for (int i = 0; i < numSlots; i++)
        {
            Transform slotTransform = parkSlotTransforms[i];
            GameObject objects = Objects[i];

            // Weise jedem Auto einen Parkplatz zu
            objects.transform.position = slotTransform.position;
        }
    }

    /// <summary>
    /// Die ShuffleList Methode mischt eine gegebene Liste.
    /// Sie verwendet den Fisher-Yates-Algorithmus zum Mischen der Liste.
    /// </summary>
    /// <param name="list">Die zu mischende Liste.</param>
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
