///////////////////////////////////////////////////////////////////////
// PieMenuCrafter.cs
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
using System;
using System.Collections;
using System.Collections.Generic;


////////////////////////////////////////////////////////////////////////


public class PieMenuCrafter : MonoBehaviour {


    public PieMenu currentPieMenu;
    public PieMenuSlice currentSlice;
    public PieMenuItem currentItem;
    public float editDelay = 1.0f;
    public int verbosity = 0;


    public void Awake()
    {

        // 1) start        change end select
        // 2) start        change end submenu start pinned ... (3, 4, 5)
        // 3) start pinned        end cancel
        // 4) start pinned change end select
        // 5) start pinned change end submenu start pinned ... (3, 4, 5)

        PieMenu.onGlobalDownEvent += delegate(PieMenu pieMenu) {
            if (verbosity > 0) {
                Debug.Log("PieMenuCrafter saw down PieMenu " + pieMenu);
            }
        };

        PieMenu.onGlobalUpEvent += delegate(PieMenu pieMenu) {
            if (verbosity > 0) {
                Debug.Log("PieMenuCrafter saw up PieMenu " + pieMenu);
            }
        };

        PieMenu.onGlobalStartEvent += delegate(PieMenu pieMenu) {
            if (verbosity > 0) {
                Debug.Log("PieMenuCrafter saw start PieMenu " + pieMenu);
            }

            currentPieMenu = 
                pieMenu;
            currentSlice = 
                null;
            currentItem = 
                null;
        };

        PieMenu.onGlobalEndEvent += delegate(PieMenu pieMenu) {
            if (verbosity > 0) {
                Debug.Log("PieMenuCrafter saw end PieMenu " + pieMenu);
            }
        };

        PieMenu.onGlobalCancelEvent += delegate(PieMenu pieMenu) {
            if (verbosity > 0) {
                Debug.Log("PieMenuCrafter saw cancel PieMenu " + pieMenu);
            }

            currentPieMenu = 
                null;
            currentSlice = 
                null;
            currentItem = 
                null;
        };

        PieMenu.onGlobalPinnedEvent += delegate(PieMenu pieMenu) {
            if (verbosity > 0) {
                Debug.Log("PieMenuCrafter saw pinned PieMenu " + pieMenu);
            }
        };

        PieMenu.onGlobalSelectEvent += delegate(PieMenu pieMenu, PieMenuItem currentItem, float direction, float distance) {
            if (verbosity > 0) {
                Debug.Log("PieMenuCrafter saw select PieMenu " + pieMenu + " PieMenuItem " + currentItem + " direction " + direction + " distance " + distance);
            }

            currentPieMenu = 
                null;
            currentSlice = 
                null;
            currentItem = 
                null;
        };

        PieMenu.onGlobalSubMenuEvent += delegate(PieMenu pieMenu, PieMenuItem currentItem, PieMenu subMenu) {
            if (verbosity > 0) {
                Debug.Log("PieMenuCrafter saw submenu PieMenu " + pieMenu);
            }

            currentPieMenu = 
                pieMenu;
            currentSlice =
                null;
            currentItem = 
                null;
        };

        PieMenu.onGlobalChangeEvent += delegate(PieMenu pieMenu, PieMenuItem currentItem0, float direction, float distance) {
            if (verbosity > 0) {
                Debug.Log("PieMenuCrafter saw change PieMenu " + pieMenu + " PieMenuItem " + currentItem + " direction " + direction + " distance " + distance);
            }

            currentItem = 
                currentItem0;
            currentSlice =
               (currentItem0 == null) ? null : currentItem0.GetSlice();
        };

    }


}


////////////////////////////////////////////////////////////////////////
