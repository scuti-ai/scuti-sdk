using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Scuti.UI {
	public class RatingStarsWidget : MonoBehaviour {
		public UnityEvent onValueChange;
		public UnityEvent onMadeInteractable;
		public UnityEvent onMadeNonInteractable;

		public float Count {
			get {
				return (int)(Value / (1f / Levels));
			}
		}

		public float m_Value;
		public float Value {
			get { return m_Value; }
			set {
				m_Value = value;
				onValueChange.Invoke();
			}
		}

		public int Levels {
			get { return m_Fillables.Length; }
		}

		public bool IsInteractable {
			get { return GetComponent<CanvasGroup>().interactable; }
		}

		[SerializeField] Image[] m_Fillables;
		[SerializeField] float m_FillRate;

		float displayValue;

        /// <summary>
        /// Set the value in normalized 0..1 range
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(float value) {
			Value = Mathf.Clamp01(value);
		}

		public void ToggleInteractable() {
			SetInteractable(!IsInteractable);
		}

		public void SetInteractable(bool state) {
			GetComponent<CanvasGroup>().interactable = state;

			if (state)
				onMadeInteractable.Invoke();
			else
				onMadeNonInteractable.Invoke();
		}

		void Update() {
			// TODO: Not very optimal, but does the job
			displayValue = Mathf.MoveTowards(displayValue, Value, Time.deltaTime * m_FillRate);
			displayValue = Mathf.Clamp01(displayValue);

			foreach (var fillable in m_Fillables)
				fillable.fillAmount = 0;

			float valuePerStar = 1f / m_Fillables.Length;
			int starCount = (int)(displayValue / valuePerStar);

			for (int i = 0; i < starCount; i++)
				m_Fillables[i].fillAmount = 1;

			if (starCount < m_Fillables.Length) {
				float leftOver = displayValue - starCount * valuePerStar;
				m_Fillables[starCount].fillAmount = leftOver / valuePerStar;
			}			
		}
	}
}
