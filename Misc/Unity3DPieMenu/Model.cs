////////////////////////////////////////////////////////////////////////
// Model.cs
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
// Base class for persistent models represented as JSON.
// Including a simple skinning system.


////////////////////////////////////////////////////////////////////////


using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using JsonFx.Json;


////////////////////////////////////////////////////////////////////////


public class Model {


    ////////////////////////////////////////////////////////////////////////
    // Static variables and constructors.


    public static string currentDir;
    public static string contentDir;
    public static string configDir;
    public static Hashtable skins = null;


    ////////////////////////////////////////////////////////////////////////
    // Class methods for globally accessable utilities, json wrangling, etc.


    ////////////////////////////////////////////////////////////////////////
    // Class constructor.


    static Model()
    {
        LoadSkins();
    }


    ////////////////////////////////////////////////////////////////////////
    // Initialization.


    static void LoadSkins()
    {
        Model.skins = (Hashtable)Model.LoadJsonFile("Skins");
        Model.BindSkins(Model.skins);
    }


    ////////////////////////////////////////////////////////////////////////
    // Skinning system.


    public static void BindSkins(Hashtable skins)
    {
        foreach (string skinName in skins.Keys) {
            Hashtable skin = (Hashtable)skins[skinName];
            Model.BindSkin(skins, skinName, skin);
        }
    }


    public static void BindSkin(Hashtable skins, string skinName, Hashtable skin)
    {
        if (!skin.Contains("parents")) {
            return;
        }

        // Inherit keys from parent skins, in reverse order so 
        // righjt most parents have higher precedence.
        string parents = (string)skin["parents"];
        skin.Remove("parents");
        string[] parentNames = parents.Split(' ');
        Array.Reverse(parentNames);

        foreach (string parentName in parentNames) {

            if (parentName == "") {
                continue;
            }

            if (!skins.Contains(parentName)) {
                Debug.Log("BindSkin skin " + skinName + " has undefined parent " + parentName);
                continue;
            }

            // Bind parent skins depth first.
            Hashtable parentSkin = (Hashtable)skins[parentName];
            Model.BindSkin(skins, parentName, parentSkin);

            // Copy values from parent skin that are not already defined in this skin.
            foreach (string key in parentSkin.Keys) {
                if (!skin.Contains(key) ||
                    (skin[key] == null)) {
                    skin[key] = parentSkin[key];
                }
            }

        }
    }


    public static object LoadSkinnedJsonFile(string fileName)
    { 
        object json = LoadJsonFile(fileName);
        Model.SkinJson(json);

        return json;
    }


    public static void SkinJson(object json)
    {
        if ((json != null) &&
            ((json.GetType() == typeof(Hashtable)) ||
             (json.GetType().IsArray))) {

            object[] children = null;

            if (json.GetType() == typeof(Hashtable)) {

                Hashtable hashtable = (Hashtable)json;

                if (hashtable.Contains("skins")) {

                    // Inherit keys from skins, in reverse order so later ones have precedence.
                    string skinsString = (string)hashtable["skins"];
                    hashtable.Remove("skins");
                    string[] skinNames = skinsString.Split(' ');
                    Array.Reverse(skinNames);

                    foreach (string skinName in skinNames) {

                        if (!Model.skins.Contains(skinName)) {
                            Debug.Log("Json hashtable missing skin " + skinName);
                            continue;
                        }

                        // Copy values from skin that are not already defined in this hashtable.
                        Hashtable skin = (Hashtable)Model.skins[skinName];
                        foreach (string key in skin.Keys) {
                            if (!hashtable.Contains(key)) {
                                hashtable[key] = skin[key];
                            }
                        }

                    }

                }

                ICollection values = hashtable.Values;
                children = new object[values.Count];
                values.CopyTo(children, 0);

            } else if (json.GetType().IsArray) {

                children = (object[])json;

            }

            // Recurse over contained arrays and hashtables.
            foreach (object child in children) {

                if (child == null) {

                    // Do nothing.

                } else if (child.GetType() == typeof(Hashtable)) {

                    // Recurse on elements of hashtable.
                    Hashtable hashtable = (Hashtable)child;
                    foreach (string hashKey in hashtable.Keys) {
                        SkinJson(hashtable[hashKey]);
                    }

                } else if (child.GetType().IsArray) {

                    // Recurse on elements of array.
                    object[] objectArray = (object[])child;
                    foreach (object obj in objectArray) {
                        SkinJson(obj);
                    }

                }

            }

        }

    }


