using System;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

using Scuti;

namespace Scuti.UI {
    public class RewardPresenter : Presenter<RewardPresenter.Model> {
        public class Model : Presenter.Model {
            public string title;
            public string subtitle;
            public int reward;
        }

        [SerializeField] Transform AnimatedCoinPrefab;
        [SerializeField] Text titleText;
        [SerializeField] Text subtitleText;
        [SerializeField] Text rewardText;
        [SerializeField] float aniDuration;
        [SerializeField] float aniIncrementalDelay;
        [SerializeField] Transform Origin;


        protected override void OnSetState() {
            titleText.text = Data.title;
            subtitleText.text = Data.subtitle;
            rewardText.text = $"+{Data.reward} SCUTIS";
        }

        public void Claim()
        {
            int count = Mathf.Min(25, Mathf.CeilToInt((float)Data.reward / 25f));
            float delay = 0;
            var startPoint = Origin.position;
            var endPoint = UIManager.TopBar.Wallet.Icon.transform.position;
            //ScutiLogger.Log("Claim");
            for (var i = 0; i < count; i++)
            {
                var coin = Instantiate(AnimatedCoinPrefab, UIManager.Effects.TargetTransform);
                coin.gameObject.SetActive(false);
                coin.localScale = new Vector3(0.8f, 0.8f, 0.8f);


                    var xDiff = endPoint.x - startPoint.x;
                    var yDiff = endPoint.y - startPoint.y;
                    xDiff *= UnityEngine.Random.Range(-1f,1f);
                    yDiff *= UnityEngine.Random.Range(-0.2f, 0.2f);
                var controlPt = new Vector2(xDiff + startPoint.x, yDiff + startPoint.y);

                UIManager.Effects.Tween(coin, startPoint, endPoint , aniDuration, delay, EffectsManager.PathType.Bezier, true, controlPt);
                delay += UnityEngine.Random.Range(0, aniIncrementalDelay);
            } 
#pragma warning disable 4014
            UIManager.TopBar.Wallet.RefreshOverTime(Data.reward, aniDuration + delay); 
#pragma warning restore 4014
            Close();

        }
    }
}
