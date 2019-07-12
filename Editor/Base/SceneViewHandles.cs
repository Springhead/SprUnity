using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SprUnity {
    public static class SceneViewHandles {

        /// xyz軸のDiscハンドルを生成する
        public static Quaternion AxisRotate(Quaternion rotation, Vector3 position, float size) {
            var rotationMatrix = Matrix4x4.TRS(Vector3.zero, rotation.normalized, Vector3.one);
            if (Event.current.type == EventType.Repaint) {
                Transform sceneCamT = SceneView.lastActiveSceneView.camera.transform;
                Handles.color = new Color(1, 1, 1, 0.5f);
                Handles.CircleHandleCap(
                    10,
                    position,
                    //Quaternion.LookRotation(sceneCamT.position,position),
                    sceneCamT.rotation,
                    size,
                    EventType.Repaint
                );
            }

            Handles.color = Handles.xAxisColor;
            rotation = Handles.Disc(rotation, position, rotationMatrix.MultiplyPoint(Vector3.right), size, true, size);
            Handles.color = Handles.yAxisColor;
            rotation = Handles.Disc(rotation, position, rotationMatrix.MultiplyPoint(Vector3.up), size, true, size);
            Handles.color = Handles.zAxisColor;
            rotation = Handles.Disc(rotation, position, rotationMatrix.MultiplyPoint(Vector3.forward), size, true, size);
            Handles.color = new Color(1, 1, 1, 0.5f);
            rotation = Handles.FreeRotateHandle(rotation, position, size * 1.1f);
            return rotation;
        }
        /// xyz軸のFreeMoveハンドルを生成する
        public static Vector3 AxisMove(Vector3 position, Quaternion rotation, float sizeS) {

            var rotationMatrix = Matrix4x4.TRS(Vector3.zero, rotation.normalized, Vector3.one);
            var dirX = rotationMatrix.MultiplyPoint(Vector3.right);
            var dirY = rotationMatrix.MultiplyPoint(Vector3.up);
            var dirZ = rotationMatrix.MultiplyPoint(Vector3.forward);
            var snap = Vector3.one;
            snap.x = EditorPrefs.GetFloat("MoveSnapX", 1.0f);
            snap.y = EditorPrefs.GetFloat("MoveSnapY", 1.0f);
            snap.z = EditorPrefs.GetFloat("MoveSnapZ", 1.0f);

            // FreeMove
            var handleCapPosOffset = Vector3.zero;
            var handleCapEuler = rotation.eulerAngles;

            var handleSize = sizeS * 0.13f;

            Handles.CapFunction RectangleHandleCap2D = (id, pos, rot, size, eventType) => {
                Handles.RectangleHandleCap(id, pos + rotationMatrix.MultiplyPoint(handleCapPosOffset), rotation * Quaternion.Euler(handleCapEuler), size, eventType);
            };
            Handles.color = Handles.zAxisColor;
            handleCapPosOffset = new Vector3(1.0f, 1.0f, 0.0f) * handleSize;
            handleCapEuler = Vector3.zero;
            var movePoint = Handles.FreeMoveHandle(position, rotation, handleSize, snap, RectangleHandleCap2D);
            // XY平面上の近傍点を新しい位置とする
            if (SceneView.lastActiveSceneView.camera.orthographic) {
                position = intersectPoint(dirZ, position,
                    SceneView.lastActiveSceneView.camera.transform.forward, movePoint);
            } else {
                position = intersectPoint(dirZ, position, movePoint -
                    SceneView.lastActiveSceneView.camera.transform.position, movePoint);
            }

            Handles.color = Handles.yAxisColor;
            handleCapPosOffset = new Vector3(1.0f, 0.0f, 1.0f) * handleSize;
            handleCapEuler = new Vector3(90.0f, 0.0f, 0.0f);
            movePoint = Handles.FreeMoveHandle(position, rotation, handleSize, snap, RectangleHandleCap2D);
            // XZ平面上の近傍点を新しい位置とする
            if (SceneView.lastActiveSceneView.camera.orthographic) {
                position = intersectPoint(dirY, position,
                    SceneView.lastActiveSceneView.camera.transform.forward, movePoint);
            } else {
                position = intersectPoint(dirY, position, movePoint -
                    SceneView.lastActiveSceneView.camera.transform.position, movePoint);
            }

            Handles.color = Handles.xAxisColor;
            handleCapPosOffset = new Vector3(0.0f, 1.0f, 1.0f) * handleSize;
            handleCapEuler = new Vector3(0.0f, 90.0f, 0.0f);
            movePoint = Handles.FreeMoveHandle(position, rotation, handleSize, snap, RectangleHandleCap2D);
            // YZ平面上の近傍点を新しい位置とする
            if (SceneView.lastActiveSceneView.camera.orthographic) {
                position = intersectPoint(dirX, position,
                    SceneView.lastActiveSceneView.camera.transform.forward, movePoint);
            } else {
                position = intersectPoint(dirX, position, movePoint -
                    SceneView.lastActiveSceneView.camera.transform.position, movePoint);
            }

            Handles.color = Handles.xAxisColor;
            position = Handles.Slider(position, rotationMatrix.MultiplyPoint(Vector3.right), sizeS, Handles.ArrowHandleCap, sizeS); //X 軸
            Handles.color = Handles.yAxisColor;
            position = Handles.Slider(position, rotationMatrix.MultiplyPoint(Vector3.up), sizeS, Handles.ArrowHandleCap, sizeS); //Y 軸
            Handles.color = Handles.zAxisColor;
            position = Handles.Slider(position, rotationMatrix.MultiplyPoint(Vector3.forward), sizeS, Handles.ArrowHandleCap, sizeS); //Z 軸
                                                                                                                                      // Slider
            return position;
        }

        /* 線と平面の交点を求める
        *https://qiita.com/edo_m18/items/c8808f318f5abfa8af1e
        */
        public static Vector3 intersectPoint(Vector3 n, Vector3 x, Vector3 m, Vector3 x0) {
            var h = Vector3.Dot(n, x);
            return x0 + ((h - Vector3.Dot(n, x0)) / (Vector3.Dot(n, m))) * m;
        }
    }
}