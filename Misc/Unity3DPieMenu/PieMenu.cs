///////////////////////////////////////////////////////////////////////
// PieMenu.cs
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
/// Pie menu widget for Unity, by Don Hopkins.
/// </summary>

////////////////////////////////////////////////////////////////////////


public class PieMenu : MonoBehaviour {


    ////////////////////////////////////////////////////////////////////////
    // Enumerated types.
	

	/// <summary>
	/// Item label justification. How to position the label relative to the 
    /// anchor point of the pie menu item.
	/// Center: center the label on the anchor point.
	/// InnerEdge: justify the inner edge of the label on the anchor point. 
	/// The directly North direction uses the middle of the lower edge. 
	/// The directly South direction uses the middle of the upper edge.
	/// Any direction on the East side uses the middle of the left edge. 
	/// Any direction on the West side uses the middle of the right edge.
	/// </summary>
    public enum ItemLabelJustification {
        Center,
        InnerEdge,
    };


    ////////////////////////////////////////////////////////////////////////
    // Delegates.

	
	/// <summary>
	/// Pie menu down handler. Called when the mouse button is 
    // pressed down during tracking.
	/// </summary>
    public delegate void PieMenuDownHandler(PieMenu pieMenu);
    
	/// <summary>
	/// Pie menu up handler. Called when the mouse button is 
    // released during tracking.
	/// </summary>
    public delegate void PieMenuUpHandler(PieMenu pieMenu);

	/// <summary>
	/// Pie menu start handler. Called when the pie menu is popped up and 
    /// tracking is started.
	/// </summary>
    public delegate void PieMenuStartHandler(PieMenu pieMenu);
    
	/// <summary>
	/// Pie menu end handler. Called when the pie menu is popped down and 
    /// tracking is finished, either because it was canceled or if an item 
    /// selected.
	/// </summary>
	public delegate void PieMenuEndHandler(PieMenu pieMenu);

	/// <summary>
	/// Pie menu cancel handler. Called when the pie menu is canceled without 
    /// selecting an item.
	/// </summary>
	public delegate void PieMenuCancelHandler(PieMenu pieMenu);

	/// <summary>
	/// Pie menu pinned handler. Called when the pie menu is pinned by 
    /// clicking it up.
	/// </summary>
	public delegate void PieMenuPinnedHandler(PieMenu pieMenu);

	/// <summary>
	/// Pie menu select handler. Called when an item is selected, but not when 
    /// a submenu is selected.
	/// </summary>
	public delegate void PieMenuSelectHandler(PieMenu pieMenu, PieMenuItem currentItem, float direction, float distance);

	/// <summary>
	/// Pie menu sub menu handler. Called when an item with a submenu is selected.
	/// </summary>
	public delegate void PieMenuSubMenuHandler(PieMenu pieMenu, PieMenuItem currentItem, PieMenu subMenu);

	/// <summary>
	/// Pie menu change handler. Called when the selected item changes during
    /// tracking.
	/// </summary>
	public delegate void PieMenuChangeHandler(PieMenu pieMenu, PieMenuItem currentItem, float direction, float distance);

	/// <summary>
	/// Pie menu update handler. Called on every update during tracking.
	/// </summary>
	public delegate void PieMenuUpdateHandler(PieMenu pieMenu, PieMenuItem currentItem, float direction, float distance);

	/// <summary>
	/// Pie menu update center handler. Called on every update when the pie 
    /// menu has a center object.
	/// </summary>
	public delegate void PieMenuUpdateCenterHandler(PieMenu pieMenu, PieMenuCenter center);

	/// <summary>
	/// Pie menu update item handler. Called on every update when there is a 
    /// current item.
	/// </summary>
	public delegate void PieMenuUpdateItemHandler(PieMenu pieMenu, PieMenuItem item);


    ////////////////////////////////////////////////////////////////////////
    // Global event handlers.

	
	/// <summary>
	/// Occurs on global down event for any menu.
	/// </summary>
    public static event PieMenuDownHandler onGlobalDownEvent;
	
	/// <summary>
	/// Occurs on global up event for any menu.
	/// </summary>
    public static event PieMenuUpHandler onGlobalUpEvent;
	
	/// <summary>
	/// Occurs on global start event for any menu.
	/// </summary>
    public static event PieMenuStartHandler onGlobalStartEvent;
	
	/// <summary>
	/// Occurs on global end event for any menu.
	/// </summary>
    public static event PieMenuEndHandler onGlobalEndEvent;
	
	/// <summary>
	/// Occurs on global cancel event for any menu.
	/// </summary>
    public static event PieMenuCancelHandler onGlobalCancelEvent;
	
	/// <summary>
	/// Occurs on global pinned event for any menu.
	/// </summary>
    public static event PieMenuPinnedHandler onGlobalPinnedEvent;
	
	/// <summary>
	/// Occurs on global select event for any menu.
	/// </summary>
    public static event PieMenuSelectHandler onGlobalSelectEvent;

	/// <summary>
	/// Occurs on global sub menu event for any menu.
	/// </summary>
    public static event PieMenuSubMenuHandler onGlobalSubMenuEvent;
	
	/// <summary>
	/// Occurs on global change event for any menu.
	/// </summary>
    public static event PieMenuChangeHandler onGlobalChangeEvent;
	
	/// <summary>
	/// Occurs on global update event for any menu.
	/// </summary>
    public static event PieMenuUpdateHandler onGlobalUpdateEvent;
	
	/// <summary>
	/// Occurs on global update center event for any menu.
	/// </summary>
    public static event PieMenuUpdateCenterHandler onGlobalUpdateCenterEvent;
	
	/// <summary>
	/// Occurs on global update item event for any menu.
	/// </summary>
    public static event PieMenuUpdateItemHandler onGlobalUpdateItemEvent;


    ////////////////////////////////////////////////////////////////////////
    // Global constants.

	
	/// <summary>
	/// Constant two PI, the number of radians in a circle.
	/// </summary>
    public const float twoPI = 2.0f * Mathf.PI;
	
	/// <summary>
	/// Constant rat to deg, to convert from radians to degrees.
	/// </summary>
    public const float radToDeg = 360.0f / twoPI;
	
	/// <summary>
	/// Constant deg to rad, to convert from degrees to radians.
	/// </summary>
    public const float degToRad = twoPI / 360.0f;


    ////////////////////////////////////////////////////////////////////////
    // Instance variables.


    // Local instance event handlers.

	
	/// <summary>
	/// Occurs on down event for a particular menu.
	/// </summary>
    public event PieMenuDownHandler onDownEvent;
	
	/// <summary>
	/// Occurs on up event for a particular menu.
	/// </summary>
    public event PieMenuUpHandler onUpEvent;
	
	/// <summary>
	/// Occurs on start event for a particular menu.
	/// </summary>
    public event PieMenuStartHandler onStartEvent;
	
	/// <summary>
	/// Occurs on end event for a particular menu.
	/// </summary>
    public event PieMenuEndHandler onEndEvent;
	
	/// <summary>
	/// Occurs on cancel event for a particular menu.
	/// </summary>
    public event PieMenuCancelHandler onCancelEvent;
	
	/// <summary>
	/// Occurs on pinned event for a particular menu.
	/// </summary>
    public event PieMenuPinnedHandler onPinnedEvent;
	
	/// <summary>
	/// Occurs on select event for a particular menu.
	/// </summary>
    public event PieMenuSelectHandler onSelectEvent;
	
	/// <summary>
	/// Occurs on sub menu event for a particular menu.
	/// </summary>
    public event PieMenuSubMenuHandler onSubMenuEvent;
	
	/// <summary>
	/// Occurs on change event for a particular menu.
	/// </summary>
    public event PieMenuChangeHandler onChangeEvent;
	
