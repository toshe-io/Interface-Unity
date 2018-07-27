using UnityEngine;
using System.Collections;

public class IKSingleAxis : MonoBehaviour
{
	private const float IK_POS_THRESH = 0.125f;
	private const int MAX_IK_TRIES = 20;
 
	public bool IsActive = true;
	public Transform Target;
	
	public bool IsDamping = false;
	public float DampingMax = 0.5f;

    private IKJoint[] joints;

    private int counts;
	
	void Start ()
	{
		if (Target == null)
			Target = transform;

        joints = GetComponentsInChildren<IKJoint>();
	}
 
	void LateUpdate ()
	{
		if(IsActive)
			Solve ();
	}
 
	void Solve ()
	{		
		Transform endEffector = joints[joints.Length - 1].transform;
		Vector3 rootPos, curEnd;
 
		Vector3 targetVector = Vector3.zero;
		Vector3 currentVector = Vector3.zero;
		Vector3 crossResult = Vector3.zero;
 
		float cosAngle,turnAngle;
 
		// START AT THE LAST LINK IN THE CHAIN
		int link = joints.Length - 1;
		int tries = 0;
		
		// SEE IF I AM ALREADY CLOSE ENOUGH
		do {
			if (link < 0)
				link = joints.Length - 1;

            Transform linkTransform = joints[link].transform;

			rootPos = linkTransform.position;
			curEnd = endEffector.position;
 
			// CREATE THE VECTOR TO THE CURRENT EFFECTOR POS
			currentVector = curEnd - rootPos;
			// CREATE THE DESIRED EFFECTOR POSITION VECTOR
			targetVector = Target.position - rootPos;
 
			// NORMALIZE THE VECTORS
			currentVector.Normalize ();
			targetVector.Normalize ();
 
			// THE DOT PRODUCT GIVES ME THE COSINE OF THE DESIRED ANGLE
			cosAngle = Vector3.Dot (currentVector, targetVector);
 
			// IF THE DOT PRODUCT RETURNS 1.0, I DON'T NEED TO ROTATE AS IT IS 0 DEGREES
			if (cosAngle < 0.99999f) {
				// USE THE CROSS PRODUCT TO CHECK WHICH WAY TO ROTATE
				crossResult = Vector3.Cross (currentVector, targetVector);
				crossResult.Normalize ();
 
				turnAngle = Mathf.Acos (cosAngle); 
				// APPLY DAMPING
				if (IsDamping) {
					if (turnAngle > DampingMax)
						turnAngle = DampingMax;
				}
				turnAngle = turnAngle * Mathf.Rad2Deg;
 
				linkTransform.rotation = Quaternion.AngleAxis (turnAngle, crossResult) * linkTransform.rotation;
 
                joints[link].updateJointAngles(linkTransform);
			}
			link--;

            Lebug.Log("IK Solving", counts, "IKSolver");
            counts++;
            
		} while (tries++ < MAX_IK_TRIES && (curEnd - Target.position).sqrMagnitude > IK_POS_THRESH);
	}
}