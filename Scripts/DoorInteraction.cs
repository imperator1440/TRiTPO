using UnityEngine;

namespace Assets.Scripts {
    public class DoorInteraction : MonoBehaviour {
        [SerializeField] private Animator m_animator;
        private bool m_state;

        public void HandleDoor() {
            m_animator.SetTrigger("Open");
            m_state = !m_state;
        }

    }
}
