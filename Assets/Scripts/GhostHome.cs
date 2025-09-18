using System.Collections;
using UnityEngine;

public class GhostHome : GhostBehavior
{
    [Header("Waypoints")]
    public Transform inside;
    public Transform outside;
    public Transform outsideLeft;
    public Transform outsideRight;

    public enum ExitMode { Random, UpOnly, LeftOnly, RightOnly, Sequence }

    [Header("Config")]
    [Tooltip("Modo de saída do corral")]
    public ExitMode exitMode = ExitMode.Random;

    [Tooltip("Tempo do LERP até 'inside' e depois até a saída")]
    public float travelDuration = 0.5f;

    [Tooltip("Centraliza no grid ao final da saída")]
    public bool snapAtEndToGrid = true;

    private static int _seqIndex = 0;

    private void OnEnable()
    {
        StopAllCoroutines();
    }

    private void OnDisable()
    {
        if (gameObject.activeInHierarchy)
            StartCoroutine(ExitTransition());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (enabled && collision.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
            ghost.Movement.SetDirection(-ghost.Movement.direction);
    }

    private IEnumerator ExitTransition()
    {
        ghost.Movement.SetDirection(Vector2.up, true);
        ghost.Movement.rb.isKinematic = true;
        ghost.Movement.enabled = false;
        
        Vector3 start = transform.position;
        float elapsed = 0f;

        while (elapsed < travelDuration)
        {
            ghost.SetPosition(Vector3.Lerp(start, inside.position, elapsed / travelDuration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        ghost.SetPosition(inside.position);
        
        Transform chosen = PickExit();

        elapsed = 0f;
        while (elapsed < travelDuration)
        {
            ghost.SetPosition(Vector3.Lerp(inside.position, chosen.position, elapsed / travelDuration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        ghost.SetPosition(chosen.position);
        
        Vector2 dir;

        if (exitMode == ExitMode.UpOnly || exitMode == ExitMode.LeftOnly || exitMode == ExitMode.RightOnly || exitMode == ExitMode.Random)
        {
            Vector2[] order = BuildForcedDirectionOrder(exitMode);
            dir = ChooseFirstFree(order);
            if (ghost.Movement.Occupied(dir))
            {
                Vector2 preferred = order[0];
                dir = PickFreeDirection(preferred);
            }
        }
        else
        {
            Vector2 preferred = GetCardinalDirection(ghost.target.position - chosen.position);
            dir = PickFreeDirection(preferred);
        }
        
        if (snapAtEndToGrid)
        {
            var rbPos = ghost.Movement.rb.position;
            float tile = GetTileSize(ghost.Movement.transform);
            Vector2 center = new Vector2(
                Mathf.Round(rbPos.x / tile) * tile,
                Mathf.Round(rbPos.y / tile) * tile
            );
            ghost.Movement.rb.position = center;
        }
        
        ghost.Movement.rb.position += dir * 0.02f;

        ghost.Movement.SetDirection(dir, true);
        ghost.Movement.rb.isKinematic = false;
        ghost.Movement.enabled = true;
    }

    private Vector2[] BuildForcedDirectionOrder(ExitMode mode)
    {
        switch (mode)
        {
            case ExitMode.UpOnly:
                return new[] { Vector2.up, Vector2.left, Vector2.right, Vector2.down };

            case ExitMode.LeftOnly:
                return new[] { Vector2.left, Vector2.right, Vector2.up, Vector2.down };

            case ExitMode.RightOnly:
                return new[] { Vector2.right, Vector2.left, Vector2.up, Vector2.down };

            case ExitMode.Random:
                int r = Random.Range(0, 3);
                if (r == 0)       return new[] { Vector2.up,    Vector2.left,  Vector2.right, Vector2.down };
                else if (r == 1)  return new[] { Vector2.left,  Vector2.right, Vector2.up,    Vector2.down };
                else              return new[] { Vector2.right, Vector2.left,  Vector2.up,    Vector2.down };

            default:
                return new[] { Vector2.up, Vector2.left, Vector2.right, Vector2.down };
        }
    }
    
    private Vector2 ChooseFirstFree(Vector2[] order)
    {
        foreach (var d in order)
        {
            if (!ghost.Movement.Occupied(d))
                return d;
        }
        return order[0];
    }

    private Vector2 PickFreeDirection(Vector2 preferred)
    {
        Vector2[] candidates;

        if (preferred == Vector2.up)
            candidates = new[] { Vector2.up, Vector2.left, Vector2.right, Vector2.down };
        else if (preferred == Vector2.left)
            candidates = new[] { Vector2.left, Vector2.up, Vector2.down, Vector2.right };
        else if (preferred == Vector2.right)
            candidates = new[] { Vector2.right, Vector2.up, Vector2.down, Vector2.left };
        else
            candidates = new[] { Vector2.down, Vector2.left, Vector2.right, Vector2.up };

        foreach (var c in candidates)
        {
            if (!ghost.Movement.Occupied(c))
                return c;
        }
        return preferred;
    }

    private Transform PickExit()
    {
        var list = new System.Collections.Generic.List<Transform>();

        if (outside)      list.Add(outside);
        if (outsideLeft)  list.Add(outsideLeft);
        if (outsideRight) list.Add(outsideRight);

        if (list.Count == 0)
            return inside != null ? inside : transform;

        switch (exitMode)
        {
            case ExitMode.UpOnly:     return outside      ? outside      : list[0];
            case ExitMode.LeftOnly:   return outsideLeft  ? outsideLeft  : (outside ?? list[0]);
            case ExitMode.RightOnly:  return outsideRight ? outsideRight : (outside ?? list[0]);
            case ExitMode.Sequence:
                var t = list[_seqIndex % list.Count];
                _seqIndex++;
                return t;
            default:
                return list[Random.Range(0, list.Count)];
        }
    }

    private Vector2 GetCardinalDirection(Vector3 delta)
    {
        if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
            return new Vector2(Mathf.Sign(delta.x), 0f);
        else
            return new Vector2(0f, Mathf.Sign(delta.y));
    }

    private float GetTileSize(Transform t)
    {
        var g = t.GetComponentInParent<Grid>();
        return (g != null) ? g.cellSize.x * g.transform.lossyScale.x : 1f;
    }
}
