using UnityEngine;

namespace InventorySystem.Demo.Runtime
{
    public class ItemSpawner : MonoBehaviour
    {
        #region Inspector Fields

        [SerializeField] private Item m_itemPrefab;
        [SerializeField, Min(1)] private int m_amount = 1;
        [SerializeField, Min(0f)] private float m_spawnExtentX = 1f;
        [SerializeField, Min(0f)] private float m_spawnExtentY = 1f;

        [Header("Gizmos")]
        [SerializeField] private bool m_drawSpawnArea = true;
        [SerializeField] private Color m_spawnAreaColor = new Color(1f, 0f, 0f, 0.25f);

        #endregion

        private void Start() => SpawnItems();

        private void OnDrawGizmos()
        {
            if (m_drawSpawnArea)
            {
                Gizmos.color = m_spawnAreaColor;

                Gizmos.DrawWireCube(transform.position, new(m_spawnExtentX * 2f, m_spawnExtentY * 2f));
            }
        }

        private void SpawnItems() 
        {
            for (int i = 0; i < m_amount; i++) SpawnItem();
        }
        private void SpawnItem()
        {
            float x = Random.Range(-m_spawnExtentX, m_spawnExtentX);
            float y = Random.Range(-m_spawnExtentY, m_spawnExtentY);

            Instantiate(m_itemPrefab, transform.position + new Vector3(x, y), Quaternion.identity);
        }
    }
}
