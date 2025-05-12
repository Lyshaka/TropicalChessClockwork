using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
	[SerializeField, Tooltip("Update per second")] float updateFrequency = 10f;

	TextMeshProUGUI _fpsText;
	float _elapsedTime;
	float _updateDuration;
	float _fps;
	float _worstFps = float.MaxValue;
	float _average;
	float[] _lastPlots = new float[100];
	int _lastPlotIndex = 0;

	private void Start()
	{
		_fpsText = GetComponentInChildren<TextMeshProUGUI>();
		_updateDuration = 1f / updateFrequency;
		_elapsedTime = _updateDuration;

	}

	private void Update()
	{
		// Storing FPS
		_fps = (1f / Time.deltaTime);

		// Storing last fps and calulating the average
		_lastPlots[_lastPlotIndex] = _fps;
		_lastPlotIndex = (_lastPlotIndex + 1) % 100;

		float sum = 0f;
		_worstFps = float.MaxValue;
		for (int i = 0; i < _lastPlots.Length; i++)
		{
			// Calulating  worst FPS out of the lasts
			if (_lastPlots[i] < _worstFps)
				_worstFps = _lastPlots[i];
			// Summing all the lasts for average
			sum += _lastPlots[i];
		}
		_average = sum / _lastPlots.Length;

		if (_elapsedTime < _updateDuration)
		{
			_elapsedTime += Time.deltaTime;
		}
		else
		{
			_elapsedTime = 0f;
			string text = "FPS : " + _fps.ToString("#.0");
			text += "\nWorst : " + _worstFps.ToString("#.0");
			text += "\nAverage : " + _average.ToString("#.0");
			_fpsText.text = text;
		}
	}
}