	/// <summary>
	/// Occurs on update event for a particular menu.
	/// </summary>
    public event PieMenuUpdateHandler onUpdateEvent;
	
	/// <summary>
	/// Occurs on update center event for a particular menu.
	/// </summary>
    public event PieMenuUpdateCenterHandler onUpdateCenterEvent;
	
	/// <summary>
	/// Occurs on update item event for a particular menu.
	/// </summary>
    public event PieMenuUpdateItemHandler onUpdateItemEvent;


    // Instance variables configured in PieMenu.json.
	
	
	/// <summary>
	/// The instance data, the JSON that defined this menu.
	/// </summary>
    public Hashtable instanceData = null;

	/// <summary>
	/// The title of this menu, shown in the center when no item is selected.
	/// </summary>
    public string title = "";
	
	/// <summary>
	/// The description of this menu, shown in the center when no item is
    /// selected.
	/// </summary>
    public string description = "";
	
	/// <summary>
	/// The parameters of this menu, any object which the application may use 
    /// for its own purposes.
	/// </summary>
    public object parameters = null;
	
	/// <summary>
	/// The center texture to show in the center of the menu, if not null.
	/// </summary>
    public Texture centerTexture = null;
	
	/// <summary>
	/// The size of the center texture to show in the center of the menu, in 
    /// screen pixel coordinates.
	/// </summary>
    public float centerTextureSize = 100.0f;
	
	/// <summary>
	/// The size of the items to show around the menu, in screen pixel 
    /// coordinates.
	/// </summary>
    public float itemSize = 70.0f;
	
	/// <summary>
	/// The radius of the menu, in screen pixel coordinates.
	/// </summary>
    public float radius = 100.0f;
	
	/// <summary>
	/// The inactive radius of the menu, that doesn't select any items, in 
    /// screen pixel coordinates.
	/// </summary>
    public float inactiveRadius = 10.0f;
	
	/// <summary>
	/// The initial angle in which to place the first item of the menu, 
    /// in degrees.
	/// </summary>
    public float initialAngle = 90.0f;
	
	/// <summary>
	/// The clockwise flag, which controls if the items are layed out from 
    /// the initial angle clockwise or counterclockwise.
	/// </summary>
    public bool clockwise = true;
	
	/// <summary>
	/// The item label justification, how to justify the item labels relative 
    /// to the item anchors (plus the itemLabelExtraRadius).
	/// </summary>
    public ItemLabelJustification itemLabelJustification = ItemLabelJustification.InnerEdge;
	
	/// <summary>
	/// The item label extra radius, how far out from the item anchor to 
    /// justify the item labels, in screen pixel coordinates.
	/// </summary>
    public float itemLabelExtraRadius = 5.0f;
	
	/// <summary>
	/// The duration of the popup animation, in seconds.
	/// </summary>
    public float popupDuration = 0.25f;
	
	/// <summary>
	/// The popout radius minimum, how far out from the menu center the 
    /// cursor must be to start popping the current item out, in screen 
    /// pixel coordinates.
	/// </summary>
    public float popoutRadiusMin = 10.0f;
	
	/// <summary>
	/// The popout radius max, how far out from the menu center the cursor must
    /// be to stop popping the current item out, in screen pixel coordinates.
	/// </summary>
    public float popoutRadiusMax = 400.0f;
	
	/// <summary>
	/// The popout scale, how much to scale the current item when popping it out.
	/// </summary>
    public float popoutScale = 0.6f;
	
	/// <summary>
	/// The popin scale, how much to scale the non-current items when popping 
    /// them in.
	/// </summary>
    public float popinScale = 1.0f;
	
	/// <summary>
	/// The object scale, how much to scale the pie menu objects.
	/// </summary>
    public float objectScale = 1.0f;
	
	/// <summary>
	/// The center scale, how much to scale the pie menu center object.
	/// </summary>
    public float centerScale = 1.0f;
	
	/// <summary>
	/// The pie menu center object.
	/// </summary>
    public PieMenuCenter center = null;
	
	/// <summary>
	/// The default item texture to use for items that don't define their own
    /// texture, or null to not use a texture.
	/// </summary>
    public Texture defaultItemTexture;


    // Dynamic instance variables.

	
	/// <summary>
	/// The subject, what object this pie menu is operating on, set by the 
    /// pie menu target that was clicked on to invoke the pie menu, and 
    /// passed down to sub-menus.
	/// </summary>
    public object subject = null;
	
	/// <summary>
	/// The center x position of the pie menu, in screen pixel coordinates.
	/// </summary>
    public float centerX = 0.0f;
	
	/// <summary>
	/// The center y position of the pie menu, in screen pixel coordinates.
	/// </summary>
    public float centerY = 0.0f;
	
	/// <summary>
	/// The current x position of the cursor, in screen pixel coordinates.
	/// </summary>
    public float currentX = 0.0f;
	
	/// <summary>
	/// The current y position of the cursor, in screen pixel coordinates.
	/// </summary>
    public float currentY = 0.0f;
	
	/// <summary>
	/// Down x position of the cursor when the button was pressed, in screen
    /// pixel coordinates.
	/// </summary>
    public float downX = 0.0f;
	
	/// <summary>
	/// Down y position of the cursor when the button was pressed, in screen
    /// pixel coordinates.
	/// </summary>
    public float downY = 0.0f;
	
	/// <summary>
	/// Up x position of the cursor when the button was released, in screen
    /// pixel coordinates.
	/// </summary>
    public float upX = 0.0f;
	
	/// <summary>
	/// Up y position of thecursor when the button was released, in screen
    /// pixel coordinates.
	/// </summary>
    public float upY = 0.0f;
	
	/// <summary>
	/// The delta x offset of the cursor from the pie menu center, in screen
    /// pixel coordinates.
	/// </summary>
    public float dx = 0.0f;
	
	/// <summary>
	/// The delta y offset of the cursor from the pie menu center, in screen
    /// pixel coordinates.
	/// </summary>
    public float dy = 0.0f;
	
	/// <summary>
	/// The popup time of the pie menu, when it started tracking, in seconds
    /// since the app started.
	/// </summary>
    public float popupTime = 0.0f;
	
	/// <summary>
	/// The tracking flag, to tell if the pie menu is currently tracking 
    /// the mouse.
	/// </summary>
    public bool tracking = false;
	
	/// <summary>
	/// The visible flag, to tell if the pie menu is currently visible on 
    /// the screen.
	/// </summary>
    public bool visible = false;
	
	/// <summary>
	/// The pinned flag, to tell if the pie menu is currently clicked up
    /// without the button released.
	/// </summary>
    public bool pinned = false;
	
	/// <summary>
	/// The editing flag, to tell if the pie menu is currently being edited.
	/// </summary>
    public bool editing = false;
	
	/// <summary>
	/// The item scale, by which the pie menu items are scaled.
	/// </summary>
    public float itemScale = 1.0f;
	
	/// <summary>
	/// The current direction of the cursor from the menu center, in radians, 
    /// from 0 to 2*PI.
	/// </summary>
    public float direction = 0.0f;
	
	/// <summary>
	/// The distance of the cursor from the menu center, in screen pixel 
    /// coordinates.
	/// </summary
    public float distance = 0.0f;

	/// <summary>
	/// The popout scale of the menu, the normalized distance of the 
    /// cursor from popoutRadiusMin to popoutRadiusMax, in the range 
    /// from 0 to 1 inclusive.
	/// </summary>
    [System.NonSerialized]
    public float popout = 0.0f;
	
	/// <summary>
	/// The currently selected pie menu slice.
	/// </summary>
    [System.NonSerialized]
    public PieMenuSlice currentSlice = null;
	
