﻿using BepInEx;
using System.Xml.Linq;
using UnityEngine;
using Utilla;
using static UnityEngine.GraphicsBuffer;

namespace SekiroGrapplingHook
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")] // Make sure to add Utilla 1.5.0 as a dependency!
    [ModdedGamemode]
    public class Plugin : BaseUnityPlugin
    {

        bool inAllowedRoom = false;
        bool holding;
        LineRenderer lr;
        Vector3 grapplePoint;


        private void Awake()
        {

            // Plugin startup logic
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            

            

            // Alternative way:
            
        }
        

        private void Update()
        {
            if (inAllowedRoom)
            {
                if (ControllerInputPoller.instance.rightControllerPrimaryButton && !holding)
                {
                    holding = true;
                    RaycastHit hit;
                    if (Physics.Raycast(GorillaLocomotion.Player.Instance.rightControllerTransform.position, GorillaLocomotion.Player.Instance.rightControllerTransform.forward, out hit, 100))
                    {
                        grapplePoint = hit.point;
                        lr.positionCount = 2;
                        

                    }
                }
                else if (!ControllerInputPoller.instance.rightControllerPrimaryButton && holding)
                {
                    holding = false;
                    RaycastHit hit;
                    lr.positionCount = 0;

                    GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.velocity = calculateBestThrowSpeed(GorillaLocomotion.Player.Instance.rightControllerTransform.position, grapplePoint, 2);

                }
                else if(ControllerInputPoller.instance.rightControllerPrimaryButton && holding)
                {
                    lr.SetPosition(0, GorillaLocomotion.Player.Instance.rightControllerTransform.position);
                    lr.SetPosition(1, grapplePoint);
                }
            }
        }

        [ModdedGamemodeJoin]
        private void RoomJoined(string gamemode)
        {
            var child = new GameObject();
            lr = child.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = Color.black;
            lr.endColor = Color.black;
            lr.startWidth = 0.02f;
            lr.endWidth = 0.02f;
            // The room is modded. Enable mod stuff.
            inAllowedRoom = true;
        }

        [ModdedGamemodeLeave]
        private void RoomLeft(string gamemode)
        {
            // The room was left. Disable mod stuff.
            inAllowedRoom = false;
        }
        private Vector3 calculateBestThrowSpeed(Vector3 origin, Vector3 target, float timeToTarget)
        {
            // calculate vectors
            Vector3 toTarget = target - origin;
            Vector3 toTargetXZ = toTarget;
            toTargetXZ.y = 0;

            // calculate xz and y
            float y = toTarget.y;
            float xz = toTargetXZ.magnitude;

            // calculate starting speeds for xz and y. Physics forumulase deltaX = v0 * t + 1/2 * a * t * t
            // where a is "-gravity" but only on the y plane, and a is 0 in xz plane.
            // so xz = v0xz * t => v0xz = xz / t
            // and y = v0y * t - 1/2 * gravity * t * t => v0y * t = y + 1/2 * gravity * t * t => v0y = y / t + 1/2 * gravity * t
            float t = timeToTarget;
            float v0y = y / t + 0.5f * Physics.gravity.magnitude * t;
            float v0xz = xz / t;

            // create result vector for calculated starting speeds
            Vector3 result = toTargetXZ.normalized;     // get direction of xz but with magnitude 1
            result *= v0xz;                             // set magnitude of xz to v0xz (starting speed in xz plane)
            result.y = v0y;                             // set y to v0y (starting speed of y plane)

            return result;
        }

    }
}