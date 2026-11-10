////////////////////////////////////////////////////////////////////////
// JsonData.cs
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
// Base class for Json data component.


////////////////////////////////////////////////////////////////////////


using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using JsonFx.Json;


////////////////////////////////////////////////////////////////////////


public class JsonData : MonoBehaviour {


     ////////////////////////////////////////////////////////////////////////
     // Delegates.


     public delegate void UpdateDataHandler(JsonData jsonData, object obj, int serial);


     ////////////////////////////////////////////////////////////////////////
     // Instance variables.


     public string dataFileName = null;
     public bool skinned = false;
     [System.NonSerialized]
     public object data = null;
     [System.NonSerialized]
     public int serial;
     public event UpdateDataHandler updateDataHandler;


     ////////////////////////////////////////////////////////////////////////
     // Instance methods.


     void Awake()
     {
         // TODO: Would it be better to do this later in Start or first Update after more objects are initialized?

         LoadData();
     }


     public object GetData()
     {
         return data;
     }


     public void SetData(object data0)
     {
         data = data0;

         UpdateData();
     }


     public void UpdateData()
     {
         serial++;

         if (updateDataHandler != null) {
             updateDataHandler(this, data, serial);
         }
     }


     public void LoadData()
     {
         SetData(
             ((dataFileName == null) || (dataFileName == ""))
                 ? null
                 : (skinned
                     ? Model.LoadSkinnedJsonFile(dataFileName)
                     : Model.LoadJsonFile(dataFileName)));

         UpdateData();
     }


     public void SaveData()
     {
         if ((dataFileName == null) || (dataFileName == "")) {
             return;
         }

         SetData(
             skinned
                 ? Model.LoadSkinnedJsonFile(dataFileName)
                 : Model.LoadJsonFile(dataFileName));

         UpdateData();
     }


     ////////////////////////////////////////////////////////////////////////


}


////////////////////////////////////////////////////////////////////////
