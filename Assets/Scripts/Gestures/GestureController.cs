using PDollarGestureRecognizer;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using Leap;
using Leap.Unity;
using UnityEngine.UI;

public class GestureController : MonoBehaviour
{
    private LeapProvider provider;

    private Gesture[] trainingSet;
    private List<Point> points = new List<Point>();

    private WaitForSeconds checkDelay = new WaitForSeconds(0.1f);
    private WaitForSeconds gestureSuccessDelay = new WaitForSeconds(0.2f);

    void Start()
    {
        provider = FindObjectOfType<LeapProvider>() as LeapProvider;
        LoadGestures();
        InitializePoints();

        StartCoroutine(CheckForGestureLoop());

        initialCameraTransform = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }

    private void InitializePoints()
    {
        for (int i = 0; i < 100; i++)
        {
            //points.Add(new Point(0, 0, 0));
        }
    }

    private IEnumerator CheckForGestureLoop()
    {
        while (true)
        {
            var gestureType = GetGestureType(GetCurrentGesture());
            if (gestureType != "")
            {
                var playerController = GetPlayerController();
                if (playerController != null)
                {
                    if (gestureType == "circle")
                    {
                        playerController.FireBolt();
                    }
                    else if (gestureType == "five point star")
                    {
                        playerController.FireBlast();
                    }
                }

                yield return gestureSuccessDelay;

                points.Clear();
            }

            yield return checkDelay;
        }
    }

    private PlayerController GetPlayerController()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player)
        {
            return player.GetComponent<PlayerController>();
        }
        return null;
    }

    private string GetGestureType(Result? gestureResult)
    {
        if (gestureResult.HasValue)
        {
            var result = gestureResult.Value;

            if (result.Score > 0.8f)
            {
                Debug.Log(result.GestureClass + " " + result.Score);
                return result.GestureClass;
            }
        }
        return "";
    }

    private Transform initialCameraTransform;

    private Result? GetCurrentGesture()
    {
        for (var i = 1; i < points.Count; i++)
        {
            float lineZ = initialCameraTransform.position.z + initialCameraTransform.forward.z * 2.0f;

            Vector3 startPos = new Vector3(points[i - 1].X, points[i - 1].Y, lineZ);
            Vector3 endPos = new Vector3(points[i].X, points[i].Y, lineZ);
            Debug.DrawLine(startPos, endPos, Color.green, 0.2f);
        }

        if (points.Count < 20) return null;

        // Check for minimum gesture size

        if (!IsGestureLargeEnough(points)) return null;

        Gesture candidate = new Gesture(points.ToArray());

        /* foreach (var point in candidate.Points)
        {
            Debug.Log(point.X + " " + point.Y + " " + point.StrokeID);
        } */
        return PointCloudRecognizer.Classify(candidate, trainingSet);
    }

    private bool IsGestureLargeEnough(List<Point> points)
    {
        float maxX = float.NegativeInfinity, maxY = float.NegativeInfinity;
        float minX = float.PositiveInfinity, minY = float.PositiveInfinity;
        foreach (var point in points)
        {
            maxX = Math.Max(maxX, point.X);
            minX = Math.Min(minX, point.X);
            maxY = Math.Max(maxY, point.Y);
            minY = Math.Min(minY, point.Y);
        }

        const float minimumSize = 0.1f;
        return maxX - minX > minimumSize && maxY - minY > minimumSize;
    }

    private void LoadGestures()
    {
        var path = Path.Combine(Application.dataPath, "Scripts/Gestures/GestureSet");
        var files = Directory.GetFiles(path, "*.xml");

        var trainingSetList = new List<Gesture>();

        foreach (var filePath in files)
        {
            var fileContent = File.ReadAllText(filePath);
            trainingSetList.Add(GestureIO.ReadGestureFromXML(fileContent));
        }

        trainingSet = trainingSetList.ToArray();

        /* var trainingSetList = new List<Gesture>();

        TextAsset[] gesturesXml = Resources.LoadAll<TextAsset>("GestureSet/10-stylus-MEDIUM/");
        foreach (TextAsset gestureXml in gesturesXml)
        {
            trainingSetList.Add(GestureIO.ReadGestureFromXML(gestureXml.text));
        }
        trainingSet = trainingSetList.ToArray(); */
    }

    void Update()
    {
        UpdateFireTriggerer();
        UpdatePlayerPosition();
    }

    private void UpdatePlayerPosition()
    {
        var playerController = GetPlayerController();
        if (playerController == null)
        {
            return;
        }

        foreach (Hand hand in provider.CurrentFrame.Hands)
        {
            if (hand.IsLeft)
            {
                foreach (var finger in hand.Fingers)
                {
                    if (finger.Type == Finger.FingerType.TYPE_INDEX)
                    {
                        const float velocityFactor = 5.0f;
                        Vector3 pos = new Vector3(finger.TipVelocity.x * velocityFactor, 0, finger.TipVelocity.z * velocityFactor);
                        playerController.SetDesiredVelocity(pos);
                        return;
                    }
                }
            }
        }

        playerController.UnsetDesiredVelocity();
    }

    private float slowMoveStartTime = 0;
    private bool isRecording = false;

    private void UpdateFireTriggerer()
    {
        bool isSlowMoving = true;

        foreach (Hand hand in provider.CurrentFrame.Hands)
        {
            if (hand.IsRight)
            {
                foreach (var finger in hand.Fingers)
                {
                    // && finger.IsExtended
                    if (finger.Type == Finger.FingerType.TYPE_INDEX)
                    {
                        if (finger.TipVelocity.MagnitudeSquared > 5.0f)
                        {
                            isSlowMoving = false;
                            isRecording = true;
                            slowMoveStartTime = Time.time;
                        }

                        if (isRecording)
                        {
                            // Add point to last position
                            var point = finger.TipPosition;
                            points.Add(new Point(point.x, point.y, 0));
                        }
                    }
                }
            }
        }

        if (isSlowMoving && isRecording)
        {
            if (Time.time - slowMoveStartTime > 0.3f)
            {
                points.Clear();
                isRecording = false;
            }
        }
    }
}
