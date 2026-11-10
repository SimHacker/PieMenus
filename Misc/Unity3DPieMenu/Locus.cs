///////////////////////////////////////////////////////////////////////
// Locus.cs
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

/// <summary>
/// Locus for Unity, by Don Hopkins.
/// </summary>

////////////////////////////////////////////////////////////////////////


public class Locus : MonoBehaviour {


    ////////////////////////////////////////////////////////////////////////
    // Enumerated types.
	

    ////////////////////////////////////////////////////////////////////////
    // Delegates.

	
    ////////////////////////////////////////////////////////////////////////
    // Global event handlers.

	
    ////////////////////////////////////////////////////////////////////////
    // Class variables.

	
    static int objectLayer = 10;


    ////////////////////////////////////////////////////////////////////////
    // Instance variables.


    // Local instance event handlers.

	
    // Instance variables configured in Locus.json.
	
	
    public string title = "";

    public string description = "";

    public object parameters = null;

    public Texture locusTexture = null;


    // Dynamic instance variables.

	
    ////////////////////////////////////////////////////////////////////////
    // Static methods.

	
    public static Locus CreateLocus(string id, Hashtable locusJson)
    {
/*
        GameObject obj = 
            new GameObject();
        obj.name = 
            id;
        obj.transform.parent = 
            root.gameObject.transform;
        obj.layer = 
            Locus.objectLayer;
        Locus locus = 
            obj.AddComponent<Locus>();

        if (locusJson != null) {
            locus.LoadFromJson("Locus", locusJson);
        }

        return locus;
*/
        return null;
    }

	
    ////////////////////////////////////////////////////////////////////////
    // Instance methods.


    ////////////////////////////////////////////////////////////////////////
    // Loading loci from JSON.

	
    public void Awake()
    {
    }

	
    public void LoadFromJson(string typeName, Hashtable hashtable)
    {
        if (!hashtable.Contains("type") ||
            ((string)hashtable["type"] != "Locus")) {
            Debug.Log("Locus LoadFromJson type " + this.GetType() + " got invalid typeName " + typeName);
            return;
        }

/*
        instanceData = hashtable;
*/

        foreach (string key in hashtable.Keys) {
            object value = hashtable[key];
            LoadKeyValue(key, value);
        }
    }

	
    public void LoadKeyValue(string key, object value)
    {
        switch (key) {

            case "type":
                break;

            case "title":
                title =
                    Model.ConvertToString(value);
                break;

            case "description":
                description =
                    Model.ConvertToString(value);
                break;

            case "parameters":
                parameters =
                    value;
                break;

            case "locusTexture":
                locusTexture =
                    Model.ConvertToTexture(value);
                break;

            default:
                Debug.Log("Locus LoadKeyValue got unexpected key '" + key + "' value '" + value + "'.");
                break;

        }
    }


}


////////////////////////////////////////////////////////////////////////
