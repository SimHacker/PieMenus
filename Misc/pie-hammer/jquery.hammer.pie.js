/*
 * jquery.hammer.pies.js
 *
 * Pies for jQuery and Hammer.
 *
 * Defines a pie Hammer gesture recognizer, using jQuery.
 *
 * Copyright (C) 2013 By Don Hopkins.
 * All rights reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * Handlers for pies, pie slices and pie items:
 * 
 * pie(config) - pie configuration (define parameters and pies)
 * piedown(event, hammer, pie) - start event (low level mouse/touch/pen tracking)
 * piemove(event, hammer, pie) - move event (low level mouse/touch/pen tracking)
 * pieup(event, hammer, pie) - end event (low level mouse/touch/pen tracking)
 * piestart(event, hammer, pie) - pie starts tracking (popped up, either top level or sub menu)
 * pieend(event, hammer, pie) - pie ends tracking (popped down, either by select or cancel)
 * piepin(event, hammer, pie) - pie pinned (clicked up, stays up while button not pressed)
 * piecancel(event, hammer, pie) - pie canceled (clicked down without selecting, click in center inactive, press escape)
 * piechange(event, hammer, pie) - pie changes (select submenu)
 * pieupdate(event, hammer, pie) - pie real time update (betwen piedown and pieup)
 * pieselect(event, hammer, pie) - pie menu selected (selected the menu itself, a slice or an item, but not cancel)
 * pieslicechange(event, hammer, pie, slice) - changed between slices or no slice (enter or leave slice or inactive region)
 * piesliceupdate(event, hammer, pie, slice) - pie slice real time update (when in a slice)
 * piesliceselect(event, hammer, pie, slice) - pie slice selected (selected the current slice or current item, but not cancel)
 * pieitemchange(event, hammer, pie, slice, item) - changed between items or no item (enter or leave item or inactive region)
 * pieitemupdate(event, hammer, pie, slice, item) - pie item real time update (when in an item)
 * pieitemselect(event, hammer, pie, slice, item) - pie item selecte (selected the current item, but not cancel)
 *
 */


////////////////////////////////////////////////////////////////////////
// Hammer pieslice gesture.


