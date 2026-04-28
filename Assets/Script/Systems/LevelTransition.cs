using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelTransition : Interactable
{
    [Header("Level Settings")]
    [SerializeField] private string sceneToLoad;
    [SerializeField] private string spawnPointName = "SpawnPoint"; 

    public override void Interact()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        SceneManager.LoadScene(sceneToLoad);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        GameObject spawnPoint = GameObject.Find(spawnPointName);

        if (player != null && spawnPoint != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.transform.position = spawnPoint.transform.position;
            player.transform.rotation = spawnPoint.transform.rotation;

            if (cc != null) cc.enabled = true;

            Debug.Log("Player berhasil muncul di: " + spawnPointName);
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}