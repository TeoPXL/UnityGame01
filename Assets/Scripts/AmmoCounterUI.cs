using UnityEngine;
using System.Collections.Generic;

public class AmmoCounterUI : MonoBehaviour
{
    public GameObject bulletIconPrefab;
    private Transform ammoContainer;

    // Keep a list of all bullet icons
    private List<GameObject> bulletIcons = new();

    void Awake()
    {
        ammoContainer = transform;
    }

    public void UpdateAmmoDisplay(int currentAmmo)
    {
        if (ammoContainer == null)
            ammoContainer = transform;

        // Ensure we have enough icons in the pool
        while (bulletIcons.Count < currentAmmo)
        {
            GameObject newIcon = Instantiate(bulletIconPrefab, ammoContainer);
            bulletIcons.Add(newIcon);
        }

        // Enable the correct number of icons
        for (int i = 0; i < bulletIcons.Count; i++)
        {
            bulletIcons[i].SetActive(i < currentAmmo);
        }
    }
}