// using System;
// using System.Collections.Generic;
// using System.Linq;
// using DG.Tweening;
// using UnityEngine;
//
// namespace Core.Systems
// {
//     public class IOScrollingObject : IObject
//     {
//         protected float autoScrollThreshold = 0.4f;
//         public static Action<bool> minMaxZoomCamera;
//         public static Action<float> ZoomCamera;
//         public  Action<float> ZoomCameraAction;
//         public bool lockX, lockY = false, multiLevelScroll = true;
//         protected float multiLevelScrolThreshold = 1f;
//         protected int pointerId = EMPTY, autoScrollPointerId = EMPTY;
//         int multiLevelIndex;
//
//         float[] multiLevels;
//         float autoMultiLevelScrollingTime = 1, autoMultiLevelScrollingTimeCount;
//         protected Vector3 dragStartTouch, lastTouch, currentTouch, draggedDistance, targetPosition;
//         protected Vector2 DPdragStartTouch, DPstartTouchPos, DPFirstTouchPos;
//         private bool isDoubleTap;
//         private int SecondPointerid = EMPTY, FirstPointerid = EMPTY;
//         private float lastSize = 16.0f, Starttimer, initialOrthographicSize;
//         private float lastTouchTime, dragVelocityX, dragVelocityY;
//         public Vector3 camPosition;
//         float leftLimit, rightLimit, leftLimitSoft, rightLimitSoft;
//         float topLimit, bottomLimit, topLimitSoft, bottomLimitSoft;
//         const float zoomSpeed = 0.35f;
//         const float scrollSpeed = 0.75f;
//         private Vector3 initPosition;
//
//         public float RightLimit
//         {
//             get => rightLimit;
//         }
//
//         public float LeftLimit
//         {
//             get => leftLimit;
//         }
//
//         int startFrameCount;
//         Vector2 startViewport;
//
//         struct ParallaxObject
//         {
//             public Vector3 position;
//             public Transform transform;
//         }
//
//         private List<ParallaxObject> parallaxObjects;
//         readonly float D = 100;
//
//         Coroutine panCameraCoroutine;
//
//         private static Dictionary<string, float> _camPositions = new Dictionary<string, float>();
//         private List<float> dragVelocitiesX = new List<float>();
//         private List<float> dragVelocitiesY = new List<float>();
//         private float friction = 0.05f;
//
//         
//         public override void Init()
//         {
//             targetPosition = iSystem.CameraPosition;
//
//
//             camPosition = iSystem.CameraPosition;
//             initPosition = iSystem.CameraPosition;
//             camPosition = initPosition;
//             //camPosition.y = targetPosition.y = multiLevels[multiLevelIndex];
//             targetPosition = camPosition;
//             if (iSystem.IsZoomInOn)
//             {
//                 ScrollOpen(true);
//             }
//             MainCollider.offset = iSystem.GetComponent<BoxCollider2D>().offset;
//             MainCollider.size = iSystem.GetComponent<BoxCollider2D>().size;
//
//             float screenWith = Mathf.Min(iSystem.cameraSize.x, MainCollider.size.x);
//             float screenheight = Mathf.Min(iSystem.cameraSize.y, MainCollider.size.y);
//
//             leftLimit = leftLimitSoft = Left + (screenWith * 0.5f);
//             rightLimit = rightLimitSoft = Right - (screenWith * 0.5f); ;
//
//             // Array is already sorted in ascending order
//             bottomLimit = bottomLimitSoft = Bottom + (screenheight * 0.5f);
//             topLimit = topLimitSoft = Top - (screenheight * 0.5f);
//             GameObject[] gos = GameObject.FindGameObjectsWithTag("Parallax");
//             parallaxObjects = new List<ParallaxObject>();
//             for (int i = 0; i < gos.Length; i++)
//             {
//                 parallaxObjects.Add(new ParallaxObject() { position = gos[i].transform.position, transform = gos[i].transform });
//             }
//         }
//
//         public override void OnRegistered() { }
//
//         public override void OnUnRegistered() { }
//
//         public override void IUpdate()
//         {
//             if (pointerId == EMPTY)
//             {
//                 AutoScrollUpdate();
//                 targetPosition.x += dragVelocityX * Time.deltaTime;
//                 targetPosition.y += dragVelocityY * Time.deltaTime;
//                 dragVelocityX = Mathf.Lerp(dragVelocityX, 0, friction);
//                 dragVelocityY = Mathf.Lerp(dragVelocityY, 0, friction);
//                 targetPosition.x = Mathf.Clamp(targetPosition.x, leftLimitSoft, rightLimitSoft);
//                 targetPosition.y = Mathf.Clamp(targetPosition.y, bottomLimitSoft, topLimitSoft);
//             }
//
// #if UNITY_EDITOR
//             float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
//             if (scrollWheel != 0)
//             {
//                 float newSize = iSystem.Camera.orthographicSize - scrollWheel * 2f;
//                 ZoomUpdate(newSize);
//             }
// #endif
//             
//             if (!lockX)
//             {
//                 camPosition.x = targetPosition.x;
//                 camPosition.x = Mathf.Clamp(camPosition.x, leftLimit, rightLimit);
//             }
//
//             if (!lockY)
//             {
//                 camPosition.y = targetPosition.y;
//                 camPosition.y = Mathf.Clamp(targetPosition.y, bottomLimit, topLimit);
//             }
//         }
//
//         private void ZoomUpdate(float newSize, float duration = 0.1f)
//         {
//             newSize = Mathf.Clamp(newSize, Constant.REF_CAMERA_SIZE, iSystem.Max_Zoom);
//
//             float screenWith = Mathf.Min(iSystem.GetCameraSize().x, MainCollider.size.x);
//             float screenheight = Mathf.Min(iSystem.GetCameraSize().y, MainCollider.size.y);
//             leftLimit = leftLimitSoft = Left + (screenWith * 0.5f);
//             rightLimit = rightLimitSoft = Right - (screenWith * 0.5f);
//
//             bottomLimit = bottomLimitSoft = Bottom + (screenheight * 0.5f);
//             topLimit = topLimitSoft = Top - (screenheight * 0.5f);
//             if (newSize < iSystem.Max_Zoom && lastSize == iSystem.Max_Zoom)
//             {
//                 minMaxZoomCamera?.Invoke(true);
//             }
//             if (newSize == iSystem.Max_Zoom)
//             {
//                 minMaxZoomCamera?.Invoke(false);
//             }
//             ZoomCamera?.Invoke(newSize);
//             lastSize = newSize;
//             iSystem.Camera.DOOrthoSize(newSize, duration);// = newSize;
//         }
//
//         protected void AutoScrollUpdate()
//         {
//             if (autoScrollPointerId == EMPTY) return;
//             if (iSystem == null || iSystem.iInputSystem == null) return;
//
//             IObject listener = iSystem.iInputSystem.GetPointerEventListener(autoScrollPointerId);
//             if (!listener || listener.IsStatic) return;
//
//             Vector2 autoScrollViewport = iSystem.iInputSystem.GetPointerEventData(autoScrollPointerId).position;
//             autoScrollViewport = iSystem.Camera.ScreenToViewportPoint(autoScrollViewport);
//             autoScrollViewport.x -= 0.5f;
//             autoScrollViewport.y -= 0.5f;
//
//             if (AutoScroll(autoScrollViewport, autoScrollPointerId) && listener.id != ID.CHARACTER_PALETTE)
//             {
//                 // Scene is scrolled
//                 // trigger Pointer Drag event on listener
//
//                 Vector2 worldPoint = autoScrollViewport;
//                 worldPoint.x += 0.5f;
//                 worldPoint.y += 0.5f;
//
//                 worldPoint = iSystem.Camera.ViewportToWorldPoint(worldPoint);
//                 if (listener) listener.OnPointerDrag(worldPoint, autoScrollPointerId).AutoScrolled();
//             }
//             else
//             {
//                 autoScrollPointerId = EMPTY;
//                 autoMultiLevelScrollingTimeCount = 0;
//             }
//         }
//
//         public override IObject OnPointerDown(Vector2 worldPoint, int pointerId)
//         {
//
//             if (!isDoubleTap)
//             {
//                 if (FirstPointerid == EMPTY)
//                 {
//                     FirstPointerid = pointerId;
//                     DPdragStartTouch = worldPoint;
//                     Starttimer = Time.time;
//                     DPstartTouchPos = worldPoint;
//                 }
//                 else
//                 {
//                     if (Time.time - Starttimer <= 1f)
//                     {
//                         if (FirstPointerid != pointerId)
//                         {
//                             isDoubleTap = true;
//                             SecondPointerid = pointerId;
//                             initialOrthographicSize = Camera.main.orthographicSize;
//                         }
//                     }
//                 }
//             }
//
//             if (this.pointerId == EMPTY)
//             {
//                 this.pointerId = pointerId;
//                 multiLevelScroll = true;
//                 dragStartTouch = worldPoint;
//                 lastTouch = worldPoint;
//                 draggedDistance = Vector2.zero;
//                 targetPosition = iSystem.Camera.transform.localPosition;
//                 lastTouchTime = Time.time;
//                 dragVelocitiesX.Clear();
//                 dragVelocitiesY.Clear();
//                 startViewport = iSystem.Camera.WorldToViewportPoint(worldPoint);
//                 startFrameCount = Time.frameCount;
//             }
//
//             return this;
//         }
//
//         public override IObject OnPointerDrag(Vector2 worldPoint, int pointerId)
//         {
//
//             if (isDoubleTap)
//             {
//
//                 if (FirstPointerid == pointerId)
//                 {
//                     DPFirstTouchPos = worldPoint;
//                 }
//                 if (SecondPointerid == pointerId)
//                 {
//                     float newSize;
//                     Vector2 SecondPos = worldPoint;
//                     float touchDelta = Vector2.Distance(DPFirstTouchPos, SecondPos) - Vector2.Distance(DPstartTouchPos, SecondPos);
//                     newSize = iSystem.Camera.orthographicSize - (touchDelta * zoomSpeed);
//                     ZoomUpdate(newSize);
//                 }
//
//                 return this;
//             }
//
//             if (this.pointerId == pointerId)
//             {
//                 currentTouch = worldPoint;
//                 draggedDistance = lastTouch - currentTouch;
//                 AddDragDistance(draggedDistance.x, draggedDistance.y, Time.time - lastTouchTime);
//
//                 multiLevelScroll = multiLevelScroll || Mathf.Abs(worldPoint.y - dragStartTouch.y) > multiLevelScrolThreshold;
//                 draggedDistance.y = !lockY && multiLevelScroll ? draggedDistance.y : 0;
//
//                 draggedDistance.z = 0;
//                 targetPosition += draggedDistance * scrollSpeed;
//                 lastTouch = (Vector3)worldPoint + draggedDistance;
//                 lastTouchTime = Time.time;
//             }
//             return this;
//         }
//
//         public override IObject OnPointerUp(Vector2 worldPoint, int pointerId)
//         {
//             if (pointerId == FirstPointerid || pointerId == SecondPointerid)
//             {
//                 isDoubleTap = false;
//                 FirstPointerid = EMPTY;
//                 SecondPointerid = EMPTY;
//                 DPstartTouchPos = Vector2.zero;
//                 Starttimer = 0;
//             }
//             if (this.pointerId == pointerId)
//             {
//                 offset = Vector2.zero;
//                 lastTouch = Vector3.zero;
//                 this.pointerId = EMPTY;
//
//                 dragVelocityX = Time.time - lastTouchTime < 0.1f && dragVelocitiesX.Count > 0 ? dragVelocitiesX.Average() : 0;
//                 dragVelocityY = Time.time - lastTouchTime < 0.1f && dragVelocitiesY.Count > 0 ? dragVelocitiesY.Average() : 0;
//                 OnReleased();
//             }
//
//             return this;
//         }
//
//         void AddDragDistance(float distanceX, float distanceY, float dt)
//         {
//             dragVelocitiesX.Add(distanceX / dt);
//             if (dragVelocitiesX.Count > 5) dragVelocitiesX.RemoveAt(0);
//
//             dragVelocitiesY.Add(distanceY / dt);
//             if (dragVelocitiesY.Count > 5) dragVelocitiesY.RemoveAt(0);
//         }
//
//         public virtual bool AutoScroll(Vector2 viewportNormalized, int pointerId)
//         {
//             bool horizontalScroll = Mathf.Abs(viewportNormalized.x) >= autoScrollThreshold;
//             bool verticalScroll = Mathf.Abs(viewportNormalized.y) >= autoScrollThreshold;
//
//             if (horizontalScroll)
//             {
//                 targetPosition.x += 0.15f * Mathf.Sign(viewportNormalized.x);
//             }
//
//             if (verticalScroll)
//             {
//                 targetPosition.y += 0.15f * Mathf.Sign(viewportNormalized.y);
//             }
//             else
//             {
//                 autoMultiLevelScrollingTimeCount = 0;
//             }
//
//             if (horizontalScroll || verticalScroll)
//             {
//                 autoScrollPointerId = pointerId;
//                 return true;
//             }
//
//             return false;
//         }
//     }
// }