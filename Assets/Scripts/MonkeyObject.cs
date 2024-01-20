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
    private const float BreatheOffset = 0.05f;

    private float _headDefaultHeight;
    private float _armDefaultHeight;
    private float _bodyDefaultHeight;
    private float _spawnTime;

    private float? _targetPosition;

    void Start()
    {
        _headDefaultHeight = Head.transform.position.y;
        _armDefaultHeight = ArmLeft.transform.position.y;
        _bodyDefaultHeight = Body.transform.position.y;
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

        var pos = transform.position;
        var distanceToTarget = pos.x - nonNullTargetPosition;
        var distanceToMove = MoveSpeed * Time.deltaTime;

        if (Math.Abs(distanceToTarget) <= distanceToMove)
        {
            transform.position = pos.WithX(nonNullTargetPosition);
            _targetPosition = null;
            return;
        }

        if (nonNullTargetPosition > pos.x)
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
        var isIdle = _targetPosition == null;
        if (isIdle)
        {
            UpdateSpritesIdle();
        }
        else
        {
            UpdateSpritesWalking();
        }
    }

    private void UpdateSpritesIdle()
    {
        var eyePos = Eyes.transform.position;
        var pos = transform.position;

        LegRight.GetComponent<SpriteRenderer>().sprite = SpriteLegDown;
        LegLeft.GetComponent<SpriteRenderer>().sprite = SpriteLegDown;
        Eyes.transform.position = new Vector3(pos.x, eyePos.y, eyePos.z);

        var headPos = Head.transform.position;
        var armLeftPos = ArmLeft.transform.position;
        var armRightPos = ArmRight.transform.position;
        var bodyPos = Body.transform.position;

        if (IsFrameAlternative(2))
        {
            Head.transform.position = headPos.WithY(_headDefaultHeight);
            Eyes.transform.position = eyePos.WithY(_headDefaultHeight).WithX(pos.x);
            ArmLeft.transform.position = armLeftPos.WithY(_armDefaultHeight);
            ArmRight.transform.position = armRightPos.WithY(_armDefaultHeight);
            Body.transform.position = bodyPos.WithY(_bodyDefaultHeight);
        }
        else
        {
            Head.transform.position = headPos.WithY(_headDefaultHeight - BreatheOffset - BreatheOffset);
            Eyes.transform.position = eyePos.WithY(_headDefaultHeight - BreatheOffset - BreatheOffset).WithX(pos.x);
            ArmLeft.transform.position = armLeftPos.WithY(_armDefaultHeight - BreatheOffset);
            ArmRight.transform.position = armRightPos.WithY(_armDefaultHeight - BreatheOffset);
            Body.transform.position = bodyPos.WithY(_bodyDefaultHeight - BreatheOffset);
        }
    }

    private void UpdateSpritesWalking()
    {
        var eyePos = Eyes.transform.position;
        var pos = transform.position;

        if (IsFrameAlternative(5))
        {
            LegLeft.GetComponent<SpriteRenderer>().sprite = SpriteLegUp;
            LegRight.GetComponent<SpriteRenderer>().sprite = SpriteLegDown;
        }
        else
        {
            LegLeft.GetComponent<SpriteRenderer>().sprite = SpriteLegDown;
            LegRight.GetComponent<SpriteRenderer>().sprite = SpriteLegUp;
        }

        var movingRight = _targetPosition > pos.x;
        var newEyePos = pos.x + (movingRight ? EyeOffset : -EyeOffset);
        Eyes.transform.position = eyePos.WithX(newEyePos);
    }

    private bool IsFrameAlternative(int animationSpeed)
    {
        var aliveTime = Time.time - _spawnTime;
        return aliveTime * animationSpeed % 2 < 1;
    }
}