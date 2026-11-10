////////////////////////////////////////////////////////////////////////
// PieMenuTarget.cs
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


using UnityEngine;
using System.Collections;


////////////////////////////////////////////////////////////////////////


public class PieMenuTarget : MonoBehaviour {


    public string pieMenuId = null;
    public PieMenu pieMenu = null;
    public int pieMenuButton = 0;
    public object pieMenuSubject = null;
    public bool mouseOver = false;
    public bool trackMouse = true;
    public bool trackTouch = true;
    public int lastTouchCount = 0;


    void OnMouseEnter()
    {
        Debug.Log("PieMenuTarget OnMouseEnter this " + this + " gameObject " + gameObject + " pieMenu " + pieMenu + " pieMenuId " + pieMenuId + " subject " + pieMenuSubject);
        mouseOver = true;
    }


    void OnMouseExit()
    {
        mouseOver = false;
    }


    void Update()
    {
        if (trackTouch) {
            TrackTouch();            
        }

        if (trackMouse) {
            TrackMouse();
        }

    }


    void TrackTouch()
    {
        int curTouchCount = 
            Input.touchCount;
        
        bool touching =
            curTouchCount != 0;
        bool touchStart =
            (lastTouchCount == 0) &&
            (curTouchCount >= 1);
        bool touchStop =
            (lastTouchCount >= 1) &&
            (curTouchCount == 0);
        Collider myCollider =
            gameObject.collider;

        if (touchStart) {

            Touch touch =
                Input.touches[0];

            RaycastHit hit;
            Ray ray =
                Camera.main.ScreenPointToRay(
                    touch.position);

            Debug.Log("PieMenuTarget TrackTouch  touchStart touch " + touch + " position " + touch.position.x + " " + touch.position.y + " ray " + ray);

            if (Physics.Raycast(
                    ray,
                    out hit,
                    100000)) {
                Debug.Log("PieMenuTarget TrackTouch  touchStart Raycast succeeded, hit.collider " + hit.collider + " myCollider " + myCollider + " equal " + (hit.collider == myCollider));
                if (hit.collider == myCollider) {
                    Debug.Log("HIT ME! " + this);
                }
                
            }

        }

        lastTouchCount =
            curTouchCount;
    }


    void TrackMouse()
    {
        PieMenuRoot root =
            PieMenuRoot.GetRoot();

        if ((root.trackingPieMenu != null) ||
            ((pieMenu == null) && ((pieMenuId == null) || (pieMenuId == ""))) ||
            (!mouseOver) ||
            (!Input.GetMouseButton(pieMenuButton))) {
            return;
        }

        if (pieMenuSubject == null) {
            pieMenuSubject = gameObject;
        }

        if (pieMenu == null) {
            pieMenu =
                root.FindPieMenu(pieMenuId);
        }

        if (pieMenu != null) {
            pieMenu.Popup(pieMenuButton, false, pieMenuSubject);
        }
    }


}


////////////////////////////////////////////////////////////////////////
