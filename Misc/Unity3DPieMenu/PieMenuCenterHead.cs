////////////////////////////////////////////////////////////////////////
// PieMenuCenterHead.cs
// By Don Hopkins (dhopkins@DonHopkins.com)
//
// Copyright 2012 Don Hopkins.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.


////////////////////////////////////////////////////////////////////////
// Demo of how to make pie menu feedback like the head in the middle
// of The Sims pie menus, but with YOUR head from the webcam!


////////////////////////////////////////////////////////////////////////


using UnityEngine;
using System.Collections;


////////////////////////////////////////////////////////////////////////


public class PieMenuCenterHead : MonoBehaviour {


    public Vector3 baseScale;
    public Quaternion baseRotation;
    public float animationDuration = 0.25f;
    public float distanceScale = 0.005f;
    public float turnScale = 0.5f;
    public float maxTurn = 40.0f;
    public float maxSwell = 80.0f;
    public Vector2 textureScale = new Vector2(-0.7f, 0.7f);
    public Vector2 textureOffset = new Vector2(0.53f, 0.1f);

    public bool started;
    public PieMenuItem lastItem;
    public Vector3 lastScale;
    public Quaternion lastRotation;
    public bool animating = false;
    public float animationStartTime;
    public Vector3 animationStartScale;
    public Vector3 animationEndScale;
    public Quaternion animationStartRotation;
    public Quaternion animationEndRotation;


    void Start()
    {
        //Debug.Log("PieMenuCenterHead Start");

        World.AuthorizeAndGetWebCamTexture(
            delegate(WebCamTexture webCamTexture) {

                //Debug.Log("PieMenuCenterHead Start AuthorizeAndGetWebCamTexture delegate webCamTexture " + webCamTexture);

                if (webCamTexture == null) {

                    //Debug.Log("No WebCam");

                } else {

                    //Debug.Log("Authorized WebCam");

                    MeshRenderer meshRenderer =
                        (MeshRenderer)gameObject.renderer;

                    if (meshRenderer == null) {

                        Debug.Log("PieMenuCenterHead Start expected its gameObject to have a meshRenderer component.");

                    } else {

                        meshRenderer.materials[0].mainTexture =
                            webCamTexture;
                        meshRenderer.materials[0].mainTextureOffset =
                            textureOffset;
                        meshRenderer.materials[0].mainTextureScale =
                            textureScale;

                    }
                }
            });

        baseScale = transform.localScale;
        baseRotation = transform.localRotation;

        PieMenuCenter center =
            PieMenuCenter.GetCenter(transform);
        PieMenu pieMenu =
            PieMenu.GetPieMenu(center.transform);

        pieMenu.onStartEvent += delegate(PieMenu pie) {
            animating = false;
            lastItem = null;
            started = false;

/*
            transform.localScale =
                new Vector3(
                    scale.x * baseScale.x, 
                    scale.y * baseScale.y, 
                    scale.z * baseScale.z);
*/
            transform.localRotation =
                baseRotation;
        };

        pieMenu.onUpdateCenterEvent += delegate(PieMenu pie, PieMenuCenter center0) {

            PieMenuItem currentItem =
                pie.currentItem;
            Vector3 scale =
                transform.localScale;
            //Debug.Log("scale is " + scale.x + " " + scale.y + " " + scale.z);
            Vector3 trackScale;
            Quaternion trackRotation;

            if (currentItem == null) {

                trackScale =
                    new Vector3(
                        scale.x * baseScale.x,
                        scale.y * baseScale.y,
                        scale.z * baseScale.z);
                //Debug.Log("currentItem null, trackScale " + trackScale.x + " " + trackScale.y + " " + trackScale.z);

                trackRotation =
                    baseRotation;

            } else {

                float dist = 
                    pie.distance - pie.inactiveRadius;
                float turn =
                    Mathf.Min(
                        dist * turnScale, 
                        maxTurn);
                float swell = 
                    Mathf.Min(
                        1.0f + (dist * distanceScale),
                        maxSwell);
                PieMenuSlice slice =
                    currentItem.GetSlice();
                float angle =
                    slice.angle;
                float dx = 
                    Mathf.Cos(angle);
                float dy = 
                    Mathf.Sin(angle);
                Vector3 axis =
                    new Vector3(
                        0.0f, 
                        -dx, 
                        -dy);
                //Debug.Log("currentItem " + currentItem.title + " angle " + angle + " dx " + dx + " dy " + dy + " axis " + axis);
                //Debug.Log("scale " + scale.x + " " + scale.y + " " + scale.z + " baseScale " + baseScale.x + " " + baseScale.y + " " + baseScale.z);

                trackScale =
                    new Vector3(
                        scale.x * baseScale.x * swell,
                        scale.y * baseScale.y * swell,
                        scale.z * baseScale.z * swell);

                trackRotation =
                    baseRotation *
                    Quaternion.AngleAxis(turn, axis);

            }

            Vector3 goalScale;
            Quaternion goalRotation;

            if (animating) {

                animationEndRotation = trackRotation;
                animationEndScale = trackScale;

                float elapsed =
                    (Time.time - animationStartTime) / animationDuration;

                if (elapsed >= 1.0f) {

                    animating = false;
                    goalRotation = animationEndRotation;
                    goalScale = animationEndScale;

                    //Debug.Log("done animation, goalScale " + goalScale.x + " " + goalScale.y + " " + goalScale.z);

                } else {

                    goalRotation = 
                        Quaternion.Slerp(
                            animationStartRotation,
                            animationEndRotation,
                            elapsed);
                    goalScale =
                        new Vector3(
                          ((animationStartScale.x * (1.0f - elapsed)) +
                           (animationEndScale.x * elapsed)),
                          ((animationStartScale.y * (1.0f - elapsed)) +
                           (animationEndScale.y * elapsed)),
                          ((animationStartScale.z * (1.0f - elapsed)) +
                           (animationEndScale.z * elapsed)));
                }

            } else {

                goalScale = trackScale;
                goalRotation = trackRotation;

            }

            if (currentItem == lastItem) {

                transform.localRotation = goalRotation;
                //transform.localScale = goalScale;

            } else {

                //Debug.Log("NEW ITEM " + ((currentItem == null) ? "NULL" : currentItem.title));

                if (!started) {
                    lastRotation = goalRotation;
                    lastScale = goalScale;
                }

                animating = true;
                animationStartTime = Time.time;
                lastItem = currentItem;

                animationStartRotation = lastRotation;
                animationStartScale = lastScale;
                animationEndRotation = goalRotation;
                animationEndScale = goalScale;
                //Debug.Log("animation rotation start " + animationStartRotation.x + " " + animationStartRotation.y + " " + animationStartRotation.z + " " + animationStartRotation.w + " end " + animationEndRotation.x + " " + animationEndRotation.y + " " + animationEndRotation.z + " " + animationEndRotation.w);
                //Debug.Log("animation scale start " + animationStartScale.x + " " + animationStartScale.y + " " + animationStartScale.z + " end " + animationEndScale.x + " " + animationEndScale.y + " " + animationEndScale.z);

                transform.localRotation = lastRotation;
                //transform.localScale = lastScale;

            }

            started = true;
            lastRotation = transform.localRotation;
            //lastScale = transform.localScale;
        };

    }


}


////////////////////////////////////////////////////////////////////////
