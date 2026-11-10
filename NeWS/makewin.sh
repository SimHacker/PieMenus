#!/bin/csh -f
#
# makemenu.sh
#
# Copyright (C) 1988
# By Don Hopkins, Human Computer Interaction Lab, University of Maryland
#
# Emits the PS code to make and shape a window.
#
# usage makewindow.sh <name>
#
cat <<EOF
/win framebuffer /new DefaultWindow send def
/reshapefromuser win send
{
  /FrameLabel ($1) def
  /IconLabel ($1) def
  userdict /ClientMenu known {
    /ClientMenu userdict /ClientMenu get def
  } if
} win send
/map win send
/can /GetCanvas win send def
can setcanvas
EOF
