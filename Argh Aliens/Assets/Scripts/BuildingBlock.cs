﻿using UnityEngine;
using System.Collections;

public class BuildingBlock : MonoBehaviour {
    public Vector3 coords;
    public GameObject explosionParticles;
    public float buildingHeight;

    void OnCollisionEnter(Collision collision)
    {
        Destroy(true);
    }

    public void Destroy(bool setFire)
    {
        if (gameObject.activeSelf)
        {
            Instantiate(explosionParticles, gameObject.transform.position, Quaternion.identity);
            gameObject.SetActive(false);

            if (coords.y < buildingHeight -1)
            {
                if (setFire && coords.y > 0)
                {
                    LevelManager.instance.SetFireBuildingBelow(coords);
                }
                LevelManager.instance.BlowUpBuildingAbove(coords);
            }
        }
    }

    public void OnFire()
    {
        gameObject.transform.GetChild(0).gameObject.SetActive(true);
    }
}
