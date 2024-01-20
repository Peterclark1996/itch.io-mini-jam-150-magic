using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class MonkeyObject : MonoBehaviour
{
    public GameObject Head;
    public GameObject Eyes;
    public GameObject Body;
    public GameObject ArmLeft;
    public GameObject ArmRight;
    public GameObject LegLeft;
    public GameObject LegRight;
    
    public Sprite SpriteEyesAngry;
    public Sprite SpriteLegUp;
    public Sprite SpriteLegDown;

    private const float MoveSpeed = 6.0f;
    private const float EyeOffset = 0.1f;
    
    private float? _targetPosition;
    private float _spawnTime;

    void Start()
    {
        _spawnTime = Time.time;
        StartMovingTo(Random.Range(Constants.Instance.LiftMaxLeftPosition, Constants.Instance.LiftMaxRightPosition));
    }

    void Update()
    {
        UpdateMovement();
        UpdateSprites();
    }

    private void StartMovingTo(float newPosition)
    {
        _targetPosition = newPosition;
    }

    private void UpdateMovement()
    {
        if (_targetPosition == null) return;
        var nonNullTargetPosition = (float) _targetPosition;

        var readonlyPosition = transform.position;
        var distanceToTarget = readonlyPosition.x - nonNullTargetPosition;
        var distanceToMove = MoveSpeed * Time.deltaTime;

        if (Math.Abs(distanceToTarget) <= distanceToMove)
        {
            transform.position = new Vector3(nonNullTargetPosition, readonlyPosition.y, readonlyPosition.z);
            _targetPosition = null;
            return;
        }

        if (nonNullTargetPosition > readonlyPosition.x)
        {
            transform.position += new Vector3(distanceToMove, 0, 0);
        }
        else
        {
            transform.position -= new Vector3(distanceToMove, 0, 0);
        }
    }

    private void UpdateSprites()
    {
        var readonlyEyePosition = Eyes.transform.position;
        var readonlyPosition = transform.position;
        if (_targetPosition == null)
        {
            LegRight.GetComponent<SpriteRenderer>().sprite = SpriteLegDown;
            LegLeft.GetComponent<SpriteRenderer>().sprite = SpriteLegDown;
            Eyes.transform.position = new Vector3(readonlyPosition.x, readonlyEyePosition.y, readonlyEyePosition.z);
            return;
        }

        var aliveTime = Time.time - _spawnTime;
        var leftLegUp = aliveTime * 5 % 2 < 1;

        if (leftLegUp)
        {
            LegLeft.GetComponent<SpriteRenderer>().sprite = SpriteLegUp;
            LegRight.GetComponent<SpriteRenderer>().sprite = SpriteLegDown;
        }
        else
        {
            LegLeft.GetComponent<SpriteRenderer>().sprite = SpriteLegDown;
            LegRight.GetComponent<SpriteRenderer>().sprite = SpriteLegUp;
        }

        var movingRight = (float) _targetPosition > readonlyPosition.x;
        var newEyePos = readonlyPosition.x + (movingRight ? EyeOffset : -EyeOffset);
        Eyes.transform.position = new Vector3(newEyePos , readonlyEyePosition.y, readonlyEyePosition.z);
    }
}