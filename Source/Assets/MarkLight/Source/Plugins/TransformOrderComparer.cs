using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using MarkLight.Views.UI;

namespace MarkLight
{
    /// <summary>
    /// Orders components/transforms by relative position in the object tree.
    /// </summary>
    public class UIViewOrderComparer : IComparer<UIView>
    {
        private TransformOrderComparer _transformComparer = new TransformOrderComparer();
        public int Compare(UIView x, UIView y)
        {
            return _transformComparer.Compare(x.transform, y.transform);
        }
    }

    /// <summary>
    /// Orders components/transforms by relative position in the object tree.
    /// </summary>
    public class TransformOrderComparer : IComparer<Transform>
    {
        public int Compare(Transform x, Transform y)
        {
            if (x == y)
                return 0;
            if (y.IsChildOf(x))
            {
                return -1;
            }
            if (x.IsChildOf(y))
            {
                return 1;
            }

            List<Transform> xparentList = GetParents(x);
            List<Transform> yparentList = GetParents(y);

            for (int xIndex = 0; xIndex < xparentList.Count; xIndex++)
            {
                if (y.IsChildOf(xparentList[xIndex]))
                {
                    int yIndex = yparentList.IndexOf(xparentList[xIndex]) - 1;
                    xIndex -= 1;
                    return xparentList[xIndex].GetSiblingIndex() - yparentList[yIndex].GetSiblingIndex();
                }
            }

            return xparentList[xparentList.Count - 1].GetSiblingIndex() - yparentList[yparentList.Count - 1].GetSiblingIndex();
        }


        private List<Transform> GetParents(Transform t)
        {
            List<Transform> parents = new List<Transform>();
            parents.Add(t);

            while (t.parent != null)
            {
                parents.Add(t.parent);
                t = t.parent;
            }
            return parents;
        }
    }
}
