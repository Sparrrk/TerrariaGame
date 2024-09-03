using UnityEngine;

public class DistanceChecker : MonoBehaviour
{
    [SerializeField] float minimumDistance = 10.24f;
    [SerializeField] float maximumDistance = 25.70f;
    [SerializeField] float updateDistance = 8.0f;
    private float epsilon = 1.0f;

    public delegate void DistanceDelegate(int x, int y, bool playerGetsClose, GameObject sender);

    public event DistanceDelegate PlayerPositionChanged;


    private GameObject player;
    private ChunkNotifier notifier;

    private bool _isInitialized = false;
    private bool _playerIsClose = false;
    private bool _playerIsFar = false;
    public int _yPosition;
    public int _xPosition;


    private void Awake()
    {
        player = GameObject.Find("MC");
        notifier = GameObject.Find("BlockGenerator").GetComponent<ChunkNotifier>();
    }

    public void Initialize(int posX, int posY)
    {
        _xPosition = posX;
        _yPosition = posY;
        notifier.SubscribeChunk(this);
        _isInitialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (_isInitialized)
            CheckDistance();
    }

    /// <summary> 
    /// </summary>
    private void CheckDistance()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance > maximumDistance)
        {
            PlayerPositionChanged?.Invoke(_xPosition, _yPosition, false, gameObject);
        }
        else if (distance > minimumDistance)
        {
            _playerIsClose = false;
        }
        else if (distance < minimumDistance && !_playerIsClose)
        {
            PlayerPositionChanged?.Invoke(_xPosition, _yPosition, true, gameObject);
            _playerIsClose = true;
        }
    }

}
