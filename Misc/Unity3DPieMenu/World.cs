////////////////////////////////////////////////////////////////////////
// World.cs
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
using System.IO;
using System.Collections;
using System.Collections.Generic;


////////////////////////////////////////////////////////////////////////


public class World : MonoBehaviour {


    ////////////////////////////////////////////////////////////////////////
    // Static class variables.


	public static World world;
    public static bool webCamEnabled = true;
    public static bool webCamAuthorizationRequired = false;
    public static bool webCamAuthorizationRequested = false;
    public static bool webCamAuthorizationFinished = false;
    public static bool webCamAuthorized = false;
    public static WebCamTexture webCamTexture = null;


    ////////////////////////////////////////////////////////////////////////
    // Instance variables.


    private bool pieMenuActive = false;


    ////////////////////////////////////////////////////////////////////////
    // Static webcam stuff.


    public static void AuthorizeAndGetWebCamTexture(WebCamAuthorizedHandler handler) {
        if (World.webCamAuthorizationFinished) {
            //Debug.Log("World AuthorizeAndGetWebCamTexture already authorized");
            handler(World.GetWebCamTexture());
        } else {
            //Debug.Log("World AuthorizeAndGetWebCamTexture authorizing, so adding handler to onWebCamAuthorizedEvent");
            World.onWebCamAuthorizedEvent += handler;
        }
    }


    public static WebCamTexture GetWebCamTexture()
    {
        if (!World.webCamEnabled ||
            !World.webCamAuthorized) {
            return null;
        }

        if (webCamTexture == null) {
            World.webCamTexture = new WebCamTexture();
            World.webCamTexture.wrapMode = TextureWrapMode.Repeat;
            World.webCamTexture.Play();
        }

        return webCamTexture;
    }


    ////////////////////////////////////////////////////////////////////////
    // Handlers and events.


    public delegate void WebCamAuthorizedHandler(WebCamTexture webCamTexture);
    public static event WebCamAuthorizedHandler onWebCamAuthorizedEvent;


    ////////////////////////////////////////////////////////////////////////
    // Initialization


    ////////////////////////////////////////////////////////////////////////
    // Instance initialization.


    IEnumerator Start()
    {
        //Debug.Log("World Start");

		world = this;

        PieMenu.onGlobalStartEvent += delegate(PieMenu pie) {
            pieMenuActive = true;
        };

        PieMenu.onGlobalEndEvent += delegate(PieMenu pie) {
            pieMenuActive = false;
        };

        webCamAuthorizationRequested = true;

        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

        webCamAuthorizationFinished = true;

        webCamAuthorized =
            Application.HasUserAuthorization(
                UserAuthorization.WebCam);

        //Debug.Log("World Start after RequestUserAuthorization webCamAuthorized " + webCamAuthorized);

        WebCamTexture texture =
            World.GetWebCamTexture();

        //Debug.Log("onWebCamAuthorizedEvent [" + World.onWebCamAuthorizedEvent + "] " + ((World.onWebCamAuthorizedEvent == null) ? "null" : "event"));

        if (onWebCamAuthorizedEvent != null) {
            onWebCamAuthorizedEvent(
                texture);
        }
    }


    ////////////////////////////////////////////////////////////////////////
    // GUI updating main loop.


    void OnGUI()
    {
        if (!pieMenuActive) {

            PieMenuRoot root =
                PieMenuRoot.GetRoot();

            GUISkin lastSkin = GUI.skin;
            GUI.skin = root.guiSkin;

            string message =
                "Unity Pie Menu Demo,\nby Don Hopkins.\n\nClick the left mouse button\nto pop up a pie menu!";

            float width;
            float height;
            PieMenu.MeasureText(
                message,
                root.titleFont,
                true,
                out width,
                out height);

            PieMenu.DrawText(
                message,
                Mathf.Floor((Screen.width * 0.5f) - (width * 0.5f)),
                Mathf.Floor((Screen.height * 0.25f) - (height * 0.5f)),
                root.titleFont,
                root.titleTopColor,
                root.titleUpperLeftColor,
                root.titleLowerRightColor);

            GUI.skin = lastSkin;
        }
    }


    ////////////////////////////////////////////////////////////////////////
    // Utilities.


    public bool Near(float f1, float f2)
    {
        float epsilon = 0.02f;
        float delta = (f1 > f2) ? (f1 - f2) : (f2 - f1);
        return delta < epsilon;
    }


    public float EaseInOut(float val)
    {
        return val * val * (3.0f - (2.0f * val));  
    }
    
    
    public float EaseIn(float val)
    {
        return (val < 0.5f) ? val : EaseInOut(val);
    }
    

    public float EaseOut(float val)
    {
        return (val < 0.5f) ? EaseInOut(val) : val;
    }


    public bool Overlaps(Rect r0, Rect r1)
    {
		bool overlaps =
			((r0.xMin < r1.xMax) && 
			 (r0.xMax > r1.xMin) &&
			 (r0.yMin < r1.yMax) &&
			 (r0.yMax > r1.yMin));
         return overlaps;
    }


    public Dictionary<string,string> GetQueryParams()
	{
        string src = Application.srcValue;
                        
        if ((src == null) ||
		    (src == "") || 
            (src.IndexOf("?") == -1) ||
		    (src.IndexOf("?") == (src.Length - 1))) {
			return new Dictionary<string, string>();
		}

		src = src.Substring(src.IndexOf("?") + 1);

        return ParseQueryString(src);
    }
    
	
    private Dictionary<string,string> ParseQueryString(string query)
	{
        Dictionary<string,string> urlParams = new Dictionary<string, string>();
        
        string[] paramList = query.Split('&');
        
        for (int i = 0; i < paramList.Length; i++) {
            string[] temp = paramList[i].Split('='); 
            string key = WWW.UnEscapeURL(temp[0]);
            string val =
				(temp.Length > 1) 
					? WWW.UnEscapeURL(temp[1]) 
					: "";

            urlParams.Add(key, val);
        }
        
        return urlParams;       
    }       
    
	
    public string QueryString(Dictionary<string,string> urlParams)
	{
        string parameters = "";
        bool first = true;
        
        foreach (KeyValuePair<string,string> kvp in urlParams) {
            parameters +=
				(first ? "?" : "&") + 
				WWW.EscapeURL(kvp.Key) +
				"=" +
				WWW.EscapeURL(kvp.Value);
            first = false;
        }
        
        return parameters;
    }
    
	
    public string GetFileName()
	{
        string src = Application.srcValue;
        int i = src.IndexOf("?");

		if (i >= 0) {
            return src.Substring(0, i);
		}

        return src;
    }
    

	public string GetDomain()
	{
        if (Application.isEditor) {
			return "http://localhost:8080";
		}

        string url = Application.absoluteURL;
        
        string protocol = url.Substring(0, url.IndexOf("://") + 3);
                        
        url = url.Substring(url.IndexOf("://") + 3);
                        
        string domain = url.Substring(0, url.IndexOf("/"));
        
        return protocol + domain;
    }


}


////////////////////////////////////////////////////////////////////////
