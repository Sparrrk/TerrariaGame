using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MCScript : MonoBehaviour
{
    [SerializeField] private Sprite sprite1;
    [SerializeField] private Sprite sprite2;
    [SerializeField] private float speed;
    private SpriteRenderer spriteRenderer;
    private Transform thisTransform;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        thisTransform = GetComponent<Transform>();
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(IdleAnimSpriteChange());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        Move();
    }

    private IEnumerator IdleAnimSpriteChange()
    {
        spriteRenderer.sprite = sprite1;
        yield return new WaitForSeconds(1);
        spriteRenderer.sprite = sprite2;
        yield return new WaitForSeconds(1);
        StartCoroutine(IdleAnimSpriteChange());
    }

    private Vector3 speedVector;
    private void Move()
    {
        speedVector = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        speedVector *= speed;

        thisTransform.position = thisTransform.position + speedVector * Time.deltaTime;
        
    }

}

public enum MCState
{
    Idle,

    Fight,

    Run
}
