using UnityEngine;
// General XR (Extended Reality) support library. This one provides the XRHandSubsytem class
using UnityEngine.XR.Management;
// The library that provides the finger, hand, wrist, joint objects and classes.
using UnityEngine.XR.Hands;
using System.Collections.Generic;


public class XRHandsManager : MonoBehaviour
{
    [System.Serializable]
    public class HandData
    {
        public string handedness;
        public Vector3[] positions; // 0-4: fingers, 5: elbow
        public Quaternion[] rotations;
    }

    // This is object that manages the hand tracking data
    XRHandSubsystem handSubsystem;
    [SerializeField] private SendData sendDataScript;
    private float timer = 0f;
    private float timerInterval = 0.2f;

    void Start() {
        // Get the active XR loader and hand subsystem. 
        var xrManagerSettings = XRGeneralSettings.Instance.Manager;
        var xrLoader = xrManagerSettings.activeLoader;

        if (xrLoader != null) {
            // Initializes our handSubsytem by retrieving it from xrLoader
            handSubsystem = xrLoader.GetLoadedSubsystem<XRHandSubsystem>();

            // Subsystem was not found
            if (handSubsystem == null) {
                Debug.LogError("XRHandSubsystem not found");
            } else {                        // Success
                Debug.Log("XRHandSubsystem initialized");
            }
        } else {                            // Broader issue, no xrLoader, maybe no xrManagerSettings
            Debug.LogError("No XR Loader active");
        }
    }

    void FixedUpdate() {
        timer += Time.fixedDeltaTime;

        if (timer >= timerInterval) {
            if (handSubsystem != null) {
                // Gets data for left and right hands
                // XRHand leftHand = handSubsystem.leftHand;
                // XRHand rightHand = handSubsystem.rightHand;

                // // Left hand is being tracked
                // if (leftHand.isTracked)
                // {
                //     //Debug.Log("Left hand is being tracked");
                //     //XRHandJoint leftWristJoint = leftHand.GetJoint(XRHandJointID.Wrist);
                //     HandData leftData = ProcessHandData(leftHand);
                //     sendDataScript.SendToArduino(leftData);
                // }

                // // Right hand is being tracked
                // if (rightHand.isTracked)
                // {
                //     //Debug.Log("Right hand is being tracked");
                //     //XRHandJoint rightWristJoint = rightHand.GetJoint(XRHandJointID.Wrist);
                //     HandData rightData = ProcessHandData(rightHand);
                //     sendDataScript.SendToArduino(rightData);
                // }
                ProcessHand(handSubsystem.leftHand);
                ProcessHand(handSubsystem.rightHand);
                timer = 0f;
            }
        }
    }

    //New
    void ProcessHand(XRHand hand) {
            if (!hand.isTracked) return;
            
            HandData handData = new HandData
            {
                handedness = hand.handedness == Handedness.Left ? "Left" : "Right",
                positions = new Vector3[6],
                rotations = new Quaternion[6]
            };

            // Process fingers
            ProcessFingers(hand, handData);
            
            // Process elbow (5)
            ProcessElbow(hand, handData);

            sendDataScript.SendToArduino(handData);
        }

    //New
    void ProcessFingers(XRHand hand, HandData handData)
    {
        int index = 0;
        
        //Special case for the thumb
        var thumbTip = hand.GetJoint(XRHandJointID.ThumbTip);
        var thumbProximal = hand.GetJoint(XRHandJointID.ThumbProximal);
        var thumbMetacarpal = hand.GetJoint(XRHandJointID.ThumbMetacarpal);

        if (thumbTip.TryGetPose(out Pose thumbTipPose) &&
            thumbProximal.TryGetPose(out Pose thumbProximalPose) &&
            thumbMetacarpal.TryGetPose(out Pose thumbMetacarpalPose))
        {
            // Vector from metacarpal to proximal joint
            Vector3 metacarpalToProximal = (thumbProximalPose.position - thumbMetacarpalPose.position).normalized;
            // Vector from proximal to tip joint
            Vector3 proximalToTip = (thumbTipPose.position - thumbProximalPose.position).normalized;
            // Calculate the bend angle for the thumb
            float thumbBendAngle = Vector3.Angle(metacarpalToProximal, proximalToTip);
            
            handData.positions[0] = thumbTipPose.position;
            handData.rotations[0] = Quaternion.Euler(thumbBendAngle, 0, 0);
            //Debug.Log($"{hand.handedness} Hand - ThumbTip: Bend Angle: {thumbBendAngle:F2}");
            index++;
        }

        // Process the other fingers (index, middle, ring, pinky)
        foreach (XRHandJointID jointID in new XRHandJointID[] {
            XRHandJointID.IndexTip, XRHandJointID.MiddleTip,
            XRHandJointID.RingTip, XRHandJointID.LittleTip})
        {
            var tipJoint = hand.GetJoint(jointID);
            var middleJoint = hand.GetJoint(jointID - 1);   //Distal joint
            var proximalJoint = hand.GetJoint(jointID - 2); //Proximal joint

            if (tipJoint.TryGetPose(out Pose tipPose) &&
                middleJoint.TryGetPose(out Pose middlePose) &&
                proximalJoint.TryGetPose(out Pose proximalPose))
            {
                // Vector from proximal to middle joint
                Vector3 proximalToMiddle = (middlePose.position - proximalPose.position).normalized;
                // Vector from middle to tip joint
                Vector3 middleToTip = (tipPose.position - middlePose.position).normalized;
                float bendAngle = Vector3.Angle(proximalToMiddle, middleToTip);
                // Calculate the bend angle for the finger
                handData.positions[index] = tipPose.position;
                handData.rotations[index] = Quaternion.Euler(bendAngle, 0, 0);
                //Debug.Log($"{hand.handedness} Hand - {jointID}: Bend Angle: {bendAngle:F2}");
                index++;
            }
        }
    }    

