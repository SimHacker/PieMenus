(define-keymap "@menu-keymap")
(define-keymap "@point-menu-keymap")

(bind-to-key "@menu-keymap" "\e[m")
(bind-to-key "@point-menu-keymap" "\e[p")

(defun (list-menu-keys
    (#list-keys "*menu-keys*")
))

; #define-menu-keys is just like #define-special-keys, except that it stores
; the key definitions in *menu-keys* instead of *special-keys*.
; 
(defun (#define-menu-keys  %special-key-buffer
    (setq %special-key-buffer "*menu-keys*")
    (temp-use-buffer %special-key-buffer
	(erase-buffer)
    )
    (progn-args 1)
))


(use-local-map "@point-menu-keymap"
    (set-keymap-hook "#to-mouse-context")
)

(#define-menu-keys
    ; define the key label variables, whose values are the raw strings
    ; 
    (define-key "#menu"			    "\e[m"	"Menu")
    (define-key "#menu-100"		    "\e[m100\n" "Menu100")
    (define-key "#menu-101"		    "\e[m101\n" "Menu101")
    (define-key "#menu-102"		    "\e[m102\n" "Menu102")
    (define-key "#menu-103"		    "\e[m103\n" "Menu103")
    (define-key "#menu-104"		    "\e[m104\n" "Menu104")
    (define-key "#menu-105"		    "\e[m105\n" "Menu105")
    (define-key "#menu-106"		    "\e[m106\n" "Menu106")
    (define-key "#menu-107"		    "\e[m107\n" "Menu107")
    (define-key "#menu-108"		    "\e[m108\n" "Menu108")
    (define-key "#menu-109"		    "\e[m109\n" "Menu109")
    (define-key "#menu-110"		    "\e[m110\n" "Menu110")

    (define-key "#point-menu"		    "\e[p"	"PointMenu")
    (define-key "#point-menu-100"	    "\e[p100\n" "PointMenu100")
    (define-key "#point-menu-101"	    "\e[p101\n" "PointMenu101")
    (define-key "#point-menu-102"	    "\e[p102\n" "PointMenu102")
    (define-key "#point-menu-103"	    "\e[p103\n" "PointMenu103")
    (define-key "#point-menu-104"	    "\e[p104\n" "PointMenu104")
    (define-key "#point-menu-105"	    "\e[p105\n" "PointMenu105")
    (define-key "#point-menu-106"	    "\e[p106\n" "PointMenu106")
    (define-key "#point-menu-107"	    "\e[p107\n" "PointMenu107")
    (define-key "#point-menu-108"	    "\e[p108\n" "PointMenu108")
    (define-key "#point-menu-109"	    "\e[p109\n" "PointMenu109")
    (define-key "#point-menu-110"	    "\e[p110\n" "PointMenu110")
)

(bind-to-key "previous-line"		#menu-100)
(bind-to-key "previous-page"		#menu-101)
(bind-to-key "forward-character"	#menu-102)
(bind-to-key "next-page"		#menu-103)
(bind-to-key "next-line"		#menu-104)
(bind-to-key "end-of-file"		#menu-105)
(bind-to-key "backward-character"	#menu-106)
(bind-to-key "beginning-of-file"	#menu-107)

(bind-to-key "point-select"			#point-menu-100)
(bind-to-key "set-mark"				#point-menu-101)
