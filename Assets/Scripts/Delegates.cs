using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Delegates
{
    public delegate float BeginOffset(Vector3 endPosition);
    public delegate float InterpolationValue(float t);
}
