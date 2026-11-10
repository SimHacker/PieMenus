////////////////////////////////////////////////////////////////////////
// PieMenuItem.cs
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
using System.Collections.Generic;


////////////////////////////////////////////////////////////////////////


[System.Serializable]
public class PieMenuItem : MonoBehaviour {

    // Instance variables configured in PieMenu.json.

    public Hashtable instanceData;
    public string id;
    public string title;
    public string description;
    public string textureName;
    public Texture texture;
    public string gameObjectName;
    public string subMenu;
    public object parameters;
    public bool hidden;
    public bool disabled;
    public string disabledReason;
    public bool subMenuCenter;


    // Instance variables initialized by constructor.

    public int sliceIndex = 0;
    public int index = 0;


    // Dynamic instance variables.

    public Vector3 basePosition;
    public Quaternion baseRotation;
    public Vector3 baseScale;
    public float x = 0.0f;
    public float y = 0.0f;
    public float distance = 0.0f;
    public float scale = 0.0f;
    public float xGoal = 0.0f;
    public float yGoal = 0.0f;
    public float scaleGoal = 0.0f;
    public float pinX = 0.0f;
    public float pinY = 0.0f;
    public bool gameObjectVisible = false;


    ////////////////////////////////////////////////////////////////////////
    // Class methods.


    public static int CompareItemsByDistance(PieMenuItem item1, PieMenuItem item2)
    {
        return item1.distance.CompareTo(item2.distance);
    }


    ////////////////////////////////////////////////////////////////////////
    // Instance methods.


    ////////////////////////////////////////////////////////////////////////
    // Loading and saving pie menu items as JSON.


    public void LoadFromJson(Hashtable hashtable)
    {
        instanceData = hashtable;

        foreach (string key in hashtable.Keys) {
            object value = hashtable[key];
            LoadKeyValue(key, value);
        }

        HideGameObject();
    }


    public void LoadKeyValue(string key, object value)
    {
        switch (key) {

            case "id": id = Model.ConvertToString(value); break;
            case "title": title = Model.ConvertToString(value); break;
            case "description": description = Model.ConvertToString(value); break;
            case "texture": textureName = Model.ConvertToString(value); texture = Model.ConvertToTexture(value); break;
            case "subMenu": subMenu = Model.ConvertToString(value); break;
            case "parameters": parameters = value; break;
            case "hidden": hidden = Model.ConvertToBool(value); break;
            case "disabled": disabled = Model.ConvertToBool(value); break;
            case "disabledReason": disabledReason = Model.ConvertToString(value); break;
            case "subMenuCenter": subMenuCenter = Model.ConvertToBool(value); break;

            default: Debug.Log("PieMenuItem LoadKeyValue got unexpected key '" + key + "' value '" + value + "'."); break;

        }
    }


    public Hashtable SaveToJson()
    {
        if (instanceData == null) {
            instanceData = new Hashtable();
        }

        instanceData["id"] = id;
        instanceData["title"] = title;
        instanceData["description"] = description;
        instanceData["texture"] = textureName;
        instanceData["subMenu"] = subMenu;
        instanceData["parameters"] = parameters;
        instanceData["hidden"] = hidden;
        instanceData["disabled"] = disabled;
        instanceData["disabledReason"] = disabledReason;

        return instanceData;
    }


    ////////////////////////////////////////////////////////////////////////
    // Initialization.

    
    public void Start()
    {
        basePosition = transform.localPosition;
        baseRotation = transform.localRotation;
        baseScale = transform.localScale;
    }


    ////////////////////////////////////////////////////////////////////////
    // Parents.


	/// <summary>
	/// Gets the PieMenuSlice of this PieMenuItem.
	/// </summary>
    public PieMenuSlice GetSlice()
    {
        return PieMenuSlice.GetSlice(transform);
    }


