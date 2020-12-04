using UnityEngine;

namespace Assets.Scripts {
    public class DoorInteraction : MonoBehaviour {
        [SerializeField] private Animator m_animator; // Wrong naming ('m_'), private field must start with "_"
        private bool m_state; // Wrong naming ('m_'), private field must start with "_"

        public void HandleDoor() { // Wrong (You need to change a bit name to correspond logic)
            m_animator.SetTrigger("Open");
            m_state = !m_state;
        }

    }
}