	/// <summary>
	/// The currently seleceted pie menu item.
	/// </summary>
    [System.NonSerialized]
    public PieMenuItem currentItem = null;

	/// <summary>
	/// The currently selected disabled pie menu item.
	/// </summary>
	[System.NonSerialized]
    public PieMenuItem currentDisabledItem = null;
	
	/// <summary>
	/// The last pie menu item selected.
	/// </summary>
	[System.NonSerialized]
    public PieMenuItem lastItem = null;
	
	/// <summary>
	/// The last pie menu slice selected.
	/// </summary>
    [System.NonSerialized]
    public PieMenuSlice lastSlice = null;
	
	/// <summary>
	/// The button used to pop up this pie menu.
	/// </summary>
    public int button;
	
	/// <summary>
	/// The button down flag, true when the button is down.
	/// </summary>
    public bool buttonDown;
	
	/// <summary>
	/// The center visible flag, true when the pie menu center is visible.
	/// </summary>
    public bool centerVisible = false;
	
	/// <summary>
	/// The pie menu slices, which are child GameObjects of the pie menu 
    /// with PieMenuSlice components.
	/// </summary>
    [SerializeField]
    private List<PieMenuSlice> slices;

	/// <summary>
	/// All visible items, that are not currently hidden.
	/// </summary>
    [System.NonSerialized]
    private List<PieMenuItem> allVisibleItems;
	
	/// <summary>
	/// The no visible items updates flag, to inhibit updating the visible 
    /// items, for optimization.
	/// </summary>
    [System.NonSerialized]
    public bool noVisibleItemsUpdates = false;


    ////////////////////////////////////////////////////////////////////////
    // Static methods.

	
	/// <summary>
	/// Creates the pie menu.
	/// </summary>
	/// <returns>
	/// The pie menu.
	/// </returns>
	/// <param name='id'>
	/// Identifier to use for the pie menu.
	/// </param>
	/// <param name='pieMenuJson'>
	/// Pie menu JSON, with which to initialize the pie menu.
	/// </param>
    public static PieMenu CreatePieMenu(string id, Hashtable pieMenuJson)
    {
        PieMenuRoot root =
            PieMenuRoot.GetRoot();

        GameObject obj = 
            new GameObject();
        obj.name = 
            id;
        obj.transform.parent = 
            root.gameObject.transform;
        obj.layer = 
            root.objectLayer;
        PieMenu pieMenu = 
            obj.AddComponent<PieMenu>();

        if (pieMenuJson != null) {
            pieMenu.LoadFromJson("PieMenu", pieMenuJson);
        }

        return pieMenu;
    }

	
	/// <summary>
	/// Draws the text.
	/// </summary>
	/// <param name='text'>
	/// Text to draw, with escaped "\\n" for a newline.
	/// </param>
	/// <param name='x'>
	/// X position of the text to draw, in screen pixel coordinates.
	/// </param>
	/// <param name='y'>
	/// Y position of the text to draw, in screen pixel coordinates.
	/// </param>
	/// <param name='font'>
	/// Font to use for the text.
	/// </param>
	/// <param name='topColor'>
	/// Top color of the text.
	/// </param>
	/// <param name='upperLeftColor'>
	/// Upper left color of the text shadow.
	/// </param>
	/// <param name='lowerRightColor'>
	/// Lower right color of the text shadow.
	/// </param>
    public static void DrawText(
        string text, 
        float x, 
        float y, 
        Font font,
        Color topColor,
        Color upperLeftColor,
        Color lowerRightColor)
    {
        PieMenuRoot root =
            PieMenuRoot.GetRoot();

        //Debug.Log("PieMenu DrawText font " + font + " material " + font.material);

        root.guiSkin.font = font;
        root.guiSkin.button.font = font;
        root.guiSkin.label.font = font;

        float textWidth;
        float textHeight;
        MeasureText(text, font, false, out textWidth, out textHeight);

        float backgroundX = x;
        float backgroundY = y;
        float backgroundWidth =
            textWidth + (2.0f * root.textBackgroundLeftRightMargin);
        float backgroundHeight =
            textHeight + (2.0f * root.textBackgroundTopBottomMargin);

        GUI.DrawTexture(
            new Rect(backgroundX, backgroundY, backgroundWidth, backgroundHeight),
            root.textBackgroundTexture);

        x += root.textBackgroundLeftRightMargin;
        y += root.textBackgroundTopBottomMargin;

        //GUIStyle style = 
        //    new GUIStyle();
        //style.font = 
        //    font;
        GUI.skin.label.font =
            font;

        GUI.skin.label.normal.textColor =
            upperLeftColor;
        GUI.Label(new Rect(x - 1.0f, y - 1.0f, textWidth, textHeight), text);

        GUI.skin.label.normal.textColor =
            lowerRightColor;
        GUI.Label(new Rect(x + 1.0f, y + 1.0f, textWidth, textHeight), text);

        GUI.skin.label.normal.textColor =
            topColor;
        GUI.Label(new Rect(x, y, textWidth, textHeight), text);

    }

	
	/// <summary>
	/// Measures the text.
	/// </summary>
	/// <param name='text'>
	/// Text to draw, with escaped "\\n" for a newline.
	/// </param>
	/// <param name='font'>
	/// Font to use for the text.
	/// </param>
	/// <param name='includeMargin'>
	/// Include margin when measuring the text.
	/// </param>
	/// <param name='width'>
	/// Width output of the measured text, in screen pixel coordinates.
	/// </param>
	/// <param name='height'>
	/// Height output of the measured text, in screen pixel coordinates.
	/// </param>
    public static void MeasureText(
        string text,
        Font font,
        bool includeMargin,
        out float width,
        out float height)
    {
        PieMenuRoot root =
            PieMenuRoot.GetRoot();

        GUI.skin.label.font = font;

        Vector2 size =
            root.guiSkin.label.CalcSize(new GUIContent(text));
        width =
            Mathf.Ceil(size.x);
        height =
            Mathf.Ceil(size.y);

        if (includeMargin) {
            width +=
                2.0f * root.textBackgroundLeftRightMargin;
            height +=
                2.0f * root.textBackgroundTopBottomMargin;
        }
    }

	
	/// <summary>
	/// Normalizes the angle in radians, from 0 to 2*PI.
	/// </summary>
	/// <returns>
	/// The angle in radians. from 0 to 2*PI.
	/// </returns>
	/// <param name='angle'>
	/// Angle, the abnormal angle.
	/// </param>
    public static float NormalizeAngle(float angle)
    {
		// TODO: This is silly -- optimize it.
		
        while (angle < 0) {
            angle += twoPI;
        }

        while (angle >= twoPI) {
            angle -= twoPI;
        }

        return angle;
    }

	
	/// <summary>
	/// Sets the active flag of this GameObject and all its children.
	/// </summary>
	/// <param name='obj'>
	/// The top level GameObject.
	/// </param>
	/// <param name='activeFlag'>
	/// Active flag, false for inactive, true for active.
	/// </param>
    public static void SetActiveAll(GameObject obj, bool activeFlag)
    {
        obj.SetActive(activeFlag);

        foreach (Transform childTransform in obj.transform) {
            PieMenu.SetActiveAll(childTransform.gameObject, activeFlag);
        }
    }

	
	/// <summary>
	/// Returns the name of the direction given an angle in radians.
	/// </summary>
	/// <returns>
	/// The name of the direction.
	/// </returns>
	/// <param name='angle'>
	/// An angle in radians, which does not need to be normalized.
	/// </param>
	/// <param name='sliceCount'>
	/// Slice count, the number of slices of the pie menu. 
	/// If 12, then clock directions are returned, otherwise compass 
    /// directions are returned.
	/// </param>
    public static string DirectionName(float angle, int sliceCount)
    {
        int deg = 
            (int)Mathf.Floor(
                0.5f + (PieMenu.NormalizeAngle(angle) * PieMenu.radToDeg));

        if (sliceCount == 12) {

            switch (deg) {
                case 0: return "3 O'Clock";
                case 30: return "2 O'Clock";
                case 60: return "1 O'Clock";
                case 90: return "12 O'Clock";
                case 120: return "11 O'Clock";
                case 150: return "10 O'Clock";
                case 180: return "9 O'Clock";
                case 210: return "8 O'Clock";
                case 240: return "7 O'Clock";
                case 270: return "6 O'Clock";
                case 300: return "5 O'Clock";
                case 330: return "4 O'Clock";
                default: return deg + " deg";
            }

        } else {

            switch (deg) {
                case 0: return "East";
                case 45: return "NorthEast";
                case 90: return "North";
                case 135: return "NorthWest";
                case 180: return "West";
                case 225: return "SouthWest";
                case 270: return "South";
                case 315: return "SouthEast";
                default: return deg + " deg";
            }

        }

    }

	
	/// <summary>
	/// Gets the pie menu associated with a transform, by walking up until 
    /// finding a GameObject with a PieMenu component.
	/// </summary>
	/// <returns>
	/// The pie menu found.
	/// </returns>
	/// <param name='transform'>
	/// The transform to start searching.
	/// </param>
    public static PieMenu GetPieMenu(Transform transform)
    {
        while (transform != null) {
            PieMenu pieMenu =
                transform.gameObject.GetComponent<PieMenu>();

            if (pieMenu != null) {
                return pieMenu;
            }

            transform =
                transform.parent;
        }

        return null;
    }


