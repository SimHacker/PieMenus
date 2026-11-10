////////////////////////////////////////////////////////////////////////
// PieMenuItemTumble.cs
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


public class PieMenuItemTumble : MonoBehaviour {


    ////////////////////////////////////////////////////////////////////////
    // Instance variables.


    public float tumbleSpeedX = 100.0f;
    public float tumbleSpeedY = 120.0f;
    public float tumbleSpeedZ = 140.0f;


    ////////////////////////////////////////////////////////////////////////
    // Instance methods.


    public void Update()
    {
        transform.localRotation = 
            Quaternion.Euler(
                Time.time * tumbleSpeedX,
                Time.time * tumbleSpeedY,
                Time.time * tumbleSpeedZ);
    }


};


////////////////////////////////////////////////////////////////////////
