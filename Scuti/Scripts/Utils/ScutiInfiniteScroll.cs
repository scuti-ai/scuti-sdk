﻿/// Credit Tomasz Schelenz
/// Sourced from - https://bitbucket.org/SimonDarksideJ/unity-ui-extensions/issues/81/infinite-scrollrect
/// Demo - https://www.youtube.com/watch?v=uVTV7Udx78k  - configures automatically.  - works in both vertical and horizontal (but not both at the same time)  - drag and drop  - can be initialized by code (in case you populate your scrollview content from code)
/// Updated by Febo Zodiaco - https://bitbucket.org/UnityUIExtensions/unity-ui-extensions/issues/349/magnticinfinitescroll

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Scuti.UI
{
    /// <summary>
    /// Infinite scroll view with automatic configuration
    ///
    /// Fields
    /// - InitByUSer - in case your scrollrect is populated from code, you can explicitly Initialize the infinite scroll after your scroll is ready
    /// by calling Init() method
    ///
    /// Notes
    /// - does not work in both vertical and horizontal orientation at the same time.
    /// - in order to work it disables layout components and size fitter if present(automatically)
    ///
    /// </summary>
    //[AddComponentMenu("UI/Extensions/Scuti Infinite Scroll")]
    public class ScutiInfiniteScroll : MonoBehaviour
    {
        //if true user will need to call Init() method manually (in case the contend of the scrollview is generated from code or requires special initialization)
        [Tooltip("If false, will Init automatically, otherwise you need to call Init() method")]
        public bool InitByUser = false;
        protected ScrollRect _scrollRect;
        private ContentSizeFitter _contentSizeFitter;
        private VerticalLayoutGroup _verticalLayoutGroup;
        private HorizontalLayoutGroup _horizontalLayoutGroup;
        private GridLayoutGroup _gridLayoutGroup;
        protected bool _isVertical = false;
        protected bool _isHorizontal = false;
        private float _disableMarginX = 0;
        private float _disableMarginY = 0;
        private bool _hasDisabledGridComponents = false;
        protected List<RectTransform> items = new List<RectTransform>();
        private Vector2 _newAnchoredPosition = Vector2.zero;
        //TO DISABLE FLICKERING OBJECT WHEN SCROLL VIEW IS IDLE IN BETWEEN OBJECTS
        private float _threshold = 100f;
        private int _itemCount = 0;
        private float _recordOffsetX = 0;
        private float _recordOffsetY = 0;

        public Action<Transform> OnSiblingUpdate;

        private int _count = 0;

        protected virtual void Awake()
        {
            if (!InitByUser)
                Init();
        }

        public virtual void SetNewItems(ref List<Transform> newItems)
        {
            if (_scrollRect != null)
            {
                if (_scrollRect.content == null && newItems == null)
                {
                    return;
                }

                RemoveItems();

                foreach (Transform newItem in newItems)
                {
                    newItem.SetParent(_scrollRect.content);
                }

                SetItems();
            }
        }

        private void SetItems()
        {
            for (int i = 0; i < _scrollRect.content.childCount; i++)
            {
                items.Add(_scrollRect.content.GetChild(i).GetComponent<RectTransform>());
            }

            _itemCount = _scrollRect.content.childCount;
        }

        private bool initialized = false;

        public void Init()
        {
            if (initialized) return;
            initialized = true;
            Debug.LogWarning("-------------------------------------------- INIT --------------------------------------------");
            //Debug.LogWarning(" ==== childCount::  " + items.Count);
            _count = 0;
            //Debug.LogWarning("--------------------------------------- POST INIT ---------------------------------------");

            if (_scrollRect != null && _scrollRect.content != null)
            {
                _scrollRect.onValueChanged.RemoveAllListeners();
                _scrollRect.StopMovement();
                Vector2 npos = Vector2.zero;
                npos.x = -_scrollRect.content.sizeDelta.x / 2;
                _scrollRect.content.anchoredPosition = npos;
                _newAnchoredPosition = Vector2.zero;
            }

            var tempRect = GetComponent<ScrollRect>();
            if (tempRect != null)
            {
                _scrollRect = tempRect;
                _scrollRect.onValueChanged.AddListener(OnScroll);
                _scrollRect.movementType = ScrollRect.MovementType.Unrestricted;

                if (_scrollRect.content.GetComponent<VerticalLayoutGroup>() != null)
                {
                    _verticalLayoutGroup = _scrollRect.content.GetComponent<VerticalLayoutGroup>();
                }
                if (_scrollRect.content.GetComponent<HorizontalLayoutGroup>() != null)
                {
                    _horizontalLayoutGroup = _scrollRect.content.GetComponent<HorizontalLayoutGroup>();
                }
                if (_scrollRect.content.GetComponent<GridLayoutGroup>() != null)
                {
                    _gridLayoutGroup = _scrollRect.content.GetComponent<GridLayoutGroup>();
                }
                if (_scrollRect.content.GetComponent<ContentSizeFitter>() != null)
                {
                    _contentSizeFitter = _scrollRect.content.GetComponent<ContentSizeFitter>();
                }

                _isHorizontal = _scrollRect.horizontal;
                _isVertical = _scrollRect.vertical;

                if (_isHorizontal && _isVertical)
                {
                    //Debug.LogError("ScutiInfiniteScroll doesn't support scrolling in both directions, please choose one direction (horizontal or vertical)");
                }

                SetItems();

                CleanupItems();
                EnableGridComponents();
            }
            else
            {
                //Debug.LogError("ScutiInfiniteScroll => No ScrollRect component found");
            }
        }

        void DisableGridComponents()
        {
            //Debug.LogWarning(" DisableGridComponents");
            try
            {
                if (_isVertical)
                {
                    _recordOffsetY = 0;
                    for (int i = 0; i < items.Count; i++)
                    {
                        _recordOffsetY += items[i].GetComponent<RectTransform>().sizeDelta.y;
                    }

                    if (_verticalLayoutGroup)
                    {
                        _recordOffsetY += _verticalLayoutGroup.padding.bottom;
                        _recordOffsetY += _verticalLayoutGroup.padding.top;
                        _recordOffsetY += _verticalLayoutGroup.spacing * items.Count;
                    }


                    _disableMarginY = _recordOffsetY / 2;

                    _disableMarginY *= _scrollRect.content.localScale.y;

                    _recordOffsetY /= items.Count;
                    /*Debug.LogWarning("       "+ items[1].name+" ** "+ items[0].name);
                    _recordOffsetY = items[1].GetComponent<RectTransform>().anchoredPosition.y - items[0].GetComponent<RectTransform>().anchoredPosition.y;
                    if (_recordOffsetY < 0)
                    {
                        _recordOffsetY *= -1;
                    }*/
                    //_disableMarginY = _recordOffsetY * _itemCount / 2;
                    //Debug.LogWarning(" _isVertical "+ _disableMarginY + "  "+ _recordOffsetY);
                }
                if (_isHorizontal)
                {
                    _recordOffsetX = items[1].GetComponent<RectTransform>().anchoredPosition.x - items[0].GetComponent<RectTransform>().anchoredPosition.x;
                    if (_recordOffsetX < 0)
                    {
                        _recordOffsetX *= -1;
                    }
                    _disableMarginX = _recordOffsetX * _itemCount / 2;
                }
            

            if (_verticalLayoutGroup)
                {
                    _verticalLayoutGroup.enabled = false;
                }
                if (_horizontalLayoutGroup)
                {
                    _horizontalLayoutGroup.enabled = false;
                }
                if (_contentSizeFitter)
                {
                    _contentSizeFitter.enabled = false;
                }
                if (_gridLayoutGroup)
                {
                    _gridLayoutGroup.enabled = false;
                }
                _hasDisabledGridComponents = true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("ERROR  DisableGridComponents:: " + e.Message);
                ResetItems();
                EnableGridComponents();
            }
        }

        void EnableGridComponents()
        {
            //Debug.LogWarning("     EnableGridComponents:: ");
            if (_verticalLayoutGroup)
            {
                _verticalLayoutGroup.enabled = true;
            }
            if (_horizontalLayoutGroup)
            {
                _horizontalLayoutGroup.enabled = true;
            }
            if (_contentSizeFitter)
            {
                _contentSizeFitter.enabled = true;
            }
            if (_gridLayoutGroup)
            {
                _gridLayoutGroup.enabled = true;
            }
            _hasDisabledGridComponents = false;
        }

        public void OnScroll(Vector2 pos)
        {
            _count++;
            if (_count < 5)
                return;

                CheckBounds();
            
        }

        public void CheckBounds()
        {
            //Debug.LogWarning(" --==*==-- OnScroll::  ");
            if (!_hasDisabledGridComponents)
            {
                //Debug.LogWarning("     OnScroll::DisableGridComponents ");
                DisableGridComponents();
            }

            try
            {

                for (int i = 0; i < items.Count; i++)
                {
                    if (_isHorizontal)
                    {
                        if (_scrollRect.transform.InverseTransformPoint(items[i].gameObject.transform.position).x > _disableMarginX + _threshold)
                        {
                            _newAnchoredPosition = items[i].anchoredPosition;
                            _newAnchoredPosition.x -= _itemCount * _recordOffsetX;
                            items[i].anchoredPosition = _newAnchoredPosition;

                            var trans = _scrollRect.content.GetChild(_itemCount - 1).transform;
                            trans.SetAsFirstSibling();
                            OnSiblingUpdate?.Invoke(items[i].transform);
                        }
                        else if (_scrollRect.transform.InverseTransformPoint(items[i].gameObject.transform.position).x < -_disableMarginX)
                        {
                            _newAnchoredPosition = items[i].anchoredPosition;
                            _newAnchoredPosition.x += _itemCount * _recordOffsetX;
                            items[i].anchoredPosition = _newAnchoredPosition;

                            var trans = _scrollRect.content.GetChild(0);
                            trans.transform.SetAsLastSibling();
                            OnSiblingUpdate?.Invoke(items[i].transform);
                        }
                    }

                    if (_isVertical)
                    {
                        if (_scrollRect.transform.InverseTransformPoint(items[i].gameObject.transform.position).y > _disableMarginY + _threshold)
                        {
                            _newAnchoredPosition = items[i].anchoredPosition;
                            _newAnchoredPosition.y -= _itemCount * _recordOffsetY;
                            items[i].anchoredPosition = _newAnchoredPosition;
                            var trans = _scrollRect.content.GetChild(_itemCount - 1).transform;
                            trans.SetAsFirstSibling();
                            OnSiblingUpdate?.Invoke(items[i].transform);
                        }
                        else if (_scrollRect.transform.InverseTransformPoint(items[i].gameObject.transform.position).y < -_disableMarginY)
                        {
                            _newAnchoredPosition = items[i].anchoredPosition;
                            _newAnchoredPosition.y += _itemCount * _recordOffsetY;
                            items[i].anchoredPosition = _newAnchoredPosition;
                            var trans = _scrollRect.content.GetChild(0);
                            trans.transform.SetAsLastSibling();
                            OnSiblingUpdate?.Invoke(items[i].transform);
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                ScutiLogger.LogError("ERROR  OnScroll:: " + e.Message);
                ResetItems();
                CleanupItems();
                EnableGridComponents();
            }
        }

        public void CleanupItems()
        {
            int childCount = _scrollRect.content.childCount;
            //Debug.LogWarning(" **** childCount::" + childCount + "  " + items.Count);
            for (int i = childCount - 1; i >= 0; i--)
            {
                if(!items.Contains(_scrollRect.content.GetChild(i).GetComponent<RectTransform>()))
                {
                    Transform child = _scrollRect.content.GetChild(i);
                    child.SetParent(null);
                    GameObject.DestroyImmediate(child.gameObject);
                }else
                {

                }
                
            }
        }

        public void ResetItems()
        {
            if (_scrollRect != null && _scrollRect.content != null)
            {
                _scrollRect.onValueChanged.RemoveAllListeners();
                _scrollRect.StopMovement();
                Vector2 npos = Vector2.zero;
                npos.x = -_scrollRect.content.sizeDelta.x/2;
                _scrollRect.content.anchoredPosition = npos;
                _newAnchoredPosition = Vector2.zero;
            }
            //Debug.LogWarning(" ResetItems");
            if (items != null)
            {
                items.Clear();
            }

            if (_scrollRect != null && _scrollRect.content != null)
            {
                CleanupItems();
            }
        }

        public void RemoveItems()
        {
            //Debug.LogWarning(" RemoveItems");
            ResetItems();

            if (_scrollRect == null || _scrollRect.content == null)
                return;

            for (int i = _scrollRect.content.childCount - 1; i >= 0; i--)
            {
                Transform child = _scrollRect.content.GetChild(i);
                child.SetParent(null);
                GameObject.DestroyImmediate(child.gameObject);
            }
        }
    }
}