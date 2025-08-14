using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorationSpawner : MonoBehaviour
{
    public string[] decorativeTags;

    public float spawnY = -6f;
    public Transform leftOuterLane;
    public Transform rightOuterLane;

    private float timer;
    private float nextSpawn;

    private void Start()
    {
        StartCoroutine(InitSpawn());
    }
    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (timer > nextSpawn)
        {
            nextSpawn = Random.Range(1.5f, 4f);
            timer = 0;

            var pos = new Vector3(Random.Range(leftOuterLane.position.x, rightOuterLane.position.y), spawnY);
            Spawn(pos);
        }

    }

    IEnumerator InitSpawn()
    {
        yield return null;
        nextSpawn = Random.Range(0f, 3f);

        for (int i = 0; i < 6; i++)
        {
            var pos = new Vector3(Random.Range(leftOuterLane.position.x, rightOuterLane.position.y), Random.Range(spawnY, -spawnY));
            Spawn(pos);
        }
    }

    void Spawn(Vector3 pos)
    {
        if (decorativeTags.Length > 0)
        {
            int index = Random.Range(0, decorativeTags.Length);
            ObjectPool.Instance.SpawnFromPool(decorativeTags[index], pos, Quaternion.identity, transform);
            //Instantiate(decoratives[index], pos, Quaternion.identity, transform);
        }
    }
}