    ////////////////////////////////////////////////////////////////////////
    // Json utilities.


    // Utility to parse a Json string into a hashtable.
	public static Hashtable JsonToHashtable(string jsonString)
	{
		JsonReader reader = new JsonReader(jsonString);
		object obj = null;
		try {
			obj =
				reader.Deserialize();
		} catch (JsonFx.Json.JsonDeserializationException e) {
			Debug.Log("JSON ERROR: " + e.Message + " : " + jsonString);
		} catch (Exception e) {
			Debug.Log("WEIRD ERROR: " + e.Message + " : " + jsonString);
		}
		Hashtable result =
			(Hashtable)obj;
		return result;
	}


    // Utility to parse a Json string into an array.
	public static object[] JsonToArray(string jsonString)
	{
		JsonReader reader =
			new JsonReader(jsonString);
		var json =
			reader.Deserialize();
		object[] result =
			(object[])json;
		return result;
	}


    ////////////////////////////////////////////////////////////////////////
    // Json file loading and saving.


    public static object LoadJsonFile(string fileName)
    {
        Debug.Log("LoadJsonFile " + fileName);

        TextReader textReader = null;

        TextAsset textAsset =
            (TextAsset)Resources.Load("Config/" + fileName, typeof(TextAsset));
        Debug.Log("LoadJsonFile textAsset " + textAsset);
        textReader = new StringReader(textAsset.text);

        object data = null;

        if (textReader == null) {

            Debug.Log("LoadJsonFile null textReader for " + fileName);

        } else {

            var jsonReader = new JsonReader(textReader);

            data = jsonReader.Deserialize<object>();

            Debug.Log("LoadJsonFile got data " + data);

        }

        return data;
    }


    public static bool SaveJsonFile(string fileName, object data)
    {
        //Debug.Log("SaveJsonFile " + fileName);

        // TODO

        return true;
    }


    ////////////////////////////////////////////////////////////////////////
    // Reading text resources.


    public static string[] GetTextResourceLines(string textResourceName)
    {
    	//Debug.Log("GetTextResourceLines " + textResourceName);
        List<string> lines = new List<String>();
        string line;
        TextAsset textFile = (TextAsset)Resources.Load(textResourceName, typeof(TextAsset));
        System.IO.StringReader textStream = new System.IO.StringReader(textFile.text);

        while ((line = textStream.ReadLine()) != null) {
	    	//Debug.Log(line);
            lines.Add(line);
        }

        textStream.Close();

        if (lines.Count > 0) {
            return lines.ToArray();
        }

        return new string[0];
    }


    ////////////////////////////////////////////////////////////////////////
    // Data type conversion for loading and saving.


    public static string ConvertToString(object obj)
    {
        if (obj == null) {
            return "";
        }

        string val;
        if (obj is string) {
            val = (string)obj;
        } else {
            val = obj.ToString();
        }

        if (val == null) {
            val = "";
        }

        return val;
    }


    public static int ConvertToInt32(object obj)
    {
        if (obj == null) {
            return 0;
        }

        if (obj is int) {
            return (int)obj;
        }

        if (obj is float) {
            return (int)(float)obj;
        }

        if (obj is double) {
            return (int)(double)obj;
        }

        if (obj is bool) {
            return ((bool)obj) ? 1 : 0;
        }

        string str = (string)obj;
        if (str == null) {
            return 0;
        }

        int val = 0;
        if (!Int32.TryParse(str, out val)) {
            val = 0;
        }

        return val;
    }


