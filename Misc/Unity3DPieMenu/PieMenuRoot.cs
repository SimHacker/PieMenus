///////////////////////////////////////////////////////////////////////
// PieMenuRoot.cs
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


public class PieMenuRoot : MonoBehaviour {


    ////////////////////////////////////////////////////////////////////////
    // Static variables.


    public static PieMenuRoot root;


    ////////////////////////////////////////////////////////////////////////
    // Instance variables.


    public PieMenu trackingPieMenu = null;
    public Camera pieMenuCamera = null;
    public Light pieMenuLight = null;
    public Texture2D textBackgroundTexture = null;


    // Instance variables configured in PieMenu.json.

    public Hashtable data = null;
    public GUISkin guiSkin = null;
    public Color _textBackgroundColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
    public float textBackgroundLeftRightMargin = 4.0f;
    public float textBackgroundTopBottomMargin = 2.0f;
    public Font titleFont = null;
    public Color titleTopColor;
    public Color titleUpperLeftColor;
    public Color titleLowerRightColor;
    public float titleOffsetX = 0.0f;
    public float titleOffsetY = 0.0f;
    public Font descriptionFont = null;
    public Color descriptionTopColor;
    public Color descriptionUpperLeftColor;
    public Color descriptionLowerRightColor;
    public float descriptionOffsetX = 0.0f;
    public float descriptionOffsetY = 0.0f;
    public Font itemTitleFont = null;
    public Color itemTitleTopColor;
    public Color itemTitleUpperLeftColor;
    public Color itemTitleLowerRightColor;
    public Font itemTitleDisabledFont = null;
    public Color itemTitleDisabledTopColor;
    public Color itemTitleDisabledUpperLeftColor;
    public Color itemTitleDisabledLowerRightColor;
    public float itemTitleOffsetX = 0.0f;
    public float itemTitleOffsetY = 0.0f;
    public Font itemDescriptionFont = null;
    public Color itemDescriptionTopColor;
    public Color itemDescriptionUpperLeftColor;
    public Color itemDescriptionLowerRightColor;
    public float itemDescriptionOffsetX = 0.0f;
    public float itemDescriptionOffsetY = 0.0f;
    public Font itemDescriptionDisabledFont = null;
    public Color itemDescriptionDisabledTopColor;
    public Color itemDescriptionDisabledUpperLeftColor;
    public Color itemDescriptionDisabledLowerRightColor;
    public int objectLayer = 10;
    public float objectDistance = 40.0f;
    public float itemObjectDistance = 60.0f;
    public float itemObjectSelectedDistance = 50.0f;
    public float itemObjectScale = 1.0f;
    public float itemObjectCameraScale = 1.0f;
    public float animationMaxMoveSpeed = 0.1f;
    public float animationMaxMoveTime = 0.25f;
    public float animationMinDistance = 2.0f;
    public float worldToScreenScale = 40.0f;
    public float cameraDistance = 10.0f;
    public float _cameraSize = 2.0f;
    public float _cameraDepth = 10.0f;
    public float _cameraNearClipPlane = 0.1f;
    public float _cameraFarClipPlane = 100.0f;
    public float _lightRange = 200.0f;
    public float _lightIntensity = 0.5f;


    ////////////////////////////////////////////////////////////////////////
    // Class methods.


    public static PieMenuRoot GetRoot()
    {
        InitializePieMenuRoot();

        return PieMenuRoot.root;
    }


    public static void InitializePieMenuRoot()
    {
        if (PieMenuRoot.root != null) {
            return;
        }

        GameObject rootObj =
            GameObject.Find("PieMenuRoot");

        if (rootObj == null) {
            rootObj = new GameObject();
            rootObj.name = "PieMenuRoot";
        }

        PieMenuRoot.root =
            rootObj.GetComponent<PieMenuRoot>();

        if (PieMenuRoot.root == null) {

            PieMenuRoot.root =
                rootObj.AddComponent<PieMenuRoot>();

        }

    }


    ////////////////////////////////////////////////////////////////////////
    // Instance methods.


