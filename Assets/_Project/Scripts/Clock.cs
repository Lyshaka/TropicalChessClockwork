using UnityEngine;
using TMPro;

public class Clock : MonoBehaviour
{
	[SerializeField] Transform pendulumPivot;
	[SerializeField] TextMeshPro clockTimeTMP;
	[SerializeField] float pendulumAngle = 30f;
	[SerializeField] int ticks = 0;

	float _elapsedTime = 0f;
	float _angleValue;
	bool _tock = false;

	private void Update()
	{
		_angleValue = pendulumAngle * Mathf.Sin(_elapsedTime * Mathf.PI) * (_tock ? 1f : -1f);

		clockTimeTMP.text = _elapsedTime.ToString("0.000");
		pendulumPivot.rotation = Quaternion.AngleAxis(_angleValue, Vector3.up);

		_elapsedTime += Time.deltaTime;
		if (_elapsedTime >= 1f)
		{
			_tock = !_tock;
			ticks++;
			_elapsedTime -= 1f;
		}
	}
}
