/*
 * Generic pie menu tracking code (optimized for speed)
 * Example pie menu layout code (optimized for screen space)
 *
 * Copyright (C) 1989 by Don Hopkins. All rights reserved.
 * This program is provided for unrestricted use, provided that this 
 * copyright message is preserved. There is no warranty, and no author 
 * or distributer accepts responsibility for any damage caused by this 
 * program. 
 */

/*
 * Doing:
 * Generic pie menu layout code
 */

/*
 * This supports pie menus with any number of items. 
 * Each item can be of any positive angular width, as long as
 * all the item widths add up to 360 degrees.
 * Each item records the quadrant and slope of its leading edge.
 * A leading edge is a ray going out from the menu center in
 * the direction given by an item's (quadrant,slope) pair (see below).
 * The area of a pie menu item is the wedge between its leading edge, 
 * and the leading edge of the next menu item.
 */

/* ------------------------------------------------------------------------ */

#include <math.h>
#define PI 3.1415926535897932
#define TWO_PI 6.2831853071795865

/* ------------------------------------------------------------------------ */

struct item {
  char *name;				/* Menu item name */
  char *data;				/* Data associated with menu item */
  int slice;				/* Desired relative slice size */
  /* The following are set by the layout procedure: */
  double angle;				/* Angle through center of slice */
  double cosine, sine;			/* Cosine and sine of angle */
  double subtend;			/* Angle subtended by slice */
  int x, y, width, height;		/* Label layout information */
  int quadrant;				/* Quadrant of leading edge */
  double slope;				/* Slope of leading edge */
};

struct menu {
  int x, y, width, height;		/* Menu position and size */
  double initial_angle;			/* Angle first slice is centered on */
  int label_radius;			/* Radius of labels from menu center */
  int label_radius_min;			/* Minimum value of label_radius */
  int label_radius_step;		/* Step for increasing label_radius */
  int center_x, center_y;		/* Menu center */
  int inactive_radius_squared;		/* Radius of menu center ^2 */
  int current;				/* Index of current item (or -1) */
  int items;				/* Number of items */
  struct item *item;			/* Pointer to array of items */
};

/* ------------------------------------------------------------------------ */

/*
 * The slope is defined such that it is greater than or equal to zero,
 * less than infinity, and increasing counter-clockwise around the menu. 
 * Each of the four quadrants encompasses one range of slope.
 *
 *                 Y
 *               ^
 *               |     x>0, y>=0
 *  x<=0, y>0 <--+       y/x
 *    -x/y       |        ^
 *        quad 1 | quad 0 |     X
 * -----+--------+--------+----> 
 *      | quad 2 | quad 3
 *      V        |      -x/y
 *   x<0, y<=0   +--> x>=0, y<0
 *     y/x       |
 *               |
 * 
 * The quadrants and slopes of the item edges are all precalculated,
 * during menu layout.
 * The quadrant and slope of the cursor must be calculated frequently
 * during menu tracking, so we just calculate the numerator and
 * denominator of the slope, and avoid an unnecessary division.
 * Instead of calculating "slope = numerator / denominator" then
 * testing "slope < it->slope", every time the cursor moves, we can
 * just test "numerator < (denominator * it->slope)".
 */

#define abs(x) ((x) < 0 ? -(x) : (x))

#define QUADRANT_SLOPE(x, y, quadrant, numerator, denominator) \
    if ((y) > 0) (quadrant) = ((x) > 0 ? 0 : 1); \
    else if ((y) < 0) (quadrant) = ((x) < 0 ? 2 : 3); \
    else (quadrant) = ((x) > 0 ? 0 : 2); \
    if ((quadrant) & 1) { \
	(numerator) = abs((x)); (denominator) = abs((y)); \
    } else { \
	(numerator) = abs((y)); (denominator) = abs((x)); \
    }

