using System;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class MonkeyObject : MonoBehaviour
{
    public GameObject head;
    public GameObject eyes;
    public GameObject body;
    public GameObject armLeft;
    public GameObject armRight;
    public GameObject legLeft;
    public GameObject legRight;
    public GameObject item;

    public Sprite spriteEyesNormal;
    public Sprite spriteEyesAngry;
    public Sprite spriteLegUp;
    public Sprite spriteLegDown;
    public Sprite spriteItemBook;
    public Sprite spriteItemFlask;

    public SortingGroup sortingGroup;

    private const float MoveSpeed = 6.0f;
    private const float EyeOffset = 0.075f;
    private const float BreatheOffset = 0.05f;
    private const float ItemHorizontalOffset = 0.4f;
    private const float ItemRaisedOffset = 0.5f;

    private float _headDefaultHeight;
    private float _eyesDefaultHeight;
    private float _armDefaultHeight;
    private float _bodyDefaultHeight;
    private float _itemDefaultHeight;
    private float _spawnTime;
    private FloorName _desiredFloorName;
    private float? _targetPosition;
    private bool _isAngry;

    public void Init(FloorName desiredFloorName)
    {
        _desiredFloorName = desiredFloorName;

        var itemRenderer = item.GetComponent<SpriteRenderer>();
        switch (desiredFloorName)
        {
            case FloorName.LIBRARY_RED:
                itemRenderer.sprite = spriteItemBook;
                itemRenderer.color = new Color(0.8f, 0.2f, 0.2f);
                break;
            case FloorName.LIBRARY_BLUE:
                itemRenderer.sprite = spriteItemBook;
                itemRenderer.color = new Color(0.2f, 0.2f, 0.8f);
                break;
            case FloorName.LIBRARY_GREEN:
                itemRenderer.sprite = spriteItemBook;
                itemRenderer.color = new Color(0.2f, 0.8f, 0.2f);
                break;
            case FloorName.ALCHEMY_RED:
                itemRenderer.sprite = spriteItemFlask;
                itemRenderer.color = new Color(0.8f, 0.2f, 0.2f);
                break;
            case FloorName.ALCHEMY_BLUE:
                itemRenderer.sprite = spriteItemFlask;
                itemRenderer.color = new Color(0.2f, 0.2f, 0.8f);
                break;
            case FloorName.ALCHEMY_GREEN:
                itemRenderer.sprite = spriteItemFlask;
                itemRenderer.color = new Color(0.2f, 0.8f, 0.2f);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(desiredFloorName), desiredFloorName, null);
        }

        var itemPos = item.transform.localPosition;
        item.transform.localPosition = itemPos.WithX(Util.RandomBool() ? -ItemHorizontalOffset : ItemHorizontalOffset);

        sortingGroup.sortingOrder = (int) itemPos.y * 1000;

        StartMovingTo(Random.Range(Constants.Instance.liftMaxLeftPosition, Constants.Instance.liftMaxRightPosition));
    }

    public bool IsMoving() => _targetPosition.HasValue;

    public void OnLiftArrivedAtFloor()
    {
        if (GameControl.Instance.currentFloor == _desiredFloorName)
        {
            _isAngry = false;
            StartMovingTo(Constants.Instance.offScreenPosition);
            return;
        }

        _isAngry = true;
    }

    private void Start()
    {
        _headDefaultHeight = head.transform.localPosition.y;
        _eyesDefaultHeight = eyes.transform.localPosition.y;
        _armDefaultHeight = armLeft.transform.localPosition.y;
        _bodyDefaultHeight = body.transform.localPosition.y;
        _itemDefaultHeight = item.transform.localPosition.y;
        _spawnTime = Time.time;
    }

    private void StartMovingTo(float newPosition) => _targetPosition = newPosition;

    private void Update()
    {
        UpdateMovement();

        var isMoving = _targetPosition.HasValue;
        if (isMoving)
        {
            UpdateSpritesWalking();
        }
        else
        {
            UpdateSpritesIdle();
        }

        eyes.GetComponent<SpriteRenderer>().sprite = _isAngry ? spriteEyesAngry : spriteEyesNormal;
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
            GameControl.Instance.OnMonkeyFinishedMoving();
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

    private void UpdateSpritesIdle()
    {
        var eyePos = eyes.transform.localPosition;

        legRight.GetComponent<SpriteRenderer>().sprite = spriteLegDown;
        legLeft.GetComponent<SpriteRenderer>().sprite = spriteLegDown;
        eyes.transform.localPosition = new Vector3(0, 0, 0);

        var headPos = head.transform.localPosition;
        var armLeftPos = armLeft.transform.localPosition;
        var armRightPos = armRight.transform.localPosition;
        var itemPos = item.transform.localPosition;
        var bodyPos = body.transform.localPosition;

        if (IsFrameAlternative(2))
        {
            head.transform.localPosition = headPos.WithY(_headDefaultHeight);
            eyes.transform.localPosition = eyePos.WithY(_eyesDefaultHeight).WithX(0);
            armLeft.transform.localPosition = armLeftPos.WithY(_armDefaultHeight);
            armRight.transform.localPosition = armRightPos.WithY(_armDefaultHeight);
            item.transform.localPosition = itemPos.WithY(_itemDefaultHeight);
            body.transform.localPosition = bodyPos.WithY(_bodyDefaultHeight);

            armLeft.transform.eulerAngles = new Vector3(0, 0, 0);
            armRight.transform.eulerAngles = new Vector3(0, 180, 0);
        }
        else
        {
            head.transform.localPosition = headPos.WithY(_headDefaultHeight - BreatheOffset - BreatheOffset);
            eyes.transform.localPosition = eyePos.WithY(_eyesDefaultHeight - BreatheOffset - BreatheOffset).WithX(0);
            armLeft.transform.localPosition = armLeftPos.WithY(_armDefaultHeight - BreatheOffset);
            armRight.transform.localPosition = armRightPos.WithY(_armDefaultHeight - BreatheOffset);
            body.transform.localPosition = bodyPos.WithY(_bodyDefaultHeight - BreatheOffset);

            if (!_isAngry)
            {
                item.transform.localPosition = itemPos.WithY(_itemDefaultHeight - BreatheOffset);
                return;
            }

            item.transform.localPosition = itemPos.WithY(_itemDefaultHeight - BreatheOffset + ItemRaisedOffset);
            armLeft.transform.eulerAngles = new Vector3(180, 0, 0);
            armRight.transform.eulerAngles = new Vector3(180, 180, 0);
        }
    }

    private void UpdateSpritesWalking()
    {
        if (IsFrameAlternative(5))
        {
            legLeft.GetComponent<SpriteRenderer>().sprite = spriteLegUp;
            legRight.GetComponent<SpriteRenderer>().sprite = spriteLegDown;
        }
        else
        {
            legLeft.GetComponent<SpriteRenderer>().sprite = spriteLegDown;
            legRight.GetComponent<SpriteRenderer>().sprite = spriteLegUp;
        }

        var pos = transform.position;
        var movingRight = _targetPosition > pos.x;
        eyes.transform.localPosition = eyes.transform.localPosition
            .WithX(movingRight ? EyeOffset : -EyeOffset)
            .WithY(_eyesDefaultHeight);

        head.transform.localPosition = head.transform.localPosition.WithY(_headDefaultHeight);
        armLeft.transform.localPosition = armLeft.transform.localPosition.WithY(_armDefaultHeight);
        armRight.transform.localPosition = armRight.transform.localPosition.WithY(_armDefaultHeight);
        item.transform.localPosition = item.transform.localPosition.WithY(_itemDefaultHeight);
        body.transform.localPosition = body.transform.localPosition.WithY(_bodyDefaultHeight);

        armLeft.transform.eulerAngles = new Vector3(0, 0, 0);
        armRight.transform.eulerAngles = new Vector3(0, 180, 0);
    }

    private bool IsFrameAlternative(int animationSpeed)
    {
        var aliveTime = Time.time - _spawnTime;
        return aliveTime * animationSpeed % 2 < 1;
    }
}