    public static float ConvertToSingle(object obj)
    {
        if (obj == null) {
            return 0.0f;
        }

        if (obj is float) {
            return (float)obj;
        }

        if (obj is double) {
            return (float)(double)obj;
        }

        if (obj is int) {
            return (float)(int)obj;
        }

        if (obj is bool) {
            return ((bool)obj) ? 1.0f : 0.0f;
        }

        string str = (string)obj;
        if (str == null) {
            return 0.0f;
        }

        float val = 0.0f;
        if (!Single.TryParse(str, out val)) {
            val = 0.0f;
        }

        return val;
    }


    public static double ConvertToDouble(object obj)
    {
        if (obj == null) {
            return 0.0;
        }

        if (obj is double) {
            return (double)obj;
        }

        if (obj is float) {
            return (double)(float)obj;
        }

        if (obj is int) {
            return (float)(int)obj;
        }

        if (obj is bool) {
            return ((bool)obj) ? 1.0 : 0.0;
        }

        string str = (string)obj;
        if (str == null) {
            return 0.0;
        }

        double val = 0.0;
        if (!Double.TryParse(str, out val)) {
            val = 0.0;
        }

        return val;
    }


    public static string ConvertBoolToString(bool val)
    {
        return val ? "true" : "false";
    }


    public static bool ConvertToBool(object obj)
    {
        if (obj == null) {
            return false;
        }

        if (obj is bool) {
            return (bool)obj;
        }

        if (obj is int) {
            return ((int)obj) != 0;
        }

        if (obj is float) {
            return ((float)obj) == 0.0f;
        }

        if (obj is double) {
            return ((double)obj) == 0.0;
        }

        if (!(obj is string)) {
            return false;
        }

        string val = (string)obj;
        if ((val == null) || (val == "")) {
            return false;
        }

        string valLower = val.ToLower();
        return ((valLower == "true") ||
                (valLower == "t") ||
                (valLower == "yes") ||
                (valLower == "y") ||
                (valLower == "1"));
    }


    public static EnumType ConvertToEnum<EnumType>(object obj)
    {
        string str = (string)obj;

        EnumType result = (EnumType)Enum.Parse(typeof(EnumType), str);

        return result;
    }


    public static string[] ConvertToStringArray(object obj)
    {
        if (obj == null) {
            return new string[0];
        }

        object[] value = (object[])obj;
        if ((value == null) || (value.Length == 0)) {
            return new string[0];
        }

        string[] result = new string[value.Length];
        for (int i = 0; i < result.Length; i++) {
            string str = (string)value[i];
            if (str == null) {
                str = obj.ToString();
            }
            result[i] = str;
        }

        return result;
    }


    public static Texture[] ConvertToTextureArray(object obj)
    {
        if (obj == null) {
            return new Texture[0];
        }

        object[] value = (object[])obj;
        if ((value == null) || (value.Length == 0)) {
            return new Texture[0];
        }

        string[] textureNames = Model.ConvertToStringArray(value);
        Texture[] result = new Texture[value.Length];
        for (int i = 0; i < result.Length; i++) {
            string textureName = textureNames[i];
            result[i] = Model.ConvertToTexture(textureName);
        }

        return result;
    }


    public static GameObject ConvertToGameObject(object obj)
    {
        if (obj == null) {
            return null;
        }

        string value = (string)obj;
        if ((value == null) || (value == "")) {
            return null;
        }

        GameObject result = GameObject.Find(value);

        if (result == null) {
            Debug.Log("ConvertToGameObject got undefined GameObject name " + value);
        }

        return result;
    }


