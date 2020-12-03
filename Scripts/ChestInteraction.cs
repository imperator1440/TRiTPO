using UnityEngine;

namespace Assets.Scripts {
    public class ChestInteraction : MonoBehaviour {
        [SerializeField]
        private Animator m_animator;
        [SerializeField]
        private Material m_default;
        [SerializeField]
        private Material m_outlined;

        private const int m_gold = 50;

        private bool m_materialState;
        private bool m_opened ;

        private void Start() {
            GetComponent<Renderer>().material = m_default;
        }

        public void ChangeMaterial() {
            if (!m_opened) {
                GetComponent<Renderer>().material = m_materialState ? m_default : m_outlined;
                m_materialState = !m_materialState;
            }
        }

        public void Open() {
            m_opened = true;
            GetComponent<Renderer>().material = m_default;
            m_animator.SetTrigger("Open");
        }

        public int GetGold() {
            return m_gold;
        }
    }
}
