using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Events;

public class MonoTimer : MonoBehaviour
{
    [SerializeField][Min(0f)] private float _duration = 5f;
    [SerializeField] private bool _autoStart = false;
    [SerializeField] private bool _autoRestart = false;

    public UnityEvent Started;
    public UnityEvent Ended;
    public UnityEvent Stopped;

    private float _counter = 0f;
    private bool _isStarted = false;
    private bool _isEnded = false;

    private void Start()
    {
        if (_autoStart) StartTimer();
    }

    private void Update()
    {
        if (!_isStarted) return;
        if (_counter > 0f) _counter -= Time.deltaTime;
        else if (!_isEnded)
        {
            EndTimer();
        }
    }

    public void StartTimer()
    {
        ResetTimer();
        _isStarted = true;
        Started.Invoke();
    }

    public void EndTimer()
    {
        _counter = 0f;
        _isEnded = true;
        Ended?.Invoke();
        if (_autoRestart) StartTimer();
    }

    public void StopTimer()
    {
        _isStarted = false;
        Stopped?.Invoke();
    }

    public void ResetTimer()
    {
        _counter = _duration;
        _isStarted = false;
        _isEnded = false;
    }
}
