using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTrail : MonoBehaviour
{
    [Header("Mesh Settings")]
    public float meshRefreshRate = 0.1f;
    public float meshDestroyDelay = 3f;
    public Transform positionToSpawn;

    [Header("Shader Settings")]
    public Material trailMaterial;
    public string shaderVarAlpha = "_Alpha";
    public float fadeRate = 0.05f;
    public float fadeRefreshRate = 0.05f;

    private SkinnedMeshRenderer[] skinnedMeshRenderers;
    private bool isTrailActive = false;

    public void SetTrailActive(bool active, float duration = 0)
    {
        if (active && !isTrailActive)
        {
            Debug.Log("Trail Aktif!");
            isTrailActive = true;
            StartCoroutine(ActivateTrail(duration));
        }
        else if (!active)
        {
            isTrailActive = false;
        }
    }

    IEnumerator ActivateTrail(float duration)
    {
        float timer = 0;

        while (isTrailActive)
        {
            if (duration > 0)
            {
                timer += meshRefreshRate;
                if (timer >= duration) isTrailActive = false;
            }

            if (skinnedMeshRenderers == null || skinnedMeshRenderers.Length == 0)
            {
                skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
                Debug.Log("SkinnedMeshRenderers ditemukan: " + skinnedMeshRenderers.Length);
            }

            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                if (!skinnedMeshRenderers[i].gameObject.activeInHierarchy) continue;

                GameObject gObj = new GameObject();
                gObj.name = "TrailGhost_" + skinnedMeshRenderers[i].name;

                gObj.transform.position = skinnedMeshRenderers[i].transform.position;
                gObj.transform.rotation = skinnedMeshRenderers[i].transform.rotation;
                gObj.transform.localScale = skinnedMeshRenderers[i].transform.lossyScale;

                MeshRenderer mr = gObj.AddComponent<MeshRenderer>();
                MeshFilter mf = gObj.AddComponent<MeshFilter>();

                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; 
                mr.receiveShadows = false;

                Mesh mesh = new Mesh();
                skinnedMeshRenderers[i].BakeMesh(mesh, true);

                mf.mesh = mesh;

                int subMeshCount = mesh.subMeshCount;
                Material[] mats = new Material[subMeshCount];
                for (int m = 0; m < subMeshCount; m++)
                {
                    mats[m] = trailMaterial;
                }
                mr.materials = mats;

                StartCoroutine(AnimateMaterialFloat(mr.materials, 0, fadeRate, fadeRefreshRate));
                Destroy(gObj, meshDestroyDelay); 
                Destroy(mesh, meshDestroyDelay);
            }

            yield return new WaitForSeconds(meshRefreshRate);
        }
    }

    IEnumerator AnimateMaterialFloat(Material[] mats, float goal, float rate, float refreshRate)
    {
        float valueToAnimate = mats[0].GetFloat(shaderVarAlpha);

        while (valueToAnimate > goal)
        {
            valueToAnimate -= rate;
            foreach (Material m in mats)
            {
                m.SetFloat(shaderVarAlpha, valueToAnimate);
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }
}