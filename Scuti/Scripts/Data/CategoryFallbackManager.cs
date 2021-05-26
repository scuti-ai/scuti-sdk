using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scuti
{
    // Ideally, we should replace this class with a server call so that categories can be dynamic. Fine for MVP though. -mg
    public class CategoryFallbackManager : MonoBehaviour
    {
        [Serializable]
        public struct CategoryData
        {
            public Categories Category;
            public Sprite Icon;
            public bool InActive;
        }

        [SerializeField] List<CategoryData> Categories;

        public List<CategoryData> ActiveCategories { get; private set; }
        private Dictionary<Categories, CategoryData> _categoryMap;

        private void Awake()
        {
            MapCategories();
        }

        private void MapCategories()
        {
            ActiveCategories = new List<CategoryData>();
            _categoryMap = new Dictionary<Categories, CategoryData>();
            foreach(var catData in Categories)
            {
                _categoryMap[catData.Category] = catData;
                if (!catData.InActive) ActiveCategories.Add(catData);
            }
        }

        public Sprite GetIcon(Categories category)
        {
            return _categoryMap[category].Icon;
        }

    }
}
