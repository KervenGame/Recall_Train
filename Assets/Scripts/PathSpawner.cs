using System.Collections.Generic;
using UnityEngine;

public class PathSpawner : MonoBehaviour
{
    public List<Vector3> pathPositions;
    public List<Vector3> pathRotations;
    Rigidbody _rb;
    private readonly float _recordFrequency = 0.05f;
    private float _recordTimer;
    private readonly float _recallMaxTime = 10.0f;
    private float _recallTimer;
    [SerializeField] private Vector3 destPosition;
    [SerializeField] private Vector3 destRotation;
    private readonly int _maxPathPoints = 120;
    private bool _isMoving;
    private bool _isRecalling;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

        ResetRecordTimer();
        ResetRecallTimer();
    }

    // private void Update()
    private void FixedUpdate()
    {
        // If the object is in recalling, then other regular movements are disabled
        if (_isRecalling)
        {
            // disable the sphere's self move physic
            _rb.isKinematic = true;

            float distance = Vector3.Distance(transform.position, destPosition);
            // If arrived at the last path point, update the destination
            if (distance <= 0.1f)
            {
                StartRecallingPath();
            }
            else
            {
                // recall position
                float speed = distance / _recordFrequency;
                Transform transform1;
                (transform1 = transform).position = Vector3.MoveTowards(transform.position, destPosition, speed * Time.deltaTime);

                // recall rotation
                float angle = Quaternion.Angle(transform1.rotation, Quaternion.Euler(destRotation));
                float angleSpeed = angle / _recordFrequency;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(destRotation), angleSpeed * Time.deltaTime);
            }
        }

        // If the object is not in recalling, record the path points if it is moving
        else
        {
            _rb.isKinematic = false;
            _isMoving = _rb.velocity.magnitude != 0;

            if (_isMoving)
            {
                // For every recordFrequency time, record a path point with position and rotation
                _recordTimer -= Time.deltaTime;

                if (_recordTimer <= 0)
                {
                    var transform1 = transform;
                    RecordPathPoint(transform1.position, transform1.eulerAngles);
                    ResetRecordTimer();
                }
            }

            // if the object stays for a long time, start remove the path points from the beginning
            // every recordFrequency time
            else
            {
                _recallTimer -= Time.deltaTime;
                if (_recallTimer <= 0)
                {
                    if (pathPositions.Count > 0 && pathRotations.Count > 0)
                    {
                        _recordTimer -= Time.deltaTime;
                        if (_recordTimer <= 0)
                        {
                            pathPositions.RemoveAt(0);
                            pathRotations.RemoveAt(0);
                            ResetRecordTimer();
                        }
                    }
                    else
                    {
                        ResetRecallTimer();
                    }
                }

            }
        }
    }

    /**
     *
     * Record the path point with position and rotation
     * @param {position} the position of the path point
     * @param {rotation} the rotation of the path point
     */
    private void RecordPathPoint(Vector3 position, Vector3 rotation)
    {
        // if there are too many path points, remove the oldest one
        if (pathPositions.Count >= _maxPathPoints && pathRotations.Count >= _maxPathPoints)
        {
            pathPositions.RemoveAt(0);
            pathRotations.RemoveAt(0);
        }
        pathPositions.Add(position);
        pathRotations.Add(rotation);
    }

    /**
     *
     * Try to recall on a object. If two pathPointsArray are empty, 
        meaning the object has arrived the start point;
        If not, set the last point as the next destination, 
        and remove it from the array after using it;
     */
    public void StartRecallingPath()
    {
        if (pathPositions.Count <= 0 || pathRotations.Count <= 0)
        {
            _isRecalling = false;
            return;
        }

        destPosition = pathPositions[^1];
        destRotation = pathRotations[^1];

        pathPositions.RemoveAt(pathPositions.Count - 1);
        pathRotations.RemoveAt(pathRotations.Count - 1);
        _isRecalling = true;
    }

    /**
     *
     * Reset the record timer
     */
    private void ResetRecordTimer()
    {
        _recordTimer = _recordFrequency;
    }

    /**
     *
     * Reset the recall timer
     */
    private void ResetRecallTimer()
    {
        _recallTimer = _recallMaxTime;
    }
}