    void Awake()
    {
        GetCamera();
        GetLight();
    }


    public Camera GetCamera()
    {
        if (pieMenuCamera == null) {

            pieMenuCamera =
                gameObject.GetComponent<Camera>();

            if (pieMenuCamera == null) {
                pieMenuCamera = gameObject.AddComponent<Camera>();
                pieMenuCamera.orthographic = true;
                pieMenuCamera.orthographicSize = cameraSize;
                pieMenuCamera.clearFlags = CameraClearFlags.Depth;
                pieMenuCamera.depth = cameraDepth;
                pieMenuCamera.nearClipPlane = cameraNearClipPlane;
                pieMenuCamera.farClipPlane = cameraFarClipPlane;
                pieMenuCamera.cullingMask = 1 << objectLayer;
            }
        }

        Vector3 screenPoint =
            new Vector3(1.0f, 1.0f, 1.0f);
        Vector3 worldPoint =
            pieMenuCamera.ScreenToWorldPoint(
                screenPoint);
        worldToScreenScale = 
            Mathf.Abs(worldPoint.x);

        Debug.Log("PieMenuRoot GetCamera worldPoint " + worldPoint.x + " " + worldPoint.y + " " + worldPoint.z + " scale " + worldToScreenScale);

        return pieMenuCamera;
    }


    public Light GetLight()
    {
        if (pieMenuLight == null) {

            pieMenuLight =
                gameObject.GetComponent<Light>();

            if (pieMenuLight == null) {
                pieMenuLight = gameObject.AddComponent<Light>();
                pieMenuLight.type = LightType.Point;
                pieMenuLight.range = lightRange;
                pieMenuLight.intensity = lightIntensity;
                pieMenuLight.cullingMask = 1 << objectLayer;
            }

        }

        return pieMenuLight;
    }


    public void LoadJSONProperties()
    {
        data = (Hashtable)Model.LoadSkinnedJsonFile("PieMenuRoot");

        OnBeforeLoadFromJson(data);

        foreach (string key in data.Keys) {
            LoadKeyValue(key, data[key]);
        }

        OnAfterLoadFromJson(data);
    }


    public void SaveJSONProperties()
    {
        Debug.Log("PieMenuRoot SaveJSONProperties: TODO");
    }


    public void OnBeforeLoadFromJson(Hashtable hashtable)
    {
    }


