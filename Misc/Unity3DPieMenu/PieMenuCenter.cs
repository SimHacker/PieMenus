////////////////////////////////////////////////////////////////////////
// PieMenuCenter.cs
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


public class PieMenuCenter : MonoBehaviour {


    ////////////////////////////////////////////////////////////////////////
    // Static methods.


    public static PieMenuCenter GetCenter(Transform transform)
    {
        while (transform != null) {
            PieMenuCenter center =
                transform.gameObject.GetComponent<PieMenuCenter>();

            if (center != null) {
                return center;
            }

            transform =
                transform.parent;
        }

        return null;
    }


    ////////////////////////////////////////////////////////////////////////
    // Instance methods.


    ////////////////////////////////////////////////////////////////////////
    // Parents.


	/// <summary>
	/// Gets the PieMenu of this PieMenuCenter.
	/// </summary>
    public PieMenu GetPieMenu()
    {
        return PieMenu.GetPieMenu(transform);
    }


	/// <summary>
	/// Gets the PieMenuRoot of this PieMenuCenter.
	/// </summary>
    public PieMenuRoot GetRoot()
    {
        return PieMenuRoot.GetRoot();
    }


}


////////////////////////////////////////////////////////////////////////
