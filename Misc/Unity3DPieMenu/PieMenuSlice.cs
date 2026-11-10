////////////////////////////////////////////////////////////////////////
// PieMenuSlice.cs
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
public class PieMenuSlice : MonoBehaviour {


    // Instance variables configured in PieMenu.json.

    public Hashtable instanceData;
    public string id;
    public float distancePerItem = 20f;


    // Instance variables initialized by constructor.

    public int sliceIndex = 0;
    public float angle = 0.0f;
    public float dx = 0;
    public float dy = 0;
    [SerializeField]
    private List<PieMenuItem> items;
    [System.NonSerialized]
    public List<PieMenuItem> visibleItems;


    ////////////////////////////////////////////////////////////////////////
    // Static methods.


    public static PieMenuSlice GetSlice(Transform transform)
    {
        while (transform != null) {
            PieMenuSlice slice =
                transform.gameObject.GetComponent<PieMenuSlice>();

            if (slice != null) {
                return slice;
            }

            transform =
                transform.parent;
        }

        return null;
    }


    ////////////////////////////////////////////////////////////////////////
    // Instance methods.


    ////////////////////////////////////////////////////////////////////////
    // Loading and saving pie menu slices as JSON.


    public void LoadFromJson(Hashtable hashtable)
    {
        instanceData = hashtable;

        foreach (string key in hashtable.Keys) {
            object value = hashtable[key];
            LoadKeyValue(key, value);
        }
    }


    public void LoadKeyValue(string key, object value)
    {
        switch (key) {

            case "id": id = Model.ConvertToString(value); break;
            case "distancePerItem": distancePerItem = Model.ConvertToSingle(value); break;
            case "items": SetItemsFromJson(Model.ConvertToHashtableArray(value)); break;

            default: Debug.Log("PieMenuSlice LoadKeyValue got unexpected key '" + key + "' value '" + value + "'."); break;

        }
    }


    public void SetItemsFromJson(Hashtable[] jsonArray)
    {
        PieMenu pieMenu =
            GetPieMenu();

        bool noVisibleItemsUpdatesLast = 
            pieMenu.noVisibleItemsUpdates;
        pieMenu.noVisibleItemsUpdates = 
            true;

        items =
            new List<PieMenuItem>();

        if (jsonArray != null) {
            foreach (Hashtable pieMenuItemJson in jsonArray) {
                PieMenuItem item = 
                    CreateItem(null);
                item.LoadFromJson(
                    pieMenuItemJson);
            }
        }

        pieMenu.noVisibleItemsUpdates = 
            noVisibleItemsUpdatesLast;
        pieMenu.UpdateVisibleItems();
    }


    ////////////////////////////////////////////////////////////////////////
    // Parents.


	/// <summary>
	/// Gets the PieMenu of this PieMenuSlice.
	/// </summary>
    public PieMenu GetPieMenu()
    {
        return PieMenu.GetPieMenu(transform);
    }


	/// <summary>
	/// Gets the PieMenuRoot of this PieMenuSlice.
	/// </summary>
    public PieMenuRoot GetRoot()
    {
        return PieMenuRoot.GetRoot();
    }


    ////////////////////////////////////////////////////////////////////////
    // Items.


    public void SetItems(List<PieMenuItem> items0)
    {
         PieMenu pieMenu =
             GetPieMenu();

         items =
             items0;

         pieMenu.UpdateVisibleItems();
     }


     public List<PieMenuItem> GetItems()
     {
         if (items == null) {
             items = new List<PieMenuItem>();
         }

         return items;
     }


     public int CountItems()
     {
         return (items == null) ? 0 : items.Count;
     }


     public PieMenuItem GetItem(int i)
     {
         if ((items == null) || (i < 0) || (i >= items.Count)) {
             return null;
         }

         return items[i];
     }


     public void ClearItems()
     {
         items.Clear();
     }


     public void AddItem(PieMenuItem item)
     {
         PieMenu pieMenu =
            GetPieMenu();

         items.Add(item);

        pieMenu.UpdateVisibleItems();
    }


    public void RemoveItem(PieMenuItem item)
    {
        if (item == null) {
            return;
        }

        PieMenu pieMenu =
            GetPieMenu();

        if (!items.Contains(item)) {
            Debug.Log("PieMenu RemoveItem given somebody else's item.");
            return;
        }

        items.Remove(item);

        pieMenu.UpdateVisibleItems();
    }


    public void DestroyItem(PieMenuItem item, bool immediate)
    {
        if (item == null) {
            return;
        }

        RemoveItem(item);

        if (immediate) {
            DestroyImmediate(item.gameObject);
        } else {
            Destroy(item.gameObject);
        }
    }


    public PieMenuItem CreateItem(GameObject obj)
    {
        PieMenuRoot root =
            GetRoot();

        if (obj == null) {
            obj = new GameObject();
            obj.name = "PieMenuItem";
            obj.transform.parent = transform;
            obj.layer = root.objectLayer;
        } else {
            obj.transform.parent = transform;
            obj.layer = root.objectLayer;
        }

        PieMenuItem item = obj.GetComponent<PieMenuItem>();
        if (item == null) {
            item = obj.AddComponent<PieMenuItem>();
        }

        AddItem(item);

        return item;
    }


