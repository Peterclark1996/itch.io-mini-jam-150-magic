using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class MonkeyObject : MonoBehaviour
{
    public GameObject hat;
    public GameObject head;
    public GameObject anger;
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
    public AudioSource audioSource;

    public AudioClip audioTalking1;
    public AudioClip audioTalking2;
    public AudioClip audioAngry1;
    public AudioClip audioAngry2;
    public AudioClip audioHappy1;
    public AudioClip audioHappy2;

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
    public Sprite spriteHatRed;
    public Sprite spriteHatBlue;
    public Sprite spriteHatGreen;

    public SortingGroup sortingGroup;

    private const float EyeOffset = 0.03f;
    private const float BreatheOffset = 0.03f;
    private const float AudioPitchRange = 0.2f;
    private const float AudioMaxDelay = 0.5f;
    private const float AudioVolume = 0.2f;
    private readonly Vector3 _itemRaisedOffset = new(0.2f, 0.5f, 0);
    private float _moveSpeed = 6.0f;

    private Vector3 _hatDefaultPos;
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
                item.SetActive(false);
                hat.SetActive(false);
                eyes.SetActive(false);
                StartMovingTo(Constants.Instance.standingSpotPlayer);
                return;
            case MonkeyType.MANAGER:
                item.SetActive(false);
                hat.SetActive(false);
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
        
        hat.GetComponent<SpriteRenderer>().sprite = desiredFloorName switch
        {
            FloorName.LIBRARY_RED => spriteHatRed,
            FloorName.LIBRARY_BLUE => spriteHatBlue,
            FloorName.LIBRARY_GREEN => spriteHatGreen,
            FloorName.ALCHEMY_RED => spriteHatRed,
            FloorName.ALCHEMY_BLUE => spriteHatBlue,
            FloorName.ALCHEMY_GREEN => spriteHatGreen,
            _ => throw new ArgumentOutOfRangeException(nameof(desiredFloorName), desiredFloorName, null)
        };

        StartMovingTo(Random.Range(Constants.Instance.liftMaxLeftPosition, Constants.Instance.liftMaxRightPosition));
    }

    public bool IsMoving() => _targetPosition.HasValue;

    public void LeaveAngrily()
    {
        if (_monkeyType != MonkeyType.RIDER) return;

        _isAngry = true;
        MakeNoise();
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
        }
        else
        {
            _isAngry = true;
        }

        MakeNoise();
    }

    private void Start()
    {
        _hatDefaultPos = hat.transform.localPosition;
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
        eyes.GetComponent<SpriteRenderer>().sprite = _isAngry ? spriteEyesAngry : spriteEyesNormal;

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

            if (_monkeyType == MonkeyType.PLAYER)
            {
                UpdatePlayer();
            }
        }
    }

    private void UpdatePlayer()
    {
        if (GameControl.Instance.currentPhase != GamePhase.LIFT_MOVEMENT &&
            GameControl.Instance.currentPhase != GamePhase.LIFT_MOVEMENT_FAILED) return;

        armLeft.GetComponent<SpriteRenderer>().sprite = spriteArmLeftUp;
        armRight.GetComponent<SpriteRenderer>().sprite = spriteArmRightUp;
    }

    private int _managerIntroConversationStage;
    private void UpdateManager()
    {
        var playerFailed = GameControl.Instance.currentPhase == GamePhase.PLAYER_FAILED;
        if (GameControl.Instance.currentPhase != GamePhase.INTRO && !playerFailed) return;

        _isAngry = playerFailed;

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
                Spell.Instance.StartFading();
            }

            StartMovingTo(Constants.Instance.offScreenHorizontalPosition);
            return;
        }

        if (Util.IsAnyKeyPressed())
        {
            _managerIntroConversationStage++;

            if (_managerIntroConversationStage <= 2)
            {
                MakeNoise();
            }
        }

        speechBubble.SetActive(true);
        speechTextMesh.text = _managerIntroConversationStage switch
        {
            0 => playerFailed ? "LIFT TOO HEAVY, THIS BAD!" : "TAKE MONKEY TO FLOOR",
            1 => playerFailed ? "YOU FIRED!" : "LIFT WEIGHT MATTER!",
            2 => "LIGHT IS THE WAY!!!",
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
            else if (_monkeyType == MonkeyType.MANAGER)
            {
                MakeNoise();
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

        if (Util.IsFrameAlternative(_spawnTime, 2))
        {
            hat.transform.localPosition = _hatDefaultPos;
            head.transform.localPosition = _headDefaultPos;
            eyes.transform.localPosition = _eyesDefaultPos;
            armLeft.transform.localPosition = _armLeftDefaultPos;
            armRight.transform.localPosition = _armRightDefaultPos;
            item.transform.localPosition = _itemDefaultPos;
            body.transform.localPosition = _bodyDefaultPos;
            tail.transform.localPosition = _tailDefaultPos;

            armLeft.GetComponent<SpriteRenderer>().sprite = spriteArmLeftDown;
            armRight.GetComponent<SpriteRenderer>().sprite = spriteArmRightDown;
            anger.SetActive(false);
        }
        else
        {
            hat.transform.localPosition = _hatDefaultPos.WithY(_hatDefaultPos.y - BreatheOffset - BreatheOffset);
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
            anger.SetActive(_isAngry);
        }
    }

    private void UpdateSpritesWalking()
    {
        armLeft.GetComponent<SpriteRenderer>().sprite = spriteArmLeftDown;
        armRight.GetComponent<SpriteRenderer>().sprite = spriteArmRightDown;
        anger.SetActive(false);

        if (Util.IsFrameAlternative(_spawnTime, 5))
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

        hat.transform.localPosition = _hatDefaultPos;
        head.transform.localPosition = _headDefaultPos;
        armLeft.transform.localPosition = _armLeftDefaultPos;
        armRight.transform.localPosition = _armRightDefaultPos;
        item.transform.localPosition = _itemDefaultPos;
        body.transform.localPosition = _bodyDefaultPos;
        tail.transform.localPosition = _tailDefaultPos;
    }

    private void MakeNoise()
    {
        audioSource.Stop();

        if (_isAngry)
        {
            audioSource.clip = Util.RandomBool() ? audioAngry1 : audioAngry2;
        }
        else
        {
            if (_monkeyType == MonkeyType.MANAGER)
            {
                audioSource.clip = Util.RandomBool() ? audioTalking1 : audioTalking2;
            }
            else
            {
                audioSource.clip = Util.RandomBool() ? audioHappy1 : audioHappy2;
            }
        }

        audioSource.volume = AudioVolume;
        audioSource.pitch = Random.Range(1 - AudioPitchRange, 1 + AudioPitchRange);

        audioSource.PlayDelayed(Random.Range(0, AudioMaxDelay));
    }
}