using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Helpers {
    public class Math : MonoBehaviour {
        
        public static Vector3 normalizeVector(Vector3 v) {
            if (v.x > 180f) v.x -= 360f;
            if (v.y > 180f) v.y -= 360f;
            if (v.z > 180f) v.z -= 360f;

            return v;
        }

        public static float normalizeAngle(float angle) {
            if (angle > 180f) angle -= 360f;

            return angle;
        }

        public static Vector3 normalizeEuler(Vector3 euler) {
            if (euler.x > 180f) euler.x -= 360f;
            if (euler.y > 180f) euler.y -= 360f;
            if (euler.z > 180f) euler.z -= 360f;

            return euler;
        }

        public static bool V3Equal(Vector3 a, Vector3 b, float precision = 0.0001f){
            return Vector3.SqrMagnitude(a - b) < precision;
        }

        public static float map(float x, float in_min, float in_max, float out_min, float out_max)
        {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }

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

        public static Vector3 roundVectorToInt(Vector3 v) {
            return new Vector3(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
        }
    }
}