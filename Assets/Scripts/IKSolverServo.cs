using UnityEngine;
using System.Collections;

public class IKSolverServo : MonoBehaviour
{
    public AnimationCurve thisCurve;

	private const float IK_POS_THRESH = 0.125f;
	private const int MAX_IK_TRIES = 20;

	[System.Serializable]
	public class JointEntity
	{
        public bool enabled = true;
		
        public Transform Joint;
        public int servoID;

        public JointActiveAngle activeJointAngle;
		public float angleMin;
        public float angleMax;
        public float servoMin;
        public float servoMax;
        public bool invertedAngles;
		
		internal Quaternion _initialRotation;
        internal int servoAngle;
	}
 
	/// <summary>
	/// Rotation AngleRestriction values in degrees (from -180 to 180).
	/// </summary>
    public enum JointActiveAngle { X, Y, Z };
 
	public bool IsActive = true;
    public bool DisplayDebug = true;
	public Transform Target;
	public JointEntity[] JointEntities;
	
	public bool IsDamping = false;
	public float DampingMax = 0.5f;
	
	void Start ()
	{   
		if (Target == null)
			Target = transform;

		foreach(JointEntity jointEntity in JointEntities) {
			jointEntity._initialRotation = jointEntity.Joint.localRotation;
		}
	}
 
	void LateUpdate ()
	{
		if(IsActive)
			Solve ();

        if(DisplayDebug)
			ShowInfo ();
	}

    //////////////
    // HELPERS
    //////////////

    public static float WrapAngle(float angle)
    {
        angle%=360;
        if(angle >180)
            return angle - 360;

        return angle;
    }

    public static float UnwrapAngle(float angle)
    {
        if(angle >=0)
            return angle;

        angle = -angle%360;

        return 360-angle;
    }

    public static float map(float x, float in_min, float in_max, float out_min, float out_max)
	{
		return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
	}

    private static Vector3 normalizeEuler(Vector3 euler) {
        if (euler.x > 180f) euler.x -= 360f;
        if (euler.y > 180f) euler.y -= 360f;
        if (euler.z > 180f) euler.z -= 360f;

        return euler;
    }

    public static float getActiveAngle(IKSolverServo.JointEntity jointEntity)
    {
        Vector3 euler = jointEntity.Joint.localEulerAngles;
        JointActiveAngle jointActiveAngle = jointEntity.activeJointAngle;

        if (jointActiveAngle == JointActiveAngle.X) {
			return euler.x;
        }
        else if (jointActiveAngle == JointActiveAngle.Y) {
			return euler.y;
        }
        
        return euler.z;
    }

    // Convert the current active angle from a joint to a servo angle
    private static int AngleJoint2Servo(IKSolverServo.JointEntity joint)
    {
        float jointAngle = WrapAngle(getActiveAngle(joint));

        float in_min = joint.angleMin;
        float in_max = joint.angleMax;

        float out_min = joint.servoMin;
        float out_max = joint.servoMax;

        // Account for inverted axis
        if (joint.invertedAngles) {
            out_min *= -1;
            out_max *= -1;
        }

        return Mathf.RoundToInt(map(jointAngle, in_min, in_max, out_min, out_max));
    }

    //////////////
    // END HELPERS
    //////////////
 
	void Solve ()
	{		
		Transform endEffector = JointEntities[JointEntities.Length - 1].Joint;
		Vector3 rootPos, curEnd;
 
		Vector3 targetVector = Vector3.zero;
		Vector3 currentVector = Vector3.zero;
		Vector3 crossResult = Vector3.zero;
 
		float cosAngle,turnAngle;
 
		// START AT THE LAST LINK IN THE CHAIN
		int link = JointEntities.Length - 1;
		int tries = 0;
		
		// SEE IF I AM ALREADY CLOSE ENOUGH
		do {
			if (link < 0)
				link = JointEntities.Length - 1;

			rootPos = JointEntities[link].Joint.position;
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
 
				JointEntities[link].Joint.rotation = Quaternion.AngleAxis (turnAngle, crossResult) * JointEntities[link].Joint.rotation;
                JointEntities[link].servoAngle = AngleJoint2Servo(JointEntities[link]);
 
				CheckAngleRestrictions (JointEntities[link]);
			}
			link--;
		} while (tries++ < MAX_IK_TRIES && (curEnd - Target.position).sqrMagnitude > IK_POS_THRESH);
	}
 
	/// <summary>
	/// Checks the angle restrictions.
	/// </summary>
	/// <param name="jointEntity">Joint entity</param>
	void CheckAngleRestrictions (JointEntity jointEntity)
	{
		Vector3 euler = normalizeEuler(jointEntity.Joint.localRotation.eulerAngles);
        Vector3 newEuler = Vector3.zero;

        JointActiveAngle jointActiveAngle = jointEntity.activeJointAngle;

        if (jointEntity.enabled) {
            if (jointActiveAngle == JointActiveAngle.X) {
                newEuler.x = Mathf.Clamp (euler.x, jointEntity.angleMin, jointEntity.angleMax);
            }

            if (jointActiveAngle == JointActiveAngle.Y) {
                newEuler.y = Mathf.Clamp (euler.y, jointEntity.angleMin, jointEntity.angleMax);
            }

            if (jointActiveAngle == JointActiveAngle.Z) {
                newEuler.z = Mathf.Clamp (euler.z, jointEntity.angleMin, jointEntity.angleMax);
            }
        }
		
		jointEntity.Joint.localEulerAngles = newEuler;
	}

    void ShowInfo () {
        for(int i=0;i<JointEntities.Length-1;i++){
            JointEntity joint = JointEntities[i];

            int jointAngle = Mathf.RoundToInt(WrapAngle(getActiveAngle(joint)));

            Lebug.Log(joint.Joint.name, jointAngle + ":" + joint.servoAngle, "IKServo");
		}
    }
}