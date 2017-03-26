using System.Collections.Generic;
using UnityEngine;

public class MaterialResetter : MonoBehaviour {

    List<Material> tintedMaterials = new List<Material>();

	public void OnMaterialTinted(Material m)
    {
        tintedMaterials.Add(m);
    }

    public void ResetTintedMaterials()
    {
        foreach(Material m in tintedMaterials)
        {
            m.SetColor("_EmissionColor", Color.black);
        }
    }
}