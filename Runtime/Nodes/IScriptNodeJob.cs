using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public interface IScriptNodeJob : IAnimationJob { }

public struct NullScriptNodeJob : IScriptNodeJob
{
    public void ProcessAnimation(AnimationStream stream) { }

    public void ProcessRootMotion(AnimationStream stream) { }
}