int calc_quadrant_slope(x, y, numeratorp, denominatorp)
  register int x, y;
  int *numeratorp, *denominatorp;
{
    register int quadrant;

    /*
     * Calculate quadrant number.
     */
    if (y > 0)
	quadrant = (x > 0 ? 0 : 1);
    else if (y < 0)
	quadrant = (x < 0 ? 2 : 3);
    else /* y == 0 */
	quadrant = (x > 0 ? 0 : 2);

    /*
     * Calculate slope such that it's always positive, increasing
     * clockwise.
     */

    if (quadrant & 1) {
	*numeratorp = abs(x);
	*denominatorp = abs(y);
    } else {
	*numeratorp = abs(y);
	*denominatorp = abs(x);
    }

    return(quadrant);
}

/* ------------------------------------------------------------------------ */

int calc_pie_menu_item(menu, x, y)
  struct menu *menu;
  int x, y;
{
    register int i, order, quadrant;
    int numerator, denominator;
    register struct item *it;
    int first, last_i, last_order;

    /*
     * Translate x and y so they are relative to the menu center.
     */

    x -= menu->center_x;
    y -= menu->center_y;

    /*
     * If there are no menu items,
     * or we are within the inactive region in the menu center,
     * then there is no item selected.
     */
    if ((menu->items == 0) ||
	((x * x) + (y * y) < menu->inactive_radius_squared))
        return(menu->current = -1);

    /*
     * If there's only one item, then that must be it. 
     */
    if (menu->items == 1)
        return(menu->current = 0);

    /*
     * Calculate the quadrant, slope numerator, and slope denominator of
     * the cursor slope, to be used for comparisons.
     */
/*
    quadrant = calc_quadrant_slope(x, y, &numerator, &denominator);
*/
    QUADRANT_SLOPE(x, y, quadrant, numerator, denominator);

    /*
     * In most cases, during cursor tracking, the menu item that the
     * cursor is over will be the same as it was before (almost all
     * of the time), or one of the neighboring items (most of the
     * rest of the time). So we check those items first. But to keep
     * things simple, instead of actually checking the items in order of
     * frequency (the current, the two neighbors, then the rest), we just
     * start our loop around the menu items at the item *before* the
     * last selected menu item, so we still check the three most common
     * cases first (neighbor, current, neighbor, rest), without having 
     * to complicate the code with special cases. Another strategy, that
     * might be good for menus with ridiculously many items, would be
     * [to check the current item first, then the two neighbors, then]
     * to do a binary search of the menu items (since they are ordered).
     * But that's more complicated and you shouldn't have that many menu
     * items anyway.
     */

    /*
     * Start at the item before current one.
     */
    first = menu->current - 1;
    if (first < 0)
        first = menu->items - 1;

    /*
     * Initialize last_order such that we will go through the loop
     * at least once, validating last_i and last_order for the next
     * time through the loop.
     */
    last_i = last_order = -1;

    i = first;
    while (1) {
        it = &menu->item[i];

/* Legend: c = cursor, e = edge
   <cursor quad>,<edge quad>
         quad 1 | quad 0
         -------+-------
         quad 2 | quad 3
*/

        /* Set order = 1, if shortest direction from edge to cursor is ccw */
        switch ((quadrant - it->quadrant) & 3) {

case 0: /*
	 0,0	 1,1	 2,2	 3,3
	  |ce	ce|	  |	  |	
	--+--	--+--	--+--	--+--	
	  |	  |	ce|	  |ce
*/
	    /* slope >= it->slope */
	    order = ((double)numerator >= (double)(denominator * it->slope));
	    break;

case 1: /*
	 1,0	 2,1	 3,2	 0,3
	 c|e	 e|	  |	  |c
	--+--	--+--	--+--	--+--	
	  |	 c|	 e|c	  |e
*/
	    order = 1;
	    break;

case 2: /*
	 2,0	 3,1	 0,2	 1,3
	  |e	 e|	  |c	 c|
	--+--	--+--	--+--	--+--	
	 c|	  |c	 e|	  |e
*/
	    /* slope < it->slope */
	    order = ((double)numerator < (double)(denominator * it->slope));
	    break;

case 3: /*
	 3,0	 0,1	 1,2	 2,3
	  |e	 e|c	 c|	  |
	--+--	--+--	--+--	--+--	
	  |c	  |	 e|	 c|e
*/
	    order = 0;
	    break;
	}

	/*
	 * If we were counter-clockwise of the last leading edge,
	 * and we're clockwise of this leading edge,
	 * then we were in the last menu item.
	 * (Note: first time through this loop last_order = -1 so we'll
	 * go back through the loop at least once, after validating
	 * last_order and last_i.)
	 */
	if (last_order == 1 && order == 0)
	    return(menu->current = last_i);
	last_order = order;

	/*
	 * Remember this menu item index, and move on to the next one
	 * counter-clockwise around the circle.
	 */
	last_i = i;
	if (++i >= menu->items)
	    i = 0;

	/* 
         * If we've checked all the others, then that must have been it. 
	 * This saves us from checking the leading edge of the first
	 * item again (It's also insurance against layout bugs.)
	 */
	if (i == first)
	    return(menu->current = last_i);
    }
}

