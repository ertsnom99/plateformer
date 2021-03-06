﻿using UnityEngine;

public class LevelEndSequence : MonoBehaviour
{
    [Header("Scene Change")]
    [SerializeField]
    private Inputs _forcedControls;
    [SerializeField]
    private int _sceneToLoad;
    [SerializeField]
    private bool _deleteAmbientManager = false;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag(GameManager.PlayerTag))
        {
            GameManager.Instance.LoadNextLevel(_forcedControls, _sceneToLoad, _deleteAmbientManager);
        }
        else if (col.CompareTag(GameManager.EnemyTag))
        {
            PossessableCharacterController controller = col.GetComponent<PossessableCharacterController>();

            if (controller.IsPossessed)
            {
                GameObject player = controller.Unpossess();

                GameManager.Instance.LoadNextLevel(_forcedControls, _sceneToLoad);
            }
        }
    }
}
