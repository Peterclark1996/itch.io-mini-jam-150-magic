using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class MonkeyObject : MonoBehaviour
{
    private const float MoveSpeed = 6.0f;

    private bool _isMoving;
    private float? _targetPosition;

    void Start()
    {
        StartMovingTo(Random.Range(Constants.Instance.LiftMaxLeftPosition, Constants.Instance.LiftMaxRightPosition));
    }

    void Update()
    {
        if (_targetPosition == null) return;
        var nonNullTargetPosition = (float) _targetPosition;

        var position = transform.position;
        var distanceToTarget = position.x - nonNullTargetPosition;
        var distanceToMove = MoveSpeed * Time.deltaTime;

        if (Math.Abs(distanceToTarget) <= distanceToMove)
        {
            transform.position = new Vector3(nonNullTargetPosition, position.y, position.z);
            return;
        }

        if (nonNullTargetPosition > position.x)
        {
            transform.position += new Vector3(distanceToMove, 0, 0);
        }
        else
        {
            transform.position -= new Vector3(distanceToMove, 0, 0);
        }
    }

    private void StartMovingTo(float newPosition)
    {
        _isMoving = true;
        _targetPosition = newPosition;
    }
}