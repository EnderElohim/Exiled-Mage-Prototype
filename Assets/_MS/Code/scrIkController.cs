using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class scrIkController : MonoBehaviour
{
    [Header("Assignment")]
    public Transform leftHandTarget;
    public Transform rightHandTarget;
    public float weight = 1;
    private bool ikActive = true;
    private Animator anim;
    private Vector3 leftHandTargetStartingPosition;
    private Vector3 rightHandTargetStartingPosition;
    private bool isLedgeJumpOn;
    private void Awake()
    {
        anim = GetComponent<Animator>();
        leftHandTargetStartingPosition = leftHandTarget.localPosition;
        rightHandTargetStartingPosition = rightHandTarget.localPosition;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        //print("OnAnimatorIK");
        if (anim == null)
        {
            Debug.LogWarning("OnAnimatorIK is missing Animator(anim)");
            return;
        }

        if (ikActive)
        {
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, weight);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, weight);


            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, weight);
            anim.SetIKRotationWeight(AvatarIKGoal.RightHand, weight);



            anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
            anim.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);

            anim.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
            anim.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);

        }
        else
        {
            if (anim == null) return;

            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);

            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
            anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
        }
    }

    public void ToggleIK(bool _val)
    {
        ikActive = _val;
    }

    public void LedgeJumpAnim()
    {
        if (isLedgeJumpOn == true) return;
        isLedgeJumpOn = true;

        leftHandTarget.DOLocalMove(leftHandTarget.localPosition - (leftHandTarget.transform.up * 0.7f), 0.3f).SetEase(Ease.Linear).OnComplete(() =>
        {
            leftHandTarget.DOLocalMove(leftHandTargetStartingPosition, 0.1f).SetEase(Ease.Linear).OnComplete(() =>
            {
                isLedgeJumpOn = false;
            });
        });

        rightHandTarget.DOLocalMove(rightHandTarget.localPosition - (rightHandTarget.transform.up * 0.7f), 0.3f).SetEase(Ease.Linear).OnComplete(() =>
        {
            rightHandTarget.DOLocalMove(rightHandTargetStartingPosition, 0.3f).SetEase(Ease.Linear).OnComplete(() =>
            {
                isLedgeJumpOn = false;
            });
        });
    }

}