    public void LoadKeyValue(string key, object value)
    {
        switch (key) {

            case "type": break;
            case "guiSkin": guiSkin = Model.ConvertToGUISkin(value); break;
            case "textBackgroundColor": textBackgroundColor = Model.ConvertToColor(value); break;
            case "textBackgroundLeftRightMargin": textBackgroundLeftRightMargin = Model.ConvertToSingle(value); break;
            case "textBackgroundTopBottomMargin": textBackgroundTopBottomMargin = Model.ConvertToSingle(value); break;
            case "titleFont": titleFont = Model.ConvertToFont(value); break;
            case "titleTopColor": titleTopColor = Model.ConvertToColor(value); break;
            case "titleUpperLeftColor": titleUpperLeftColor = Model.ConvertToColor(value); break;
            case "titleLowerRightColor": titleLowerRightColor = Model.ConvertToColor(value); break;
            case "titleOffsetX": titleOffsetX = Model.ConvertToSingle(value); break;
            case "titleOffsetY": titleOffsetY = Model.ConvertToSingle(value); break;
            case "descriptionFont": descriptionFont = Model.ConvertToFont(value); break;
            case "descriptionTopColor": descriptionTopColor = Model.ConvertToColor(value); break;
            case "descriptionUpperLeftColor": descriptionUpperLeftColor = Model.ConvertToColor(value); break;
            case "descriptionLowerRightColor": descriptionLowerRightColor = Model.ConvertToColor(value); break;
            case "descriptionOffsetX": descriptionOffsetX = Model.ConvertToSingle(value); break;
            case "descriptionOffsetY": descriptionOffsetY = Model.ConvertToSingle(value); break;
            case "itemTitleFont": itemTitleFont = Model.ConvertToFont(value); break;
            case "itemTitleTopColor": itemTitleTopColor = Model.ConvertToColor(value); break;
            case "itemTitleUpperLeftColor": itemTitleUpperLeftColor = Model.ConvertToColor(value); break;
            case "itemTitleLowerRightColor": itemTitleLowerRightColor = Model.ConvertToColor(value); break;
            case "itemTitleDisabledFont": itemTitleDisabledFont = Model.ConvertToFont(value); break;
            case "itemTitleDisabledTopColor": itemTitleDisabledTopColor = Model.ConvertToColor(value); break;
            case "itemTitleDisabledUpperLeftColor": itemTitleDisabledUpperLeftColor = Model.ConvertToColor(value); break;
            case "itemTitleDisabledLowerRightColor": itemTitleDisabledLowerRightColor = Model.ConvertToColor(value); break;
            case "itemTitleOffsetX": itemTitleOffsetX = Model.ConvertToSingle(value); break;
            case "itemTitleOffsetY": itemTitleOffsetY = Model.ConvertToSingle(value); break;
            case "itemDescriptionFont": itemDescriptionFont = Model.ConvertToFont(value); break;
            case "itemDescriptionTopColor": itemDescriptionTopColor = Model.ConvertToColor(value); break;
            case "itemDescriptionUpperLeftColor": itemDescriptionUpperLeftColor = Model.ConvertToColor(value); break;
            case "itemDescriptionLowerRightColor": itemDescriptionLowerRightColor = Model.ConvertToColor(value); break;
            case "itemDescriptionDisabledFont": itemDescriptionDisabledFont = Model.ConvertToFont(value); break;
            case "itemDescriptionDisabledTopColor": itemDescriptionDisabledTopColor = Model.ConvertToColor(value); break;
            case "itemDescriptionDisabledUpperLeftColor": itemDescriptionDisabledUpperLeftColor = Model.ConvertToColor(value); break;
            case "itemDescriptionDisabledLowerRightColor": itemDescriptionDisabledLowerRightColor = Model.ConvertToColor(value); break;
            case "itemDescriptionOffsetX": itemDescriptionOffsetX = Model.ConvertToSingle(value); break;
            case "itemDescriptionOffsetY": itemDescriptionOffsetY = Model.ConvertToSingle(value); break;
            case "objectLayer": objectLayer = Model.ConvertToInt32(value); break;
            case "objectDistance": objectDistance = Model.ConvertToSingle(value); break;
            case "itemObjectDistance": itemObjectDistance = Model.ConvertToSingle(value); break;
            case "itemObjectSelectedDistance": itemObjectSelectedDistance = Model.ConvertToSingle(value); break;
            case "itemObjectScale": itemObjectScale = Model.ConvertToSingle(value); break;
            case "itemObjectCameraScale": itemObjectCameraScale = Model.ConvertToSingle(value); break;
            case "animationMaxMoveSpeed": animationMaxMoveSpeed = Model.ConvertToSingle(value); break;
            case "animationMaxMoveTime": animationMaxMoveTime = Model.ConvertToSingle(value); break;
            case "animationMinDistance": animationMinDistance = Model.ConvertToSingle(value); break;
            case "worldToScreenScale": worldToScreenScale = Model.ConvertToSingle(value); break;
            case "cameraDistance": cameraDistance = Model.ConvertToSingle(value); break;
            case "cameraSize": cameraSize = Model.ConvertToSingle(value); break;
            case "cameraDepth": cameraDepth = Model.ConvertToSingle(value); break;
            case "cameraNearClipPlane": cameraNearClipPlane = Model.ConvertToSingle(value); break;
            case "cameraFarClipPlane": cameraFarClipPlane = Model.ConvertToSingle(value); break;
            case "lightRange": lightRange = Model.ConvertToSingle(value); break;
            case "lightIntensity": lightIntensity = Model.ConvertToSingle(value); break;
            case "pieMenus": SetPieMenusFromJson(value); break;

            default: Debug.Log("PieMenuRoot LoadKeyValue got unexpected key '" + key + "' value '" + value + "'."); break;

        }
    }