void layout_pie_menu(menu)
  struct menu *menu;
{
  int i;
  int total_slice, radius, border = 2;
  struct item *it, *last;
  double angle;

  /*
   * Calculate the sum of the menu item slice sizes.
   * Each menu item will get a (slice / total_slice) sized slice of the pie.
   */
  total_slice = 0;
  for (i=0; i < menu->items; i++) {
    total_slice += menu->item[i].slice;
  }

  /*
   * Calculate the subtend, angle, cosine, sine, quadrant, and slope
   * of each menu item.
   */
  angle = menu->initial_angle;
  for (i=0; i < menu->items; i++) {
    register double cosine, sine, numerator, denominator;
    register int quadrant;

    it = &menu->item[i];
    it->subtend = TWO_PI * it->slice / total_slice;
    if (i) angle += it->subtend / 2;
    it->angle = angle;
    it->cosine = cos(angle);
    it->sine = sin(angle);
    cosine = cos(angle - it->subtend / 2);
    sine = sin(angle - it->subtend / 2);
    QUADRANT_SLOPE(cosine, sine, quadrant, numerator, denominator);
    it->quadrant = quadrant;
    it->slope = (double)numerator / (double)denominator;
    angle += it->subtend / 2;
  }

  radius = menu->label_radius_min;
  last = &menu->item[menu->items - 1];
  for (i=0; i <= menu->items; i++) {
    double cosine, sine, lcosine, lsine;
    int width, height, lwidth, lheight;

    it = &menu->item[i == menu->items ? 0 : i];

    cosine = it->cosine;  sine = it->sine;
    width = it->width;  height = it->height;
    lcosine = last->cosine;  lsine = last->sine;
    lwidth = last->width;  lheight = last->height;
    while (1) {
      register int x, y, lx, ly,
      		   x0max, y0max, x1min, y1min;

      x = cosine * radius;  y = sine * radius;
      lx = lcosine * radius; ly = lsine * radius;

      /* Translate x y with respect to label width */
      /* Here we just center on the label -- could be nicer. */
      x -= width/2; y -= height/2;
      lx -= lwidth/2; ly -= lheight/2;

      /* Do rects (x y width height) and (lx ly lwidth lheight) overlap? */
      x0max = (x > lx ? x : lx) + border;
      y0max = (y > ly ? y : ly) + border;
      x1min = (x+width < lx+lwidth ? x+width : lx+lwidth) - border;
      y1min = (y+height < ly+lheight ? y+height : ly+lheight) - border;
      if (!(x0max<x1min && y0max<y1min)) { /* If they don't overlap */
	/* They are far enough out so they don't overlap, move on. */
	break;
      }
      /* Push the menu radius out a step and try again */
      radius += menu->label_radius_step;
    }
    /* Loop on to next menu item */
    last = it;
  }
}

/* ------------------------------------------------------------------------ */

#ifdef notdef

struct item Items[] = {
  {0, 0.0},
  {1, .2},
  {2, .3},
  {3, .4},
  {3, .5},
  {3, .6},
  {3, .7},
  {3, 10.0},
};

struct menu Menu = {
  32, 32,
  16,
  -1,
  8,
  Items
};

main()
{
  int x, y, i;

  for (y=63; y>=0; y--) {
    for (x=0; x<64; x++) {
      i = calc_pie_menu_item(&Menu, x, y);
      putchar(" 0123456789abcdefghijklmnopqrstuvwxyz"[i + 1]);
    }
    putchar('\n');
  }
}

#endif
