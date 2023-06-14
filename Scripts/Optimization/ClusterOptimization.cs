using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClusterOptimization : MonoBehaviour
{
    public float Radius = 1000f;
    public float PartitioningValue = 25f;

    private GameObject player;
    private Vector2Int playerCluster;
    private Dictionary<Vector2Int, List<GameObject>> clusters;

    void Start()
    {
        player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogError("Player object not found");
            return;
        }

        InitializeClusters();

        playerCluster = GetClusterKey(player.transform.position);
        EnableSurroundingClusters(playerCluster);
    }

    void Update()
    {
        Vector2Int currentPlayerCluster = GetClusterKey(player.transform.position);
        if (currentPlayerCluster != playerCluster)
        {
            DisableSurroundingClusters(playerCluster);
            playerCluster = currentPlayerCluster;
            EnableSurroundingClusters(playerCluster);
        }
    }

    void InitializeClusters()
    {
        clusters = new Dictionary<Vector2Int, List<GameObject>>();
        Vector2 optimizerPos2D = new Vector2(transform.position.x, transform.position.z);
        int numberOfClusters = Mathf.CeilToInt(Radius / PartitioningValue) * 2;

        for (int i = 0; i < numberOfClusters; i++)
        {
            for (int j = 0; j < numberOfClusters; j++)
            {
                Vector2Int clusterKey = new Vector2Int(i - numberOfClusters / 2, j - numberOfClusters / 2);
                Vector2 clusterCenter2D = optimizerPos2D + (Vector2)clusterKey * PartitioningValue;

                if (Vector2.Distance(clusterCenter2D, optimizerPos2D) <= Radius)
                    clusters[clusterKey] = new List<GameObject>();
            }
        }

        GameObject environment = GameObject.Find("Environment");

        if (environment == null)
        {
            Debug.LogError("Environment object not found");
            return;
        }

        foreach (Transform child in environment.transform) 
            AssignToCluster(child.gameObject);

        foreach (var key in clusters.Keys) 
            DisableCluster(key);
    }

    void AssignToCluster(GameObject obj)
    {
        Vector2Int clusterKey = GetClusterKey(obj.transform.position);

        if (clusters.ContainsKey(clusterKey))
            clusters[clusterKey].Add(obj);
    }

    Vector2Int GetClusterKey(Vector3 position)
    {
        float buffer = PartitioningValue * 0.1f;
        return new Vector2Int(
            Mathf.FloorToInt((position.x + buffer) / PartitioningValue),
            Mathf.FloorToInt((position.z + buffer) / PartitioningValue)
        );
    }

    void EnableSurroundingClusters(Vector2Int centerCluster)
    {
        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
                EnableCluster(new Vector2Int(centerCluster.x + i, centerCluster.y + j));
    }

    void DisableSurroundingClusters(Vector2Int centerCluster)
    {
        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
                DisableCluster(new Vector2Int(centerCluster.x + i, centerCluster.y + j));
    }

    void EnableCluster(Vector2Int key)
    {
        if (clusters.TryGetValue(key, out List<GameObject> cluster))
            foreach (GameObject obj in cluster)
                if (!obj.activeSelf) obj.SetActive(true);
    }

    void DisableCluster(Vector2Int key)
    {
        if (clusters.TryGetValue(key, out List<GameObject> cluster))
            foreach (GameObject obj in cluster)
                if (obj.activeSelf) obj.SetActive(false);
    }

    void OnDrawGizmos()
    {
        if (clusters == null) return;

        Gizmos.color = Color.green;
        foreach (var key in clusters.Keys)
        {
            Vector3 bottomLeft = new Vector3(key.x * PartitioningValue, 0, key.y * PartitioningValue);
            Vector3 topLeft = new Vector3(key.x * PartitioningValue, 0, key.y * PartitioningValue + PartitioningValue);
            Vector3 topRight = new Vector3(key.x * PartitioningValue + PartitioningValue, 0, key.y * PartitioningValue + PartitioningValue);
            Vector3 bottomRight = new Vector3(key.x * PartitioningValue + PartitioningValue, 0, key.y * PartitioningValue);

            Gizmos.DrawLine(bottomLeft, topLeft);
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
        }
    }
}