    public void OnAfterLoadFromJson(Hashtable hashtable)
    {
    }


    public Color textBackgroundColor
    {
        set {
            if ((textBackgroundTexture == null) ||
                (_textBackgroundColor != value)) {
                _textBackgroundColor = value;
                textBackgroundTexture = new Texture2D(1, 1);
                textBackgroundTexture.SetPixel(0, 0, value);
                textBackgroundTexture.wrapMode = TextureWrapMode.Repeat;
                textBackgroundTexture.Apply();
            }
        }
        get {
            return _textBackgroundColor;
        }
    }


    public float cameraSize
    {
        set {
            _cameraSize = value;
            if (pieMenuCamera != null) {
                pieMenuCamera.orthographicSize = value;
            }
        }
        get {
            return _cameraSize;
        }
    }


    public float cameraDepth
    {
        set {
            _cameraDepth = value;
            if (pieMenuCamera != null) {
                pieMenuCamera.depth = value;
            }
        }
        get {
            return _cameraDepth;
        }
    }


    public float cameraNearClipPlane
    {
        set {
            _cameraNearClipPlane = value;
            if (pieMenuCamera != null) {
                pieMenuCamera.nearClipPlane = value;
            }
        }
        get {
            return _cameraNearClipPlane;
        }
    }


    public float cameraFarClipPlane
    {
        set {
            _cameraFarClipPlane = value;
            if (pieMenuCamera != null) {
                pieMenuCamera.farClipPlane = value;
            }
        }
        get {
            return _cameraFarClipPlane;
        }
    }


    public float lightRange
    {
        set {
            _lightRange = value;
            if (pieMenuLight != null) {
                pieMenuLight.range = value;
            }
        }
        get {
            return _lightRange;
        }
    }


    public float lightIntensity
    {
        set {
            _lightIntensity = value;
            if (pieMenuLight != null) {
                pieMenuLight.intensity = value;
            }
        }
        get {
            return _lightIntensity;
        }
    }


    public void SetPieMenusFromJson(object obj)
    {
        Hashtable hashtable = 
            (Hashtable)obj;
        foreach (string key in hashtable.Keys) {
            Hashtable pieMenuJson = 
                (Hashtable)hashtable[key];
            PieMenu.CreatePieMenu(key, pieMenuJson);
        }
    }


    public PieMenu FindPieMenu(string key)
    {
        Transform childTransform =
            transform.Find(key);

        return childTransform.gameObject.GetComponent<PieMenu>();
    }


    public void Update()
    {
        if (trackingPieMenu == null) {
            return;
        }

        bool buttonDown =
            Input.GetMouseButton(trackingPieMenu.button);

        if (trackingPieMenu.buttonDown != buttonDown) {

            if (buttonDown) {
                trackingPieMenu.TrackDown();
            } else {
                trackingPieMenu.TrackUp();
            }

        } else {
            trackingPieMenu.TrackMove();
        }

    }


    public void StartTracking(PieMenu pieMenu)
    {
        if (trackingPieMenu != null) {
            CancelTracking(trackingPieMenu);
        }

        trackingPieMenu = pieMenu;
    }


    public void CancelTracking(PieMenu pieMenu)
    {
        if (pieMenu == trackingPieMenu) {
            trackingPieMenu = null;
        }
    }


    public List<string> GetPieMenuKeys()
    {
        List<string> keys =
            new List<string>();

        foreach (Transform childTransform in transform) {

            PieMenu pieMenu =
                childTransform.gameObject.GetComponent<PieMenu>();
            if (pieMenu == null) {
                continue;
            }

            string name = 
                childTransform.gameObject.name;

            if (keys.Contains(name)) {
                Debug.Log("PieMenuRoot GetPieMenuKeys duplicate PieMenu GameObject name: " + name);
                continue;
            }

            keys.Add(name);
        }

        return keys;
    }


}


////////////////////////////////////////////////////////////////////////
