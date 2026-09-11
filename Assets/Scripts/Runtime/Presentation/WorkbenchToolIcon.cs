using UnityEngine;
using UnityEngine.UI;

namespace CurioClerk.Presentation
{
    public sealed class WorkbenchToolIcon : MaskableGraphic
    {
        private string _toolId = string.Empty;
        public void Configure(string toolId) { _toolId = toolId; raycastTarget = false; SetVerticesDirty(); }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            if (_toolId == "dry-cloth")
            {
                Line(mesh, -.39f, .19f, .27f, .36f);
                Line(mesh, .27f, .36f, .38f, -.21f);
                Line(mesh, .38f, -.21f, -.28f, -.36f);
                Line(mesh, -.28f, -.36f, -.39f, .19f);
                Line(mesh, -.24f, .09f, .18f, .20f, .03f);
                Line(mesh, -.13f, -.20f, .24f, -.11f, .03f);
                for (var i = 0; i < 4; i++) Line(mesh, -.25f + i * .15f, -.36f + i * .035f, -.27f + i * .15f, -.42f + i * .035f, .025f);
            }
            else if (_toolId == "insulating-putty")
            {
                Line(mesh, -.34f, .18f, .17f, .18f);
                Line(mesh, -.34f, .18f, -.25f, -.33f);
                Line(mesh, -.25f, -.33f, .12f, -.33f);
                Line(mesh, .12f, -.33f, .17f, .18f);
                Line(mesh, -.34f, .18f, -.25f, .29f);
                Line(mesh, -.25f, .29f, .12f, .29f);
                Line(mesh, .12f, .29f, .17f, .18f);
                Ring(mesh, new Vector2(.29f, -.28f), .12f);
            }
            else if (_toolId == "warm-pad" || _toolId == "rubber-grip")
            {
                Line(mesh, -.36f, -.21f, -.36f, .20f);
                Line(mesh, -.36f, .20f, -.25f, .29f);
                Line(mesh, -.25f, .29f, .25f, .29f);
                Line(mesh, .25f, .29f, .36f, .20f);
                Line(mesh, .36f, .20f, .36f, -.21f);
                Line(mesh, .36f, -.21f, .25f, -.29f);
                Line(mesh, .25f, -.29f, -.25f, -.29f);
                Line(mesh, -.25f, -.29f, -.36f, -.21f);
                for (var i = 0; i < 3; i++)
                {
                    var x = -.18f + i * .18f;
                    Line(mesh, x, -.16f, x + .04f, .04f, .03f);
                    if (_toolId == "warm-pad") Line(mesh, x + .04f, .04f, x, .18f, .03f);
                }
            }
            else if (_toolId == "wooden-wedge")
            {
                Line(mesh, -.4f, -.22f, .29f, -.22f);
                Line(mesh, .29f, -.22f, .29f, .29f);
                Line(mesh, .29f, .29f, -.4f, -.22f);
                Line(mesh, -.18f, -.14f, .19f, .13f, .025f);
            }
            else if (_toolId == "thin-lever" || _toolId == "paper-folder")
            {
                Line(mesh, -.37f, -.35f, .07f, .25f, .12f);
                Line(mesh, .07f, .25f, .26f, .38f, .16f);
                Line(mesh, .26f, .38f, .34f, .29f, .12f);
                Line(mesh, .34f, .29f, .15f, .16f, .12f);
            }
            else if (_toolId == "brass-lid")
            {
                Ring(mesh, Vector2.zero, .34f);
                Ring(mesh, Vector2.zero, .24f);
                Line(mesh, -.10f, -.04f, -.10f, .09f);
                Line(mesh, -.10f, .09f, .10f, .09f);
                Line(mesh, .10f, .09f, .10f, -.04f);
            }
            else if (_toolId == "glass-dropper")
            {
                Ring(mesh, new Vector2(.17f, .25f), .17f);
                Line(mesh, .03f, .12f, -.24f, -.23f, .075f);
                Line(mesh, -.24f, -.23f, -.31f, -.34f, .03f);
                Ring(mesh, new Vector2(-.34f, -.43f), .035f);
            }
            else if (_toolId == "soft-ribbon")
            {
                Ring(mesh, new Vector2(-.18f, .12f), .17f);
                Ring(mesh, new Vector2(.18f, .12f), .17f);
                Line(mesh, -.04f, .04f, -.23f, -.34f, .10f);
                Line(mesh, .04f, .04f, .23f, -.34f, .10f);
            }
            else if (_toolId == "sealed-reply")
            {
                Line(mesh, -.39f, -.26f, .39f, -.26f);
                Line(mesh, .39f, -.26f, .39f, .26f);
                Line(mesh, .39f, .26f, -.39f, .26f);
                Line(mesh, -.39f, .26f, -.39f, -.26f);
                Line(mesh, -.38f, .25f, 0, -.02f, .035f);
                Line(mesh, 0, -.02f, .38f, .25f, .035f);
            }
            else if (_toolId.Contains("key"))
            {
                Ring(mesh, new Vector2(-.24f, .16f), .19f);
                Line(mesh, -.1f, .05f, .37f, -.3f);
                Line(mesh, .21f, -.18f, .14f, -.32f);
                Line(mesh, .34f, -.28f, .27f, -.42f);
            }
            else if (_toolId.Contains("tweezers") || _toolId.Contains("pliers"))
            {
                Line(mesh, -.10f, .40f, -.26f, -.35f);
                Line(mesh, -.1f, .40f, .27f, -.31f);
                Line(mesh, -.26f, -.35f, -.16f, -.42f);
                Line(mesh, .27f, -.31f, .17f, -.40f);
            }
            else if (_toolId.Contains("brush"))
            {
                Line(mesh, .15f, .42f, 0, -.12f, .10f);
                for (var i = 0; i < 6; i++) Line(mesh, -.18f + i * .065f, -.12f, -.22f + i * .07f, -.40f, .04f);
            }
            else if (_toolId.Contains("thread") || _toolId.Contains("sewing"))
            {
                Ring(mesh, new Vector2(-.19f, -.04f), .23f);
                Line(mesh, -.02f, -.4f, .31f, .39f, .03f);
                Line(mesh, .23f, .3f, .36f, .32f, .03f);
            }
            else if (_toolId.Contains("pencil"))
            {
                Line(mesh, -.31f, -.34f, .3f, .30f, .13f);
                Line(mesh, .27f, .27f, .35f, .38f, .04f);
                Line(mesh, -.32f, -.34f, -.39f, -.42f, .03f);
            }
            else if (_toolId.Contains("jar") || _toolId.Contains("cup") || _toolId.Contains("bowl"))
            {
                Line(mesh, -.3f, .3f, .3f, .3f);
                Line(mesh, -.25f, .29f, -.22f, -.36f);
                Line(mesh, .25f, .29f, .22f, -.36f);
                Line(mesh, -.22f, -.36f, .22f, -.36f);
                Line(mesh, -.2f, -.04f, .2f, -.04f, .03f);
            }
            else
            {
                Line(mesh, -.34f, -.32f, .29f, -.32f);
                Line(mesh, .29f, -.32f, .29f, .20f);
                Line(mesh, .29f, .20f, -.18f, .34f);
                Line(mesh, -.18f, .34f, -.34f, -.32f);
                Line(mesh, -.18f, .34f, -.12f, .08f, .03f);
                Line(mesh, -.12f, .08f, .29f, .20f, .03f);
            }
        }