    public static GameObject[] ConvertToGameObjectArray(object obj)
    {
        if (obj == null) {
            return new GameObject[0];
        }

        object[] value = (object[])obj;
        if ((value == null) || (value.Length == 0)) {
            return new GameObject[0];
        }

        string[] gameObjectNames = Model.ConvertToStringArray(value);
        GameObject[] result = new GameObject[value.Length];
        for (int i = 0; i < result.Length; i++) {
            string gameObjectName = gameObjectNames[i];
            result[i] = ConvertToGameObject(gameObjectName);
        }

        return result;
    }


    public static ComponentClass ConvertToComponent<ComponentClass>(object obj) where ComponentClass:Component
    {
        if (obj == null) {
            return default(ComponentClass);
        }

        GameObject gameObject =
            Model.ConvertToGameObject(obj);
        if (gameObject == null) {
            return default(ComponentClass);
        }
        
        ComponentClass component =
           gameObject.GetComponent<ComponentClass>();

        return component;
    }


    public static ComponentClass[] ConvertToComponentArray<ComponentClass>(object obj) where ComponentClass:Component
    {
        if (obj == null) {
            return new ComponentClass[0];
        }

        object[] value = (object[])obj;
        if ((value == null) || (value.Length == 0)) {
            return new ComponentClass[0];
        }

        string[] gameObjectNames = Model.ConvertToStringArray(value);
        ComponentClass[] result = new ComponentClass[value.Length];
        for (int i = 0; i < result.Length; i++) {
            string gameObjectName = gameObjectNames[i];
            result[i] = Model.ConvertToComponent<ComponentClass>(gameObjectName);
        }

        return result;
    }


    public static Hashtable[] ConvertToHashtableArray(object obj)
    {
        if (obj == null) {
            return new Hashtable[0];
        }

        object[] value = (object[])obj;
        if ((value == null) || (value.Length == 0)) {
            return new Hashtable[0];
        }

        Hashtable[] result = new Hashtable[value.Length];
        for (int i = 0; i < result.Length; i++) {
            Hashtable hash = (Hashtable)value[i];
            if (hash == null) {
                hash = new Hashtable();
            }
            result[i] = hash;
        }

        return result;
    }


    public static string ConvertSingleArrayToString(float[] singleArray)
    {
        string[] stringArray = new string[singleArray.Length];

        for (int i = 0; i < singleArray.Length; i++) {
            stringArray[i] = Convert.ToString(singleArray[i]);
        }

        return String.Join(",", stringArray);
    }


    public static int[] ConvertToInt32Array(object obj)
    {
        if (obj == null) {
            return new int[0];
        }

        string value = (string)obj;
        if ((value == null) || (value == "")) {
            return new int[0];
        }

        string[] tokens = value.Split(',');
        int[] result = new int[tokens.Length];
        int i;
        for (i = 0; i < tokens.Length; i++) {
            int val = 0;
            if (!Int32.TryParse(tokens[i], out val)) {
                val = 0;
            }
            result[i] = val;
        }

        return result;
    }


    public static float[] ConvertToSingleArray(object obj)
    {
        if (obj == null) {
            return new float[0];
        }

        string value = (string)obj;
        string[] tokens = value.Split(',');
        float[] result = new float[tokens.Length];
        int i;
        for (i = 0; i < tokens.Length; i++) {
            float val = 0.0f;
            if (!Single.TryParse(tokens[i], out val)) {
                val = 0.0f;
            }
            result[i] = val;
        }
        return result;
    }


    public static string ConvertCurveToString(AnimationCurve curve)
    {
        if (curve == null) {
            return "";
        }

        List<string> tokens = new List<string>();
        for (int i = 0; i < curve.length; i++) {
            Keyframe key = curve[i];
            tokens.Add(key.time + "," + key.value);
        }

        return String.Join(";", tokens.ToArray());
    }


