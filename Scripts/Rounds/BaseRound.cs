using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseRound : MonoBehaviour
{
    private Vector3 oldPosition = Vector2.zero;

    public float lifetime = 5.0f;
    public float weaponForceModifier = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        oldPosition = transform.position;
        Destroy(gameObject, lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pointDirection = (oldPosition - transform.position).normalized;
        transform.rotation = Quaternion.AngleAxis(Mathf.Atan2(pointDirection.y, pointDirection.x) * Mathf.Rad2Deg, Vector3.forward);
        oldPosition = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject);
    }
}
