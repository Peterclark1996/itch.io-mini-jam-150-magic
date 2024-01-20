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
    public GameObject Item;

    public Sprite SpriteEyesAngry;
    public Sprite SpriteLegUp;
    public Sprite SpriteLegDown;
    public Sprite SpriteItemBook;
    public Sprite SpriteItemFlask;

    private const float MoveSpeed = 6.0f;
    private const float EyeOffset = 0.075f;
    private const float BreatheOffset = 0.05f;
    private const float ItemOffset = 0.4f;

    private float _headDefaultHeight;
    private float _armDefaultHeight;
    private float _bodyDefaultHeight;
    private float _itemDefaultHeight;
    private float _spawnTime;
    private Floor _desiredFloor;

    private float? _targetPosition;

    public void Init(Floor desiredFloor)
    {
        _desiredFloor = desiredFloor;

        var itemRenderer = Item.GetComponent<SpriteRenderer>();
        switch (desiredFloor)
        {
            case Floor.LIBRARY_RED:
                itemRenderer.sprite = SpriteItemBook;
                itemRenderer.color = new Color(0.8f, 0.2f, 0.2f);
                break;
            case Floor.LIBRARY_BLUE:
                itemRenderer.sprite = SpriteItemBook;
                itemRenderer.color = new Color(0.2f, 0.2f, 0.8f);
                break;
            case Floor.LIBRARY_GREEN:
                itemRenderer.sprite = SpriteItemBook;
                itemRenderer.color = new Color(0.2f, 0.8f, 0.2f);
                break;
            case Floor.ALCHEMY_RED:
                itemRenderer.sprite = SpriteItemFlask;
                itemRenderer.color = new Color(0.8f, 0.2f, 0.2f);
                break;
            case Floor.ALCHEMY_BLUE:
                itemRenderer.sprite = SpriteItemFlask;
                itemRenderer.color = new Color(0.2f, 0.2f, 0.8f);
                break;
            case Floor.ALCHEMY_GREEN:
                itemRenderer.sprite = SpriteItemFlask;
                itemRenderer.color = new Color(0.2f, 0.8f, 0.2f);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(desiredFloor), desiredFloor, null);
        }

        var itemPos = Item.transform.localPosition;
        Item.transform.localPosition = itemPos.WithX(Random.Range(0, 2) == 0 ? -ItemOffset : ItemOffset);

        StartMovingTo(Random.Range(Constants.Instance.LiftMaxLeftPosition, Constants.Instance.LiftMaxRightPosition));
    }

    public bool isMoving() => _targetPosition.HasValue;

    private void Start()
    {
        _headDefaultHeight = Head.transform.position.y;
        _armDefaultHeight = ArmLeft.transform.position.y;
        _bodyDefaultHeight = Body.transform.position.y;
        _itemDefaultHeight = Item.transform.position.y;
        _spawnTime = Time.time;
    }

    private void Update()
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
        if (!_targetPosition.HasValue) return;

        var pos = transform.position;
        var distanceToTarget = pos.x - _targetPosition.Value;
        var distanceToMove = MoveSpeed * Time.deltaTime;

        if (Math.Abs(distanceToTarget) <= distanceToMove)
        {
            transform.position = pos.WithX(_targetPosition.Value);
            _targetPosition = null;
            return;
        }

        if (_targetPosition.Value > pos.x)
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
        var isMoving = _targetPosition.HasValue;
        if (isMoving)
        {
            UpdateSpritesWalking();
        }
        else
        {
            UpdateSpritesIdle();
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
        var itemPos = Item.transform.position;
        var bodyPos = Body.transform.position;

        if (IsFrameAlternative(2))
        {
            Head.transform.position = headPos.WithY(_headDefaultHeight);
            Eyes.transform.position = eyePos.WithY(_headDefaultHeight).WithX(pos.x);
            ArmLeft.transform.position = armLeftPos.WithY(_armDefaultHeight);
            ArmRight.transform.position = armRightPos.WithY(_armDefaultHeight);
            Item.transform.position = itemPos.WithY(_itemDefaultHeight);
            Body.transform.position = bodyPos.WithY(_bodyDefaultHeight);
        }
        else
        {
            Head.transform.position = headPos.WithY(_headDefaultHeight - BreatheOffset - BreatheOffset);
            Eyes.transform.position = eyePos.WithY(_headDefaultHeight - BreatheOffset - BreatheOffset).WithX(pos.x);
            ArmLeft.transform.position = armLeftPos.WithY(_armDefaultHeight - BreatheOffset);
            ArmRight.transform.position = armRightPos.WithY(_armDefaultHeight - BreatheOffset);
            Item.transform.position = itemPos.WithY(_itemDefaultHeight - BreatheOffset);
            Body.transform.position = bodyPos.WithY(_bodyDefaultHeight - BreatheOffset);
        }
    }

    private void UpdateSpritesWalking()
    {
        var eyePos = Eyes.transform.localPosition;
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
        Eyes.transform.localPosition = eyePos.WithX(movingRight ? EyeOffset : -EyeOffset);
    }

    private bool IsFrameAlternative(int animationSpeed)
    {
        var aliveTime = Time.time - _spawnTime;
        return aliveTime * animationSpeed % 2 < 1;
    }
}