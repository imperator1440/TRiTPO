﻿using UnityEngine;

namespace Assets.Scripts {
    public class CamMove : MonoBehaviour {
        [SerializeField]
        private GameObject m_player; // Wrong naming ('m_')

        // Update is called once per frame
        private void Update() {
            if(m_player) transform.position = new Vector3(m_player.transform.position.x, m_player.transform.position.y, -10);
        }
    }
}