	/// <summary>
	/// Gets the PieMenu of this PieMenuItem.
	/// </summary>
    public PieMenu GetPieMenu()
    {
        return PieMenu.GetPieMenu(transform);
    }


	/// <summary>
	/// Gets the PieMenuRoot of this PieMenuItem.
	/// </summary>
    public PieMenuRoot GetRoot()
    {
        return PieMenuRoot.GetRoot();
    }


    ////////////////////////////////////////////////////////////////////////
    // Items.

    public void ResetItem()
    {
        x = 0.0f;
        y = 0.0f;
        scale = 0.0f;
        yGoal = 0.0f;
        xGoal = 0.0f;
        scaleGoal = 0.0f;
    }


    public void Layout(int sliceIndex0, int index0)
    {
        sliceIndex = sliceIndex0;
        index = index0;

        if (title == null) {
            title = "";
        }

        if (description == null) {
            description = "";
        }

        if (disabledReason == null) {
            disabledReason = "";
        }
    }


    public void Draw()
    {
        PieMenuSlice slice =
            GetSlice();
        PieMenu pieMenu =
            GetPieMenu();
        PieMenuRoot root = 
            GetRoot();

        bool current =
            pieMenu.currentItem == this;
        bool currentDisabled =
            pieMenu.currentDisabledItem == this;

        float itemRadius =
            pieMenu.radius + (index * slice.distancePerItem);

        if (current) {

            scaleGoal = pieMenu.itemScale * (1.0f + (pieMenu.popout * pieMenu.popoutScale));
            Vector3 mousePosition = Input.mousePosition;
            xGoal = mousePosition.x - pieMenu.centerX;
            yGoal = mousePosition.y - pieMenu.centerY;

        } else if (currentDisabled) {

            scaleGoal = pieMenu.itemScale;
            float curRadius = pieMenu.itemScale * itemRadius * scaleGoal;
            xGoal = (curRadius * slice.dx);
            yGoal = (curRadius * slice.dy);

        } else {

            scaleGoal = pieMenu.itemScale * (1.0f - (pieMenu.popout * pieMenu.popinScale));
            float curRadius = pieMenu.itemScale * itemRadius * scaleGoal;
            xGoal = (curRadius * slice.dx);
            yGoal = (curRadius * slice.dy);

        }

        float dscale = scaleGoal - scale;
        float xChange = xGoal - x;
        float yChange = yGoal - y;
        float dist = Mathf.Sqrt((xChange * xChange) + (yChange * yChange));

        if (dist < root.animationMinDistance) {

            scale = scaleGoal;
            x = xGoal;
            y = yGoal;

        } else {

            float speedToFinish = dist / Time.deltaTime;

            if (speedToFinish < root.animationMaxMoveSpeed) {

                scale = scaleGoal;
                x = xGoal;
                y = yGoal;

            } else {

                float speedToKeepFromGettingBored = dist / root.animationMaxMoveTime;
                float goalSpeed = Mathf.Max(speedToKeepFromGettingBored, root.animationMaxMoveSpeed);
                float speedScale = goalSpeed / speedToFinish;
                
                scale += dscale * speedScale;
                x += xChange * speedScale;
                y += yChange * speedScale;

            }

        }

        pinX = pieMenu.centerX + x;
        pinY = pieMenu.centerY + y;

        //Vector3 screenPosition =
        //    new Vector3(pieMenu.centerX, pieMenu.centerY, root.pieMenuCamera.nearClipPlane);
        //Vector3 worldPosition =
        //    root.pieMenuCamera.ScreenToWorldPoint(screenPosition);
        //Debug.DrawLine(worldPosition, new Vector3(worldPosition.x, 0.0f, worldPosition.z), Color.green);

        bool drawTexture =
            texture != null;
        bool drawTitle =
            (title != "") &&
            (current ||
             currentDisabled ||
             ((pieMenu.currentItem == null) && (pieMenu.currentDisabledItem == null)));
        bool drawDescription =
            (description != "") && current;
        bool drawDisabled =
            (disabledReason != "") && currentDisabled;
        float size =
            pieMenu.itemSize * scale;
        float screenX =
            Mathf.Floor(pinX + 0.5f);
        float screenY =
            Mathf.Floor(pinY + 0.5f);

        if (drawTexture) {

            float width;
            float height;
            float imageAspect =
                ((float)texture.width / (float)texture.height);
            if (imageAspect < 1.0f) {
                width = size * imageAspect;
                height = size;
            } else {
                width = size;
                height = size / imageAspect;
            }

            float iconX = screenX - Mathf.Floor(width / 2.0f);
            float iconY = screenY - Mathf.Floor(height / 2.0f) + height;

            GUI.Label(
                new Rect(
                    iconX, 
                    Screen.height - iconY,
                    width, 
                    height),
                texture);

        }

        float extraX = pieMenu.itemLabelExtraRadius * slice.dx * pieMenu.itemScale;
        float extraY = pieMenu.itemLabelExtraRadius * slice.dy * pieMenu.itemScale;
        string titleCooked = "";
        float titleX = Mathf.Floor(0.5f + screenX + extraX);
        float titleY = Mathf.Floor(0.5f + screenY + extraY);
        float titleWidth = 0.0f;
        float titleHeight = 0.0f;
        string descriptionCooked = "";
        float descriptionX = Mathf.Floor(0.5f + screenX + extraX);
        float descriptionY = Mathf.Floor(0.5f + screenY + extraY);
        float descriptionWidth = 0.0f;
        float descriptionHeight = 0.0f;

        if (drawTitle) {

            titleCooked =
                title.Replace("\\n", "\n");

            PieMenu.MeasureText(
                titleCooked, 
                disabled ? root.itemTitleDisabledFont : root.itemTitleFont,
                true,
                out titleWidth,
                out titleHeight);

            float epsilon  = 0.05f;

            switch (pieMenu.itemLabelJustification) {

                case PieMenu.ItemLabelJustification.InnerEdge:

                    titleY += titleHeight;

                    if ((slice.dx >= -epsilon) && (slice.dx <= epsilon)) {
                        titleX -= Mathf.Floor(titleWidth / 2.0f);
                    } else if (slice.dx < 0.0f) {
                        titleX -= titleWidth;
                    } else {
                    }

                    if ((slice.dy >= -epsilon) && (slice.dy <= epsilon)) {
                        titleY -= Mathf.Floor(titleHeight / 2.0f);
                        descriptionY = titleY - titleHeight;
                    } else if (slice.dy < 0.0f) {
                        titleY -= titleHeight;
                        descriptionY = titleY - titleHeight;
                    } else {
                    }

                    break;

                case PieMenu.ItemLabelJustification.Center:

                    titleY += Mathf.Floor(titleHeight / 2.0f);
                    titleX -= Mathf.Floor(titleWidth / 2.0f);
                    descriptionY = titleY - titleHeight;

                    break;

            }

            //Debug.Log("TEXT " + titleCooked + " x " + titleX + " y " + titleY);

            titleX =
                Mathf.Max(
                    0.0f,
                    Mathf.Min(
                        titleX,
                        Screen.width - titleWidth));
            titleY =
                Mathf.Max(
                    titleHeight,
                    Mathf.Min(
                        titleY,
                        Screen.height));

        }

        if (drawDescription || drawDisabled) {

            descriptionCooked =
                drawDisabled
                    ? disabledReason.Replace("\\n", "\n")
                    : description.Replace("\\n", "\n");

            PieMenu.MeasureText(
                descriptionCooked,
                currentDisabled ? root.itemDescriptionDisabledFont : root.itemDescriptionFont,
                true,
                out descriptionWidth,
                out descriptionHeight);

            descriptionX +=
                root.itemDescriptionOffsetX - Mathf.Floor(descriptionWidth / 2.0f);
            descriptionY +=
                root.itemDescriptionOffsetY;

            descriptionX =
                Mathf.Max(
                    0.0f,
                    Mathf.Min(
                        descriptionX,
                        Screen.width - descriptionWidth));

            descriptionY =
                Mathf.Max(
                    descriptionHeight,
                    Mathf.Min(
                        descriptionY,
                        Screen.height));

            if (drawTitle) {
                if (titleY - titleHeight < descriptionY) {
                    if (descriptionY - descriptionHeight == 0.0f) {
                        titleY = descriptionY + titleHeight;
                    } else {
                        descriptionY = titleY - titleHeight;
                    }
                }
            }
        }

        if (drawTitle) {
            PieMenu.DrawText(
                titleCooked,
                titleX,
                Screen.height - titleY,
                disabled ? root.itemTitleDisabledFont : root.itemTitleFont,
                disabled ? root.itemTitleDisabledTopColor : root.itemTitleTopColor,
                disabled ? root.itemTitleDisabledUpperLeftColor : root.itemTitleUpperLeftColor,
                disabled ? root.itemTitleDisabledLowerRightColor : root.itemTitleLowerRightColor);
        }

        if (drawDescription || drawDisabled) {
            PieMenu.DrawText(
                descriptionCooked,
                descriptionX,
                Screen.height - descriptionY,
                currentDisabled ? root.itemDescriptionDisabledFont : root.itemDescriptionFont,
                currentDisabled ? root.itemDescriptionDisabledTopColor : root.itemDescriptionTopColor,
                currentDisabled ? root.itemDescriptionDisabledUpperLeftColor : root.itemDescriptionUpperLeftColor,
                currentDisabled ? root.itemDescriptionDisabledLowerRightColor : root.itemDescriptionLowerRightColor);
        }

        UpdateGameObject();
    }