    // void ProcessElbow(XRHand hand, HandData handData)
    // {
    //     var wristJoint = hand.GetJoint(XRHandJointID.Wrist);
    //     if (!wristJoint.TryGetPose(out Pose wristPose)) return;

    //     // Shoulder approximation (adjust values based on your setup)
    //     Vector3 shoulderPos = Camera.main.transform.position + 
    //                          new Vector3(hand.handedness == Handedness.Left ? -0.2f : 0.2f, -0.2f, 0);
        
    //     Vector3 upperArm = wristPose.position - shoulderPos;
    //     Vector3 forearm = wristPose.position - (shoulderPos + upperArm.normalized * 0.3f);
        
    //     float elbowAngle = Vector3.Angle(upperArm, forearm);
    //     elbowAngle = Mathf.Clamp(elbowAngle, 0, 180);

    //     handData.positions[5] = shoulderPos + upperArm.normalized * 0.3f;
    //     handData.rotations[5] = Quaternion.Euler(elbowAngle, 0, 0);
    // }

    void ProcessElbow(XRHand hand, HandData handData)           //All positions need to be approximated and thus is very inaccurate. Might need to get measurement in higher intervals.
{
    var wristJoint = hand.GetJoint(XRHandJointID.Wrist);
    if (!wristJoint.TryGetPose(out Pose wristPose)) return;

    Transform headTransform = Camera.main.transform;        //Get head transform for reference
    
    
    float shoulderHeight = headTransform.position.y - 0.2f;     //Calculate shoulder position relative to head
    float shoulderDistance = 0.2f;
    
    Vector3 shoulderPos = new Vector3(
        headTransform.position.x + (hand.handedness == Handedness.Left ? -shoulderDistance : shoulderDistance),
        shoulderHeight,
        headTransform.position.z);
    
    
    var palmJoint = hand.GetJoint(XRHandJointID.Palm);      //Get a mid-forearm position To try and get better approximation
    Vector3 forearmPos = palmJoint.TryGetPose(out Pose palmPose) ? palmPose.position : wristPose.position;
    
    Vector3 fullArmVector = forearmPos - shoulderPos;       //Shoulder to Forearm
    float fullArmLength = fullArmVector.magnitude;
    
    Vector3 horizontalVector = new Vector3(fullArmVector.x, 0, fullArmVector.z);        //Find the horizontal distance to see how much the arm is extended
    float horizontalDistance = horizontalVector.magnitude;
    
    float armExtension = horizontalDistance / fullArmLength;        //Get the normalized val (Find how "straight the arm is")
    armExtension = Mathf.Clamp01(armExtension);
    //When the arm is fully extended the horizontal distance approaches full arm length
    
    //Invert and scale the val (0 = straight, 180 = fully bent) 
    float elbowAngle = (1 - armExtension) * 180f;           //In reality looks to range from 20 to 75
    
    // Better without this as it is less "varient
    /*
    // Add a non-linear mapping(Trying to make small bends a little more accurate) - MIGHT REMOVE
    elbowAngle = Mathf.Pow(elbowAngle / 180f, 0.7f) * 180f;
    
    //Set minimum angle - MIGHT REMOVE
    elbowAngle = Mathf.Max(20f, elbowAngle);
    */
    
    handData.positions[5] = shoulderPos + fullArmVector * 0.5f; //Approximate elbow position
    handData.rotations[5] = Quaternion.Euler(elbowAngle, 0, 0);
    
    // Debug output
    Debug.Log($"{hand.handedness} Elbow: Raw={armExtension:F2}, Angle={elbowAngle:F0}");
}

/*
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
*/

/*
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
*/
}
