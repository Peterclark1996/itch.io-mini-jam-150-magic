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
    public GameObject armLeft;
    public GameObject armRight;
    public GameObject legLeft;
    public GameObject legRight;
    public GameObject item;
    public GameObject speechBubble;
    public TextMeshPro speechTextMesh;

    public Sprite spriteEyesNormal;
    public Sprite spriteEyesAngry;
    public Sprite spriteLegUp;
    public Sprite spriteLegDown;
    public Sprite spriteItemBook;
    public Sprite spriteItemFlask;

    public SortingGroup sortingGroup;

    private const float EyeOffset = 0.075f;
    private const float BreatheOffset = 0.05f;
    private const float ItemHorizontalOffset = 0.4f;
    private const float ItemRaisedOffset = 0.5f;

    private float _moveSpeed = 6.0f;
    private float _headDefaultHeight;
    private float _eyesDefaultHeight;
    private float _armDefaultHeight;
    private float _bodyDefaultHeight;
    private float _itemDefaultHeight;
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

        StartMovingTo(Random.Range(Constants.Instance.liftMaxLeftPosition, Constants.Instance.liftMaxRightPosition));
    }

    public bool IsMoving() => _targetPosition.HasValue;

    public void LeaveAngrily()
    {
        if (_monkeyType != MonkeyType.RIDER) return;

        _isAngry = true;
        StartMovingTo(Constants.Instance.offScreenPosition);
    }

    public void OnLiftArrivedAtFloor()
    {
        if (_monkeyType != MonkeyType.RIDER) return;

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

            StartMovingTo(Constants.Instance.offScreenPosition);
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
            0 => playerFailed ? "The lift is too heavy, this is a disaster!" : "Take the monkey to their floor",
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

            if ((int) target == (int) Constants.Instance.offScreenPosition)
            {
                GameControl.Instance.DestroyMonkey(this);
            }

            return;
        }

        transform.position += new Vector3(target > pos.x ? distanceToMove : -distanceToMove, 0, 0);
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