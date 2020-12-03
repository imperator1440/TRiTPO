using UnityEngine;

namespace Assets.Scripts {
    public class LeverInteraction : MonoBehaviour {
        [SerializeField]
        private DoorInteraction m_doorInteraction;
        [SerializeField]
        private Animator m_animator;

        [SerializeField] private Material m_default;
        [SerializeField] private Material m_outlined;
        private bool m_materialState;

        private void Start() {
            GetComponent<Renderer>().material = m_default;
        }

        public void HandleLever() {
            m_doorInteraction.HandleDoor();
            m_animator.SetTrigger("Open");
        }

        public void ChangeMaterial() {
            if (!m_materialState) GetComponent<Renderer>().material = m_outlined;
            if (m_materialState) GetComponent<Renderer>().material = m_default;
            m_materialState = !m_materialState;
        }
        private void Update() {

        }
    }
}