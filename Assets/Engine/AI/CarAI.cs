#region

using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

#endregion

namespace OSMStreetNetwork.AI
{
    public class CarAI
    {
        private GameObject _carRoot;
        private GameObject _carVisual;
        private SplineContainer _splineContainer;
        private float _normalizedTime = 0f;
        private float _speed = 0.1f;
        private float _rotationSmoothness = 5f;

        public CarAI(SplineContainer splineContainer, GameObject carPrefab = null, float speed = 0.1f)
        {
            _splineContainer = splineContainer;
            _speed = speed;
            _carRoot = new GameObject("CarRoot");
            if (carPrefab)
            {
                _carVisual = MonoBehaviour.Instantiate(carPrefab);
            }
            else
            {
                _carVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            }
            _carVisual.name = "CarVisual";
            _carVisual.transform.SetParent(_carRoot.transform, false);
            _carVisual.transform.localPosition = Vector3.zero;
            _carVisual.transform.localRotation = Quaternion.identity;
            _carVisual.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
        }

        public void Update(float deltaTime)
        {
            if (_splineContainer == null || _splineContainer.Spline.Count < 2)
                return;

            _normalizedTime += _speed * deltaTime;
            _normalizedTime = Mathf.Clamp01(_normalizedTime);

            float lookAhead = 0.01f;
            float tNext = Mathf.Clamp01(_normalizedTime + lookAhead);

            Spline spline = _splineContainer.Spline;

            float3 posNow, posNext, _;
            SplineUtility.Evaluate(spline, _normalizedTime, out posNow, out _, out _);
            SplineUtility.Evaluate(spline, tNext, out posNext, out _, out _);
            _carRoot.transform.position = (Vector3)posNow;

            Vector3 forward = ((Vector3)(posNext - posNow)).normalized;
            if (forward.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(forward, Vector3.up);
                _carVisual.transform.rotation = Quaternion.Slerp(_carVisual.transform.rotation,
                    targetRot, deltaTime * _rotationSmoothness);
            }
        }
    }
}