    public void ChangeItemIndex(int fromIndex, int toIndex)
    {
        PieMenuItem fromItem = 
            items[fromIndex];

        if (toIndex < fromIndex) {
            for (int i = fromIndex; i > toIndex; i--) {
                items[i] = items[i - 1];
            }
        } else {
            for (int i = fromIndex; i < toIndex; i++) {
                items[i] = items[i + 1];
            }
        }

        items[toIndex] = fromItem;
    }


    public int CountVisibleItems()
    {
        return (visibleItems == null) ? 0 : visibleItems.Count;
    }


    public PieMenuItem GetVisibleItem(int i)
    {
        if ((visibleItems == null) || (i < 0) || (i >= visibleItems.Count)) {
            return null;
        }

        return visibleItems[i];
    }


    public void ResetItems()
    {
        foreach (PieMenuItem item in items) {
            if (item == null) {
                continue;
            }
            item.ResetItem();
        }
    }


    public void UpdateVisibleItems(List<PieMenuItem> allVisibleItems)
    {
        if (items == null) {
            items = new List<PieMenuItem>();
        }

        if (visibleItems == null) {
            visibleItems = new List<PieMenuItem>();
        } else {
            visibleItems.Clear();
        }

        foreach (PieMenuItem item in items) {
            if ((item != null) && (!item.hidden)) {
                visibleItems.Add(item);
                allVisibleItems.Add(item);
            }
        }
    }


    public void Layout(int sliceIndex0, float angle0)
    {
        sliceIndex = sliceIndex0;
        angle = angle0;
        dx = Mathf.Cos(angle);
        dy = Mathf.Sin(angle);

        int index = 0;
        foreach (PieMenuItem item in items) {
            if (item != null) {
                item.Layout(sliceIndex, index);
            }
            index++;
        }
    }


    public void Draw()
    {
        PieMenu pieMenu =
            GetPieMenu();

        foreach (PieMenuItem item in visibleItems) {
            // Draw the current or current disabled item last so it goes on top of the others if there is any overlap.
            if ((item != pieMenu.currentItem) && (item != pieMenu.currentDisabledItem)) {
                item.Draw();
            }
        }
    }


    public PieMenuItem TrackItem()
    {
        int visibleItemCount =
            CountVisibleItems();
        if (visibleItemCount == 0) {
            return null;
        }

        PieMenu pieMenu =
            GetPieMenu();

        float chunk =
            (pieMenu.distance - pieMenu.radius) / distancePerItem;

        int pullOut = 
            (distancePerItem <= 0.0f)
                ? 0
                : Mathf.Max(
                    0,
                    Mathf.Min(
                        visibleItems.Count - 1,
                        (int)Mathf.Floor(
                            chunk)));

        return visibleItems[pullOut];
    }


    public void ShowPieMenuSlice()
    {
        if (items != null) {
            foreach (PieMenuItem item in items) {
                if (item == null) {
                    continue;
                }
                if (item.hidden) {
                    item.HidePieMenuItem();
                } else {
                    item.ShowPieMenuItem();
                }
            }
        }
    }


    public void HidePieMenuSlice()
    {
        if (items != null) {
            foreach (PieMenuItem item in items) {
                if (item == null) {
                    continue;
                }
                item.HidePieMenuItem();
            }
        }
    }


    [ContextMenu("Get Items from Sub-Objects")]
    public void GetItemsFromSubObjects()
    {
        PieMenu pieMenu =
            GetPieMenu();
        PieMenuRoot root =
            GetRoot();

        ClearItems();

        List<PieMenuItem> newItems =
            new List<PieMenuItem>();

        foreach (Transform childTransform in transform) {

            PieMenuItem item =
                childTransform.gameObject.GetComponent<PieMenuItem>();

            if (item == null) {

                // Found an object that doesn't already have a PieMenuItem component.
                // Make it a pieMenuItem.

                childTransform.gameObject.layer =
                    root.objectLayer;
                item =
                    childTransform.gameObject.AddComponent<PieMenuItem>();
            }

            float dx =
                childTransform.localPosition.x;
            float dy =
                childTransform.localPosition.x;
            item.distance =
                Mathf.Sqrt((dx * dx) + (dy * dy));

            newItems.Add(item);

        }

        newItems.Sort(PieMenuItem.CompareItemsByDistance);

        foreach (PieMenuItem item in newItems) {
            AddItem(item);
        }

        LayoutItemSubObjects();

        pieMenu.UpdateVisibleItems();
    }


    [ContextMenu("Layout Item Sub-Objects")]
    public void LayoutItemSubObjects()
    {
        PieMenuRoot root =
            GetRoot();
        int itemCount =
            CountItems();

        Layout(sliceIndex, angle);

        for (int i = 0; i < itemCount; i++) {
            PieMenuItem item =
                GetItem(i);

            if (item == null) {
                continue;
            }

            float x =
                i * dx * distancePerItem / root.worldToScreenScale;
            float y =
                i * dy * distancePerItem / root.worldToScreenScale;

            item.gameObject.transform.localPosition =
                new Vector3(x, 0.0f, y);
        }

    }


};


////////////////////////////////////////////////////////////////////////
