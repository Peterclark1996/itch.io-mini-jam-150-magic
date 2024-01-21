using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class MonkeyObject : MonoBehaviour
{
    public GameObject head;
    public GameObject eyes;
    public GameObject body;
    public GameObject tail;
    public GameObject armLeft;
    public GameObject armRight;
    public GameObject legLeft;
    public GameObject legRight;
    public GameObject item;
    public GameObject speechBubble;
    public TextMeshPro speechTextMesh;

    public Sprite spriteEyesNormal;
    public Sprite spriteEyesAngry;
    public Sprite spriteLegLeftUp;
    public Sprite spriteLegLeftDown;
    public Sprite spriteLegRightUp;
    public Sprite spriteLegRightDown;
    public Sprite spriteArmLeftUp;
    public Sprite spriteArmLeftDown;
    public Sprite spriteArmRightUp;
    public Sprite spriteArmRightDown;
    public Sprite spriteItemBookRed;
    public Sprite spriteItemBookBlue;
    public Sprite spriteItemBookGreen;
    public Sprite spriteItemFlaskRed;
    public Sprite spriteItemFlaskBlue;
    public Sprite spriteItemFlaskGreen;

    public SortingGroup sortingGroup;

    private const float EyeOffset = 0.03f;
    private const float BreatheOffset = 0.03f;

    private readonly Vector3 _itemRaisedOffset = new(0.2f, 0.5f, 0);
    private float _moveSpeed = 6.0f;
    private Vector3 _headDefaultPos;
    private Vector3 _eyesDefaultPos;
    private Vector3 _armLeftDefaultPos;
    private Vector3 _armRightDefaultPos;
    private Vector3 _bodyDefaultPos;
    private Vector3 _tailDefaultPos;
    private Vector3 _itemDefaultPos;
    private float _spawnTime;
    private FloorName _desiredFloorName;
    private MonkeyType _monkeyType;
    private float? _targetPosition;
    private bool _isAngry;

    public void Init(MonkeyType monkeyType, FloorName? desiredFloorName = null)
    {
        _monkeyType = monkeyType;
        switch (monkeyType)
        {
            case MonkeyType.PLAYER:
                _moveSpeed = 8.0f;
                item.GetComponent<SpriteRenderer>().enabled = false;
                StartMovingTo(Constants.Instance.standingSpotPlayer);
                return;
            case MonkeyType.MANAGER:
                StartMovingTo(Constants.Instance.standingSpotManager);
                return;
            case MonkeyType.RIDER:
                InitRiderMonkey(desiredFloorName.GetValueOrDefault());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(monkeyType), monkeyType, null);
        }

        sortingGroup.sortingOrder = (int) (Math.Abs(transform.position.y) * 1000);
    }

    private void InitRiderMonkey(FloorName desiredFloorName)
    {
        _moveSpeed = 6.0f + Random.Range(-1.0f, 1.0f);
        _desiredFloorName = desiredFloorName;

        item.GetComponent<SpriteRenderer>().sprite = desiredFloorName switch
        {
            FloorName.LIBRARY_RED => spriteItemBookRed,
            FloorName.LIBRARY_BLUE => spriteItemBookBlue,
            FloorName.LIBRARY_GREEN => spriteItemBookGreen,
            FloorName.ALCHEMY_RED => spriteItemFlaskRed,
            FloorName.ALCHEMY_BLUE => spriteItemFlaskBlue,
            FloorName.ALCHEMY_GREEN => spriteItemFlaskGreen,
            _ => throw new ArgumentOutOfRangeException(nameof(desiredFloorName), desiredFloorName, null)
        };

        StartMovingTo(Random.Range(Constants.Instance.liftMaxLeftPosition, Constants.Instance.liftMaxRightPosition));
    }

    public bool IsMoving() => _targetPosition.HasValue;

    public void LeaveAngrily()
    {
        if (_monkeyType != MonkeyType.RIDER) return;

        _isAngry = true;
        StartMovingTo(Constants.Instance.offScreenHorizontalPosition);
    }

    public void OnLiftArrivedAtFloor()
    {
        if (_monkeyType != MonkeyType.RIDER) return;

        if (GameControl.Instance.currentFloor == _desiredFloorName)
        {
            GameControl.Instance.AwardScore(_isAngry, gameObject.transform.position);
            _isAngry = false;
            StartMovingTo(Constants.Instance.offScreenHorizontalPosition);
            return;
        }

        _isAngry = true;
    }

    private void Start()
    {
        _headDefaultPos = head.transform.localPosition;
        _eyesDefaultPos = eyes.transform.localPosition;
        _armLeftDefaultPos = armLeft.transform.localPosition;
        _armRightDefaultPos = armRight.transform.localPosition;
        _bodyDefaultPos = body.transform.localPosition;
        _tailDefaultPos = tail.transform.localPosition;
        _itemDefaultPos = item.transform.localPosition;
        _spawnTime = Time.time;
    }

    private void StartMovingTo(float newPosition) => _targetPosition = newPosition;

    private void Update()
    {
        var isMoving = _targetPosition.HasValue;
        if (isMoving)
        {
            UpdateMovement(_targetPosition.Value);
            UpdateSpritesWalking();
        }
        else
        {
            UpdateSpritesIdle();
            if (_monkeyType == MonkeyType.MANAGER)
            {
                UpdateManager();
            }
        }

        eyes.GetComponent<SpriteRenderer>().sprite = _isAngry ? spriteEyesAngry : spriteEyesNormal;
    }

    private int _managerIntroConversationStage;

    private void UpdateManager()
    {
        var playerFailed = GameControl.Instance.currentPhase == GamePhase.PLAYER_FAILED;
        if (GameControl.Instance.currentPhase != GamePhase.INTRO && !playerFailed) return;

        if (_managerIntroConversationStage > 2)
        {
            speechBubble.SetActive(false);

            if (playerFailed)
            {
                GameControl.Instance.GoToGameOverScreenPhase();
            }
            else
            {
                GameControl.Instance.GoToMonkeyMovementPhase();
            }

            StartMovingTo(Constants.Instance.offScreenHorizontalPosition);
            return;
        }

        if (Input.GetButtonDown("Jump"))
        {
            _managerIntroConversationStage++;
        }

        _isAngry = playerFailed;

        speechBubble.SetActive(true);
        speechTextMesh.text = _managerIntroConversationStage switch
        {
            0 => playerFailed ? "The lift was too heavy, this is a disaster!" : "Take the monkeys to their floor",
            1 => playerFailed ? "You're fired!" : "The lifts weight is what matters!",
            2 => "Light is the way!!!",
            _ => ""
        };
    }

    private void UpdateMovement(float target)
    {
        var pos = transform.position;
        var distanceToTarget = pos.x - target;
        var distanceToMove = _moveSpeed * Time.deltaTime;

        if (Math.Abs(distanceToTarget) <= distanceToMove)
        {
            transform.position = pos.WithX(target);
            _targetPosition = null;

            GameControl.Instance.OnMonkeyFinishedMoving();

            if ((int) target == (int) Constants.Instance.offScreenHorizontalPosition)
            {
                GameControl.Instance.DestroyMonkey(this);
            }

            return;
        }

        transform.position += new Vector3(target > pos.x ? distanceToMove : -distanceToMove, 0, 0);
    }

    private void UpdateSpritesIdle()
    {
        legRight.GetComponent<SpriteRenderer>().sprite = spriteLegRightDown;
        legLeft.GetComponent<SpriteRenderer>().sprite = spriteLegLeftDown;
        eyes.transform.localPosition = new Vector3(0, 0, 0);

        if (IsFrameAlternative(2))
        {
            head.transform.localPosition = _headDefaultPos;
            eyes.transform.localPosition = _eyesDefaultPos;
            armLeft.transform.localPosition = _armLeftDefaultPos;
            armRight.transform.localPosition = _armRightDefaultPos;
            item.transform.localPosition = _itemDefaultPos;
            body.transform.localPosition = _bodyDefaultPos;
            tail.transform.localPosition = _tailDefaultPos;

            armLeft.GetComponent<SpriteRenderer>().sprite = spriteArmLeftDown;
            armRight.GetComponent<SpriteRenderer>().sprite = spriteArmRightDown;
        }
        else
        {
            head.transform.localPosition = _headDefaultPos.WithY(_headDefaultPos.y - BreatheOffset - BreatheOffset);
            eyes.transform.localPosition = _eyesDefaultPos.WithY(_eyesDefaultPos.y - BreatheOffset - BreatheOffset);
            armLeft.transform.localPosition = _armLeftDefaultPos.WithY(_armLeftDefaultPos.y - BreatheOffset);
            armRight.transform.localPosition = _armRightDefaultPos.WithY(_armRightDefaultPos.y - BreatheOffset);
            body.transform.localPosition = _bodyDefaultPos.WithY(_bodyDefaultPos.y - BreatheOffset);
            tail.transform.localPosition = _tailDefaultPos.WithY(_tailDefaultPos.y - BreatheOffset);

            var newItemPos = _itemDefaultPos.WithY(_itemDefaultPos.y - BreatheOffset);
            item.transform.localPosition = _isAngry ? newItemPos + _itemRaisedOffset : newItemPos;

            armLeft.GetComponent<SpriteRenderer>().sprite = _isAngry ? spriteArmLeftUp : spriteArmLeftDown;
            armRight.GetComponent<SpriteRenderer>().sprite = _isAngry ? spriteArmRightUp : spriteArmRightDown;
        }
    }

    private void UpdateSpritesWalking()
    {
        armLeft.GetComponent<SpriteRenderer>().sprite = spriteArmLeftDown;
        armRight.GetComponent<SpriteRenderer>().sprite = spriteArmRightDown;

        if (IsFrameAlternative(5))
        {
            legLeft.GetComponent<SpriteRenderer>().sprite = spriteLegLeftUp;
            legRight.GetComponent<SpriteRenderer>().sprite = spriteLegRightDown;
        }
        else
        {
            legLeft.GetComponent<SpriteRenderer>().sprite = spriteLegLeftDown;
            legRight.GetComponent<SpriteRenderer>().sprite = spriteLegRightUp;
        }

        var pos = transform.position;
        var movingRight = _targetPosition > pos.x;
        eyes.transform.localPosition =
            _eyesDefaultPos.WithX(_eyesDefaultPos.x + (movingRight ? EyeOffset : -EyeOffset));

        head.transform.localPosition = _headDefaultPos;
        armLeft.transform.localPosition = _armLeftDefaultPos;
        armRight.transform.localPosition = _armRightDefaultPos;
        item.transform.localPosition = _itemDefaultPos;
        body.transform.localPosition = _bodyDefaultPos;
        tail.transform.localPosition = _tailDefaultPos;
    }

    private bool IsFrameAlternative(int animationSpeed)
    {
        var aliveTime = Time.time - _spawnTime;
        return aliveTime * animationSpeed % 2 < 1;
    }
}