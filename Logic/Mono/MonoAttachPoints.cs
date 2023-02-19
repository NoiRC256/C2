using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoAttachPoints : MonoBehaviour
{
    [System.Serializable]
    public class MonoAttachPointEntry
    {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public Transform Transform { get; private set; }
    }

    [SerializeField] MonoAttachPointEntry[] _attachPoints = new MonoAttachPointEntry[] { };

    public Dictionary<string, Transform> AttachPointsDict => _attachPointsDict;

    private Dictionary<string, Transform> _attachPointsDict = new Dictionary<string, Transform>();

    private void Awake()
    {
        for(int i = 0; i < _attachPoints.Length; i++)
        {
            var attachPoint = _attachPoints[i];
            _attachPointsDict[attachPoint.Name] = attachPoint.Transform;
        }
    }

}
