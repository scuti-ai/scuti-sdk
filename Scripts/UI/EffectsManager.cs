using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Scuti
{
    public class EffectsManager : MonoBehaviour
    {

        public enum PathType
        {
            Linear = 0,
            Bezier = 1
        }

        public class TweenData
        {
            public Transform Trans;
            public Vector2 StartPosition;
            public Vector2 EndPosition;
            public Vector2 ControlPoint;
            public float Duration;
            public float CurrentTime;
            public PathType Path;
            public bool DestroyOnComplete;
            public bool Complete;
        }

        public Transform TargetTransform;

        [SerializeField] // for testing
        private List<TweenData> Tweens = new List<TweenData>();

        internal void Tween(Transform trans, Vector2 startPoint, Vector2 endPoint, float duration, float delay, PathType path, bool destroyOnComplete)
        {
            Tween(trans, startPoint, endPoint, duration, delay, path, destroyOnComplete, Vector2.zero);
        }

        internal async void Tween(Transform trans, Vector2 startPoint, Vector2 endPoint, float duration, float delay, PathType path, bool destroyOnComplete, Vector2 controlPt)
        {
            if (delay > 0)
                await Task.Delay(TimeSpan.FromSeconds(delay));


            trans.gameObject.SetActive(true);
            trans.position = startPoint;
            var tweenData = new TweenData()
            {
                EndPosition = endPoint,
                StartPosition = startPoint,
                Duration = duration,
                CurrentTime = 0f,
                Trans = trans,
                Path = path,
                ControlPoint = controlPt,
                DestroyOnComplete = destroyOnComplete,
                Complete = false
            };
            Tweens.Add(tweenData);
        }

        void Update()
        {
            for (var i = 0; i < Tweens.Count; i++)
            {
                var tween = Tweens[i];
                if (tween.Complete)
                {
                    if (tween.DestroyOnComplete)
                    {
                        Destroy(tween.Trans.gameObject);
                    }
                    Tweens.RemoveAt(i);
                    i--;
                    continue;
                }

                tween.CurrentTime += Time.deltaTime;
                var percent = tween.CurrentTime / tween.Duration;

                if (percent > 1)
                {
                    percent = 1;
                    tween.Complete = true;
                }
                switch (tween.Path)
                {
                    case PathType.Linear:
                        tween.Trans.position = (percent * (tween.EndPosition - tween.StartPosition)) + tween.StartPosition;
                        break;
                    case PathType.Bezier:
                        tween.Trans.position = ScutiUtils.GetQuadraticCoordinates(percent, tween.StartPosition, tween.ControlPoint, tween.EndPosition);
                        break;
                }
            }
        }
    }
}
