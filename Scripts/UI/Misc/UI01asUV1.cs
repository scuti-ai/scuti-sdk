using UnityEngine;
using UnityEngine.UI;

public class UI01asUV1 : BaseMeshEffect
{
	protected UI01asUV1()
	{
	}

	private RectTransform _rect;

	private RectTransform rect
	{
		get
		{
			if (_rect == null)
			{
				_rect = GetComponent<RectTransform>();
			}

			return _rect;
		}
	}

	public override void ModifyMesh(VertexHelper vh)
	{
		UIVertex vert = new UIVertex();
		UIVertex first = new UIVertex();
		UIVertex last = new UIVertex();
		vh.PopulateUIVertex(ref first, 0);
		vh.PopulateUIVertex(ref last, vh.currentVertCount - 1);

		var width = Mathf.Abs(first.position.x - last.position.x);
		var height = Mathf.Abs(first.position.y - last.position.y);

		for (int i = 0; i < vh.currentVertCount; i++)
		{
			vh.PopulateUIVertex(ref vert, i);
			float x = (vert.position.x + width * rect.pivot.x) / width;
			float y = (vert.position.y + height * rect.pivot.y) / height;
			vert.uv1 = new Vector2(x, y);
			vh.SetUIVertex(vert, i);
		}
	}
}