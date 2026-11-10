var radius=40;
var basex = 300;
var basey = 300;
var hidemenu = false;
var alreadyredrawn = false;
var lastx =0;
var lasty=0;

// Decide browser version
var ns4 = (document.layers)? true:false;
var ns6 = (document.getElementById)? true:false;
var ie4 = (document.all)? true:false;
var ie5 = false;

function eatmenu() {
	hidemenu = false;
	document.all.circleArea.innerHTML = "";
	alreadyredrawn = false;
}

if ( (ns4) || (ie4) || (ns6)) {
	document.onmousemove = mouseMove
	if (ns4) document.captureEvents(Event.MOUSEMOVE)
} 

function mouseMove(e) {
	lastx=event.x+document.body.scrollLeft; 
	lasty=event.y+document.body.scrollTop;
	//if ( (ns4) || (ns6) ) {lastx=e.pageX; lasty=e.pageY;}
	//if (ie4) {lastx=event.x; lasty=event.y;}
	//if (ie5) {lastx=event.x+document.body.scrollLeft; lasty=event.y+document.body.scrollTop;}

}

function showmenu (srcele, mylinks, mylabels, startangle, endangle, radius) {
	minx=9999;
	miny=9999;
	maxx=0;
	maxy=0;
	if(hidemenu) { clearTimeout(hidemenu); hidemenu = false;}
	document.all.circleArea.innerHTML ="";
	//alert(document.body.scrollTop);
	str = document.body.createTextRange();
	str.moveToElementText(srcele); 
	//alert(srcele);
	//basex = str.offsetLeft + (str.boundingWidth /2);
	basex = lastx;
	//alert(basex);
	//basey = str.offsetTop + document.body.scrollTop ;
	basey = lasty - (lasty - str.offsetTop );
	//alert(basex + ' ' + basey);
	links = mylinks.split("|");
	labels = mylabels.split("|");
	numeles = labels.length - 1;
	if(!radius)	radius = 20 + numeles*8;
	mydiv = "";
	angleIncrement = 360/(numeles+1) ;
	angle=30;
	redraw = false;

	//alert(numeles);
	for(i=0;i<=numeles;i++) {
		trans = Math.PI / 180;
		if(startangle != 0 && i == 0){
			angle = startangle;
			angleIncrement = angleIncrement * .75;
		}
		if(endangle != 360 && i == 0){
			angle = startangle;
			angleIncrement = angleIncrement * .75;
		}
		
		x = radius * Math.cos(trans * angle);
		y = radius * Math.sin(trans * angle);
		//alert(angle + " " + x + "," + y)
		mydiv = '<div id="d' + i + '"';
		mydiv = mydiv + ' onmouseover="if(hidemenu){clearTimeout(hidemenu);hidemenu=false;}" ';
		mydiv = mydiv + 'style="background-color: cccccc; filter: alpha(opacity=90);z-index:99;position:absolute;width:auto;left:';
		newx = basex + x;
		newy = basey - y;
		//alert(links);labels[i-1]
		// Capture bounding area of all menu items for controller div
		if (newx<minx) minx=newx;
		if (newy<miny) miny=newy;
		mydiv += newx + ';top:' + newy + ';"';
		
		mydiv += '><A HREF="' +  links[i] + '">'  + labels[i]  +  '</a></div>';
		//mydiv += '><A HREF="' +  links[i] + '">'  + labels[i] + angle +  '</a></div>';
		//alert(mydiv);
		
		document.all.circleArea.innerHTML = document.all.circleArea.innerHTML + mydiv;
		//adjust alignment
		mydiv = eval("document.all.d" + i);
		tr = document.body.createTextRange();
		tr.moveToElementText(mydiv);
		//alert(tr.boundingWidth);
		
		// Adjust Controller Div size Horizontal
		if(maxx < (tr.offsetLeft + parseInt(tr.boundingWidth))) maxx = tr.offsetLeft + parseInt(tr.boundingWidth);
		
		if(maxy < (tr.offsetTop + document.body.scrollTop + parseInt(tr.boundingHeight))) maxy  = tr.offsetTop + document.body.scrollTop + parseInt(tr.boundingHeight);
		

		
		// Adjust position for elements on top
		if((angle > 250 && angle < 310) || (angle > 75 && angle < 120))
	 	{
			//alert('adjusting' + i);
			mydiv.style.left = parseInt(mydiv.style.left) - (parseInt(tr.boundingWidth)/2);
		} else {
			
			if(x<0 && parseInt(tr.boundingWidth)) {
				//alert('adjusting');
				newx =  parseInt(mydiv.style.left) - parseInt(tr.boundingWidth);
				mydiv.style.left = newx;
				if(minx > newx) minx=newx;
			}
		}
		//alert('adjusting');
		if(mydiv.style.top >= basey) {
			newy = parseInt(mydiv.style.top) - parseInt(tr.boundingHeight);
			mydiv.style.top = newy;
		}
		if(newy < 0 || newy > document.body.clientHeight  || newx < 0 || newx > document.body.clientWidth) {
			if(angle > 0 && angle < 90) {
				startangle = 90;
				radius = radius + radius/4;
				redraw = true;
				}
			if(angle > 90 && angle < 180) {
				startangle = 180;
				endangle = 90;
				radius = radius + radius/4;
				redraw = true;
			}
			if(angle > 180 && angle < 270) {
				endangle = 180;
				startangle = 270;
				radius = radius + radius/4;
				redraw = true;
			}
			if(angle > 270 && angle < 360) {
				startangle = 360
				endangle = 270;
				radius = radius + radius/4;
				redraw = true;
			}
			
		}
		// pagewidth = document.body.clientWidth;
	    //pageheight = document.body.clientHeight;
		angle += angleIncrement;
	}
	if(redraw && !alreadyredrawn) {
		showmenu(srcele, mylinks, mylabels, startangle, endangle, radius)
		alreadyredrawn = srcele;
	} else {
		
		width = maxx - minx;
		height = maxy - miny;
		controllerdiv = '<div id="controller" style="position:absolute;top:';
		controllerdiv = controllerdiv + Math.round(miny) + ';left:' + Math.round(minx) + ';width:' + Math.round(width) + ';height:' + Math.round(height) + ';';
		//controllerdiv = controllerdiv + 'background-color:cccccc;';
		controllerdiv = controllerdiv + '" onmouseout="hidemenu=window.setTimeout(\'eatmenu()\',2200)';
		controllerdiv = controllerdiv + '">&nbsp;</div>';
		//alert(controllerdiv);
		document.all.circleArea.innerHTML = controllerdiv + document.all.circleArea.innerHTML;
		//alert('done');
		//hidemenu = window.setTimeout("eatmenu()",20000);
	}
}


