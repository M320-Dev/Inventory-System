using UnityEditor;
using UnityEngine;

namespace Room.Runtime
{
    public class Room : MonoBehaviour
    {
        #region Inspector Fields

        [SerializeField] private Camera m_camera;

        [Header("Sides")]
        [SerializeField] private Transform m_floor;
        [SerializeField] private Transform m_ceiling;
        [SerializeField] private Transform m_wallLeft;
        [SerializeField] private Transform m_wallRight;

        #endregion

        private void OnValidate()
        {
            Setup();
        }

        private void Awake()
        {
            Setup();
        }

        [ContextMenu("Setup")]
        private void Setup()
        {
            if (!m_camera) return;

            Vector3 cameraPosition = m_camera.transform.position;
            transform.position = new Vector3(cameraPosition.x, cameraPosition.y, 0f);

            float height = m_camera.orthographicSize * 2f;
            float width = height * m_camera.aspect;

            TrySetupRoom(width, height);
        }

        #region Try Setup Room

        private void TrySetupRoom(float width, float height)
        {
            float widthHalf = width * 0.5f;
            float heightHalf = height * 0.5f;

            TrySetupHorizontalRoomComponent(m_floor, width, heightHalf, -1f);
            TrySetupHorizontalRoomComponent(m_ceiling, width, heightHalf, 1f);
            TrySetupVerticalRoomComponent(m_wallLeft, widthHalf, height, -1f);
            TrySetupVerticalRoomComponent(m_wallRight, widthHalf, height, 1f);
        }
        private void TrySetupHorizontalRoomComponent(Transform roomComponent, float width, float heightHalf, float sign)
        {
            if (roomComponent)
            {
                roomComponent.transform.localScale = new Vector3(width + 1f, 1f, 1f);
                roomComponent.transform.localPosition = new Vector3(0f, heightHalf * sign, 0f);
            }
        }
        private void TrySetupVerticalRoomComponent(Transform roomComponent, float widthHalf, float height, float sign)
        {
            if (roomComponent)
            {
                roomComponent.transform.localScale = new Vector3(1f, height + 1f, 1f);
                roomComponent.transform.localPosition = new Vector3(widthHalf * sign, 0f, 0f);
            }
        }

        #endregion
    }
}
