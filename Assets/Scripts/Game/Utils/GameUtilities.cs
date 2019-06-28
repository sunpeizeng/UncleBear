using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UncleBear
{
    public class GameUtilities
    {
        //Raycast
        public static RaycastHit GetRaycastHitInfo(Ray ray, int distance = 1000, int layerMask = -1)
        {
            RaycastHit hit = default(RaycastHit);
            if (layerMask > 0)
                Physics.Raycast(ray, out hit, distance, layerMask);
            else
                Physics.Raycast(ray, out hit, distance);

            return hit;
        }

        public static RaycastHit GetRaycastHitInfo(Vector3 pos, Vector3 dir, int distance = 1000, int layerMask = -1)
        {
            RaycastHit hit = default(RaycastHit);
            if (layerMask != -1)
                Physics.Raycast(pos, dir, out hit, distance, layerMask);
            else
                Physics.Raycast(pos, dir, out hit, distance);

            return hit;
        }

        public static RaycastHit[] GetRaycastAllHitInfo(Ray ray, int distance = 1000, int layerMask = -1)
        {
            return layerMask > 0 ? Physics.RaycastAll(ray, distance, layerMask) : Physics.RaycastAll(ray, distance);
        }

        public static RaycastHit[] GetRaycastAllHitInfo(Vector3 pos, Vector3 dir, int distance = 1000, int layerMask = -1)
        {
            return layerMask > 0 ? Physics.RaycastAll(pos, dir, distance, layerMask) : Physics.RaycastAll(pos, dir, distance);
        }

        //计算两个向量在相对于一个平面上的角度差
        public static float GetDeltaDegrees(Vector2 srcPos, Vector2 desPos, Vector2 referencePoint)
        {
            var a = Mathf.Atan2(srcPos.x - referencePoint.x, srcPos.y - referencePoint.y);
            var b = Mathf.Atan2(desPos.x - referencePoint.x, desPos.y - referencePoint.y);
            var d = Mathf.Repeat(a - b, Mathf.PI * 2.0f);

            if (d > Mathf.PI)
            {
                d -= Mathf.PI * 2.0f;
            }

            return d * Mathf.Rad2Deg;
        }

        //以一个物体为距离基准并修正高度后的世界坐标
        public static Vector3 GetFingerTargetWolrdPos(Lean.Touch.LeanFinger finger, Vector3 posSrc, float fixY)
        {
            var pos = finger.GetWorldPosition(Vector3.Distance(CameraManager.Instance.MainCamera.transform.position, posSrc), CameraManager.Instance.MainCamera);
            pos.y = fixY;
            return pos;
        }
        public static Vector3 GetFingerTargetWolrdPos(Lean.Touch.LeanFinger finger, GameObject objTarget, float fixY)
        {
            return GetFingerTargetWolrdPos(finger, objTarget.transform.position, fixY);
        }
        public static Vector3 GetFingerTargetWolrdPos(Lean.Touch.LeanFinger finger, GameObject objTarget)
        {
            var pos = finger.GetWorldPosition(Vector3.Distance(CameraManager.Instance.MainCamera.transform.position, objTarget.transform.position), CameraManager.Instance.MainCamera);
            return pos;
        }
        public static Vector3 GetFingerTargetWolrdPos(Lean.Touch.LeanFinger finger, Vector3 posSrc)
        {
            return finger.GetWorldPosition(Vector3.Distance(CameraManager.Instance.MainCamera.transform.position, posSrc), CameraManager.Instance.MainCamera);
        }
        public static Vector3 GetScreenRayWorldPos(Vector3 screenPos, float distance)
        {
            return CameraManager.Instance.MainCamera.ScreenPointToRay(screenPos).GetPoint(distance);
        }

        //判断刚体速度是否趋于静止
        public static bool IsAllRigidBodyQuiet(List<GameObject> objs, float speedLimit = 0.5f)
        {
            bool isQuiet = true;
            for (int i = 0; i < objs.Count; i++)
            {
                var body = objs[i].GetComponent<Rigidbody>();
                if (body != null)
                {
                    if (Mathf.Abs(body.velocity.x) > speedLimit || Mathf.Abs(body.velocity.y) > speedLimit || Mathf.Abs(body.velocity.z) > speedLimit)
                    {
                        isQuiet = false;
                        break;
                    }
                }
            }

            return isQuiet;
        }
        //获取一堆物体中最高的那个点
        public static float GetMeshMaxHeight(List<GameObject> objs)
        {
            float maxHeight = 0;
            if (objs.Count > 0)
            {
                for (int i = 0; i < objs.Count; i++)
                {
                    var max = objs[i].GetComponent<Renderer>().bounds.max.y;
                    if (max > maxHeight)
                        maxHeight = max;
                }
            }
            return maxHeight;
        }
        //获取网格的中心
        public static Vector3 GetMeshCenter(List<GameObject> objRoots)
        {
            int count = 0;
            Vector3 boundsCenter = Vector3.zero;
            for (int i = 0; i < objRoots.Count; i++)
            {
                var meshes = objRoots[i].GetComponentsInChildren<Renderer>();
                for (int j = 0; j < meshes.Length; j++)
                {
                    boundsCenter += meshes[j].bounds.center;
                    count += 1;
                }

            }
            boundsCenter /= count * 1.0f;

            if (count > 0)
                return boundsCenter;
            else
                return Vector3.zero;
        }

        //获取网格的高度
        public static float GetMeshHeight(GameObject objRoot)
        {
            float maxExtend = 0;
            var meshes = objRoot.GetComponentsInChildren<Renderer>();
            for (int j = 0; j < meshes.Length; j++)
            {
                maxExtend = Mathf.Max(maxExtend, meshes[j].bounds.extents.y);
            }
            return maxExtend;
        }

        //计算一个点和一个线段的位置关系
        public static int JudgePointToLineXZ(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
        {
            float result = (lineEnd.x - lineStart.x) * (point.z - lineStart.z) - (lineEnd.z - lineStart.z) * (point.x - lineStart.x);
            if (result > 0)
                return 1;//在逆时针,左边
            if (result < 0)
                return -1;//在顺时针,右边
            return 0;//在线上
        }

        public static T LoadAndInstantiateT<T>(string name) where T : UnityEngine.Object
        {
            var res = Resources.Load<T>(name) as T;
            if (res == null)
            {
                return default(T);
            }
            var inited = GameObject.Instantiate(res) as T;
            if (inited is GameObject)
                (inited as GameObject).transform.position = Vector3.one * 500;
            return inited;
        }
        public static T InstantiateT<T>(GameObject obj) where T : UnityEngine.Object
        {
            var inited = GameObject.Instantiate(obj) as T;
            if (inited is GameObject)
                (inited as GameObject).transform.position = Vector3.one * 500;
            return inited;
        }

        public static void ResetMeshPivotToCenter(GameObject obj)
        {
            MeshFilter[] rens = obj.GetComponentsInChildren<MeshFilter>();
            for (int i = 0; i < rens.Length; i++)
            {
                var centerPos = rens[i].mesh.bounds.center;
                var deltaPos = rens[i].mesh.bounds.center - rens[i].transform.localPosition;
                var rensVertices = rens[i].mesh.vertices;
                //仅横向
                for (int j = 0; j < rensVertices.Length; j++)
                {
                    rensVertices[j] += Vector3.left * deltaPos.x;
                }
                rens[i].mesh.vertices = rensVertices;
                rens[i].mesh.RecalculateBounds();
                rens[i].transform.localPosition = new Vector3(centerPos.x, rens[i].transform.localPosition.y, rens[i].transform.localPosition.z);
                var col = rens[i].GetComponent<BoxCollider>();
                if (col != null)
                {
                    GameObject.DestroyImmediate(col);
                    rens[i].gameObject.AddComponent<BoxCollider>();
                }
            }
        }

        public static void SetCamFOVByScreen(Camera cam)
        {
            if (cam.orthographic)
            {
                float designedSize = 34f;
                UnityEngine.UI.CanvasScaler canvasScaler2dUI = DoozyUI.UIManager.GetUiContainer.GetComponent<UnityEngine.UI.CanvasScaler>();
                float desginedAspect = canvasScaler2dUI.referenceResolution.x / canvasScaler2dUI.referenceResolution.y;
                float actualAspect = (float)cam.pixelWidth / cam.pixelHeight;

                cam.orthographicSize = (designedSize / actualAspect) * desginedAspect;
            }
            else
            {
                float designedVFOV = 60f;
                UnityEngine.UI.CanvasScaler canvasScaler2dUI = DoozyUI.UIManager.GetUiContainer.GetComponent<UnityEngine.UI.CanvasScaler>();
                float desginedAspect = canvasScaler2dUI.referenceResolution.x / canvasScaler2dUI.referenceResolution.y;
                float actualAspect = (float)cam.pixelWidth / cam.pixelHeight;

                float newVFOVInRads = 2 * Mathf.Atan(desginedAspect / actualAspect * Mathf.Tan(designedVFOV * Mathf.Deg2Rad * 0.5f));
                cam.fieldOfView = newVFOVInRads * Mathf.Rad2Deg;
            }
        }

        public static void ShowFinishEff()
        {
            var finishEff = EffectCenter.Instance.SpawnEffect("Stars", Vector3.zero, Vector3.zero);
            finishEff.transform.SetParent(CameraManager.Instance.MainCamera.transform);
            finishEff.transform.localPosition = new Vector3(0, 0, 30);
            finishEff.transform.localEulerAngles = new Vector3(0, 180, 0);
        }

		public static string GetParam(string key, string defaultVal)
		{
			#if UNITY_EDITOR
			return defaultVal;
			#endif

			if (SDKBridge.GetChannel () == "App Store") 
			{
				return SDKBridge.GetPTParam (key, defaultVal);
			}

			string ret = SDKBridge.GetPTParam (key, defaultVal);
			if (ret == defaultVal)
				return defaultVal;

			Dictionary<string, object> _params = (Dictionary<string, object>)MiniJSONV.Json.Deserialize (ret);
			object o = null;
			_params.TryGetValue(SDKBridge.GetChannel(), out o);
			if (o == null)
				o = _params ["all"];
			
			return (string)o;
		}

        public static void LimitListPosition(List<GameObject> objList, Vector3 centerPos, float radius)
        {
            if (objList != null && objList.Count > 0)
            {
                objList.ForEach(p =>
                {
                    if (p != null)
                    {
                        var disVec = p.transform.position - centerPos;
                        disVec.y = 0;
                        if (disVec.sqrMagnitude > radius * radius)
                        {
                            p.transform.position = new Vector3(centerPos.x, p.transform.position.y, centerPos.z) + disVec.normalized * radius;
                        }
                    }
                });
            }
        }
        public static void LimitListPosition(List<Transform> trsList, Vector3 centerPos, float radius)
        {
            if (trsList != null && trsList.Count > 0)
            {
                trsList.ForEach(p =>
                {
                    var disVec = p.position - centerPos;
                    disVec.y = 0;
                    if (disVec.sqrMagnitude > radius * radius)
                    {
                        p.position = new Vector3(centerPos.x, p.position.y, centerPos.z) + disVec.normalized * radius;
                    }
                });
            }
        }
    }
}
