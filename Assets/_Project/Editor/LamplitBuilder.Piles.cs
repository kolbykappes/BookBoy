using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static partial class LamplitBuilder
{
    /// <summary>Bakes a room's starting layout: the items are dropped into several random piles with a physics simulation run here in the
    /// Editor, then frozen where they land, so the scene file already holds the piles. Uses the same plan as the in-game "pile up"
    /// (<see cref="RoomZone.PlanDrops"/>). The simulation runs on proxy cubes in a throwaway preview scene (own physics scene, with
    /// just a floor), so the project's physics settings are never touched (changing Physics.simulationMode would make the Editor
    /// rewrite ProjectSettings/DynamicsManager.asset).</summary>
    public static string BakePiles(RoomZone zone, int seed)
    {
        var drops = zone.PlanDrops(seed);
        const float dt = 0.02f;
        const int roundSteps = 5;        // one item per pile every 5 steps (0.1 s), like the runtime pile-up
        const int maxSteps = 3000;       // 60 s of simulated time at most: the loop is bounded

        Scene preview = EditorSceneManager.NewPreviewScene();
        var proxies = new List<Rigidbody>(drops.Count);
        int steps = 0;
        try
        {
            var physics = preview.GetPhysicsScene();
            var floor = new GameObject("PileBakeFloor");
            var box = floor.AddComponent<BoxCollider>();
            box.size = new Vector3(60f, 1f, 60f);
            floor.transform.position = new Vector3(0f, -0.5f, 0f);
            SceneManager.MoveGameObjectToScene(floor, preview);

            int next = 0, lastDropStep = 0;
            foreach (var d in drops) lastDropStep = Mathf.Max(lastDropStep, d.Round * roundSteps);
            for (steps = 0; steps < maxSteps; steps++)
            {
                while (next < drops.Count && drops[next].Round * roundSteps <= steps)
                {
                    var d = drops[next++];
                    var proxy = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    proxy.name = "ProxyFor_" + (d.Book != null ? d.Book.name : "?");
                    proxy.transform.localScale = d.Book != null ? d.Book.transform.localScale : Vector3.one * 0.1f;
                    proxy.transform.SetPositionAndRotation(d.Origin, d.Rotation);
                    proxy.GetComponent<Collider>().sharedMaterial = Book.SolidMaterial;
                    var rb = proxy.AddComponent<Rigidbody>();
                    rb.mass = 0.4f;
                    rb.linearDamping = 0.05f;
                    rb.angularDamping = 0.6f;
                    rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                    rb.linearVelocity = d.Velocity;
                    rb.angularVelocity = d.Spin;
                    SceneManager.MoveGameObjectToScene(proxy, preview);
                    proxies.Add(rb);
                }

                physics.Simulate(dt);
                if (next >= drops.Count && steps > lastDropStep + 60)
                {
                    bool asleep = true;
                    foreach (var rb in proxies) if (!rb.IsSleeping()) { asleep = false; break; }
                    if (asleep) break;
                }
            }

            // copy where each proxy came to rest onto its real item
            for (int i = 0; i < drops.Count; i++)
            {
                var book = drops[i].Book;
                if (book == null) continue;
                book.transform.SetPositionAndRotation(proxies[i].transform.position, proxies[i].transform.rotation);
                var c = book.GetComponent<Collider>();
                if (c != null) { c.enabled = true; c.isTrigger = true; }
            }
        }
        finally
        {
            EditorSceneManager.ClosePreviewScene(preview);
            Physics.SyncTransforms();
        }

        return "piled " + drops.Count + " items into " + zone.LastPlan.PileCount + " piles in " + steps + " steps";
    }

    /// <summary>Re-bakes the piles of every built room (the items stay where they were built; only their positions change).</summary>
    public static string RebakeAllPiles()
    {
        RequireLamplitScene();
        var log = new System.Text.StringBuilder();
        foreach (var zone in Object.FindObjectsByType<RoomZone>(FindObjectsInactive.Include))
        {
            log.Append("room " + zone.roomNumber + ": " + BakePiles(zone, 1000 + zone.roomNumber) + "; ");
        }

        MarkDirty();
        return log.ToString();
    }
}