    public static AnimationCurve ConvertToCurve(object obj)
    {
        if (obj == null) {
            return null;
        }

        string value = (string)obj;

        if ((value == null) || (value == "")) {
            return null;
        }

        AnimationCurve curve = new AnimationCurve();

        string[] pairs = value.Split(';');

        if (pairs.Length == 0) {
            curve.AddKey(0.0f, 0.0f);
            curve.AddKey(1.0f, 1.0f);
        } else {
            foreach (string pair in pairs) {
                string [] numbers = pair.Split(',');
                if (numbers.Length != 2) {
                    Debug.Log("Bad pair of numbers '" + pair + "' in animation curve '" + value + "'.");
                    continue;
                }

                float x = 0.0f;
                if (!Single.TryParse(numbers[0], out x)) {
                    x = 0.0f;
                }

                float y = 0.0f;
                if (!Single.TryParse(numbers[1], out y)) {
                    y = 0.0f;
                }
                Keyframe key = new Keyframe(x, y);
                key.inTangent = 0.0f;
                key.outTangent = 0.0f;
                curve.AddKey(key);
            }
        }

        return curve;
    }


    public static string ConvertColorToString(Color color)
    {
        return color.r + " " + color.g + " " + color.b + " " + color.a;
    }


    public static Color ConvertToColor(object obj)
    {
        if (obj == null) {
            return Color.black;
        }

        string colorString = (string)obj;
        if ((colorString == null) || (colorString == "")) {
            return Color.black;
        }

        float r = 0.0f;
        float g = 0.0f;
        float b = 0.0f;
        float a = 1.0f;

        string[] components = colorString.Split(',');
        if ((components.Length == 3) ||
            (components.Length == 4)) {
            if (!Single.TryParse(components[0], out r)) {
                r = 0.0f;
            }
            if (!Single.TryParse(components[1], out g)) {
                g = 0.0f;
            }
            if (!Single.TryParse(components[2], out b)) {
                b = 0.0f;
            }
            if (components.Length == 4) {
                if (!Single.TryParse(components[3], out a)) {
                    a = 1.0f;
                }
            }
        } else {
            Debug.Log("ConvertToColor got wrong color format: " + colorString);
        }

        //Debug.Log("ConvertToColor " + colorString + " = " + r + " " + g + " " + b + " " + a);

        Color color = new Color(r, g, b, a);
        return color;
    }


    public static Texture ConvertToTexture(object obj)
    {
        if (obj == null) {
            return null;
        }

        string textureName = (string)obj;
        if ((textureName == null) || (textureName == "")) {
            return null;
        }

        if (textureName == "webcam") {
            return World.GetWebCamTexture();
        }

        Texture texture = (Texture)Resources.Load(textureName);
        if (texture == null) {
            Debug.Log("ConvertToTexture can't find texture named " + textureName);
        }

        return texture;
    }


    public static Font ConvertToFont(object obj)
    {
        if (obj == null) {
            return null;
        }

        string fontName = (string)obj;
        if ((fontName == null) || (fontName == "")) {
            return null;
        }

        Font font = (Font)Resources.Load(fontName);
        if (font == null) {
            Debug.Log("ConvertToFont can't find font named " + fontName);
        }

        return font;
    }


    public static GUISkin ConvertToGUISkin(object obj)
    {
        if (obj == null) {
            return null;
        }

        string guiSkinName = (string)obj;
        if ((guiSkinName == null) || (guiSkinName == "")) {
            return null;
        }

        GUISkin guiSkin = (GUISkin)Resources.Load(guiSkinName);
        if (guiSkin == null) {
            Debug.Log("ConvertToGuiSkin can't find GUISkin named " + guiSkinName);
        }

        return guiSkin;
    }


    public static Material ConvertToFontMaterial(Font font, Color color)
    {
        //Debug.Log("ConvertToFontMaterial font " + font + " color " + color);
        Shader fontShader = Shader.Find("GUI/Text Shader");
        Texture fontTexture = font.material.mainTexture;
        Material material = new Material(fontShader);
        material.mainTexture = fontTexture;
        material.color = color;

        return material;
    }


    ////////////////////////////////////////////////////////////////////////
    // Automatically set attributes based on reflection.