        private void Ring(VertexHelper mesh, Vector2 center, float radius)
        {
            for (var i = 0; i < 20; i++)
            {
                var a = i * Mathf.PI * .1f;
                var b = (i + 1) * Mathf.PI * .1f;
                Line(mesh, center.x + Mathf.Cos(a) * radius, center.y + Mathf.Sin(a) * radius,
                    center.x + Mathf.Cos(b) * radius, center.y + Mathf.Sin(b) * radius, .045f);
            }
        }

        private void Line(VertexHelper mesh, float x1, float y1, float x2, float y2, float width = .055f)
        {
            var size = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height);
            var center = rectTransform.rect.center;
            var a = center + new Vector2(x1, y1) * size;
            var b = center + new Vector2(x2, y2) * size;
            var tangent = (b - a).normalized;
            var normal = new Vector2(-tangent.y, tangent.x) * (width * size * .5f);
            var index = mesh.currentVertCount;
            mesh.AddVert(a - normal, color, Vector2.zero);
            mesh.AddVert(a + normal, color, Vector2.zero);
            mesh.AddVert(b + normal, color, Vector2.zero);
            mesh.AddVert(b - normal, color, Vector2.zero);
            mesh.AddTriangle(index, index + 1, index + 2);
            mesh.AddTriangle(index + 2, index + 3, index);
        }
    }
}
