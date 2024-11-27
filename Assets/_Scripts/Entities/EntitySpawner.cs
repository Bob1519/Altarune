using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EntitySpawner : Entity {

    [System.Serializable]
    private class EntityGroup {
        public Entity entityPrefab;
        public int spawnCount;
        public bool needsNavMesh;
    }

    [SerializeField] private DefaultSummonProperties summonProperties;
    [SerializeField] private List<EntityGroup> spawnGroups;
    [SerializeField] private int maxEnemiesAtOnce;
    [SerializeField] private Vector2 spawnDelayRange;
    [SerializeField] private Vector2 spawnRadius;

    private readonly HashSet<Entity> linkedEnemies = new();

    void OnEnable() => QueueSpawn();

    private void TrySpawn() {
        if (linkedEnemies.Count < maxEnemiesAtOnce) {
            int groupIndex = Random.Range(0, spawnGroups.Count);
            EntityGroup group = spawnGroups[groupIndex];

            Vector2 dir = SpatialUtils.RandomPointInRing(spawnRadius.x, spawnRadius.y);
            Vector3 spawnPos = transform.position + new Vector3(dir.x, 0, dir.y);
            if (group.needsNavMesh && NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 
                                                             spawnRadius.y, NavMesh.AllAreas)) {
                spawnPos = hit.position;
            }

            Entity entity = Instantiate(group.entityPrefab, spawnPos, Quaternion.identity);
            linkedEnemies.Add(entity);

            group.spawnCount--;
            if (group.spawnCount == 0) {
                spawnGroups.Remove(group);
                if (spawnGroups.Count == 0) {
                    Perish();
                }
            }

            if (!Perished) QueueSpawn();
        }
    }

    private void Entity_OnPerish(BaseObject baseObject) {
        baseObject.OnPerish -= Entity_OnPerish;
        Entity entity = baseObject as Entity;
        linkedEnemies.Remove(entity);
        QueueSpawn();
    }

    private void QueueSpawn() {
        float delayTime = Random.Range(spawnDelayRange.x, spawnDelayRange.y);
        StartCoroutine(IDelaySpawn(delayTime));
    }

    private IEnumerator IDelaySpawn(float delayTime) {
        yield return new WaitForSeconds(delayTime);
        TrySpawn();
    }

    private IEnumerator IMaterialize(bool spawn) {
        float target = spawn ? summonProperties.growTime : 0;
        Transform t = objectBody;

        float lerpVal, growTimer = summonProperties.growTime - target;
        while (Mathf.Abs(growTimer - target) > Mathf.Epsilon) {
            growTimer = Mathf.MoveTowards(growTimer, target, Time.deltaTime);
            lerpVal = growTimer / summonProperties.growTime;
            t.localScale = new Vector3(summonProperties.growthCurveXZ.Evaluate(lerpVal),
                                       summonProperties.growthCurveY.Evaluate(lerpVal),
                                       summonProperties.growthCurveXZ.Evaluate(lerpVal));
            yield return null;
        }

        if (!spawn) Destroy(gameObject, 0.2f);
    }

    public override void Perish() {
        base.Perish();
        DetachModules();

        StopAllCoroutines();
        StartCoroutine(IMaterialize(false));
    }
}