    public static bool LoadKey(object obj, string key, object value)
    {
        Type type = obj.GetType();
        FieldInfo field = type.GetField(key);
        string fieldTypeName = (field == null) ? "NULL" : field.FieldType.ToString();
        string valueTypeName = (value == null) ? "NULL" : value.GetType().Name;

        //Debug.Log("LoadKey obj " + obj + " type " + type + " key " + key + " value " + value + " valueType " + valueTypeName + " field " + field + " fieldTypeName " + fieldTypeName);

        switch (fieldTypeName) {

            case "System.Single":
                float f = 0.0f;
                switch (valueTypeName) {
                    case "Single":
                        f = (float)value;
                        break;
                    case "Double":
                        double d = (double)value;
                        f = (float)d;
                        break;
                    default:
                        Debug.Log("Model LoadKey obj " + obj + " key " + key + " value " + value + " type " + valueTypeName + " field " + field + " fieldTypeName " + fieldTypeName + " bad conversion");
                        return false;
                }
                field.SetValue(obj, f);
                break;

            case "System.String":
                string s = null;
                switch (valueTypeName) {
                    case "String":
                        s = (string)value;
                        break;
                    case "NULL":
                        s = null;
                        break;
                    default:
                        Debug.Log("Model LoadKey obj " + obj + " key " + key + " value " + value + " type " + valueTypeName + " field " + field + " fieldTypeName " + fieldTypeName + " bad conversion");
                        return false;
                }
                field.SetValue(obj, s);
                break;

            case "System.Object":
                field.SetValue(obj, value);
                break;

            default:
                Debug.Log("Model LoadKey unknown key " + key + " " + fieldTypeName);
                return false;

        }

        return true;
    }


    ////////////////////////////////////////////////////////////////////////
    // Instance methods.


    ////////////////////////////////////////////////////////////////////////
    // Loading and saving objects as Json.
    // Subclassers should override these as appropriate.


    // Subclasses override this to load their state from Json.
	public void LoadFromJson(string typeName, object json)
	{
        Hashtable hashtable = (Hashtable)json;

		//Debug.Log("LoadFromJson type " + this.GetType() + " typeName " + typeName + " hashtable " + hashtable);

        if ((string)hashtable["type"] != typeName) {
            Debug.Log("LoadFromJson type " + this.GetType() + " got invalid typeName " + typeName);
            return;
        }

		OnBeforeLoadFromJson(hashtable);

		foreach (string key in hashtable.Keys) {
			object value = hashtable[key];
            LoadKeyValue(key, value);
		}

		OnAfterLoadFromJson(hashtable);
	}


    // Subclasses override this to initialize their state before loading from a json object.
	public virtual void OnBeforeLoadFromJson(Hashtable hashtable)
	{
	}


    // Subclasses override this to initialize their state after loading from a json object.
	public virtual void OnAfterLoadFromJson(Hashtable hashtable)
	{
	}


    // Subclasses override this to load a property from a json key and value.
    public virtual void LoadKeyValue(string key, object value)
    {
    }

    // Subclasses override this to save their state to a json object.
    // Don't call super, but make sure to call OnBeforeSaveToJson before saving, if you need to.
	public virtual Hashtable SaveToJson(string typeName)
	{
        Hashtable hashtable = new Hashtable();

        hashtable["type"] = typeName;

		OnBeforeSaveToJson(hashtable);

		foreach (string key in hashtable.Keys) {
            object value = SaveKeyValue(key);
            hashtable[key] = value;
		}

		OnBeforeSaveToJson(hashtable);

		return null;
	}


    // Subclasses override this to prepare before saving their state to a json object.
	public virtual void OnBeforeSaveToJson(Hashtable hashtable)
	{
	}


    // Subclasses override this to clean up after saving their state to a json object.
	public virtual void OnAfterSaveToJson(Hashtable hashtable)
	{
	}


    // Subclasses override this to save a property to a json value.
    public virtual object SaveKeyValue(string key)
    {
        return null;
    }


}


////////////////////////////////////////////////////////////////////////