(function(Hammer) {
    /**
     * Pie Slice gesture
     */
    Hammer.gestures.Pie = {

        name: 'pie',
        index: 1000,
        currentPie: null,
        currentSlice: null,
        currentItem: null,
        lastPie: null,
        lastSlice: null,
        lastItem: null,
        pinned: false,

        defaults: {

            defaultPie: null,

            initialDirection: 'South',
            clockwise: true,
            inactiveDistance: 10,
            itemDistanceMin: 120,
            itemDistanceSpacing: 50,
            rotateItems: true,

            pieAttributes: [
                ['rotateItems', 'boolean'],
                ['itemRotation', 'number']
            ],

            sliceAttributes: [
                ['rotateItems', 'boolean'],
                ['itemRotation', 'number'],
                ['direction', 'string']
            ],

            itemAttributes: [
                ['rotateItems', 'boolean'],
                ['itemRotation', 'number']
            ],

            pieDefaults: {
            },

            sliceDefaults: {
            },

            itemDefaults: {
            },

            directionNames: {
                'East': 0,
                'Right': 0,
                'NorthEast': 45,
                'North': 90,
                'Up': 90,
                'NorthWest': 135,
                'West': 180,
                'Left': 180,
                'SouthWest': 225,
                'South': 270,
                'Down': 270,
                'SouthEast': 315
            },

            pies: {}

        },

        handler: function pieGesture(event, hammer) {
            //console.log("pie gesture handler", "event", event, "hammer", hammer, "this", this);

            event.preventDefault();
            if (event.preventManipulation) {
                event.preventManiplation();
            }

            switch (event.eventType) {

                case Hammer.EVENT_START:

                    //console.log("==== pie gesture handler EVENT_START", "event", [event], "currentPie", [this.currentPie], "currentSlice", [this.currentSlice], "currentItem", [this.currentItem], "pinned", this.pinned);

                    if (Hammer.detection.keep_tracking) {
                        console.log("pie gesture handler EVENT_START keep_tracking was true");
                    }

                    this.trackPieDown(event, hammer, this.currentPie);

                    if (this.currentPie != null) {
                        // Pie already active, so just track and return.
                        this.trackCurrentPie(event, hammer);

                        return;
                    }

                    // Find the pie to start.

                    this.currentPie =
                        this.findPie(event, hammer, hammer.options.defaultPie);

                    //console.log("pie gesture handler EVENT_START findPie currentPie", this.currentPie);

                    if (this.currentPie == null) {
                        // No pie active, so do nothing.
                        return;
                    }

                    Hammer.detection.current.name = this.name;
                    this.pinned = false;
                    this.currentSlice = null;
                    this.currentItem = null;

                    this.trackPieChange(event, hammer, this.currentPie);
                    this.trackPieStart(event, hammer, this.currentPie);
                    this.showPie(event, hammer, this.currentPie);
                    this.trackCurrentPie(event, hammer);

                    break;

                case Hammer.EVENT_MOVE:

                    //console.log("==== pie gesture handler EVENT_MOVE", "event", [event], "currentPie", [this.currentPie], "currentSlice", [this.currentSlice], "currentItem", [this.currentItem], "pinned", this.pinned);

                    if (this.currentPie == null) {
                        // No pie active, so do nothing.
                        return;
                    }

                    this.trackPieMove(event, hammer, this.currentPie);
                    this.trackCurrentPie(event, hammer);

                    break;

                case Hammer.EVENT_END:

                    //console.log("==== pie gesture handler EVENT_END", "event", [event], "currentPie", [this.currentPie], "currentSlice", [this.currentSlice], "currentItem", [this.currentItem], "pinned", this.pinned);

                    if (Hammer.detection.keep_tracking) {
                        //console.log("pie gesture handler EVENT_END keep_tracking was true so resetting", "detection", Hammer.detection);
                        Hammer.detection.keep_tracking = false;
                    }

                    if (this.currentPie == null) {
                        // No pie active, so do nothing.
                        return;
                    }

                    var nextPie = null;

                    this.trackPieUp(event, hammer, this.currentPie);
                    this.trackCurrentPie(event, hammer);

                    if (this.currentSlice != null) {

                        // In a slice.

                        if (this.currentItem != null) {

                            // In an item.

                            //console.log("pie gesture handler EVENT_END pieitemselect", "currentPie", this.currentPie, "currentSlice", this.currentSlice, "currentItem", this.currentItem);

                            this.trackPieItemSelect(event, hammer, this.currentPie, this.currentSlice, this.currentItem);

                            nextPie = this.findKeyDefault('nextPie', null, this.currentItem, this.currentSlice, this.currentPie, hammer.options);
                            if (nextPie) {
                                nextPie = this.findPie(event, hammer, nextPie);
                            }

                            this.highlightItem(event, hammer, this.currentPie, this.currentSlice, this.currentItem, false);

                            this.lastItem = this.currentItem;
                            this.currentItem = null;

                            this.trackPieItemChange(event, hammer, this.currentPie, this.currentSlice, this.currentItem);

                        }

                        this.trackPieSliceSelect(event, hammer, this.currentPie, this.currentSlice);

                        this.highlightSlice(event, hammer, this.currentPie, this.currentSlice, false);

                        this.lastSlice = this.currentSlice;
                        this.currentSlice = null;

                        this.trackPieSliceChange(event, hammer, this.currentPie, this.currentSlice);

                        this.trackPieSelect(event, hammer, this.currentPie);

                    } else {

                        // No slice selected.

                        if (!this.pinned) {

                            // Pin up if not already pinned.

                            this.pinned = true;

                            // Hack to keep hammer tracking after the up event.
                            Hammer.detection.keep_tracking = true;

                            this.trackPiePin(event, hammer, this.currentPie);

                            // Return without ending pie tracking.
                            return;

                        } else {

                            // Cancel if pinned.

                            this.trackPieCancel(event, hammer, this.currentPie);

                        }

                    }

                    this.trackPieEnd(event, hammer, this.currentPie);
                    this.hidePie(event, hammer, this.currentPie);

                    this.lastPie = this.currentPie;
                    this.currentPie = null;

                    if (nextPie != null) {

                        // Keep tracking, and go on to the next pie, starting with it already pinned.

                        // Hack to keep hammer tracking after the up event.
                        Hammer.detection.keep_tracking = true;

                        if (Hammer.detection.keep_tracking) {
                            Hammer.detection.current.startEvent.center = event.center;
                            event.deltaX = 0;
                            event.deltaY = 0;
                            event.distance = 0;
                        }

                        this.currentPie = nextPie;
                        this.pinned = true;

                        this.trackPieChange(event, hammer, this.currentPie);
                        this.trackPieStart(event, hammer, this.currentPie);
                        this.trackPiePin(event, hammer, this.currentPie);
                        this.showPie(event, hammer, this.currentPie);
                        this.trackCurrentPie(event, hammer);

                    } else {

                        // Finished tracking.

                        this.pinned = false;

                        this.trackPieChange(event, hammer, this.currentPie);

                    }


                    break;

            }

        },

        findPie: function(event, hammer, pieRef) {

            //console.log("pie gesture findPie", "event", event, "hammer", hammer, "pieRef", pieRef);

            var pie = null;

            var findPie = hammer.options.findPie;
            if (findPie) {

                //console.log("pie gesture findPie applying custom findPie", findPie);
                pieRef = findPie.apply(this, [event, hammer, pieRef]);
                //console.log("pieRef", typeof pieRef, pieRef);

            }

            if ((typeof pieRef) == "string") {

                pie = hammer.options.pies[pieRef];

                if (pie) {

                    if ((typeof pie) == "string") {
                        pie = this.makePieFromDom(event, hammer, pie);
                        hammer.options.pies[pieRef] = pie
                    }

                    this.makeDomFromPie(hammer, pie);

                } else {

                    pie = this.makePieFromDom(event, hammer, pieRef);
                    hammer.options.pies[pieRef] = pie;

                }

            } else if ($.isPlainObject(pieRef)) {

                pie = pieRef;

                this.makeDomFromPie(hammer, pie);

            } else {

                //console.log("pie gesture invalid", "pieRef", pieRef);

            }

            //console.log("pie gesture findPie result", "pie", pie);

            return pie;
        },

        makePieFromDom: function(event, hammer, pieName) {

            //console.log("pie gesture makePieFromDom", "event", event, "pieName", pieName, "this", this);

            var self = this;
            var pieEl = $(pieName);

            if (pieEl.length == 0) {
                return null;
            }

            var pieBackgroundEl = pieEl.find('.PieBackground');
            if (pieBackgroundEl.length == 0) { pieBackgroundEl = null; }
            var pieSlicesEl = pieEl.find('.PieSlices');
            if (pieSlicesEl.length == 0) { pieSlicesEl = null; }
            var pieOverlayEl = pieEl.find('.PieOverlay');
            if (pieOverlayEl.length == 0) { pieOverlayEl = null; }

            // If no intermediate pieSlicesEl,
            // then create it and move the slices into it,
            // and the overlay after it.
            if (!pieSlicesEl) {
                pieSlicesEl =
                    $('<div/>')
                        .attr({
                            'class': 'PieSlices'
                        })
                    .appendTo(pieEl);
                pieEl.find('.PieSlice').appendTo(pieSlicesEl);

                if (pieOverlayEl) {
                    pieSlicesEl.after(pieOverlayEl);
                }
            }

            var slices = [];
            var pie = $.extend({}, hammer.options.pieDefaults, {
                pieEl: pieEl,
                pieBackgroundEl: pieBackgroundEl,
                pieSlicesEl: pieSlicesEl,
                pieOverlayEl: pieOverlayEl,
                slices: slices
            });

            self.inheritAttributes('pie', pie, pieEl, hammer.options.pieAttributes); 

            pieSlicesEl.find('.PieSlice').each(function(i) {

                var sliceEl = $(this);
                var direction = sliceEl.data('pieslice-direction');

                var sliceBackgroundEl = sliceEl.find('.PieSliceBackground');
                var sliceItemsEl = sliceEl.find('.PieSliceItems');
                var sliceOverlayEl = sliceEl.find('.PieSliceOverlay');

                // If no intermediate sliceItemsEl,
                // then create it and move the items into it,
                // and the overlay after it.
                if (!sliceItemsEl) {
                    sliceItemsEl =
                        $('<div/>')
                            .attr({
                                'class': 'PieSliceItems'
                            })
                        .appendTo(sliceEl);
                    sliceEl.find('.PieItem').appendTo(sliceItemsEl);

                    if (sliceOverlayEl) {
                        sliceItemsEl.after(sliceOverlayEl);
                    }
                }

                var items = [];
                var slice = $.extend({}, hammer.options.sliceDefaults, {
                    sliceEl: sliceEl,
                    sliceBackgroundEl: sliceBackgroundEl,
                    sliceItemsEl: sliceItemsEl,
                    sliceOverlayEl: sliceOverlayEl,
                    direction: direction,
                    items: items
                });

                self.inheritAttributes('pieslice', slice, sliceEl, hammer.options.sliceAttributes); 
                slices.push(slice);

                sliceItemsEl.find('.PieItem').each(function(i) {

                    var itemEl = $(this);
                    var itemBackgroundEl = itemEl.find('.PieItemBackground');
                    var itemLabelEl = itemEl.find('.PieItemLabel');
                    var itemOverlayEl = itemEl.find('.PieItemOverlay');

                    var nextPie = itemEl.data('pieitem-nextpie');

                    var item = $.extend({}, hammer.options.itemDefaults, {
                        itemEl: itemEl,
                        itemBackgroundEl: itemBackgroundEl,
                        itemLabelEl: itemLabelEl,
                        itemOverlayEl: itemOverlayEl,
                        nextPie: nextPie
                    });
                    self.inheritAttributes('pieitem', item, itemEl, hammer.options.itemAttributes); 
                    items.push(item);

                });

            });

            this.layoutPie(hammer, pie);

            return pie;
        },

        makeDomFromPie: function(hammer, pie) {

            //console.log("pie gesture makeDomPie", "hammer", hammer, "pie", pie);

            if (pie.pieEl) {
                return;
            }

            this.setDefaultValues(pie, hammer.options.pieDefaults);

            var body = $(document.body);

            var pieEl = pie.pieEl;
            if (!pieEl) {
                pieEl =
                    $('<div/>')
                        .attr({
                            'class': 'Pie'
                        })
                        .css({
                            display: 'none'
                        })
                        .appendTo(body);

                pie.pieEl = pieEl;
            }
            if (pie.css) {
                pieEl.css(pie.css);
            }
            if (pie.class) {
                pieEl.addClass(pie.class);
            }

            var pieBackgroundEl = pie.pieBackgroundEl;
            if (!pieBackgroundEl) {
                pieBackgroundEl =
                    $('<div/>')
                        .attr({
                            'class': 'PieBackground'
                        })
                        .appendTo(pieEl);
                pie.pieBackgroundEl = pieBackgroundEl;
            }
            if (pie.cssBackground) {
                pieBackgroundEl.css(pie.cssBackground);
            }
            if (pie.classBackground) {
                pieBackgroundEl.addClass(pie.classBackground);
            }

            var pieSlicesEl = pie.pieSlicesEl;
            if (!pieSlicesEl) {
                pieSlicesEl =
                    $('<div/>')
                        .attr({
                            'class': 'PieSlices'
                        })
                        .appendTo(pieEl);
                pie.pieSlicesEl = pieSlicesEl;
            }
            if (pie.cssSlices) {
                pieSlicesEl.css(pie.cssSlices);
            }
            if (pie.classSlices) {
                pieSlicesEl.addClass(pie.classSlices);
            }

            var pieOverlayEl = pie.pieOverlayEl
            if (!pieOverlayEl) {
                pieOverlayEl =
                    $('<div/>')
                        .attr({
                            'class': 'PieOverlay'
                        })
                        .appendTo(pieEl);
                pie.pieOverlayEl = pieOverlayEl;
            }
            if (pie.cssOverlay) {
                pieOverlayEl.css(pie.cssOverlay);
            }
            if (pie.classOverlay) {
                pieOverlayEl.addClass(pie.classOverlay);
            }

            var slices = pie.slices || [];

            for (var sliceIndex = 0, sliceCount = slices.length;
                 sliceIndex < sliceCount;
                 sliceIndex++) {

                var slice = slices[sliceIndex];

                this.setDefaultValues(slice, hammer.options.sliceDefaults);

                var sliceEl = slice.sliceEl;
                if (!sliceEl) {
                    sliceEl =
                        $('<div/>')
                            .attr({
                                'class': 'PieSlice'
                            })
                            .appendTo(pieSlicesEl);
                    slice.sliceEl = sliceEl;
                }
                if (slice.css) {
                    sliceEl.css(slice.css);
                }
                if (slice.class) {
                    sliceEl.addClass(slice.class);
                }

                var sliceBackgroundEl = slice.sliceBackgroundEl;
                if (!sliceBackgroundEl) {
                    sliceBackgroundEl =
                        $('<div/>')
                            .attr({
                                'class': 'PieSliceBackground'
                            })
                            .appendTo(sliceEl);
                    slice.sliceBackgroundEl = sliceBackgroundEl;
                }
                if (slice.cssBackground) {
                    sliceBackgroundEl.css(slice.cssBackground);
                }
                if (slice.classBackground) {
                    sliceBackgroundEl.addClass(slice.classBackground);
                }

                var sliceItemsEl = slice.sliceItemsEl;
                if (!sliceItemsEl) {
                    sliceItemsEl =
                        $('<div/>')
                            .attr({
                                'class': 'PieSliceItems'
                            })
                            .appendTo(sliceEl);
                    slice.sliceItemsEl = sliceItemsEl;
                }
                if (slice.cssItems) {
                    sliceItemsEl.css(slice.cssItems);
                }
                if (slice.classItems) {
                    sliceItemsEl.addClass(slice.classItems);
                }

                var sliceOverlayEl = slice.sliceOverlayEl;
                if (!sliceOverlayEl) {
                    sliceOverlayEl =
                        $('<div/>')
                            .attr({
                                'class': 'PieSliceOverlay'
                            })
                            .appendTo(sliceEl);
                    slice.sliceOverlayEl = sliceOverlayEl;
                }
                if (slice.cssOverlay) {
                    sliceOverlayEl.css(slice.cssOverlay);
                }
                if (slice.classOverlay) {
                    sliceOverlayEl.addClass(slice.classOverlay);
                }

                var items = slice.items || [];

                for (var itemIndex = 0, itemCount = items.length;
                     itemIndex < itemCount;
                     itemIndex++) {

                    var item = items[itemIndex];

                    this.setDefaultValues(item, hammer.options.itemDefaults);

                    var itemEl = item.itemEl;
                    if (!itemEl) {
                        itemEl =
                            $('<div/>')
                                .attr({
                                    'class': 'PieItem'
                                })
                               .appendTo(sliceItemsEl);
                        item.itemEl = itemEl;
                    }
                    if (item.css) {
                        itemEl.css(item.css);
                    }
                    if (item.class) {
                        itemEl.addClass(item.class);
                    }

                    var itemBackgroundEl = item.itemBackgroundEl;
                    if (!itemBackgroundEl) {
                        itemBackgroundEl =
                            $('<div/>')
                                .attr({
                                    'class': 'PieItemBackground'
                                })
                                .appendTo(itemEl);
                        item.itemBackgroundEl = itemBackgroundEl;
                    }
                    if (item.cssBackground) {
                        itemBackgroundEl.css(item.cssBackground);
                    }
                    if (item.classBackground) {
                        itemBackgroundEl.addClass(item.classBackground);
                    }

                    var itemLabelEl = item.itemLabelEl;
                    if (!itemLabelEl) {
                        itemLabelEl =
                            $('<div/>')
                                .attr({
                                    'class': 'PieItemLabel'
                                })
                                .appendTo(itemEl);
                        item.itemLabelEl = itemLabelEl;
                    }
                    if (item.cssLabel) {
                        itemLabelEl.css(item.cssLabel);
                    }
                    if (item.classLabel) {
                        itemLabelEl.addClass(item.classLabel);
                    }
                    if (item.label) {
                        itemLabelEl.html(item.label);
                    }

                    var itemOverlayEl = item.itemOverlayEl;
                    if (!itemOverlayEl) {
                        itemOverlayEl =
                            $('<div/>')
                                .attr({
                                    'class': 'PieItemOverlay'
                                })
                                .appendTo(itemEl);
                        item.itemOverlayEl = itemOverlayEl;
                    }
                    if (item.cssOverlay) {
                        itemOverlayEl.css(item.cssOverlay);
                    }
                    if (item.classOverlay) {
                        itemOverlayEl.addClass(item.classOverlay);
                    }
                }

            }

            this.layoutPie(hammer, pie);

        },

        setDefaultValues: function(dict, defaults) {
            var key;
            for (key in defaults) {
                if (defaults.hasOwnProperty(key) &&
                    (dict[key] === undefined)) {
                    dict[key] = defaults[key];
                }
            }
        },

        findKeyDefault: function() {

            var argumentCount = arguments.length;

            if (argumentCount < 2) {
                return null;
            }

            var key = arguments[0];

            var i;
            for (i = 2; i < argumentCount; i++) {
                var dict = arguments[i];
                if (dict.hasOwnProperty(key)) {
                    return dict[key];
                }
            }

            return arguments[1];
        },

        inheritAttributes: function(prefix, dict, el, attributes) {

            //console.log("inheritAttributes", "prefix", prefix, "dict", [dict], "el", [el], "attributes", attributes);

            var attributeCount = attributes.length;
            var attributeIndex;
            for (attributeIndex = 0; attributeIndex < attributeCount; attributeIndex++) {

                var attributeDesc = attributes[attributeIndex];
                var attributeKey = attributeDesc[0];
                var attributeType = attributeDesc[1];
                var attributeName = 'data-' + prefix + '-' + attributeKey.toLowerCase();
                var attributeValue = el.attr(attributeName);

                //console.log("inheritAttributes", "attributeDesc", attributeDesc, "attributeKey", attributeKey, "attributeType", attributeType, "attributeName", attributeName, "attributeValue", attributeValue);

                if (attributeValue !== undefined) {
                    switch (attributeType) {
                        case 'string':
                            break;
                        case 'number':
                            attributeValue = parseInt(attributeValue);
                            break;
                        case 'boolean':
                            if (attributeValue == 'true') {
                                attributeValue = true;
                            } else if (attributeValue == 'false') {
                                attributeValue = false;
                            } else {
                                console.log("pie gesture inheritAttributes got unknown boolean", "attributeValue", typeof attributeValue, attributeValue, "attributeName", attributeName, "el", el);
                            }
                            break;
                        case 'eval':
                            try {
                                attributeValue = eval(attributeValue);
                            } catch (e) {
                                console.log("pie gesture inheritAttributes got error evaluating", "attributeValue", attributeValue, "attributeName", attributeName, "el", el);
                            }
                            break;
                        default:
                            console.log("pie gesture inheritAttributes got unknown type", "attributeValue", attributeValue, "attributeName", attributeName, "prefix", prefix, "el", el);
                            break;
                    }

                    dict[attributeKey] = attributeValue;

                    //console.log("pie gesture inheritAttributes set", "dict", [dict], "attributeKey", attributeKey, "attributeType", attributeType, "attributeValue", typeof attributeValue, attributeValue, "el", el);

                }
            }
        },

        layoutPie: function(hammer, pie) {

            //console.log("pie gesture layoutPie", "hammer", hammer, "pie", pie);

            var slices = pie.slices;
            var sliceCount = slices.length;

            if (sliceCount == 0) {
                return;
            }

            var pieEl = pie.pieEl;
            var displayLast = pieEl.css('display');

            pieEl.css({
                visibility: 'none',
                display: 'block'
            });

            var usedDirs = {};
            var sliceIndex;

            for (sliceIndex = 0; sliceIndex < sliceCount; sliceIndex++) {
                var slice = slices[sliceIndex];
                var direction = slice.direction;
                var dir = this.parseDirection(hammer, direction);

                if (dir == null) {
                    slice.dir = null;
                    continue;
                }

                var dirInt = Math.floor(dir);
                if (usedDirs[dirInt]) {
                    console.log("pie gesture layoutPie got two slices with same direction", direction, dirInt, usedDirections[dirInt], slice, "so ignoring second's direction");
                    slice.direction = null;
                    slice.dir = null;
                } else {
                    usedDirs[dirInt] = slice;
                    slice.dir = dir;
                    slice.dx = Math.cos(this.degToRad(dir));
                    slice.dy = -Math.sin(this.degToRad(dir));
                }
            }

            var clockwise = this.findKeyDefault('clockwise', true, pie, hammer.options);
            var turn = (clockwise ? -360 : 360) / sliceCount;
            var currentDir =
                this.parseDirection(hammer, this.findKeyDefault('initialDirection', 270, pie, hammer.options));

            for (sliceIndex = 0; sliceIndex < sliceCount; sliceIndex++) {
                var slice = slices[sliceIndex];
                var direction = slice.direction;
                var dir = slice.dir;

                if (dir != null) {
                    continue;
                }

                var currentDirStart = currentDir;

                while (usedDirs[Math.floor(currentDir)]) {
                    console.log("pie gesture layoutPie skipping used dir", currentDir, "and turning", turn);

                    currentDir = this.normalDeg(currentDir + turn);

                    if (currentDir == currentDirStart) {
                        console.log("pie gesture layoutPie wrapped around at", currentDir, "with turn", turn, "so halving turn and continuing");
                        turn /= 2;
                        currentDir = this.normalDeg(dir + turn);
                        currentDirStart = currentDir;
                    }
                }

                dir = currentDir;
                usedDirs[Math.floor(currentDir)] = slice;
                currentDir = this.normalDeg(currentDir + turn);

                slice.dir = dir;
                slice.dx = Math.cos(this.degToRad(dir));
                slice.dy = -Math.sin(this.degToRad(dir));

                //console.log("SLICE", sliceIndex, dir, dx, dy, slice);
            }

            var minX = 0;
            var minY = 0;
            var maxX = 0;
            var maxY = 0;

            var itemDistanceMin = this.findKeyDefault('itemDistanceMin', 50, slice, pie, hammer.options);
            var itemDistanceSpacing = this.findKeyDefault('itemDistanceSpacing', 50, slice, pie, hammer.options);

            for (sliceIndex = 0; sliceIndex < sliceCount; sliceIndex++) {
                var slice = slices[sliceIndex];
                var sliceEl = slice.sliceEl;
                var items = slice.items;
                var dir = slice.dir;
                var dx = slice.dx;
                var dy = slice.dy;

                //console.log("sliceIndex", sliceIndex, "dir", dir, "dx", dx, "dy", dy, "slice", slice);

                var itemCount = items.length;
                var itemDistance = itemDistanceMin;
                var itemIndex;

                for (itemIndex = 0; itemIndex < itemCount; itemIndex++) {
                    var item = items[itemIndex];
                    var itemEl = item.itemEl;
                    var itemBackgroundEl = item.itemBackgroundEl;
                    var itemLabelEl = item.itemLabelEl;
                    var itemOverlayEl = item.itemOverlayEl;

                    var rot = 0;
                    var rotateItems = this.findKeyDefault('rotateItems', false, item, slice, pie, hammer.options);
                    if (rotateItems) {
                        if ((dir == 90) || (dir == 270)) {
                            rot = 0;
                        } else if (dir == 0) {
                            rot = 90;
                        } else if (dir == 180) {
                            rot = -90;
                        } else {
                            if (dx < 0) {
                                rot = dir + 180;
                            } else {
                                rot = dir;
                            }
                        }
                    }

                    rot = this.findKeyDefault('itemRotation', rot, item, slice, pie, hammer.options);

                    itemEl.css({
                        transform: 'rotate(' + rot + 'deg)'
                    });

                    var itemWidth = Math.ceil(itemLabelEl.outerWidth());
                    var itemHeight = Math.ceil(itemLabelEl.outerHeight());
                    var itemCenterX = dx * itemDistance;
                    var itemCenterY = dy * itemDistance;
                    var itemMarginLeft = Math.round(itemWidth / -2);
                    var itemMarginTop = Math.round(itemHeight / -2);
                    var itemLeft = itemCenterX + itemMarginLeft;
                    var itemTop = itemCenterY + itemMarginTop;
                    var itemRight = itemLeft + itemWidth;
                    var itemBottom = itemTop + itemHeight;

                    item.x = itemLeft;
                    item.y = itemTop;
                    item.width = itemWidth;
                    item.height = itemHeight;
                    item.centerX = itemCenterX;
                    item.centerY = itemCenterY;

                    minX = Math.min(minX, itemLeft);
                    minY = Math.min(minY, itemTop);
                    maxX = Math.max(maxX, itemRight);
                    maxY = Math.max(maxY, itemBottom);

                    //console.log("itemIndex", itemIndex, "itemDistance", itemDistance, "itemCenterX", itemCenterX, "itemCenterY", itemCenterY, "itemLeft", itemLeft, "itemTop", itemTop, "itemWidth", itemWidth, "itemHeight", itemHeight, "item", item, "html", itemEl.html());

                    itemEl.css({
                        left: itemLeft,
                        top: itemTop,
                        width: itemWidth,
                        height: itemHeight
                    });

                    itemOverlayEl.css({
                        left: 0,
                        top: 0,
                        width: itemWidth,
                        height: itemHeight
                    });

                    itemBackgroundEl.css({
                        left: 0,
                        top: 0,
                        width: itemWidth,
                        height: itemHeight
                    });

                    itemDistance += itemDistanceSpacing;
                }
            }

            var pieWidth = Math.ceil(maxX - minX);
            var pieHeight = Math.ceil(maxY - minY);
            var pieMarginLeft = Math.round(minX);
            var pieMarginTop = Math.round(minY);

            var pieBackgroundEl = pie.pieBackgroundEl;
            pieBackgroundEl.css({
                left: -pieMarginLeft,
                top: -pieMarginTop
            });

            var pieSlicesEl = pie.pieSlicesEl;
            pieSlicesEl.css({
                left: -pieMarginLeft,
                top: -pieMarginTop
            });

            var pieOverlayEl = pie.pieOverlayEl;
            pieOverlayEl.css({
                left: -pieMarginLeft,
                top: -pieMarginTop
            });

            pieEl.css({
                width: pieWidth,
                height: pieHeight,
                marginLeft: pieMarginLeft,
                marginTop: pieMarginTop,
                visibility: 'visible',
                display: displayLast
            });

        },

        highlightSlice: function(event, hammer, pie, slice, highlight) {
            if (!slice) {
                return;
            }
            var sliceEl = slice.sliceEl;
            if (highlight) {
                sliceEl.addClass('PieSliceHighlight');
            } else {
                sliceEl.removeClass('PieSliceHighlight');
            }
        },

        highlightItem: function(event, hammer, pie, slice, item, highlight) {
            if (!item) {
                return;
            }
            var itemEl = item.itemEl;
            if (highlight) {
                itemEl.addClass('PieItemHighlight');
                itemEl.appendTo(itemEl.parent());
            } else {
                itemEl.removeClass('PieItemHighlight');
            }
        },

        degToRad: function(deg) {
            return deg * Math.PI / 180;
        },

        radToDeg: function(rad) {
            return rad * 180 / Math.PI;
        },

        normalDeg: function(deg) {
            while (deg < 0) {
                deg += 360;
            }
            while (deg >= 360) {
                deg -= 360;
            }
            return deg;
        },

        deltaToDeg: function(dx, dy) {
            if ((dx == 0) && (dy == 0)) {
                return 0;
            }
            return this.normalDeg(
                this.radToDeg(
                    Math.atan2(-dy, dx)));
        },

        trackCurrentPie: function(event, hammer) {
            //console.log("pie gesture handler trackCurrentPie", "hammer", hammer, "event", event);

            this.lastSlice = this.currentSlice;
            this.lastItem = this.currentItem;

            this.currentSlice = this.findSlice(event, hammer, this.currentPie);

            if (this.currentSlice != null) {
                this.currentItem = this.findItem(event, hammer, this.currentPie, this.currentSlice);
            } else {
                this.currentItem = null;
            }

            if (this.currentSlice != this.lastSlice) {
                this.highlightSlice(event, hammer, this.currentPie, this.lastSlice, false);
                this.highlightSlice(event, hammer, this.currentPie, this.currentSlice, true);
                this.trackPieSliceChange(event, hammer, this.currentPie, this.currentSlice);
            }

            if (this.currentItem != this.lastItem) {
                this.highlightItem(event, hammer, this.currentPie, this.lastSlice, this.lastItem, false);
                this.highlightItem(event, hammer, this.currentPie, this.currentSlice, this.currentItem, true);
                this.trackPieItemChange(event, hammer, this.currentPie, this.currentSlice, this.currentItem);
            }

            this.trackPieUpdate(event, hammer, this.currentPie);

            if (this.currentSlice != null) {

                this.trackPieSliceUpdate(event, hammer, this.currentPie, this.currentSlice);

                if (this.currentItem != null) {

                    this.trackPieItemUpdate(event, hammer, this.currentPie, this.currentSlice, this.currentItem);

                }

            }

        },

        parseDirection: function(hammer, directionSpec) {
            var dir = null;

            if ((directionSpec === null) ||
                (directionSpec === undefined)) {

                dir = undefined;

            } else if ((typeof directionSpec) == "number") {

                dir = this.normalDeg(directionSpec);

            } else if ((typeof directionSpec) == "string") {
                
                dir = hammer.options.directionNames[directionSpec];

                if (dir === undefined) {

                    try {
                        dir = parseFloat(directionSpec);
                    } catch (e) {
                        console.log("pie gesture parseDirection got invalid string direction name", directionSpec, "not in", hammer.options.directionNames, "or convertable to number of degrees");
                    }

                }

            } else if ($.isArray(directionSpec)) {

                if (directionSpec.length != 2) {
                    console.log("pie gesture parseDirection got invalid direction spec", directionSpec, "not array of two numbers [dx, dy]");
                } else {
                    var dx = directionSpec[0];
                    var dy = directionSpec[1];
                    dir = this.deltaToDeg(dx, dy);
                }

            } else {

                console.log("pie gesture parseDirection got invalid direction spec", directionSpec, "not number of degrees, array of two numbers [dx, dy], or direction name string from", hammer.options.directionNames);

            }

            return dir;
        },

        findSlice: function(event, hammer, pie) {
            //console.log("pie gesture findSlice", "event", event, "hammer", hammer, "pie", pie);

            var slices = pie.slices;
            var sliceCount = slices.length;

            if (sliceCount == 0) {
                return null;
            }

            var distance = event.distance;

            var inactiveDistance = this.findKeyDefault('inactiveDistance', 10, pie, hammer.options);

            if (distance <= inactiveDistance) {
                return null;
            }

            if (sliceCount == 1) {
                return slices[0];
            }

            var dx = event.deltaX;
            var dy = event.deltaY;

            var maxDot = 0;
            var maxDotIndex = null;
            var sliceIndex;
            for (sliceIndex = 0; sliceIndex < sliceCount; sliceIndex++) {
                var slice = slices[sliceIndex];
                var sliceDx = slice.dx;
                var sliceDy = slice.dy;
                var dot = (dx * sliceDx) + (dy * sliceDy);
                if (dot > maxDot) {
                    maxDot = dot;
                    maxDotIndex = sliceIndex;
                }
            }

            return slices[maxDotIndex];
        },

        findItem: function(event, hammer, pie, slice) {
            //console.log("pie gesture findItem", "event", event, "hammer", hammer, "pie", pie, "slice", slice);

            var items = slice.items;
            var itemCount = items.length;

            if (itemCount == 0) {
                return null;
            }

            if (itemCount == 1) {
                return items[0];
            }

            var dx = event.deltaX;
            var dy = event.deltaY;

            var minDistanceSquared = 1.0e+6;
            var minDistanceSquaredIndex = null;
            var itemIndex;
            for (itemIndex = 0; itemIndex < itemCount; itemIndex++) {
                var item = items[itemIndex];
                var itemCenterX = item.centerX;
                var itemCenterY = item.centerY;
                var distanceX = itemCenterX - dx;
                var distanceY = itemCenterY - dy;
                var distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);
                if (distanceSquared < minDistanceSquared) {
                    minDistanceSquared = distanceSquared;
                    minDistanceSquaredIndex = itemIndex;
                }
            }

            return items[minDistanceSquaredIndex];
        },

        showPie: function(event, hammer, pie) {
            //console.log("pie gesture showPie", "event", event, "hammer", hammer, "pie", pie, "center", event.center);
            var pieEl = pie.pieEl;
            pieEl
                .css({
                    left: event.center.pageX,
                    top: event.center.pageY,
                    display: 'block'
                });
        },

        hidePie: function(event, hammer, pie) {
            //console.log("pie gesture hidePie", "event", event, "hammer", hammer, "pie", pie);
            var pieEl = pie.pieEl;
            pieEl.css({
                display: 'none'
            });
        },

        trackPieDown: function(event, hammer, pie) {
            // piedown(event, hammer, pie)
            console.log("======== piedown", "event", event, "hammer", hammer, "pie", pie);
            hammer.trigger("piedown", event);
            if (hammer.options.trackPieDown) {
                hammer.options.trackPieDown.apply(this, [event, hammer, pie]);
            }
        },

        trackPieMove: function(event, hammer, pie) {
            // piemove(event, hammer, pie)
            //console.log("piemove", "event", event, "hammer", hammer, "pie", pie);
            hammer.trigger("piemove", event);
            if (hammer.options.trackPieMove) {
                hammer.options.trackPieMove.apply(this, [event, hammer, pie]);
            }
        },

        trackPieUp: function(event, hammer, pie) {
            // pieup(event, hammer, pie)
            //console.log("pieup", "event", event, "hammer", hammer, "pie", pie);
            hammer.trigger("pieup", event);
            if (hammer.options.trackPieUp) {
                hammer.options.trackPieUp.apply(this, [event, hammer, pie]);
            }
        },

        trackPieStart: function(event, hammer, pie) {
            // piestart(event, hammer, pie)
            //console.log("piestart", "event", event, "hammer", hammer, "pie", pie);
            hammer.trigger("piestart", event);
            if (hammer.options.trackPieStart) {
                hammer.options.trackPieStart.apply(this, [event, hammer, pie]);
            }
        },

        trackPieEnd: function(event, hammer, pie) {
            // pieend(event, hammer, pie)
            //console.log("pieend", "event", event, "hammer", hammer, "pie", pie);
            hammer.trigger("pieend", event);
            if (hammer.options.trackPieEnd) {
                hammer.options.trackPieEnd.apply(this, [event, hammer, pie]);
            }
        },

        trackPiePin: function(event, hammer, pie) {
            // piepin(event, hammer, pie)
            //console.log("piepin", "event", event, "hammer", hammer, "pie", pie);
            hammer.trigger("piepin", event);
            if (hammer.options.trackPiePin) {
                hammer.options.trackPiePin.apply(this, [event, hammer, pie]);
            }
        },

        trackPieCancel: function(event, hammer, pie) {
            // piecancel(event, hammer, pie)
            //console.log("piecancel", "event", event, "hammer", hammer, "pie", pie);
            hammer.trigger("piecancel", event);
            if (hammer.options.trackPieCancel) {
                hammer.options.trackPieCancel.apply(this, [event, hammer, pie]);
            }
        },

        trackPieChange: function(event, hammer, pie) {
            // piechange(event, hammer, pie)
            //console.log("piechange", "event", event, "hammer", hammer, "pie", pie);
            hammer.trigger("piechange", event);
            if (hammer.options.trackPieChange) {
                hammer.options.trackPieChange.apply(this, [event, hammer, pie]);
            }
        },

        trackPieUpdate: function(event, hammer, pie) {
            // pieupdate(event, hammer, pie)
            //console.log("pieupdate", "event", event, "hammer", hammer, "pie", pie);
            hammer.trigger("pieupdate", event);
            if (hammer.options.trackPieUpdate) {
                hammer.options.trackPieUpdate.apply(this, [event, hammer, pie]);
            }
        },

        trackPieSelect: function(event, hammer, pie) {
            // pieselect(event, hammer, pie)
            //console.log("pieselect", "event", event, "hammer", hammer, "pie", pie);
            hammer.trigger("pieselect", event);
            if (hammer.options.trackPieSelect) {
                hammer.options.trackPieSelect.apply(this, [event, hammer, pie]);
            }
        },

        trackPieSliceChange: function(event, hammer, pie, slice) {
            // pieslicechange(event, hammer, pie, slice)
            //console.log("pieslicechange", "event", event, "hammer", hammer, "pie", pie, "slice", slice);
            hammer.trigger("pieslicechange", event);
            if (hammer.options.trackPieSliceChange) {
                hammer.options.trackPieSliceChange.apply(this, [event, hammer, pie, slice]);
            }
        },

        trackPieSliceUpdate: function(event, hammer, pie, slice) {
            // piesliceupdate(event, hammer, pie, slice)
            //console.log("piesliceupdate", "event", event, "hammer", hammer, "pie", pie, "slice", slice);
            hammer.trigger("piesliceupdate", event);
            if (hammer.options.trackPieSliceUpdate) {
                hammer.options.trackPieSliceUpdate.apply(this, [event, hammer, pie, slice]);
            }
        },

        trackPieSliceSelect: function(event, hammer, pie, slice) {
            // piesliceselect(event, hammer, pie, slice)
            //console.log("piesliceselect", "event", event, "hammer", hammer, "pie", pie, "slice", slice);
            hammer.trigger("piesliceselect", event);
            if (hammer.options.trackPieSliceSelect) {
                hammer.options.trackPieSliceSelect.apply(this, [event, hammer, pie, slice]);
            }
        },

        trackPieItemChange: function(event, hammer, pie, slice, item) {
            // pieitemchange(event, hammer, pie, slice, item)
            //console.log("pieitemchange", "event", event, "hammer", hammer, "pie", pie, "slice", slice, item);
            hammer.trigger("pieitemchange", event);
            if (hammer.options.trackPieItemChange) {
                hammer.options.trackPieItemChange.apply(this, [event, hammer, pie, slice, item]);
            }
        },

        trackPieItemUpdate: function(event, hammer, pie, slice, item) {
            // pieitemupdate(event, hammer, pie, slice, item)
            //console.log("pieitemupdate", "event", event, "hammer", hammer, "pie", pie, "slice", slice, "item", item);
            hammer.trigger("pieitemupdate", event);
            if (hammer.options.trackPieItemUpdate) {
                hammer.options.trackPieItemUpdate.apply(this, [event, hammer, pie, slice, item]);
            }
        },

        trackPieItemSelect: function(event, hammer, pie, slice, item) {
            // pieitemselect(event, hammer, pie, slice, item)
            //console.log("pieitemselect", "event", event, "hammer", hammer, "pie", pie, "slice", slice, "item", item);
            hammer.trigger("pieitemselect", event);
            if (hammer.options.trackPieItemSelect) {
                hammer.options.trackPieItemSelect.apply(this, [event, hammer, pie, slice, item]);
            }
        }

    };
})(window.Hammer);


////////////////////////////////////////////////////////////////////////
