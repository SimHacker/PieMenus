#!/bin/csh -f
#
# makemenu.sh
#
# Copyright (C) 1988
# By Don Hopkins, Human Computer Interaction Lab, University of Maryland
#
# Reads lines from stdin and makes a popup menu that prints those keys.
#
# usage makemenu.sh <name>
#
echo /$1Menu '['
/bin/awk '\
/^.*\.\.\.$/ { print "  (" $0 ") " substr($0,1,length($0)-3) "Menu" ; next } \
/^\{.*\}$/ { print "  (" substr($0,2,length($0)-2) ") " $0 ; next } \
{ print "  (" $0 ") {(" $0 "\\n) print}" } \
'
echo ']' /new DefaultMenu send def
