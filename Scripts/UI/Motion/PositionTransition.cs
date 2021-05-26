using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Scuti.UISystem {
    public class PositionTransition : Transition {
        // ================================================
        #region CONTEXT MENUS
        // ================================================


#if UNITY_EDITOR
        [ContextMenu("Set As IN")]
        void SetInPosition() {
            m_InPosition = rectTransform.localPosition;
            initInPos = m_InPosition;
            EditorUtility.SetDirty(this);
        }

        [ContextMenu("Set As OUT")]
        void SetOutPosition() {
            m_OutPosition = rectTransform.localPosition;
            initOutPos = m_OutPosition;
            EditorUtility.SetDirty(this);
        }

        [ContextMenu("Set As AWAY")]
        void SetAwayPosition() {
            m_AwayPos = rectTransform.localPosition;
            initOutPos = m_OutPosition;
            EditorUtility.SetDirty(this);
        }

        [ContextMenu("Goto IN")]
        void GotoInPosition(){
            m_State = State.IdleAtIn;
            rectTransform.localPosition = m_InPosition;
            EditorUtility.SetDirty(this);
        }

        [ContextMenu("Goto OUT")]
        void GotoOutPosition() {
            m_State = State.IdleAtOut;
            rectTransform.localPosition = m_OutPosition;
            EditorUtility.SetDirty(this);
        }

        [ContextMenu("Goto AWAY")]
        void GotoAwayPosition() {
            m_State = State.Away;
            rectTransform.localPosition = m_AwayPos;
            EditorUtility.SetDirty(this);
        }
#endif
#endregion

        // ================================================
        #region MEMBERS
        // ================================================
        [BoxGroup("Position Transition")]
        [Required] public RectTransform rectTransform;

        // --CONFIGURATION--
        [BoxGroup("Position Transition")]
        public bool showDetailedView;

        [Tooltip("Animation curve are normalised along the X axis")]
        [ShowIf("showDetailedView")]
        [BoxGroup("Position Transition")]
        [SerializeField] AnimationCurve m_InCurve = AnimationCurve.Linear(0, 0, .2f, 1);

        [Tooltip("Animation curve are normalised along the X axis")]
        [ShowIf("showDetailedView")]
        [BoxGroup("Position Transition")]
        [SerializeField]
        AnimationCurve m_OutCurve = AnimationCurve.Linear(0, 0, .2f, 1);

        [ShowIf("showDetailedView")]
        [BoxGroup("Position Transition")]
        [SerializeField]
        Vector3 m_InPosition;

        [ShowIf("showDetailedView")]
        [SerializeField]
        [BoxGroup("Position Transition")]
        Vector3 m_OutPosition;

        [ShowIf("showDetailedView")]
        [SerializeField]
        [BoxGroup("Position Transition")]
        Vector3 m_AwayPos;

        // --PRIVATE--
        Vector3 m_StartPos;
        float m_StartTime;
        float m_EndTime;

        float InDuration {
            get {
                var first = m_InCurve.keys[0];
                var last = m_InCurve.keys[m_InCurve.keys.Length - 1];
                return last.time - first.time;
            }
        }
        float OutDuration {
            get {
                var first = m_OutCurve.keys[0];
                var last = m_OutCurve.keys[m_InCurve.keys.Length - 1];
                return last.time - first.time;
            }
        }

        // For some reason I can't set this to just private.
        // Perhaps during edit time the values are not updated if this is now serialized.
        // But these are just backup values so we hide it as well/
        [SerializeField] [HideInInspector] Vector3 initInPos, initOutPos, initAwayPos;
        #endregion

        void Start() {
            Init();
        }

        void OnValidate() {
            Init();
        }

        void Init(){
            rectTransform = GetComponent<RectTransform>();
            initAwayPos = m_AwayPos;
        }

        // ================================================
        #region IMPLEMENTATION
        // ================================================
        protected override bool TransitionInOverTime() {
            // Get the appropriate value from the AnimationCurve using a normalised X axis value and lerp
            float normX = (CurrentTime - m_StartTime) / (m_EndTime - m_StartTime);
            float curveValue = m_InCurve.Evaluate(normX);
            if (!float.IsNaN(curveValue))
                rectTransform.localPosition = Vector3.LerpUnclamped(m_StartPos, m_InPosition, curveValue);

            return m_EndTime < CurrentTime;
        }

        protected override bool TransitionOutOverTime() {
            // Get the appropriate value from the AnimationCurve using a normalised X axis value and lerp
            float normX = (CurrentTime - m_StartTime) / (m_EndTime - m_StartTime);
            normX = Mathf.Clamp01(normX);
            float curveValue = m_OutCurve.Evaluate(normX);
            if (!float.IsNaN(curveValue))
                rectTransform.localPosition = Vector3.LerpUnclamped(m_StartPos, m_OutPosition, curveValue);

            return m_EndTime < CurrentTime;
        }

        protected override void OnStartTransitionIn() {
            // Move the RT to the out position and start to transition in from there
            rectTransform.localPosition = m_OutPosition;

            m_StartTime = CurrentTime;
            m_EndTime = CurrentTime + InDuration;
            m_StartPos = rectTransform.localPosition;
        }


        public override float GetInDuration()
        {
            return InDuration;
        }

        public override float GetOutDuration()
        {
            return OutDuration;
        }

        protected override void OnStartTransitionOut() {
            // Move the RT to the in position and start to transition out from there
            rectTransform.localPosition = m_InPosition;

            m_StartTime = CurrentTime;
            m_EndTime = CurrentTime + OutDuration;
            m_StartPos = rectTransform.localPosition;
        }

        protected override void OnEndTransitionOut() {
            // Set the position precisely to make sure it's placed right
            // We move the element to the OUT position
            rectTransform.localPosition = m_AwayPos;
        }

        protected override void OnEndTransitionIn() {
            // Set the position precisely to make sure it's placed right
            rectTransform.localPosition = m_InPosition;
        }
        #endregion

        // ================================================
        #region ADJUST POSITIONS TO ASPECT RATIO
        // ================================================
        Resolution? lastRes;
        void Update() {
            HandleAspectRatioChanges();
        }

        void HandleAspectRatioChanges() {
            if (RatioUpdated())
                UpdatePositions();
        }

        // Updates the positions WRT to the new aspect ratio
        void UpdatePositions() {
            // TODO: Better to simplify this
            var currentResolution = ScreenX.GetWindowSize();

            var referenceRatio = m_ReferenceScreenResolution.x / m_ReferenceScreenResolution.y;
            var currentRatio = currentResolution.x / currentResolution.y;

            float ratioDelta = (currentRatio - referenceRatio) / referenceRatio;

            Vector2 scale = Vector3.zero;
            if (ratioDelta < 0)
                scale = new Vector2(1, 1 + ratioDelta);
            else
                scale = new Vector2(1 + ratioDelta, 1);


            m_InPosition = new Vector3(
                initInPos.x * scale.x,
                initInPos.y * scale.y,
                initInPos.z
            );
            m_AwayPos = new Vector3(
                initAwayPos.x * scale.x,
                initAwayPos.y * scale.y,
                initAwayPos.z
            );
            m_OutPosition = new Vector3(
                initOutPos.x * scale.x,
                initOutPos.y * scale.y,
                initOutPos.z
            );
        }

        bool RatioUpdated() {
            if (lastRes == null) {
                lastRes = new Resolution() {
                    width = ScreenX.Width,
                    height = ScreenX.Height
                };
                return true;
            }
            else {
                var currRes = new Resolution() {
                    width = ScreenX.Width,
                    height = ScreenX.Height
                };
                var result = (lastRes.Value.width != currRes.width || lastRes.Value.height != currRes.height);
                lastRes = currRes;
                return result;
            }
        }
        #endregion


#if UNITY_EDITOR
        void OnDrawGizmosSelected() {
            if (Selection.activeGameObject != gameObject) return;

            var worldPos = rectTransform.position;
            var worldAway = rectTransform.parent.TransformPoint(m_AwayPos);
            var worldOut = rectTransform.parent.TransformPoint(m_OutPosition);
            var worldIn = rectTransform.parent.TransformPoint(m_InPosition);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(worldPos, worldOut);
            Gizmos.DrawLine(worldOut, worldIn);
            Gizmos.DrawLine(worldIn, worldPos);

            float size = (1080 - ScreenX.Height) * .1f;
            Gizmos.color = new Color(1, 0, 0, .5f);
            Gizmos.DrawSphere(worldAway, size);
            Gizmos.color = new Color(1, 1, 0, .5f);
            Gizmos.DrawSphere(worldOut, size);
            Gizmos.color = new Color(0, 1, 0, .5f);
            Gizmos.DrawSphere(worldIn, size);
        }
#endif
    }
}
