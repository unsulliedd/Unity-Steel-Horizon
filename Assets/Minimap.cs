using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace mini
{
    public class Minimap : MonoBehaviour
    {
        public Transform playerTransform;
        // Start is called before the first frame update

        bool iscome;

        private void Awake()
        {
            iscome = true;
        }

     

        private void Update()
        {
            {
                GameObject[] player = GameObject.FindGameObjectsWithTag("Player");
                if (iscome)
                {
                    foreach (GameObject go in player)
                    {
                        if (go != null)
                        {
                            playerTransform = go.GetComponent<Transform>();
                           
                        }
                    }
                }
            }
        }

        private void LateUpdate()
        {
            Vector3 newPosition = playerTransform.position;
            newPosition.y = transform.position.y;
            transform.position = newPosition;

            transform.rotation = Quaternion.Euler(90f, playerTransform.eulerAngles.y, 0f);

        }

    }
}