    public void ShowPieMenuItem()
    {
        gameObjectVisible = true;
    }


    public void HidePieMenuItem()
    {
        gameObjectVisible = false;
        UpdateGameObject();
    }


    public void UpdateGameObject()
    {
        if (gameObjectVisible) {
            ShowGameObject();
        } else {
            HideGameObject();
        }
    }


    private void ShowGameObject()
    {
        PieMenu pieMenu =
            GetPieMenu();
        PieMenuRoot root = 
            GetRoot();

        PieMenu.SetActiveAll(gameObject, true);
        gameObject.layer =
            root.objectLayer;

        float objectScale =
            scale * pieMenu.itemScale * pieMenu.objectScale * root.itemObjectScale * root.itemObjectCameraScale;

        gameObject.transform.localScale =
            new Vector3(
                baseScale.x * objectScale, 
                baseScale.y * objectScale, 
                baseScale.z * objectScale);

        Vector3 screenPosition =
            new Vector3(pinX, pinY, root.pieMenuCamera.nearClipPlane);

        Ray ray =
            root.pieMenuCamera.ScreenPointToRay(
                screenPosition);

        bool current = 
            pieMenu.currentItem == this;

        Vector3 worldPosition =
            ray.GetPoint(
                current
                    ? root.itemObjectSelectedDistance
                    : root.itemObjectDistance);

        //Debug.Log("ShowGameObject pinX " + pinX + " pinY " + pinY + " screenPosition " + screenPosition.x + " " + screenPosition.y + " " + screenPosition.z + " worldPosition " + worldPosition.x + " " + worldPosition.y + " " + worldPosition.z);

        transform.position = 
            worldPosition;
        //transform.rotation =
        //    root.pieMenuCamera.transform.rotation;

        //Debug.DrawLine(worldPosition, root.transform.position, Color.red);

        pieMenu.ReportUpdateItem(this);
    }


    private void HideGameObject()
    {
        PieMenuRoot root =
            GetRoot();

        gameObject.layer = 
            root.objectLayer;

        PieMenu.SetActiveAll(gameObject, false);
    }


};


////////////////////////////////////////////////////////////////////////
