using System.Collections.Generic;
using UnityEngine;

namespace Scuti.UI
{
	[CreateAssetMenu(fileName = "Category", menuName = "Scuti/Categories/New Category")]
	public class CategoryData : ScriptableObject
	{
		public string Name;
		public Sprite Image;
		public bool IsSelected;
	}
}