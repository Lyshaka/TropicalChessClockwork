using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
	[SerializeField] Vector3 offset = new(0f, 10f, 10f);
	[SerializeField] float smoothDampTime = 0.3f;

	Transform _target;
	Vector3 _currentVelocity;

	private void Start()
	{
		_target = Player.Instance.transform;
		transform.position = _target.position + offset;
		transform.LookAt(_target.position);
	}

	private void LateUpdate()
	{
		Follow();
	}

	void Follow()
	{
		transform.position = Vector3.SmoothDamp(transform.position, _target.position + offset, ref _currentVelocity, smoothDampTime);
	}
}
