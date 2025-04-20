using UnityEngine;
// General XR (Extended Reality) support library. This one provides the XRHandSubsytem class
using UnityEngine.XR.Management;
// The library that provides the finger, hand, wrist, joint objects and classes.
using UnityEngine.XR.Hands;
using System.Collections.Generic;


public class XRHandsManager : MonoBehaviour
{
    // This is object that manages the hand tracking data
    XRHandSubsystem handSubsystem;
    [SerializeField]
    private SendData sendDataScript;

    private float timer = 0f;
    private float timerInterval = 0.2f;
    void Start()
    {
        // Get the active XR loader and hand subsystem. 
        var xrManagerSettings = XRGeneralSettings.Instance.Manager;
        var xrLoader = xrManagerSettings.activeLoader;

        if (xrLoader != null)
        {
            // Initializes our handSubsytem by retrieving it from xrLoader
            handSubsystem = xrLoader.GetLoadedSubsystem<XRHandSubsystem>();

            // Subsystem was not found
            if (handSubsystem == null)
            {
                Debug.LogError("XRHandSubsystem not found");
            }
            // Success
            else
            {
                Debug.Log("XRHandSubsystem initialized");
            }
        }
        // Broader issue, no xrLoader, maybe no xrManagerSettings
        else
        {
            Debug.LogError("No XR Loader active");
        }
    }

    void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;

        if (timer >= timerInterval) {
            if (handSubsystem != null) {
                // Gets data for left and right hands
                XRHand leftHand = handSubsystem.leftHand;
                XRHand rightHand = handSubsystem.rightHand;

                // Left hand is being tracked
                if (leftHand.isTracked)
                {
                    //Debug.Log("Left hand is being tracked");
                    //XRHandJoint leftWristJoint = leftHand.GetJoint(XRHandJointID.Wrist);
                    HandData leftData = ProcessHandData(leftHand);
                    sendDataScript.SendToArduino(leftData);
                }

                // Right hand is being tracked
                if (rightHand.isTracked)
                {
                    //Debug.Log("Right hand is being tracked");
                    //XRHandJoint rightWristJoint = rightHand.GetJoint(XRHandJointID.Wrist);
                    HandData rightData = ProcessHandData(rightHand);
                    sendDataScript.SendToArduino(rightData);
                }
                timer = 0f;
            }

        }
        
    }

    // Function to process hand data (joint positions and rotations)
    // https://docs.unity3d.com/Packages/com.unity.xr.hands@1.3/api/UnityEngine.XR.Hands.XRHandJointID.html
    private HandData ProcessHandData(XRHand hand)
    {
        HandData handData = new HandData
        {
            handedness = hand.handedness == Handedness.Left ? "Left" : "Right",
            positions = new Vector3[5],
            rotations = new Quaternion[5]
        };

        int index = 0;

        // Special case for the thumb
        XRHandJoint thumbTip = hand.GetJoint(XRHandJointID.ThumbTip);
        XRHandJoint thumbProximal = hand.GetJoint(XRHandJointID.ThumbProximal);
        XRHandJoint thumbMetacarpal = hand.GetJoint(XRHandJointID.ThumbMetacarpal);

        if (thumbTip.TryGetPose(out Pose thumbTipPose) &&
            thumbProximal.TryGetPose(out Pose thumbProximalPose) &&
            thumbMetacarpal.TryGetPose(out Pose thumbMetacarpalPose))
        {
            // Vector from metacarpal to proximal joint
            Vector3 metacarpalToProximal = thumbProximalPose.position - thumbMetacarpalPose.position;
            metacarpalToProximal.Normalize();

            // Vector from proximal to tip joint
            Vector3 proximalToTip = thumbTipPose.position - thumbProximalPose.position;
            proximalToTip.Normalize();

            // Calculate the bend angle for the thumb
            float thumbBendAngle = Vector3.Angle(metacarpalToProximal, proximalToTip);

            handData.positions[index] = thumbTipPose.position;
            handData.rotations[index] = Quaternion.Euler(thumbBendAngle, 0, 0);

            //Debug.Log($"{hand.handedness} Hand - ThumbTip: Bend Angle: {thumbBendAngle:F2}");

            index++;
        }
        else
        {
            Debug.LogWarning($"No valid pose for thumb joints in {hand.handedness} hand.");
        }

        // Process the other fingers (index, middle, ring, pinky)
        foreach (XRHandJointID jointID in new XRHandJointID[] {
            XRHandJointID.IndexTip,
            XRHandJointID.MiddleTip,
            XRHandJointID.RingTip,
            XRHandJointID.LittleTip })
        {
            XRHandJoint tipJoint = hand.GetJoint(jointID);
            XRHandJoint middleJoint = hand.GetJoint(jointID - 1); // Distal joint
            XRHandJoint proximalJoint = hand.GetJoint(jointID - 2); // Proximal joint

            if (tipJoint.TryGetPose(out Pose tipPose) &&
                middleJoint.TryGetPose(out Pose middlePose) &&
                proximalJoint.TryGetPose(out Pose proximalPose))
            {
                // Vector from proximal to middle joint
                Vector3 proximalToMiddle = middlePose.position - proximalPose.position;
                proximalToMiddle.Normalize();

                // Vector from middle to tip joint
                Vector3 middleToTip = tipPose.position - middlePose.position;
                middleToTip.Normalize();

                // Calculate the bend angle for the finger
                float bendAngle = Vector3.Angle(proximalToMiddle, middleToTip);

                handData.positions[index] = tipPose.position;
                handData.rotations[index] = Quaternion.Euler(bendAngle, 0, 0);

                //Debug.Log($"{hand.handedness} Hand - {jointID}: Bend Angle: {bendAngle:F2}");

                index++;
            }
            else
            {
                Debug.LogWarning($"No valid pose for {jointID} or its related joints in {hand.handedness} hand.");
            }
        }

        //
        // Add elbow data retrieval
        XRHandJoint wristJoint = hand.GetJoint(XRHandJointID.Wrist);
        XRHandJoint elbowJoint = hand.GetJoint(XRHandJointID.IndexMetacarpal); // Approximate elbow position

        if (wristJoint.TryGetPose(out Pose wristPose)) {
            Debug.Log($"{hand.handedness} Hand - Wrist Position: {wristPose.position}");
        }
        if (elbowJoint.TryGetPose(out Pose elbowPose)) {
            Debug.Log($"{hand.handedness} Hand - Estimated Elbow Position: {elbowPose.position}");
        }
        else
        {
            Debug.LogWarning($"Elbow joint tracking not available for {hand.handedness} hand.");
        }
        //

        return handData;
    }



    private bool isDistal(XRHandJointID jointID) {

        HashSet<XRHandJointID> distalIndices = new HashSet<XRHandJointID> {
            XRHandJointID.ThumbDistal,
            XRHandJointID.IndexDistal,
            XRHandJointID.MiddleDistal,
            XRHandJointID.RingDistal,
            XRHandJointID.LittleDistal 
        };

        return distalIndices.Contains(jointID);

    }
}
