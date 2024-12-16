using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [Range(0.1f, 30)]
    public float MoveSpeed = 8.5f;
    [Range(0, 30)]
    public float MinZoom = 1.2f;
    [Range(0, 30)]
    public float MaxZoom = 10.0f;
    [Range(0, 10)]
    public float ZoomSpeed = 1.0f;

    [Range(1, 60)]
    public float TargetPositionMultiplier = 24.0f;

    [Range(1, 60)]
    public float TargetZoomMultiplier = 12.0f;

    [SerializeField]
    public float TargetZoom { get { return _targetZoom; } }
    private float _targetZoom;
    private Vector2 _targetPosition;
    private Vector2? _panningClickPosition;
    private Vector3? _panningCameraPosition;

    private new Camera camera;

    void Start()
    {
        camera = GetComponent<Camera>();
        _targetZoom = camera.orthographicSize;
        _targetPosition = transform.position;
    }

    void Update()
    {
        var isPanning = Input.GetMouseButton(1);

        if (!isPanning)
        {
            if (_panningClickPosition != null)
            {
                _panningClickPosition = null;
                _panningCameraPosition = null;
                _targetPosition = (Vector2)transform.position;
                _targetZoom = camera.orthographicSize;
            }

            // calculate target zoom
            var scroll = Input.GetAxisRaw("Mouse ScrollWheel");
            if (scroll != 0)
            {
                // scroll passes on it's negative value
                var zoomAmount = camera.orthographicSize * scroll * ZoomSpeed;
                // changes the target zoom, keeps it within min/max range
                _targetZoom = Mathf.Clamp(camera.orthographicSize - zoomAmount, MinZoom, MaxZoom);

                // Camera.main.transform.position = Vector2.Lerp(Camera.main.transform.position,
                //                                               Camera.main.ScreenToWorldPoint(Input.mousePosition),
                //                                               ZoomSpeed * Time.deltaTime);


                // //KEEP THE Z POSITION BEHIND YOUR OBJECTS!
                // Camera.main.transform.position = new Vector3(Camera.main.transform.position.x,
                //                                              Camera.main.transform.position.y,
                //                                              -20);
            }

            // calculate target position
            // moveSpeed dependent on zoom, moves slower when zoomed in
            // 50% speed at 2.5 zoom, 100% speed at 5 zoom, 200% speed at 10 zoom
            // 0.5 = 1 / (5 / 2.5), 1 = (5 / 5), 2 = (5 / 10)
            var movement = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) * MoveSpeed * Time.deltaTime * (1 / (5 / camera.orthographicSize));
            _targetPosition += (Vector2)movement;

            // Move towards the target zoom
            if (_targetZoom != camera.orthographicSize)
                if (!isPanning)
                    camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, _targetZoom, Time.deltaTime * TargetZoomMultiplier);
                else
                    // panning cancels target zoom immediately
                    _targetZoom = camera.orthographicSize;

            // Move towards the target position
            if (_targetPosition != (Vector2)transform.position)
            {
                var newPosition = Vector2.Lerp(transform.position, _targetPosition, Time.deltaTime * TargetPositionMultiplier);
                transform.position = new Vector3(newPosition.x, newPosition.y, -20);
            }
        }
        else
        {
            if (_panningClickPosition == null)
            {
                _panningClickPosition = Input.mousePosition;
                _panningCameraPosition = transform.position;
            }
            // We need to apply the delta between the initial click position and the current mouse position to the camera's position
            var worldDelta = camera.ScreenToWorldPoint(Input.mousePosition) - camera.ScreenToWorldPoint(_panningClickPosition.Value);

            transform.position = new Vector3(
                _panningCameraPosition.Value.x - worldDelta.x,
                _panningCameraPosition.Value.y - worldDelta.y,
                -20
            );
        }

    }

}