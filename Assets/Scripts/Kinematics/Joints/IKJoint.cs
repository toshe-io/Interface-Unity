// TODO: Port to interface

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 public class IKJoint : MonoBehaviour
 {
    private enum JointActiveAngle { X, Y, Z };

    [SerializeField]
    private bool isEnabled = true;

    [SerializeField]
    private JointActiveAngle activeJointAngle;

    [SerializeField]
    public float minAngle;

    [SerializeField]
    public float maxAngle;

    public float getActiveAngle() {
        return transform.localEulerAngles[(int)activeJointAngle];
    }

    public void updateJointAngles(Transform newJoint) {
        Vector3 euler = Helpers.Math.normalizeEuler(newJoint.localRotation.eulerAngles);
        Vector3 newEuler = Vector3.zero;

        int activeAngle = (int)activeJointAngle;

        if (isEnabled) {
            newEuler[activeAngle] = Mathf.Clamp (euler[activeAngle], minAngle, maxAngle);
        }
		
		transform.localEulerAngles = newEuler;

        Lebug.Log(name, newEuler[activeAngle], "IKJoint");
    }
 }