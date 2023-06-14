﻿using UnityEngine;
using System.Collections;
using SprCs;
using System;

namespace SprUnity {
    public static class SprUnityExtentions {
        // ----- ----- ----- ----- -----
        // Unity --> Spr
        public static Posed ToPosed(this Transform t) {
            Vector3 p = t.position;
            Quaternion q = t.rotation;
            double norm = Math.Sqrt(q.w * q.w + q.x * q.x + q.y * q.y + q.z * q.z);
            return new Posed(q.w / norm, q.x / norm, q.y / norm, q.z / norm, p.x, p.y, p.z);
        }

        public static Vec3f ToVec3f(this Vector3 v) {
            return new Vec3f(v.x, v.y, v.z);
        }

        public static Vec3d ToVec3d(this Vector3 v) {
            return new Vec3d(v.x, v.y, v.z);
        }

        public static Quaternionf ToQuaternionf(this Quaternion q) {
            return new Quaternionf(q.w, q.x, q.y, q.z);
        }

        public static Quaterniond ToQuaterniond(this Quaternion q) {
            double norm = Math.Sqrt(q.w * q.w + q.x * q.x + q.y * q.y + q.z * q.z);
            return new Quaterniond(q.w / norm, q.x / norm, q.y / norm, q.z / norm);
        }

        public static arraywrapper_double ToArrayWrapperDouble(this double[] d) {
            int l = d.Length;
            arraywrapper_double wd = new arraywrapper_double(nelm: l);
            for (int i = 0; i < l; i++) {
                wd[i] = d[i];
            }
            return wd;
        }

        // ----- ----- ----- ----- -----
        // Spr --> Unity

        public static void FromPosed(this Transform t, Posed pose) {
            t.position = pose.Pos().ToVector3();
            t.rotation = pose.Ori().ToQuaternion();
        }

        public static Vector3 ToVector3(this Vec3f v) {
            return new Vector3(v.x, v.y, v.z);
        }

        public static Vector3 ToVector3(this Vec3d v) {
            return new Vector3((float)v.x, (float)v.y, (float)v.z);
        }

        public static Quaternion ToQuaternion(this Quaternionf q) {
            return new Quaternion(q.x, q.y, q.z, q.w);
        }

        public static Quaternion ToQuaternion(this Quaterniond q) {
            return new Quaternion((float)q.x, (float)q.y, (float)q.z, (float)q.w);
        }

        // ----- ----- ----- ----- -----
        // ObjectIf --> SprBehaviour

        public static Type GetBehaviour<Type>(this ObjectIf springheadObject) where Type : SprBehaviour {
            return SprBehaviour.GetBehaviour<Type>(springheadObject);
        }
    }
}