    ////////////////////////////////////////////////////////////////////////
    // Instance methods.


    ////////////////////////////////////////////////////////////////////////
    // Loading pie menus from JSON.

	
	/// <summary>
	/// Awake this instance.
	/// </summary>
    public void Awake()
    {
        PieMenuRoot root =
            GetRoot();

        transform.parent =
            root.gameObject.transform;

        HidePieMenu();
        HideCenter();
    }

	
	/// <summary>
	/// Loads the pie menu parameters, slices and items from JSON.
	/// </summary>
	/// <param name='typeName'>
	/// Type name, should be "PieMenu".
	/// </param>
	/// <param name='hashtable'>
	/// Hashtable containing JSON properties.
	/// </param>
    public void LoadFromJson(string typeName, Hashtable hashtable)
    {
        if (!hashtable.Contains("type") ||
            ((string)hashtable["type"] != "PieMenu")) {
            Debug.Log("PieMenu LoadFromJson type " + this.GetType() + " got invalid typeName " + typeName);
            return;
        }

        instanceData = hashtable;

        foreach (string key in hashtable.Keys) {
            object value = hashtable[key];
            LoadKeyValue(key, value);
        }

        HideCenter();
    }

	
	/// <summary>
	/// Loads the key value from a JSON dictionary into the corresponding 
    /// property, converting it to the proper type.
	/// </summary>
	/// <param name='key'>
	/// Key name of the property to load.
	/// </param>
	/// <param name='value'>
	/// Value of the property to load.
	/// </param>
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

            case "centerTexture":
                centerTexture =
                    Model.ConvertToTexture(value);
                break;

            case "centerTextureSize":
                centerTextureSize =
                    Model.ConvertToSingle(value);
                break;

            case "itemSize":
                itemSize =
                    Model.ConvertToSingle(value);
                break;

            case "radius":
                radius =
                    Model.ConvertToSingle(value);
                break;

            case "inactiveRadius":
                inactiveRadius =
                    Model.ConvertToSingle(value);
                break;

            case "initialAngle":
                initialAngle =
                    Model.ConvertToSingle(value);
                break;

            case "clockwise":
                clockwise =
                    Model.ConvertToBool(value);
                break;

            case "itemLabelJustification":
                itemLabelJustification =
                    Model.ConvertToEnum<ItemLabelJustification>(value);
                break;

            case "itemLabelExtraRadius":
                itemLabelExtraRadius =
                    Model.ConvertToSingle(value);
                break;

            case "popupDuration":
                popupDuration =
                    Model.ConvertToSingle(value);
                break;

            case "popoutRadiusMin":
                popoutRadiusMin =
                    Model.ConvertToSingle(value);
                break;

            case "popoutRadiusMax":
                popoutRadiusMax =
                    Model.ConvertToSingle(value);
                break;

            case "popoutScale":
                popoutScale =
                    Model.ConvertToSingle(value);
                break;

            case "popinScale":
                popinScale =
                    Model.ConvertToSingle(value);
                break;

            case "objectScale":
                objectScale =
                    Model.ConvertToSingle(value);
                break;

            case "defaultItemTexture":
                defaultItemTexture =
                    Model.ConvertToTexture(value);
                break;

            case "center":
                center =
                    Model.ConvertToComponent<PieMenuCenter>(value);
                break;

            case "centerScale":
                centerScale =
                    Model.ConvertToSingle(value);
                break;

            case "slices":
                SetSlicesFromJson(
                    Model.ConvertToHashtableArray(value));
                break;

            default:
                Debug.Log("PieMenu LoadKeyValue got unexpected key '" + key + "' value '" + value + "'.");
                break;

        }
    }


    ////////////////////////////////////////////////////////////////////////
    // Parents.


	/// <summary>
	/// Gets the PieMenuRoot of this pie menu.
	/// </summary>
    public PieMenuRoot GetRoot()
    {
        return PieMenuRoot.GetRoot();
    }


    ////////////////////////////////////////////////////////////////////////
    // Slices.
	
	
	/// <summary>
	/// Sets the slices of the pie menu.
	/// </summary>
	/// <param name='slices0'>
	/// Slices0 is the list of slices.
	/// </param>
    public void SetSlices(List<PieMenuSlice> slices0)
    {
        slices = 
            slices0;

        UpdateVisibleItems();
    }

	
	/// <summary>
	/// Gets the slices of the pie menu.
	/// </summary>
	/// <returns>
	/// A list of pie menu slices.
	/// </returns>
    public List<PieMenuSlice> GetSlices()
    {
        if (slices == null) {
            slices = 
               new List<PieMenuSlice>();
        }

        return slices;
    }

	
	/// <summary>
	/// Counts the slices of the pie menu.
	/// </summary>
	/// <returns>
	/// The number of slices in the pie menu.
	/// </returns>
    public int CountSlices()
    {
        return (slices == null) ? 0 : slices.Count;
    }

	
	/// <summary>
	/// Gets the slice at a given index, from 0 to CountSlices()-1.
	/// </summary>
	/// <returns>
	/// The pie menu slice.
	/// </returns>
	/// <param name='i'>
	/// The index i of the slice, from 0 to CountSlices()-1.
	/// </param>
    public PieMenuSlice GetSlice(int i)
    {
        if ((slices == null) || (i < 0) || (i >= slices.Count)) {
            return null;
        }

        return slices[i];
    }


	/// <summary>
	/// Clears the slices of the pie menu.
	/// </summary>
    public void ClearSlices()
    {
        slices.Clear();
    }


	/// <summary>
	/// Adds a slice to the pie menu.
	/// </summary>
	/// <param name='slice'>
	/// Slice.
	/// </param>
    public void AddSlice(PieMenuSlice slice)
    {
        slices.Add(slice);
    }


	/// <summary>
	/// Removes the slice from the pie menu.
	/// </summary>
	/// <param name='slice'>
	/// Slice to remove.
	/// </param>
    public void RemoveSlice(PieMenuSlice slice)
    {
        if (!slices.Contains(slice)) {
            Debug.Log("PieMenu RemoveSlice given somebody else's slice.");
            return;
        }

        slices.Remove(slice);

        UpdateVisibleItems();
    }

	
	/// <summary>
	/// Destroys the slice of the pie menu.
	/// </summary>
	/// <param name='slice'>
	/// Slice to destroy.
	/// </param>
	/// <param name='immediate'>
	/// Immediate flag, if true calls DestroyImmediate, if false
    /// calls Destroy.
	/// </param>
    public void DestroySlice(PieMenuSlice slice, bool immediate)
    {
        RemoveSlice(slice);

        if (immediate) {
            DestroyImmediate(slice.gameObject);
        } else {
            Destroy(slice.gameObject);
        }
    }

	
	/// <summary>
	/// Creates a slice in the pie menu.
	/// </summary>
	/// <returns>
	/// The slice.
	/// </returns>
	/// <param name='obj'>
	/// Object to use as the slice, or null to create a new object.
	/// </param>
    public PieMenuSlice CreateSlice(GameObject obj)
    {
        PieMenuRoot root =
            GetRoot();

        if (obj == null) {
            obj = 
                new GameObject();
            obj.name =
                "PieMenuSlice";
            obj.transform.parent = 
                transform;
            obj.layer =
                root.objectLayer;
        } else {
            obj.transform.parent = 
                transform;
            obj.layer = 
                root.objectLayer;
        }

        PieMenuSlice slice = 
            obj.GetComponent<PieMenuSlice>();
        if (slice == null) {
            slice =
                obj.AddComponent<PieMenuSlice>();
        }

        AddSlice(slice);

        return slice;
    }

	
	/// <summary>
	/// Changes the index of the slice, by moving it from one index to 
    /// another, and scolling all slices in-between.
	/// </summary>
	/// <param name='fromIndex'>
	/// From index of slice to move.
	/// </param>
	/// <param name='toIndex'>
	/// To index to move slice.
	/// </param>
    public void ChangeSliceIndex(int fromIndex, int toIndex)
    {
        if (fromIndex == toIndex) {
            return;
        }

        PieMenuSlice fromSlice = 
            slices[fromIndex];

        if (toIndex < fromIndex) {
            for (int i = fromIndex; i > toIndex; i--) {
                slices[i] =
                    slices[i - 1];
            }
        } else {
            for (int i = fromIndex; i < toIndex; i++) {
                slices[i] =
                    slices[i + 1];
            }
        }

        slices[toIndex] = 
            fromSlice;
    }

	
	/// <summary>
	/// Sets the slices from a JSON array.
	/// </summary>
	/// <param name='jsonArray'>
	/// Json array of dictionaries describing pie menu slices.
	/// </param>
    public void SetSlicesFromJson(Hashtable[] jsonArray)
    {
        bool noVisibleItemsUpdatesLast = 
            noVisibleItemsUpdates;
        noVisibleItemsUpdates = true;

        if (jsonArray == null) {

            slices = 
                new List<PieMenuSlice>();

        } else {

            slices = 
                new List<PieMenuSlice>();

            foreach (Hashtable pieMenuSliceJson in jsonArray) {

                GameObject obj = 
                    null;

                if (pieMenuSliceJson.ContainsKey("gameObjectName")) {
                    obj =
                        Model.ConvertToGameObject(
                            pieMenuSliceJson["gameObjectName"]);
                }

                PieMenuSlice slice = 
                    CreateSlice(obj);

                slice.LoadFromJson(pieMenuSliceJson);
            }
        }

        noVisibleItemsUpdates = 
            noVisibleItemsUpdatesLast;
        UpdateVisibleItems();
    }


	/// <summary>
	/// Updates the visible items of the pie menu.
	/// </summary>
    public void UpdateVisibleItems()
    {
        if (noVisibleItemsUpdates) {
            return;
        }

        if (allVisibleItems == null) {
            allVisibleItems = 
                new List<PieMenuItem>();
        } else {
            allVisibleItems.Clear();
        }

        if (slices == null) {
            slices = 
                new List<PieMenuSlice>();
        }

        foreach (PieMenuSlice slice in slices) {
            if (slice != null) {
                slice.UpdateVisibleItems(allVisibleItems);
            }
        }


        if ((currentItem != null) &&
            (!allVisibleItems.Contains(currentItem))) {
            currentItem = null;
        }

        if ((currentDisabledItem != null) &&
            (!allVisibleItems.Contains(currentDisabledItem))) {
            currentDisabledItem = null;
        }

        if ((lastItem != null) &&
            (!allVisibleItems.Contains(lastItem))) {
            lastItem = null;
        }
    }

	
	/// <summary>
	/// Layout the slices and items of the pie menu.
	/// </summary>
    public void Layout()
    {
        UpdateVisibleItems();


        int sliceCount =
            CountSlices();

        if (sliceCount == 0) {
            return;
        }

        float angle = 
            initialAngle * ((2.0f * Mathf.PI) / 360.0f);
        float turn =
            (sliceCount == 0)
                ? 0.0f
                : ((2.0f * Mathf.PI) / (float)sliceCount);
        if (clockwise) {
            turn = -turn;
        }

        int sliceIndex = 0;
        foreach (PieMenuSlice slice in slices) {
            if (slice != null) {
                slice.Layout(sliceIndex, PieMenu.NormalizeAngle(angle));
            }
            sliceIndex++;
            angle += turn;
        }

        if (title == null) {
            title = "";
        }

        if (description == null) {
            description = "";
        }
    }


	/// <summary>
	/// Finds the slice of the pie menu with a given id.
	/// </summary>
	/// <returns>
	/// The slice found, or null.
	/// </returns>
	/// <param name='sliceId'>
	/// The ID of the slice to find.
	/// </param>
    public PieMenuSlice FindSlice(string sliceId)
    {
        foreach (PieMenuSlice slice in slices) {
            if (slice.id == sliceId) {
                return slice;
            }
        }

        return null;
    }


    ////////////////////////////////////////////////////////////////////////
    // Input tracking.

	
	/// <summary>
	/// Tracks the camera, by moving the pie menu to the appropriate 
    /// position.
	/// </summary>
    public void TrackCamera()
    {
        PieMenuRoot root =
            GetRoot();

        float cameraCenterX = 
            root.pieMenuCamera.pixelWidth / 2.0f;
        float cameraCenterY = 
            root.pieMenuCamera.pixelHeight / 2.0f;

        Vector3 pieMenuScreenCenter =
            new Vector3(
                cameraCenterX,
                cameraCenterY,
                root.cameraDistance);
        Vector3 pieMenuWorldCenter =
            root.pieMenuCamera.ScreenToWorldPoint(
                pieMenuScreenCenter);

        Quaternion trackRotation =
            Quaternion.identity;

        transform.position =
            pieMenuWorldCenter;
        transform.rotation =
            root.pieMenuCamera.transform.rotation *
            Quaternion.AngleAxis(90.0f, Vector3.left);

    }

	
	/// <summary>
	/// Pops up the pie menu.
	/// </summary>
	/// <param name='button0'>
	/// Button that invoked the pie menu.
	/// </param>
	/// <param name='pinned0'>
	/// Pinned flag, false if the button is down, true if the button is up.
	/// </param>
	/// <param name='subject0'>
	/// Subject of the pie menu, the object that it applies to, used by the
    /// application, passed to submenus.
	/// </param>
    public virtual void Popup(int button0, bool pinned0, object subject0)
    {
        PieMenuRoot root =
            GetRoot();

        root.StartTracking(this);

        button = 
            button0;
        pinned = 
            pinned0;
        subject = 
            subject0;

        Vector3 mousePosition = 
            Input.mousePosition;
        float x = 
            mousePosition.x;
        float y = 
            mousePosition.y;

        downX = x;
        downY = y;
        centerX = x;
        centerY = y;
        buttonDown = !pinned;
        currentSlice = null;
        lastSlice = null;
        currentItem = null;
        currentDisabledItem = null;
        lastItem = null;
        direction = 0.0f;
        distance = 0.0f;
        popupTime = Time.time;
        tracking = true;
        visible = true;

        ResetItems();

        if (!pinned) {
            ReportDownEvent();
        }

        ReportStartEvent();

        if (pinned) {
            ReportPinnedEvent();
        }

        ShowPieMenu();

        TrackCamera();
    }

	
	/// <summary>
	/// Tracks the cursor movement event.
	/// </summary>
    virtual public void TrackMove()
    {
        if (!visible) {
            return;
        }

        TrackCamera();

        Vector3 mousePosition = 
            Input.mousePosition;
        currentX = 
            mousePosition.x;
        currentY = 
            mousePosition.y;
        dx = 
            currentX - centerX;
        dy = 
            currentY - centerY;

        distance =
            Mathf.Sqrt(
                (dx * dx) + 
                (dy * dy));

        if (distance == 0.0f) {
            direction = 
                0.0f;
        } else {
            direction = 
                PieMenu.NormalizeAngle(Mathf.Atan2(dy, dx));
        }

        if (distance < popoutRadiusMin) {
            popout = 
                0.0f;
        } else if (distance < popoutRadiusMax) {
            popout =
                (distance - popoutRadiusMin) / 
                (popoutRadiusMax - popoutRadiusMin);
        } else {
            popout = 
                1.0f;
        }

        int sliceCount = 
            CountSlices();

        if ((distance < inactiveRadius) ||
            (sliceCount == 0)) {

            currentSlice = null;
            currentItem = null;
            currentDisabledItem = null;

        } else {

            float turn =
                twoPI / sliceCount;
            float initialRadians =
                initialAngle * degToRad;
            float sliceAngle =
                PieMenu.NormalizeAngle(
                    (clockwise
                        ? (initialRadians - direction)
                        : (direction - initialRadians)) +
                    (turn * 0.5f));
            int sliceIndex =
                (int)Math.Floor(sliceAngle * sliceCount / twoPI);

            currentSlice = 
                slices[sliceIndex];

            // TODO: Hit test against labels of all items in all slices first.
            PieMenuItem item =
                currentSlice.TrackItem();

            if (item == null) {
                currentItem = null;
                currentDisabledItem = null;
            } else if (item.disabled) {
                currentItem = null;
                currentDisabledItem = item;
            } else {
                currentItem = item;
                currentDisabledItem = null;
            }

        }

        if (currentItem != lastItem) {
            ReportChangeEvent();
        }

        ReportUpdateEvent();

        lastSlice = 
            currentSlice;
        lastItem = 
            currentItem;
    }

	
	/// <summary>
	/// Tracks the button up event.
	/// </summary>
    virtual public void TrackUp()
    {
        buttonDown = false;

        ReportUpEvent();

        TrackMove();

        upX = 
            currentX;
        upY = 
            currentY;

        if (currentItem == null) {

            if (pinned || (currentDisabledItem != null)) {
                TrackCancel();
            } else {
                pinned = true;
                ReportPinnedEvent();
            }

        } else {

            TrackStop();
            SelectItem();

        }

    }

	
	/// <summary>
	/// Tracks the button down event.
	/// </summary>
    virtual public void TrackDown()
    {
        buttonDown = true;

        ReportDownEvent();

        TrackMove();
    }

	
	/// <summary>
	/// Stops tracking.
	/// </summary>
    virtual public void TrackStop()
    {
        PieMenuRoot root =
            GetRoot();

        root.CancelTracking(this);

        visible = false;
        tracking = false;
        pinned = false;

        HidePieMenu();
        ReportEndEvent();
    }

	
	/// <summary>
	/// Cancels tracking.
	/// </summary>
    virtual public void TrackCancel()
    {
        TrackStop();
        ReportCancelEvent();
    }

	
	/// <summary>
	/// Resets the pie menu items.
	/// </summary>
    public void ResetItems()
    {
        if (slices == null) {
            return;
        }

        foreach (PieMenuSlice slice in slices) {
            slice.ResetItems();
        }
    }

	
	/// <summary>
	/// Shows the pie menu.
	/// </summary>
    public virtual void ShowPieMenu()
    {
        PieMenuRoot root =
            GetRoot();

        PieMenu.SetActiveAll(gameObject, true);

        Vector3 screenPosition =
            new Vector3(centerX, centerY, root.pieMenuCamera.nearClipPlane);

        Ray ray =
            root.pieMenuCamera.ScreenPointToRay(
                screenPosition);

        //Debug.DrawRay(ray.origin, ray.direction * 10.0f, Color.green);

        Vector3 worldPosition =
            ray.GetPoint(
                root.objectDistance);

        transform.position =
            worldPosition;

        if (slices != null) {
            foreach (PieMenuSlice slice in slices) {
                slice.ShowPieMenuSlice();
            }
        }

        centerVisible = true;
    }

	
	/// <summary>
	/// Hides the pie menu.
	/// </summary>
    public virtual void HidePieMenu()
    {
        PieMenu.SetActiveAll(gameObject, false);

        if (slices != null) {
            foreach (PieMenuSlice slice in slices) {
                slice.HidePieMenuSlice();
            }
        }

        centerVisible = false;
        UpdateGameObject();
    }

	
	/// <summary>
	/// Updates the game object.
	/// </summary>
    public void UpdateGameObject()
    {
        if (center == null) {
            return;
        }

        if (centerVisible) {
            ShowCenter();
        } else {
            HideCenter();
        }
    }

	
	/// <summary>
	/// Shows the pie menu center.
	/// </summary>
    private void ShowCenter()
    {
        if (center == null) {
            return;
        }

        PieMenuRoot root =
            GetRoot();

        PieMenu.SetActiveAll(center.gameObject, true);

        center.gameObject.layer =
            root.objectLayer;

        Vector3 screenPosition =
            new Vector3(centerX, centerY, root.pieMenuCamera.nearClipPlane);

        Ray ray =
            root.pieMenuCamera.ScreenPointToRay(
                screenPosition);

        //Debug.DrawRay(ray.origin, ray.direction * 10.0f, Color.blue);

        Vector3 worldPosition =
            ray.GetPoint(
                root.objectDistance);

        float scale =
            centerScale * 
            objectScale * 
            root.itemObjectScale * 
            root.itemObjectCameraScale;

        center.gameObject.transform.position =
            worldPosition;
        center.gameObject.transform.localScale =
            new Vector3(scale, scale, scale);

        ReportUpdateCenter();
    }

	
	/// <summary>
	/// Hides the pie menu center.
	/// </summary>
    private void HideCenter()
    {
        PieMenuRoot root =
            GetRoot();

        if (center == null) {
            return;
        }

        PieMenu.SetActiveAll(center.gameObject, false);

        center.gameObject.layer =
            root.objectLayer;
    }

	
	/// <summary>
	/// Selects the current pie menu item.
	/// </summary>
    public virtual void SelectItem()
    {
        if (currentItem == null) {
            return;
        }

        PieMenuRoot root =
            GetRoot();

        if ((currentItem.subMenu != null) && (currentItem.subMenu != "")) {
            PieMenu subMenu =
                root.FindPieMenu(
                    currentItem.subMenu);
            if (subMenu == null) {
                Debug.Log("PieMenu SelectItem id " + currentItem.subMenu + " is not defined.");
            } else {

                // Do we want to use this item as the center of the submenu?
                if (currentItem.subMenuCenter) {
                    PieMenuCenter center =
                        currentItem.gameObject.GetComponent<PieMenuCenter>();
                    if (center != null) {
                        subMenu.center = center;
                    }

                }

                ReportSubMenuEvent(subMenu);
                subMenu.Popup(button, true, subject);
            }

        } else {

            ReportSelectEvent();

        }
    }

	
	/// <summary>
	/// Draws the user interface overlay.
	/// </summary>
    public void OnGUI()
    {
        if (!visible) {
            return;
        }

        PieMenuRoot root =
            GetRoot();

        GUISkin lastSkin =  GUI.skin;
        GUI.skin = root.guiSkin;

        //Debug.Log("PieMenu OnGUI GUI.skin " + GUI.skin + " font " + GUI.skin.font + " label " + GUI.skin.label + " font " + GUI.skin.label.font + " size " + GUI.skin.label.fontSize);

        itemScale = 
            Mathf.Min(
                1.0f,
                ((Time.time - popupTime) /
                 popupDuration));

        Layout();

        if (centerTexture != null) {
            float size =
                centerTextureSize;
            float imageAspect =
                ((float)centerTexture.width / (float)centerTexture.height);

            float width;
            float height;

            if (imageAspect < 1.0f) {
                width =
                    size * imageAspect;
                height =
                    size;
            } else {
                width =
                    size;
                height =
                    size / imageAspect;
            }

            float iconX =
                centerX - (width / 2.0f);
            float iconY = 
                centerY - (height / 2.0f);

            iconY =
                Screen.height - iconY - height;

            GUI.Label(
                new Rect(iconX, iconY, width, height),
                centerTexture);

        }

        if (slices != null) {
            foreach (PieMenuSlice slice in slices) {
                slice.Draw();
            }
        }

        if (currentItem != null) {

            // Draw the current item last.
            currentItem.Draw();

        } else if (currentDisabledItem != null) {

            // Let the item draw its disabled state.
            currentDisabledItem.Draw();

        } else {

            // No current or current disabled item, so draw the menu
            // title and description.

            bool drawTitle = title != "";
            string titleCooked = "";
            float titleX = 0.0f;
            float titleY = 0.0f;
            float titleWidth = 0.0f;
            float titleHeight = 0.0f;

            bool drawDescription = description != "";
            string descriptionCooked = "";
            float descriptionX = 0.0f;
            float descriptionY = 0.0f;
            float descriptionWidth = 0.0f;
            float descriptionHeight = 0.0f;

            if (drawTitle) {

                titleCooked =
                    title.Replace("\\n", "\n");

                PieMenu.MeasureText(
                    titleCooked,
                    root.titleFont,
                    true,
                    out titleWidth,
                    out titleHeight);

                titleX = 
                    Mathf.Max(
                        0.0f,
                        Mathf.Min(
                            Screen.width - titleWidth,
                            Mathf.Floor(
                                0.5f + 
                                Input.mousePosition.x + 
                                root.titleOffsetX - 
                                (titleWidth / 2.0f))));
                titleY =
                    Mathf.Max(
                        titleHeight,
                        Mathf.Min(
                            Screen.height,
                            Mathf.Floor(
                                0.5f +
                                Input.mousePosition.y +
                                root.titleOffsetY +
                                titleHeight)));
            }

            if (drawDescription) {

                descriptionCooked =
                    description.Replace("\\n", "\n");

                PieMenu.MeasureText(
                    descriptionCooked,
                    root.descriptionFont,
                    true,
                    out descriptionWidth,
                    out descriptionHeight);

                descriptionX = 
                    Mathf.Max(
                        0.0f,
                        Mathf.Min(
                            Screen.width - descriptionWidth,
                            Mathf.Floor(
                                0.5f +
                                Input.mousePosition.x +
                                root.descriptionOffsetX -
                                (descriptionWidth / 2.0f))));
                descriptionY =
                    Mathf.Max(
                        descriptionHeight,
                        Mathf.Min(
                            Screen.height - descriptionHeight,
                            Mathf.Floor(
                                0.5f +
                                Input.mousePosition.y +
                                root.descriptionOffsetY)));

                if (drawTitle) {
                    if (titleY - titleHeight < descriptionY) {
                        if (descriptionY - descriptionHeight == 0.0f) {
                            titleY = 
                                descriptionY + titleHeight;
                        } else {
                            descriptionY = 
                                titleY - titleHeight;
                        }
                    }
                }
            }

            if (drawTitle) {
                PieMenu.DrawText(
                    titleCooked,
                    titleX,
                    Screen.height - titleY,
                    root.titleFont,
                    root.titleTopColor,
                    root.titleUpperLeftColor,
                    root.titleLowerRightColor);
            }

            if (drawDescription) {
                PieMenu.DrawText(
                    descriptionCooked,
                    descriptionX,
                    Screen.height - descriptionY,
                    root.descriptionFont,
                    root.descriptionTopColor,
                    root.descriptionUpperLeftColor,
                    root.descriptionLowerRightColor);
            }

        }

        GUI.skin = lastSkin;

        UpdateGameObject();

    }


    [ContextMenu("Get Slices from Sub-Objects")]
    public void GetSlicesFromSubObjects()
    {
        PieMenuRoot root =
            GetRoot();

        List<PieMenuSlice> newSlices =
            new List<PieMenuSlice>();

        foreach (Transform childTransform in transform) {

            PieMenuSlice foundSlice =
                childTransform.gameObject.GetComponent<PieMenuSlice>();

            PieMenuCenter foundCenter =
                childTransform.gameObject.GetComponent<PieMenuCenter>();

            int foundCenters = 0;
            int foundSlices = 0;

            if ((foundSlice != null) && (foundCenter != null)) {

                Debug.Log("PieMenu GetSlicesFromSubObjects found an object that has both a PieMenuSlice component and a PieMenuCenter component! That doesn't make sense. Ignoring the object named " + childTransform.gameObject.name + ".");
                continue;

            } else if (foundCenter != null) {

                // Found an object that's already a PieMenuCenter.

                if (foundCenters > 0) {
                    Debug.Log("PieMenu GetSlicesFromSubObjects found an extra PieMenuCenter component that's different than the pie menu's centerGameObject! Ignoring the extra named " + childTransform.gameObject.name + ".");
                    continue;
                }

                // Deal with the center below.

                foundCenters++;

            } else if (foundSlice != null) {

                // Found an object that's already a PieMenuSlice.
                // Deal with the slice below.

                foundSlices++;

            } else {

                // Found an object that doesn't already have a PieMenu related component (PieMenuSlice or PieMenuCenter).
                // Figure out what to do with it: make it an slice or a center, depending on its distance from the center.

                float dx =
                    childTransform.localPosition.x;
                float dy =
                    childTransform.localPosition.x;
                float distance =
                    Mathf.Sqrt((dx * dx) + (dy * dy));

                if (distance < (inactiveRadius / root.worldToScreenScale)) {

                    // Found an object in a center to make into a PieMenuCenter.

                    if (foundCenter) {
                        Debug.Log("PieMenu GetSlicesFromSubObjects found more than one object in the middle to add a PieMenuCenter component! Ignoring the extra named " + childTransform.gameObject.name + ".");
                        continue;
                    }

                    // Make it a center, and deal with the center below.

                    childTransform.gameObject.layer =
                        root.objectLayer;
                    foundCenter =
                        childTransform.gameObject.AddComponent<PieMenuCenter>();
                    foundCenters++;

                } else {

                    // Found an object outside of the center to make into a PieMenuSlice.
                    // Make it an slice, and deal with the slice below.

                    childTransform.gameObject.layer =
                        root.objectLayer;
                    foundSlice =
                        childTransform.gameObject.AddComponent<PieMenuSlice>();
                    foundSlices++;

                }

            }

            if (foundSlice != null) {

                // Deal with a slice: Set its id to the game object name, and collect it.

                foundSlice.id =
                    childTransform.gameObject.name;
                newSlices.Add(foundSlice);

            } else if (center != null) {

                // Deal with a center: Set the pie menu's center.

                if ((center != null) &&
                    (center != foundCenter)) {
                    Debug.Log("PieMenu GetSlicesFromSubObjects found a PieMenuCenter component that's different than the pie menu's center! Overwriting the old one named " + center.name + " and replacing it with the one named " + childTransform.gameObject.name + ".");
                }

                center =
                    foundCenter;

            }

        }

        ClearSlices();

        foreach (PieMenuSlice slice in newSlices) {
            float dx =
                slice.gameObject.transform.localPosition.x;
            float dy =
                slice.gameObject.transform.localPosition.z;
            float dist =
                Mathf.Sqrt((dx * dx) + (dy * dy));

            if (dist == 0) {
                dy = 1.0f;
            } else {
                dx /= dist;
                dy /= dist;
            }

            slice.dx = dx;
            slice.dy = dy;
        }

        var sliceCount =
            newSlices.Count;
        float angle =
            initialAngle * ((2.0f * Mathf.PI) / 360.0f);
        float turn =
            (2.0f * Mathf.PI) / (float)sliceCount;
        if (clockwise) {
            turn = -turn;
        }

        for (int i = 0; i < sliceCount; i++) {
            float dx =
                Mathf.Cos(angle);
            float dy =
                Mathf.Sin(angle);
            float bestDot = -1.0f;
            PieMenuSlice bestSlice = null;

            foreach (PieMenuSlice slice in newSlices) {
                if (slice == null) {
                    continue;
                }

                float dot =
                    (dx * slice.dx) + (dy * slice.dy);
                if (dot > bestDot) {
                    bestDot = dot;
                    bestSlice = slice;
                }
            }

            AddSlice(bestSlice);
            newSlices.Remove(bestSlice);

            bestSlice.GetItemsFromSubObjects();

            angle += turn;
        }

        UpdateVisibleItems();

        LayoutSliceSubObjects();
    }


    [ContextMenu("Layout Slice Sub-Objects")]
    public void LayoutSliceSubObjects()
    {
        PieMenuRoot root =
            GetRoot();

        int sliceCount =
            CountSlices();

        Layout();

        if (center != null) {
            center.transform.localPosition =
                new Vector3(0.0f, 0.0f, 0.0f);
        }

        for (int i = 0; i < sliceCount; i++) {
            PieMenuSlice slice =
                GetSlice(i);

            if (slice == null) {
                continue;
            }

            float x =
                slice.dx * radius / root.worldToScreenScale;
            float y =
                slice.dy * radius / root.worldToScreenScale;
            slice.gameObject.transform.localPosition =
                new Vector3(x, 0.0f, y);

            slice.LayoutItemSubObjects();
        }

    }


    ////////////////////////////////////////////////////////////////////////
    // Event reporting.

	
	/// <summary>
	/// Reports the down event.
	/// </summary>
    virtual public void ReportDownEvent()
    {
        if (PieMenu.onGlobalDownEvent != null) {
            PieMenu.onGlobalDownEvent(this);
        }
        if (onDownEvent != null) {
            onDownEvent(this);
        }
    }

	
	/// <summary>
	/// Reports the up event.
	/// </summary>
    virtual public void ReportUpEvent()
    {
        if (PieMenu.onGlobalUpEvent != null) {
            PieMenu.onGlobalUpEvent(this);
        }
        if (onUpEvent != null) {
            onUpEvent(this);
        }
    }

	
	/// <summary>
	/// Reports the start event.
	/// </summary>
    virtual public void ReportStartEvent()
    {
        if (PieMenu.onGlobalStartEvent != null) {
            PieMenu.onGlobalStartEvent(this);
        }
        if (onStartEvent != null) {
            onStartEvent(this);
        }
    }

	
	/// <summary>
	/// Reports the end event.
	/// </summary>
    virtual public void ReportEndEvent()
    {
        if (PieMenu.onGlobalEndEvent != null) {
            PieMenu.onGlobalEndEvent(this);
        }
        if (onEndEvent != null) {
            onEndEvent(this);
        }
    }

	
	/// <summary>
	/// Reports the cancel event.
	/// </summary>
    virtual public void ReportCancelEvent()
    {
        if (PieMenu.onGlobalCancelEvent != null) {
            PieMenu.onGlobalCancelEvent(this);
        }
        if (onCancelEvent != null) {
            onCancelEvent(this);
        }
    }


	/// <summary>
	/// Reports the pinned event.
	/// </summary>
    virtual public void ReportPinnedEvent()
    {
        if (PieMenu.onGlobalPinnedEvent != null) {
            PieMenu.onGlobalPinnedEvent(this);
        }
        if (onPinnedEvent != null) {
            onPinnedEvent(this);
        }
    }

	
	/// <summary>
	/// Reports the select event.
	/// </summary>
    virtual public void ReportSelectEvent()
    {
        if (PieMenu.onGlobalSelectEvent != null) {
            PieMenu.onGlobalSelectEvent(this, currentItem, direction, distance);
        }
        if (onSelectEvent != null) {
            onSelectEvent(this, currentItem, direction, distance);
        }
    }

	
	/// <summary>
	/// Reports the sub menu event.
	/// </summary>
	/// <param name='subMenu'>
	/// Sub menu.
	/// </param>
    virtual public void ReportSubMenuEvent(PieMenu subMenu)
    {
        if (PieMenu.onGlobalSubMenuEvent != null) {
            PieMenu.onGlobalSubMenuEvent(this, currentItem, subMenu);
        }
        if (onSubMenuEvent != null) {
            onSubMenuEvent(this, currentItem, subMenu);
        }
    }

	
	/// <summary>
	/// Reports the change event.
	/// </summary>
    virtual public void ReportChangeEvent()
    {
        if (PieMenu.onGlobalChangeEvent != null) {
            PieMenu.onGlobalChangeEvent(this, currentItem, direction, distance);
        }
        if (onChangeEvent != null) {
            onChangeEvent(this, currentItem, direction, distance);
        }
    }

	
	/// <summary>
	/// Reports the update event.
	/// </summary>
    virtual public void ReportUpdateEvent()
    {
        if (PieMenu.onGlobalUpdateEvent != null) {
            PieMenu.onGlobalUpdateEvent(this, currentItem, direction, distance);
        }
        if (onUpdateEvent != null) {
            onUpdateEvent(this, currentItem, direction, distance);
        }
    }

	
	/// <summary>
	/// Reports the update center event.
	/// </summary>
    virtual public void ReportUpdateCenter()
    {
        if (PieMenu.onGlobalUpdateCenterEvent != null) {
            PieMenu.onGlobalUpdateCenterEvent(this, center);
        }
        if (onUpdateCenterEvent != null) {
            onUpdateCenterEvent(this, center);
        }
    }

	
	/// <summary>
	/// Reports the update item event.
	/// </summary>
	/// <param name='item'>
	/// Pie menu item.
	/// </param>
    virtual public void ReportUpdateItem(PieMenuItem item)
    {
        if (PieMenu.onGlobalUpdateItemEvent != null) {
            PieMenu.onGlobalUpdateItemEvent(this, item);
        }
        if (onUpdateItemEvent != null) {
            onUpdateItemEvent(this, item);
        }
    }


}


////////////////////////////////////////////////////////////////////////
