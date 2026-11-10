; NeWS.ml   initialization file for Emacs running PostScript driver
;
;		Copyright (c) 1988 UniPress Software, Inc.
; 
; psh starts an interactive connection to the display server, similar to a
; shell window.  execute-psh-buffer sends the current buffer through the psh
; session, as if you had typed it.  You can then type into the psh session
; to find out what your program has done, inquire the settings of variables,
; etc.
;
; FUNCTIONS
;
;Communicating with the display server
; 
; psh			Starts an interactive connection to the display server
;			in a window, or pops to its listener window if it is
;			already started.  Input that you type into the window
;			is sent to the server, and output that it generates is
;			collected in the buffer where you may edit it.  The
;			display server connection that this opens is different
;			from the one that the Emacs session is already using
;			as its terminal connection.  The program started is
;			named by the variable psh-program ("psh" by default).
;			When it is first started, the string given by
;			psh-init-string ("executive\n" by default) is sent to
;			it.  The name of the listener buffer is given by
;			psh-buffer ("psh" by default).
; 
; execute-psh-buffer	Sends the contents of the current buffer to the
;			interactive psh listener running in the buffer psh.
;
; execute-psh-string	Sends the given string to the psh listener.
; 
; execute-psh-def	Sends the def around dot to the psh listener.
;
; 
; execute-ps-buffer	Sends the contents of the current buffer to the current
;			terminal's server connection.  This will execute the
;			ps code in the buffer with the current frame's canvas
;			as the current canvas, in the context of the frame
;			object itself.  The userdict available to the code in
;			this context is shared by all frames on that terminal
;			(server connection).  Your code can use either of the
;			two matrices, which are defined in all EmacsFrames.
;			
;			cellMatrix setmatrix    for char cell grid (TextFrame)
;			    
;			showMatrix setmatrix	used for show'ing text
;
; execute-ps-string	Sends the given string to the current terminal's
;			server connection.
; 
; execute-ps-def	Sends the def around dot to the current terminal's
;			server connection.  This will make it available to
;			all the frames on that terminal.
;
; 
; ps-autoload		causes the display server to load the given file if
;			the given name is not yet defined in the current
;			terminal's server connection.
; 
; ps-load file		causes the display server to load the given file.
;
; 
;Functions that affect frames:
; 
; toggle-cursor		tells the current frame not to display the cursor that
;			marks the insertion point.  The cursor will appear or
;			disappear immediately, without waiting for the next
;			display update.
; 
; set-font-fixed-pitch	tells the current frame to consider that its font is
;			fixed pitch if its argument is true, or that it is not
;			fixed pitch if the argument is false.  The return
;			value reflects whether the frame had previously
;			thought its font was fixed pitch (nonzero return
;			value) or not fixed pitch (zero return value).  This
;			is useful for correcting the misapprehension
;			entertained by the server that Courier is a
;			variable-pitch font, for example.  The frame can
;			repaint itself faster if it thinks its font is fixed
;			pitch.
;
; set-default-frame-class   Arranges that frames created hereafter will
;			    be of the given PS class.  The name given must
;			    name a subclass of EmacsFrame that has been
;			    defined in the display server.  The default
;			    frame class is MainEmacsWindow.  If given
;			    arguments beyond the first,
;			    set-default-frame-class sets the class, progns
;			    its args, and reverts to the previous class.
; 
; VARIABLES
; 
; ps-pass-file		If nonzero, the execute-ps-xxx functions will send
;			things to the display server by writing them to a file
;			and telling the server to read that file.  This
;			requires that ps-common-directory be set.
;
; ps-common-file	Names a file, on a filesystem shared by the display
;			server and Emacs, that will be used to pass data
;			between them.  The file must be accessible by both the
;			Emacs session and by the display server.  If you don't
;			define ps-common-file yourself, it will be set to name
;			some file in the directory named by
;			ps-common-directory (which in turn takes its default
;			from the environment variable PRSTMPDIR)
;			
; ps-common-directory	OBSOLETE
;			Names a directory, on a filesystem shared by the
;			display server and Emacs, that will be used to pass
;			data between them.  The directory must be one that
;			both the Emacs session and the display server can get
;			at.  If you don't set this yourself, it will take its
;			value from the environment variable PRSTMPDIR.
;
; ps-library-directory	Names a directory wherein Emacs may find the ps
;			libraries that it tries to load into the display
;			server (or printer, etc.).  If you don't set this
;			yourself, it will take its value from the environment
;			variable PSLIBDIR.
;
; psh-program		Names the program run by psh.
;
; psh-init-string	psh sends this to the program when it first starts.
;
; psh-buffer		Names the psh listener 
; 
;----------------------------------------------------------------------

(if (! (is-function-bound "ps-engine-type"))
    (defun (ps-engine-type -1))
)

; Discern the type of NeWS server, and configure appropriately...

(if (= (ps-engine-type) 0) ; Sun
    (progn
	(setq use-ins/del-line/char 1)
    )
    (= (ps-engine-type) 1) ; SGI 4Sight
    (progn
	(setq use-ins/del-line/char 0)
;	(setq-default visible-bell 0) ; Fixed this bug -Don
    )
)

; This use of ps-common-directory to ensure a common file accessible to both
; the server and the client will not work in general.  We really need to fix
; the bug that keeps channels from sending long strings, so we could just send
; the string directly to the server instead of putting in a file and telling
; the server to load it.
; 
;   Actually, the bug has been fixed as of 16-Aug-88, so we'll phase this
;   stuff out.  -mg
; 
(if (error-occurred (setq-default ps-common-directory (getenv "PSTMPDIR")))
    (setq-default ps-common-directory "/tmp/")
)
(if (error-occurred (setq-default ps-library-directory (getenv "PSLIBDIR")))
   (setq-default ps-library-directory (concat emacs-library-directory "/ps/"))
)

(setq-option-default ps-common-file (concat ps-common-directory
					"/emacs-draw-" (users-login-name)))

(setq-option-default interactive-process-icon	    "nautilus")
(setq-option-default non-interactive-process-icon   "bulb_1")
(setq-option-default shell-icon			    "nautilus")
(setq-option-default elec-c-mode-icon		    "paper")
(setq-option-default elec-mlisp-mode-icon	    "clown")
(setq-option-default text-mode-icon		    "textedit")
(setq-option-default remote-shell-icon		    "client")

; This gets around a bug wherein string-to-process fails to send strings
; longer than 256 characters (or thereabouts).
; 
(setq-option-default ps-pass-file	0)
(setq-option-default psh-pass-file	0)

(setq map-meta-to-prefix 1)	; meta-chars (> 127) => ESC-<c&0x7f>

(setq-default font-dir "/usr/NeWS/fonts")

;(open-font "Screen.16")		; font 1
;(open-font "Courier.10")
;(open-font "Screen-Bold.15")
;(open-font "Courier-Bold.12")
;(open-font "Icon")

(load "mouse.ml")

;----------------------------------------------------------------------
; menu utilities
;----------------------------------------------------------------------

(defun (save-window-layout-key
      (save-window-layout "*window-layout*")
))
(defun (restore-window-layout-key
      (restore-window-layout "*window-layout*")
))
(defun (c-mode
      (if (error-occurred (cmacs-mode))
	  (elec-c-mode))
))

;----------------------------------------------------------------------
; obsolete menu support
;----------------------------------------------------------------------

(defun (system-menu	$i  $title $label $itemnum  menu-selection-number
    (setq $title (arg 1))
    (setq menu-selection-number -1)
    (temp-use-buffer "*menu*"
	(erase-buffer)
	(insert-string "[\n")
    )
    ; insert even-numbered args starting with arg 2
    (while (< (+= $i 2) (nargs))
	(setq $label (arg $i))
	(temp-use-buffer "*menu*"	    
	    (end-of-file)
	    (insert-string 
		"(" $label ")"
		"\t{{BUCKY (" (++ $itemnum) "\\n)}menustring}\n")
	)
    )
    
    (temp-use-buffer "*menu*"
	(end-of-file)
	(insert-string
	    "] /new EmacsDefaultSystemMenu send\n"
	    "/popup exch {send} curDsp send\n")
	(execute-ps-buffer)
    )
    (setq menu-selection-number
	  (get-tty-string (concat  "Menu: " (arg 1) " ")))
    (if (> menu-selection-number 0)
	(progn
	    (setq menu-selection-string
			    (arg (* 2 menu-selection-number)))
	    (arg (+ (* 2 menu-selection-number) 1))
	)
    )
))


;----------------------------------------------------------------------
; text selection support
;----------------------------------------------------------------------

(setq-default %selection-dot% 0)
(setq-default %selection-buffer% "")
(setq-default %selection-frame% 0)

(setq-default %last-selection-dot% 0)
(setq-default %last-selection-buffer% "")
(setq-default %last-selection-state% 0)

;
; Some day: remember what window we're in and make sure we're in that
; window on drag and button up.
; 
(defun (drag-selection
    (dot-to-mouse)
    (setq mouse-drag-cursor 2)
    (sit-for 0)
    (if show-intermediate-drag
	(set-mouse-drag-hook "#drag-selection-hook")
	(set-mouse-button-up-hook "#select-region-hook")
    )
))

(defun (#drag-selection-hook
    (if (= mouse-drag-cursor 1)
	(start-selection))
    (setq mouse-drag-cursor 2)
    (error-occurred (dot-to-mouse))
    (sit-for 0)
))

(defun (mouse-select-region
    (send-window-line-widths)
;    (start-selection)
    (drag-selection)
    (setq mouse-drag-cursor 1)
    (set-mouse-button-up-hook "#select-region-hook")
    (push-mark)
))

(setq-default select-single-characters 0)

(defun (#select-region-hook
    (finish-selection)
    (error-occurred (dot-to-mouse))
	(if (= (dot) (mark))
	    (if (& (= %last-selection-dot% (dot))
		   (= %last-selection-buffer% (current-buffer-name)))
		(if (= (++ %last-selection-state%) 1)
		    (select-word)
		    (= %last-selection-state% 2)
		    (select-line)
		    (= %last-selection-state% 3)
		    (progn (select-paragraph)
			   (setq %last-selection-state% 0)))
		(progn (pop-mark)
		       (setq %last-selection-dot% (+ (dot)))
		       (setq %last-selection-buffer% (current-buffer-name))
		       (setq %last-selection-state% 0))
	    )
	    (progn (select-region)
		   (message-for 0 "Selected region."))
	)
    (flash-selection)
))

(defun (mouse-extend-region
    (reselect-region)
    (extend-selection)
    (drag-selection)
    (set-mouse-button-up-hook "#extend-region-hook")
))

(defun (#extend-region-hook
    (#drag-selection-hook)
    (finish-selection)
    (select-region)
    (message-for 0 "Extended region.")
    (flash-selection)
))

(defun (start-selection
    (execute-ps-string "start-selection")
))

(defun (extend-selection
    (send-window-line-widths)
    (execute-ps-string "extend-selection")
))

(defun (outline-selection
    (execute-ps-string "outline-selection")
))

(defun (finish-selection
    (execute-ps-string "finish-selection")
))

; Renamed from set-selection-region!
(defun (send-window-line-widths
    (setq %selection-dot% (+ (dot)))
    (setq %selection-buffer% (current-buffer-name))
    (setq %selection-frame% (current-frame))
    (temp-use-buffer "*window-line-widths*"
	(erase-buffer)
 	(insert-string (concat (window-line-widths) 
			       " window-line-widths\n")))
    (execute-ps-string (buffer-to-string "*window-line-widths*"))
))

(defun (fake-selection &fs-x1 &fs-y1 &fs-x2 &fs-y2
    (setq &fs-x1 (arg 1 "x1: "))
    (setq &fs-y1 (arg 2 "y1: "))
    (setq &fs-x2 (arg 3 "x2: "))
    (setq &fs-y2 (arg 4 "y2: "))
    (execute-ps-string
	(concat &fs-x1 " " &fs-y1 " " &fs-x2 " " &fs-y2 " fake-selection"))
))

(defun (reselect-region
    &rr-x1 &rr-y1 &rr-x2 &rr-y2
    &rr-window-top &rr-line-top
    (if (error-occurred (mark)) (set-mark))
    (update-dot-x-y)
    (setq &rr-x1 (get-dot-x))
    (setq &rr-y1 (- (get-dot-y) 1))
    (save-excursion
	(exchange-dot-and-mark)
	(if (dot-is-visible)
	    (progn (can-one-line-opt)
		   (update-dot-x-y)
		   (setq &rr-x2 (get-dot-x))
		   (setq &rr-y2 (- (get-dot-y) 1)))
	    (if (< (dot) (mark))
		(progn (setq &rr-x2 -1000)
		       (setq &rr-y2 -1000))
		(progn (setq &rr-x2 1000)
		       (setq &rr-y2 1000)))))
    (send-window-line-widths)
    (fake-selection &rr-x1 &rr-y1 &rr-x2 &rr-y2)
    (select-region)
))

(defun (select-word 
    (beginning-of-word)
    (set-mark)
    (end-of-word)
    (exchange-dot-and-mark)
    (message-for 0 "Selected word.")
    (blink-selection)
    (save-command-context)
))

(defun (select-line
    (beginning-of-word)
    (end-of-word) ; to select line the selected word is on
    (beginning-of-line)
    (set-mark)
    (end-of-line)
    (if (! (eobp))
	(forward-character))
    (exchange-dot-and-mark)
    (save-command-context)
    (message-for 0 "Selected line.")
    (blink-selection)
    (save-command-context)
))

(defun (select-paragraph
    (beginning-of-word)
    (end-of-word) ; to select paragraph the selected word is on
    (mark-paragraph)
    (message-for 0 "Selected paragraph.")
    (blink-selection)
    (save-command-context)
))

; (defun (select-buffer
;     (beginning-of-file)
;     (set-mark)
;     (end-of-file)
;     (blink-selection)
;     (save-command-context)
; ))

; Selection feedback

(setq-option-default select-flash-interval 3)
(setq-option-default select-flashes 0)

(defun (flash-selection &fs-flips &fs-top
    (if (& select-flashes
	   select-flash-interval
	   (! (replaying-keyboard-macro)))
	(save-excursion
	    (if (error-occurred (mark)) (set-mark))
	    (exchange-dot-and-mark)
	    (setq &fs-flips (+ select-flashes 1))
	    (if (dot-is-visible)
		(while (> (-- &fs-flips) 0)
		       (progn
			     (sit-for select-flash-interval)
			     (exchange-dot-and-mark)
			     (sit-for select-flash-interval)
			     (exchange-dot-and-mark)
		))
		(save-excursion
		    (setq &fs-top (< (dot) (mark)))
		    (set-mark)
		    (if &fs-top
			(progn (end-of-line)
			       (if (= (current-column) 1)
				   (progn (next-line)
					  (end-of-line))))
			(progn (beginning-of-line)
			       (if (looking-at "\n")
				   (progn (previous-line)
					  (beginning-of-line)))))
		    (message (region-to-string))
		)
	    )
	    (exchange-dot-and-mark)
	    (sit-for 0)
	)
    )
))

(defun (show-selection
    (execute-ps-string "show-selection")
))

(defun (hide-selection
    (execute-ps-string "hide-selection")
))

(setq-default select-blinks 1)
(setq-default select-blink-interval 10)
(setq-default select-blink-fill 0)

(defun (blink-selection &bs-blink
    (if (> select-blinks 0)
	(progn 
	       (reselect-region)
;	       (toggle-cursor)
	       (sit-for 0)
	       (while (< &bs-blink select-blinks)
		      (if select-blink-fill
			  (show-selection)
			  (outline-selection))
		      (sit-for select-blink-interval)
		      (if select-blink-fill
		          (hide-selection)
			  (finish-selection))
		      (if (< (++ &bs-blink) select-blinks)
			  (sit-for select-blink-interval))
	       )
;	       (toggle-cursor)
	       (flush-terminal-output)
    ))
))

(defun (select-region
    (if (error-occurred (mark)) (set-mark))
    (set-primary-selection (region-to-string))
))

(defun (set-primary-selection &sps-string
    (setq &sps-string (arg 1 ": set-primary-selection "))
;    (execute-ps-string
;	(concat "(" (quote-postscript-string &sps-string) ")"
;		"set-primary-selection\n"))
    (send-cps-string &sps-string)
    (send-string-to-terminal "set-primary-selection\n")
))

(defun (primary-selection-to-shelf
    (send-string-to-terminal "primary-selection-to-shelf\n")
))

(defun (stuff-primary-selection
    (send-string-to-terminal "stuff-primary-selection\n")
))

(defun (stuff-shelf-selection
    (send-string-to-terminal "stuff-shelf-selection\n")
))

(bind-to-key "mouse-select-region"	    #mouse-left)
(bind-to-key "mouse-extend-region"	    #mouse-shift-left)


;----------------------------------------------------------------------
; color support
;----------------------------------------------------------------------

(defun (#color-description
    (concat "(error-occurred (execute-ps-string "
	    (quote-mlisp-string
		(concat "{" (get-color-scheme)
	        "ColorScheme /sendcolorscheme parentFrame send"
	        "} curDsp send\n")
	    )
	    "))\n"
    )
))

;----------------------------------------------------------------------
; menu support
;----------------------------------------------------------------------

; Menu "keys" are sequences sent when the user selects a menu choice.
; They are treated the same way special keyboard keys are, so you can
; ask for help on them, rebind them, etc.  
; 
(define-keymap "@menu-keymap")
(define-keymap "@menu-point-keymap")

(defun (list-menu-keys
    (#list-keys "*menu-keys*" (interactive))
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

;    define the key label variables, whose values are the raw strings
(#define-menu-keys
    (define-key "#menu"			    "\e[m"	"Menu")
    (define-key "#menu-point"		    "\e[p"	"MenuPoint")
)

(bind-to-key "@menu-keymap"	    "\e[m")
(bind-to-key "@menu-point-keymap"   "\e[p")

(use-local-map "@menu-point-keymap"
  (set-keymap-hook "#to-mouse-context")
)

; Menu loading functions

; do-load-menu <menu-name> <load-ps>
; 
; The first arg names the menu resource to load.
; If the second arg is true, loads the PS code defining the menu into NeWS.
; In any case, loads the menu key bindings into Emacs.
; 
(defun (do-load-menu
    $dlm-load-ps $dlm-ps $dlm-bindings
  (save-excursion
    (save-window-excursion
      (setq $dlm-load-ps (arg 2))
      (with-node (concat "(menu)" (arg 1))
	  (if $dlm-load-ps
	      (setq $dlm-ps (resource-to-string "MenuPS")))
	  (setq $dlm-bindings (resource-to-string "MenuBindings"))
      )
      (if $dlm-load-ps
	  (execute-ps-string $dlm-ps)
      )
      (execute-mlisp-line $dlm-bindings)
    )
  )
))

; load-menu [<menu-name>] loads the definition of the given menu into the
; display server, and loads the menu's key bindings into Emacs.  If the menu
; name is not given, the menu defined by the current info node is used.
; 
(defun (load-menu
    (do-load-menu
	(if (nargs) (arg 1 ": load-menu (menu) ") &info-current-node)
	1
    )
))

; load-menu-bindings [<menu-name>] loads the key bindings of the given menu
; into Emacs.  If the menu name is not given, the menu defined by the current
; info node is used.
; 
(defun (load-menu-bindings
    (do-load-menu
	(if (nargs) (arg 1 ": load-menu-bindings (menu) ") &info-current-node)
	0
    )
))

; show-menu <menu-name> pops up the given menu so the user can choose
; something from it.  If the menu name is not given, or is null, shows the
; menu defined by the current info node.
; 
(defun (show-menu $sm-name
    (setq $sm-name (if (nargs) (arg 1 ": show-menu (menu) ") ""))
    (if (! (length $sm-name))
	(setq $sm-name &info-current-node)
    )
    (execute-ps-string
	(concat " gsave framebuffer setcanvas currentcursorlocation /showat "
		"menu:" $sm-name
		" { send } curDsp send grestore\n"))
    (flush-terminal-output)
))

; (set-menu "MainMenu" "rootmenu")
; Set up an indirect reference to a menu
(defun (set-menu $sm-name $sm-menu
    (setq $sm-name (arg 1 ": set-menu (name) "))
    (setq $sm-menu (arg 2 ": set-menu (menu) "))
    (execute-ps-string (concat
        "{/" $sm-menu " where { pop "
	  "/" $sm-name " {" $sm-menu "} def "
        "} if } curDsp send "
    ))
))

; set-mode-menu installs the menu whose name is given so that menu will
; appear when the user selects the "Major Mode" item from the top-level menu
; in the current frame.

(defun (set-mode-menu $smm-menu
  (setq $smm-menu (arg 1 ": set-mode-menu (menu) "))
  (set-menu "ModeMenu" (concat "menu:" $smm-menu))
))

; set-client-menu installs the menu whose name is given so that menu will
; appear when the user asks for the top-level menu in the current frame (via
; MouseRight).
; 
(defun (set-client-menu $scm-menu
  (setq $scm-menu (arg 1 ": set-client-menu (menu) "))
  (set-menu "ClientMenu" (concat "menu:" $scm-menu))
))

; set-main-menu installs the menu whose name is given so that menu will
; appear when the user asks for the top-level menu in the current frame (via
; MouseRight).
; 
(defun (set-main-menu $smm-menu
  (setq $smm-menu (arg 1 ": set-main-menu (menu) "))
  (set-menu "MainMenu" (concat "menu:" $smm-menu))
))

; load-menu-tree <menu-name> [<show>]
; loads the tree of menus whose root is given by <menu-name>.
; If the <show> argument is given (its value is ignored), displays
; the menu.
;
(defun (load-menu-tree
    $lmt-menu $lmt-popup

  (save-excursion 
    (save-window-excursion
      (setq $lmt-menu (if (nargs)
			  (arg 1 ": load-menu-tree (menu) ")
			  ""
      ))
      (setq $lmt-popup (= (nargs) 2))
      (if (! (length $lmt-menu))
	  (setq $lmt-menu &info-current-node)
      )
      (with-node (concat "(menu)" $lmt-menu)
; This was quite inefficient...
;	  (execute-ps-string (resource-to-string "MenuTreePS"))
; Faster:
	  (#with-raw-resource "MenuTreePS" 0
	      (execute-ps-buffer)
	  )

	  (if $lmt-popup
	      (show-menu $lmt-menu)
	  )
; This was quite inefficient...
;	  (execute-mlisp-line (resource-to-string "MenuTreeBindings"))
; Faster:
          (#with-raw-resource "MenuTreeBindings" 0
	      (execute-mlisp-buffer)
	  )
; ARRGH! This was why I kept getting "execute-extended-command: missing
; argument 2" error messages: the point-save hook was "write-current-file",
; and the buffer did not have a current file, so it tried to use the 
; nonexistant argument 2!! Here's a kludge...
	  (set-point-save-hook "#info-put-node")
; An info frame still comes up whenever I load the main menu. Kinda annoying
; and slows things down.  
      )
    )
  )
))

; load-menu-tree-bindings <menu-name> loads the key bindings into Emacs for
; the menu tree whose root menu is given, so that Emacs will know how to
; respond to the keystrokes that the menu sends.  The PS code defining the
; menu is *not* sent to the display server.  This is suitable when the display
; server has already loaded the menu definitions, but the Emacs has not, as
; when starting a second Emacs session on the same display server.
; 
(defun (load-menu-tree-bindings &bindings
  (save-excursion
    (save-window-excursion
      (with-node (concat "(menu)" (arg 1 ": load-menu-tree-bindings (menu) "))
; This was quite inefficient...
;	  (setq &bindings (resource-to-string  "MenuTreeBindings"))
; Faster:
          (#with-raw-resource "MenuTreeBindings" 0
	      (execute-mlisp-buffer)
	  )
      )
    )
  )
))

; load-main-menu-bindings loads the key bindings into Emacs 
; for the main Emacs menu, so that Emacs will know how to respond
; to the keystrokes that the menu sends.  The PS code defining the menu is
; *not* sent to the display server.  This is suitable when the display
; server has already loaded the menu definitions, but the Emacs has not, as
; when starting a second Emacs session on the same display server.
; 
(defun (load-main-menu-bindings
    (show-menu "emacs")
    (message-for 0 "Loading menu bindings...")
    (load-menu-tree-bindings "emacs" 1)
    (message-for 0 "Menu bindings loaded.")
    (novalue)
))

; load-main-menu loads the main Emacs menu tree and pops it up so the user
; can choose something from it.  This is called the first time the user
; tries to pop up a menu; the next time they do so, the menu will appear
; immediately.
;
(defun (load-main-menu
    (popmsg "Loading menus...")
    (message-for 0 "Loading menus...")
    (ps-autoload "menuclasses.ps" "menuclasses.ps")
    (load-menu-tree "emacs" 1)	    ; 1 means show the menu after loading it
    (message-for 0 "Menus loaded.")
    (novalue)
))

; Key codes for bootstrapping menus.

(#define-menu-keys
    (define-key "#menu-100"	"\e[m100\n"	"MenuLoad"
	"load-main-menu")
    (define-key "#menu-101"	"\e[m101\n"	"MenuLoadBindings"
	"load-main-menu-bindings")
)


;----------------------------------------------------------------------
; Menu compiler
;----------------------------------------------------------------------

(defun (make-menu-template
  (beginning-of-file)
  (error-occurred (replace-string "\n[body]\n" ""))
  (if (error-occurred (search-forward "* menu:"))
      (progn (end-of-file)
	     (insert-string "\n* menu:")
      )
  )
  (set-mark)
  (end-of-file)
  (erase-region)
  (insert-string "\n\n* CompileMenu::\n* LoadMenu::\n* ShowMenu::\n\n")
  (insert-string
"------------------------------------------------------------------------\n")
  (set-mark)
  (insert-string "* Beep:     bind:       beep\n")
  (insert-string "* Mark:     bindpoint:  set-mark\n")
  (insert-string "* NeWS:     type:       NeWS\n")
  (insert-string "* Pop:      popmsg:     Pop!\n")
  (insert-string "* Time:     call:       (roundclock)forkunix\n")
  (insert-string "* Root:     literal:    rootmenu\n")
  (insert-string "* Emacs:    emacs:\n")
  (insert-string
"------------------------------------------------------------------------\n")
  (previous-line)
  (exchange-dot-and-mark)
))

; compile-menu-tree <menu-name>
;
; Compiles the menu tree in the node tree rooted at "(Menu)<menu-name>".
; 
(defun (compile-menu-tree	$cmt-root-node %cmt-key-number
    (setq $cmt-root-node
	(if (nargs)
	    (arg 1 ": compile-menu-tree (menu) ")
	    ""
	)
    )
    (if (length $cmt-root-node)
	(setq $cmt-root-node (concat "(menu)" $cmt-root-node))
	(setq $cmt-root-node (#info-full-current-node))
    )
    (temp-use-buffer "*menu-tree-ps*"	    (erase-buffer))
    (temp-use-buffer "*menu-tree-bindings*" (erase-buffer))
    (with-node $cmt-root-node
	(setq %cmt-key-number (info-get-resource "MenuKeyBase"))
	(with-each-tree-node
	    (if (&  (!= &info-current-node "literal")
		    (!= &info-current-node "bind")
		    (!= &info-current-node "bindpoint")
		    (!= &info-current-node "call")
		    (!= &info-current-node "indirect")
		    (!= &info-current-node "type")
		    (!= &info-current-node "popmsg")
		)
		(progn
		    (compile-menu)
		    (append-string-to-buffer "*menu-tree-ps*"
			(resource-to-string "MenuPS"))
		    (append-string-to-buffer "*menu-tree-ps*" "\n")
		    (append-string-to-buffer "*menu-tree-bindings*"
			(resource-to-string "MenuBindings"))
		    (append-string-to-buffer "*menu-tree-bindings*" "\n")
		)
	    )
	)
    )
    (with-node $cmt-root-node
	(buffer-to-resource "MenuTreePs"	"*menu-tree-ps*")
	(buffer-to-resource "MenuTreeBindings"	"*menu-tree-bindings*")
    )
    (message-for 0 "Compiled menus")
))
	       
; compile-menu compiles the menu in this node.  This is invoked from
; compile-menu-tree.
;
; This produces the resources MenuPS and MenuBindings.
;
; Input resources (may be set-wide, in inforesources):
;   MenuKeyBase
;   MenuBindingsHeader
;   MenuBindingsTemplate
;   MenuBindingsTrailer
;   MenuPSHeader
;   MenuPSTemplate
;   MenuPSTrailer
;
; Output resources:
;   MenuPS
;   MenuBindings
; 
(defun (compile-menu
    $cm-info-menu
    %cm-key-number	; these are passed to the #cm-xx-xxx functions
    %cm-label
    %cm-action
    %cm-arg
    
    (message-for 1 (concat "Compiling " &info-current-node "..."))
    (if (& (! (node-is-leaf))
	   (progn (beginning-of-node-child-menu)
		  (beginning-of-line)
		  (looking-at "-")))
	(with-this-node
	    (save-excursion 
		(mark-node-child-menu)
		(setq $cm-info-menu (region-to-string))
	    )
	    (if (is-bound %cmt-key-number)
		(#with-raw-resource "MenuKeyBase" 1
		    (erase-buffer)
		    (insert-string (setq %cm-key-number %cmt-key-number))
		)
		(setq %cm-key-number (info-get-resource "MenuKeyBase"))
	    )
	    (#with-raw-resource "MenuBindings" 1
		(erase-buffer)
		(insert-string
		    (info-get-resource "MenuBindingsHeader") "\n"
		    (info-get-resource "MenuBindingsTemplate") "\n"
		    (info-get-resource "MenuBindingsTrailer"))
	    )
	    (#with-raw-resource "MenuPS" 1
		    $cm-body-prefix
		(erase-buffer)
		(insert-string 
		    (info-get-resource "MenuPSHeader") "\n"
		    (info-get-resource "MenuPSTemplate") "\n"
		    (info-get-resource "MenuPSTrailer"))
		(beginning-of-file)
		(replace-string "<name>"
		    (concat "menu:" &info-current-node))
		(re-search-forward "^\\(\\([\t ]*\\)<body>.*\n\\)")
		(region-around-match 2)
		(setq $cm-body-prefix (region-to-string))
		(region-around-match 1)
		(narrow-region)
		(erase-buffer)
		(insert-string $cm-info-menu)
		(beginning-of-file)
		(while (! (error-occurred (re-search-forward
;			    label	      action		arg
;			  1	  1	    2	    2	      3	   3
	"^[\t ]*\*[\t ]*\\([^:]*\\):[\t ]*\\([^:]*\\):[\t ]*\\(.*\\)$"
		    )))
		    (region-around-match 1)
		    (setq %cm-label (region-to-string))
		    (region-around-match 2)
		    (setq %cm-action (if (region-size)
					 (region-to-string)
					 %cm-label)
		    )
		    (region-around-match 3)
		    (setq %cm-arg (region-to-string))

		    (region-around-match 0)
		    (erase-region)
		    (insert-string $cm-body-prefix "(" %cm-label ")\t")

		    (if (= %cm-action "literal")    (#cm-ps-literal)
			(= %cm-action "bind")	    (#cm-ps-binding 0)
			(= %cm-action "bindpoint")  (#cm-ps-binding 1)
			(= %cm-action "call")	    (#cm-ps-call)
			(= %cm-action "indirect")   (#cm-ps-indirect)
			(= %cm-action "type")	    (#cm-ps-type)
			(= %cm-action "popmsg")	    (#cm-ps-popmsg)
			(compile-submenu)
		    )
		)
	    )
	    (#with-raw-resource "MenuBindings" 1
		(beginning-of-file)
		(search-forward "<bindings>")
		(mark-line)
		(if (! (bobp))
		    (backward-character))
		(erase-region)
	    )
	    (if (is-bound %cmt-key-number)
		(setq %cmt-key-number %cm-key-number))
	)
    )
))
	
;----------------------------------------------------------------------
; Menu compiler "code-generation" functions.
; These are called by compile-menu with dot at the point where compiled code
; is being collected, so they may insert PS code at that point.  When these
; are called, the following bindings are in effect:
; 
;   %cm-action		name of the action being compiled (bind, popmsg, etc.)
;			or item name (same as %cm-label) if action is a submenu
;   %cm-arg		argument given to the action
;   %cm-label		the label of the menu item being compiled
;   %cm-key-number	number of the next available menu "key"

(defun (#cm-ps-literal
    (insert-string %cm-arg)
))
	
(defun (#cm-ps-call
    (insert-string "{ " %cm-arg " }")
))
	       
(defun (#cm-ps-indirect
    (insert-string "/" %cm-arg " cvx")
))
	       
(defun (compile-submenu
    (insert-string "/menu:" %cm-action " cvx")
    (if (!= (length %cm-arg) 0)
	(insert-string "\t% " %cm-arg)
    )
))
	       
(defun (#cm-ps-type
    (insert-string "{ (" %cm-arg ") menustring }")
))
	       
(defun (#cm-ps-popmsg
    (insert-string "{ gsave framebuffer setcanvas"
		   "    currentcursorlocation [(" %cm-arg ")] popmsg pop"
		   "  grestore }")
))
	       
; #cm-ps-binding <point>
;
; If point is true, compiles code to call point-menu instead of menu.
; 
(defun (#cm-ps-binding
    $cmb-prefix $cmb-point

    (setq $cmb-point (arg 1))
    ; make the mlisp binding!
    (insert-string "{ " %cm-key-number 
		        (if $cmb-point " pointmenukey" " menukey")
	                " }\t% " %cm-arg)
    (save-excursion 
	(#with-raw-resource "MenuBindings" 1
	    (beginning-of-file)
	    (re-search-forward "^\\([\t ]*\\)<bindings>.*")
	    (region-around-match 1)
	    (setq $cmb-prefix (region-to-string))
	    (beginning-of-line)
	    (insert-string (concat $cmb-prefix
				   "(define-key \"#menu-"
				   (if $cmb-point "point-" "")
				   %cm-key-number
				   "\"\t\"\\e["
				   (if $cmb-point "p" "m")
				   %cm-key-number
				   "\\n\"\t\"menu:" &info-current-node
				   ":" %cm-label "\"\n"
				   $cmb-prefix "    \"" %cm-arg "\")\n")
	    )
	)
    )
    (++ %cm-key-number)
))

;----------------------------------------------------------------------
; The following allow things in display server to go to a menu and to a
; particular key in that menu by sending keystrokes.
; 

; goto-menu <menu-name> visits the info node that stores the given menu.
; 
(defun (goto-menu &gm-index &gm-prefix &gm-name
    (setq &gm-name (arg 1 ": goto-menu "))
    (setq &gm-index (index &gm-name ":"))
    (setq &gm-prefix (substr &gm-name 0 (- &gm-index 1)))
    (setq &gm-name (substr &gm-name (+ &gm-index 1) -1))
    (info-go-node (concat "(" &gm-prefix ")" &gm-name))
))
	       
; goto-menu-key <item> moves dot to the given menu item in the info node in
; the current buffer.
; 
(defun (goto-menu-key &gmk-name
    (setq &gmk-name (arg 1 ": goto-menu-key "))
    (beginning-of-file)
    (search-forward (concat "* " &gmk-name ":"))
    (beginning-of-line)
))
	       
(define-key "#menu-go"		"\e[ms"		"MenuGo"    "goto-menu")
(define-key "#menu-key"		"\e[mk"		"MenuKey"   "goto-menu-key")
	       
	       
;----------------------------------------------------------------------
; for talking to the server
;----------------------------------------------------------------------
	       
; execute-ps-string sends the given string to the ps engine for
; interpretation.
	       
(defun (execute-ps-string	$pstring
    (setq $pstring (if (interactive)
		       (arg 1 ": execute-ps-string ")
		       (pass-mlisp-args 1 -1 (concat))
    ))
;     (send-string-to-terminal
; 	(concat " ( " 
; 		(quote-postscript-string 
; 		    ;(append-string-to-buffer "plog" $pstring "\n")
; 		    (concat $pstring "\n")
; 		)
; 		" ) execforemacs "
;     ))
    (send-cps-string $pstring)
    (send-string-to-terminal "execforemacs\n")
))
	       
; execute-ps-file sends the contents of the given file to the presentation
; engine for interpretation.
;
(defun (execute-ps-file   $pfile
    (setq $pfile (arg 1 ": execute-ps-file "))
    (if ps-pass-file
	(execute-ps-string   "(" $pfile ") run"  )
	(temp-use-buffer "*draw*"
	    (read-file $pfile)
	    (execute-ps-buffer)
	)
    )
    (message "Sent " $pfile " to display server.")
))
	       
; buffer-temp-file copies the current buffer to a temp file with an
; arbitrarily chosen name and returns the name of that file.  If the virtual
; buffer bounds have been narrowed, only the region between the virtual bounds
; is copied to the file.  If the file had already existed, it is overwritten.
;
; 
(defun (buffer-temp-file
    (save-excursion
	(mark-whole-buffer)
	(copy-region-to-file ps-common-file)
	ps-common-file
    )
))
	       
; If ps-pass-file is nonzero, we write the ps code out to a file, if it isn't
; in one already, and tell the engine to load that file, rather than sending
; it over the eipc connection.  The latter might be faster...
; 
(defun (execute-ps-buffer
    (if ps-pass-file
	(execute-ps-file (buffer-temp-file))
	(save-excursion
	    (mark-whole-buffer)
	    (execute-ps-string (region-to-string))
	)
    )
    (message "Sent " (current-buffer-name) " to display server.")
))

(defun (execute-ps-region
    (narrow-bounds-region
	(execute-ps-buffer)
    )
))

;----------------------------------------------------------------------
; interactive psh session (separate from the terminal's server connection)
;----------------------------------------------------------------------

(setq-option-default psh-program	"psh")
(setq-option-default psh-init-string
    (concat "executive\n"
	    "systemdict /dbgstart known not {(NeWS/debug.ps) run} if\n"
	    "dbgstart\n"))
(setq-option-default psh-buffer		"psh")
(setq-option-default psh-icon		"gern")

; psh starts a psh window, or pops to it if it is already started.  If not it
; starts the program named by psh-program.  Either way, it pops up an
; interactive process window on the psh listener.  Then it sets up the psh
; engine output dispatcher and sentinel so Emacs facilities can receive
; messages from the psh session and detect when the engine has terminated.
; 
(defun (psh	    $started
    (start-interactive-process
	(concat "exec " psh-program)
	psh-buffer
	(progn
	    ; the following is done only if the process was not already
	    ; running
	    (postscript-mode)
	    (listener-mode)
	    (set-primary-icon-image psh-icon)
	    (#process-do-key-bindings)
	    (setq @subsys-start-program "(psh)")
	    (setq $started 1)
	    (setq wrap-long-lines 0)
	)
    )
    (if $started
	(progn
	    (string-to-process (active-process)
		psh-init-string
	    )
	    (message "Started " psh-program)
	    (set-primary-icon-image psh-icon)
	)
    )
))

(defun (execute-psh-buffer
    (if (< (process-status "psh") 0)    ; start the psh if necessary
	(save-window-excursion
	    (psh)
	    (update-current-frame)
	)
    )
    (if (& psh-pass-file (>= (buffer-size) 128))
	(save-excursion
	    (if (= (current-file-name) "")
		(change-current-filename (#scratch-file-name "panel"))
	    )
	    (write-current-file)
	    (execute-psh-string "(" (current-file-name) ") run\n")
	)
	(save-excursion
	    (mark-whole-buffer)
	    (set-channel-input-mode "psh" 1)	; block mode
	    (string-to-process "psh" (concat "\n" (region-to-string) "\n"))
	    (set-channel-input-mode "psh" 0)	; character mode
	)
    )
    (message "Sent " (current-buffer-name) " to psh.")
))

; execute-psh-file sends the contents of the given file to the psh
; listener.
; 
(defun (execute-psh-file   $pfile
    (setq $pfile (arg 1 ": execute-psh-file "))
    (if psh-pass-file
	(execute-psh-string   "(" $pfile ") run"  )
	(temp-use-buffer "*draw*"
	    (read-file $pfile)
	    (execute-psh-buffer)
	)
    )
    (message "Sent " $pfile " to psh.")
))
(defun (execute-psh-string
    (if (< (process-status "psh") 0)    ; start the psh if necessary
	(psh)
    )
    (string-to-process "psh"
	(if (interactive)
	    (arg 1 ": execute-psh-string ")
	    (pass-mlisp-args 1 -1 (concat))
	)
    )
))
(defun (execute-psh-def	$pdef
    (save-excursion
	(mark-postscript-def)
	(setq $pdef (save-excursion 
			(set-mark) (end-of-line) (region-to-string)))
	(narrow-bounds-region
	    (execute-psh-buffer)
	)
    )
    (message "Sent def to psh: " $pdef)
))

;----------------------------------------------------------------------
; invoking server libraries (use these only when you have to)
;----------------------------------------------------------------------

; ps-autoload name file
; 
; 
(defun (ps-autoload $pa-name $pa-file
    (setq $pa-name (arg 1))
    (setq $pa-file (arg 2))
    (execute-ps-string
	"/" $pa-name " where {pop} { (" $pa-file ") runforemacs } ifelse\n")
))

; ps-load file
; 
; causes the display server to load the given file
; 
(defun (ps-load $pa-file
    (setq $pa-file (arg 1 ": ps-load (file) "))
    (execute-ps-string " (" $pa-file ") runforemacs\n")	    ; runprs
))



;----------------------------------------------------------------------
; miscellaneous
;----------------------------------------------------------------------

(defun (set-font-fixed-pitch	$isfp
    (if (| (interactive) (>= (nargs) 1))
	(setq $isfp (arg 1 ": set-font-fixed-pitch (true/false) "))
    )
    (send-string-to-terminal (concat
	" { isFixedPitch /isFixedPitch " 
	(if (+ $isfp) "true" "false")
	" def } curDsp send = "
    ))
    (= (get-tty-string "") "true")
))

; If true, the characters of variable width fonts are centered in the
; fixed width character cells. Slower, but much more readable.
(defun (set-font-center-characters	$iscc
    (if (| (interactive) (>= (nargs) 1))
	(setq $iscc (arg 1 ": set-font-center-characters (true/false) "))
    )
    (send-string-to-terminal (concat
	" { CenterCharacters? /CenterCharacters? " 
	(if (+ $iscc) "true" "false")
	" def } curDsp send = "
    ))
    (= (get-tty-string "") "true")
))

(defun (toggle-cursor
    (send-string-to-terminal (concat 
	 " { textCursor? { CH } if "
	 "   /textCursor? textCursor? not def "
	 " } curDsp send "
    ))
))

; (render-frame-image <shrinkage factor> <destination frame> <x> <y>
; <shrinkage factor> is the inverse of the scale factor
; <destination frame> designates the frame to put the scaled image in
; <x> and <y> give the position within the destination canvas in default
;   coordinates
; 
(defun (render-frame-image
    $d-shrink $d-frame $d-x $d-y
    (setq $d-shrink (arg 1 ": render-frame-image (shrinkage) "))
    (setq $d-frame (arg 2 ":_(in frame) "))
    (setq $d-x (arg 3 ":_(x) "))
    (setq $d-y (arg 4 ":_(y) "))
    (send-string-to-terminal (concat
;    (append-string-to-buffer "*log" (concat
	" gsave "
	(current-frame)
	" initmatrix currentcanvas clippath pathbbox "
	$d-shrink " div exch " $d-shrink " div exch "
	$d-frame " SF showMatrix setmatrix "
	$d-x " " $d-y " translate "
	" scale pop pop imagecanvas \n"
	" .50 .50 moveto showMatrix setmatrix FrameFont setfont "
	" (" (current-frame) ": " (get-frame-label) ") "
	" dup stringwidth pop neg 2 div 0 rmoveto \n"
	" currentpoint "
	" 2 index stringwidth pop currentfont fontheight "
	" currentpoint 4 2 roll rectpath 1 setgray fill "
	" moveto \n"
	" 0 setgray show "
	" SF "		; matches the (current-frame) above.
	" grestore "
    ))
))

; (overview <destination frame>) displays in the destination frame labeled
;  stamps of all frames.
; 
(defun (overview
    $z-x $z-y $z-frame $z-dx $z-dy $z-width
    (setq $z-frame (arg 1 ": overview (into frame) "))
    (setq $z-dx 192)
    (setq $z-dy 192)
    (switch-to-frame $z-frame
	(setq $z-width (get-frame-width))
;	(setq $z-width 512)
	(send-string-to-terminal " erasepage ")
    )
    (for-each-frame
	(render-frame-image 3 $z-frame $z-x $z-y)
	(+= $z-x $z-dx)
	(if (> $z-x $z-width)
	    (progn (+= $z-y $z-dy)
		   (setq $z-x 0)))
    )
))

;----------------------------------------------------------------------
; experimental frame manipulation
;----------------------------------------------------------------------

; make-parent-frame makes the current frame be the parent of any newly created
; frames.
; 

(defun (make-parent-frame
    (send-string-to-terminal
;	" userdict /EmacsParentCanvas /ClientCanvas curDsp send put "
	" userdict /EmacsParentCanvas /FrameCanvas curDsp send put "
    )
))

(defun (revert-parent-frame
    (send-string-to-terminal " userdict /EmacsParentCanvas framebuffer put ")
))

(defun (set-default-frame-mapped
    (send-string-to-terminal (concat
	" userdict /EmacsMapNewFrames " 
	(if (arg 1 ": set-default-frame-mapped ") "true" "false")
	" put "
    ))
))

(defun (set-default-frame-class
    (send-string-to-terminal (concat
	(if (> (nargs) 1) " userdict /EmacsDefaultWindow get " "")
	" userdict /EmacsDefaultWindow " (arg 1 ": set-default-frame-class ")
	" put "
    ))
    (if (> (nargs) 1)
	(progn
	    (progn-args 2)
	    (send-string-to-terminal " userdict exch /EmacsDefaultWindow exch put ")
	)
    )
))

(defun (dBox-class (set-default-frame-class "dBoxEmacsWindow")))
(defun (Main-class (set-default-frame-class "MainEmacsWindow")))
(defun (Tab-class
    (set-default-frame-class "EmacsTabWindow")
    (execute-ps-string "TabEmacsWindow /CenterCharacters? true put")
    (setq-default pop-up-frame-height 300)
    (setq-default pop-up-frame-x	0)
    (setq-default pop-up-frame-y	(- (framebuffer-height) pop-up-frame-height))
    (setq-default pop-up-frame-dx	0)
    (setq-default pop-up-frame-dy	-30)
    (setq-default icon-x		900)
    (setq-default icon-y		(- (framebuffer-height) 128))
    (setq-default icon-dx		-68)
))

(defun (create-doc-frame
    (set-default-frame-class "MainEmacsWindow"
	(clone-frame)
    )
))

(defun (create-dbox-frame
    (set-default-frame-class "dBoxEmacsWindow"
	(clone-frame)
    )
))

; popmsg <string>
;
(defun (popmsg &pm-string
;     (execute-ps-string (concat
; 	" gsave framebuffer setcanvas currentcursorlocation [ ("
; 	(quote-postscript-string (arg 1 ": popmsg (string) "))
; 	") ] popmsg pop grestore  "
;     ))
    (setq &pm-string  (arg 1 ": popmsg "))
    (send-string-to-terminal
	 " gsave framebuffer setcanvas currentcursorlocation [ ")
    (send-cps-string  &pm-string)
    (send-string-to-terminal
	 "] popmsg pop grestore\n")
))

(defun (popup-message &pm-string &pm-line
    (setq &pm-string (arg 1 ": popup-message "))
;     (temp-use-buffer "*popup-message*"
; 	(erase-buffer)
; 	(insert-string &pm-string)
; 	(beginning-of-file)
; 	(insert-string "currentcursorlocation [\n")
; 	(while (! (error-occurred (re-search-forward "^\\(.*\\)$")))
; 	       (region-around-match 1)
; 	       (setq &pm-line (region-to-string))
; 	       (erase-region)
; 	       (insert-string "(" (quote-postscript-string &pm-line) ")")
; 	       (if (! (eobp)) (forward-character)))
; 	(end-of-file)
; 	(insert-string "] popmsg pop\n")
; 	(execute-ps-buffer)
;     )
     (temp-use-buffer "*popup-message*"
 	(erase-buffer)
 	(insert-string &pm-string)
 	(beginning-of-file)
 	(send-string-to-terminal
	    " gsave framebuffer setcanvas currentcursorlocation [\n")
 	(while (! (| (eobp)
		     (error-occurred (re-search-forward "^.*$"))))
 	       (region-around-match 0)
	       (send-cps-string (region-to-string))
 	       (if (! (eobp)) (forward-character)))
	(send-string-to-terminal "] popmsg pop grestore\n")
	(flush-terminal-output)
     )
))

(defun (popup-region
    (popup-message (region-to-string))
))

; noisy-update is useful for rough comparisons of display update times
; 
(defun (noisy-update
    (tone 1)
    (sit-for 0)
    (redraw-display)
    (sit-for 0)
    (tone 0)
))

;======================================================================
; Special key definitions
;----------------------------------------------------------------------

; These must be done after the functions that are bound here have been
; defined, due to a weirdness in define-key.
; 
(if (= (ps-engine-type) 0) ; Sun
    (#define-special-keys

	(define-key "#key-L1"   "\e[192z"   "L1")				; stop
	(define-key "#key-L2"   "\e[193z"   "L2")				; again
	(define-key "#key-L3"   "\e[194z"   "L3"	"edit-subsystem-options")

	(define-key "#key-L4"   "\e[195z"   "L4"	"undo")			; undo
	(define-key "#key-L5"   "\e[196z"   "L5"	"nothing")		; front
	(define-key "#key-L6"   "\e[197z"   "L6"	"copy-region-to-killbuffer")
										; copy
	(define-key "#key-L7"   "\e[198z"   "L7"	"nothing")		; open
	(define-key "#key-L8"   "\e[199z"   "L8"	"yank-from-killbuffer")	; paste
	(define-key "#key-L9"   "\e[200z"   "L9"	"search-forward")	; find
	(define-key "#key-L10"  "\e[201z"   "L10"	"delete-to-killbuffer")	; cut

	(define-key "#key-help" "\e[207z"   "HELP"	"help")

	(define-key "#key-R1"   "\e[208z"   "R1"	"expand-mlisp-word")
	(define-key "#key-R2"   "\e[209z"   "R2"	"scroll-one-line-up")
	(define-key "#key-R3"   "\e[210z"   "R3"	"list-fonts")
	(define-key "#key-R4"   "\e[211z"   "R4")
	(define-key "#key-R5"   "\e[212z"   "R5"	"scroll-one-line-down")
	(define-key "#key-R6"   "\e[213z"   "R6")
	(define-key "#key-R7"   "\e[214z"   "R7"    "nothing"	"#keypad-7")
	(define-key "#key-R8"   "\e[215z"   "R8"    "nothing"	"#keypad-8")
	(define-key "#key-R9"   "\e[216z"   "R9"    "nothing"	"#keypad-9")
	(define-key "#key-R10"  "\e[217z"   "R10"   "nothing"	"#keypad-4")
	(define-key "#key-R11"  "\e[218z"   "R11"   "nothing"	"#keypad-5")
	(define-key "#key-R12"  "\e[219z"   "R12"   "nothing"	"#keypad-6")
	(define-key "#key-R13"  "\e[220z"   "R13"   "nothing"	"#keypad-1")
	(define-key "#key-R14"  "\e[221z"   "R14"   "nothing"	"#keypad-2")
	(define-key "#key-R15"  "\e[222z"   "R15"   "nothing"	"#keypad-3")

	(define-key "#key-F1"   "\e[224z"   "F1"	"create-doc-frame")
	(define-key "#key-F2"   "\e[225z"   "F2"	"create-dbox-frame")
	(define-key "#key-F3"   "\e[226z"   "F3")
	(define-key "#key-F4"   "\e[227z"   "F4")
	(define-key "#key-F5"   "\e[228z"   "F5")
	(define-key "#key-F6"   "\e[229z"   "F6")
	(define-key "#key-F7"   "\e[230z"   "F7")
	(define-key "#key-F8"   "\e[231z"   "F8")
	(define-key "#key-F9"   "\e[232z"   "F9")
	(define-key "#key-F10"  "\e[233z"   "F10")
	(define-key "#key-F11"  "\e[234z"   "F11")
	(define-key "#key-F12"  "\e[235z"   "F12")
	(define-key "#key-right" "\e[241z"  "right")
	(define-key "#key-frame-resumed" "\e[300z" "FrameResume" "novalue")
    )
    (= (ps-engine-type) 1) ; SGI 4Sight
    (#define-special-keys
	; These agree with the keypad key labels on a 4D.
	; 
	(define-key "#key-R1"   "\e[146q"   "End"	"end-of-window"	"#keypad-1")
	(define-key "#key-R2"   "\e[B"	    "Down"	"next-line"	"#keypad-2")
	(define-key "#key-R3"   "\e[154q"   "PageDown"	"next-page"	"#keypad-3")
	(define-key "#key-R4"   "\e[D"	    "Left"	"backward-character"  "#keypad-4")
	(define-key "#key-R5"   "\e[000q"   "Keypad 5"	"argument-prefix"    "#keypad-5")
	(define-key "#key-R6"   "\e[C"	    "Right"	"forward-character" "#keypad-6")
	(define-key "#key-R7"   "\e[H"	    "Home"	"beginning-of-window" "#keypad-7")
	(define-key "#key-R8"   "\e[A"	    "Up"	"previous-line"	"#keypad-8")
	(define-key "#key-R9"   "\e[150q"   "PageUp"    "previous-page"	"#keypad-9")

	(define-key "#key-shift-up"	"\e[161q" "ShiftUp"	"scroll-one-line-up")
	(define-key "#key-shift-down"	"\e[164q" "ShiftRight"	"scroll-one-line-down")
	(define-key "#key-shift-left"	"\e[158q" "ShiftLeft"	"scroll-left")
	(define-key "#key-shift-right"	"\e[167q" "ShiftRight"	"scroll-right")

	; these don't send anything under NeWS, at least with the current
	; TrmPS driver.  Perhaps we have to express interest in them specially??
	; 
	(define-key "#key-F1"   "\e[224z"   "F1"	"create-doc-frame")
	(define-key "#key-F2"   "\e[225z"   "F2"	"create-dbox-frame")
	(define-key "#key-F3"   "\e[226z"   "F3")
	(define-key "#key-F4"   "\e[227z"   "F4")
	(define-key "#key-F5"   "\e[228z"   "F5")
	(define-key "#key-F6"   "\e[229z"   "F6")
	(define-key "#key-F7"   "\e[230z"   "F7")
	(define-key "#key-F8"   "\e[231z"   "F8")
	(define-key "#key-F9"   "\e[232z"   "F9")
	(define-key "#key-F10"  "\e[233z"   "F10")
	(define-key "#key-F11"  "\e[234z"   "F11")
	(define-key "#key-F12"  "\e[235z"   "F12")
	(define-key "#key-right" "\e[241z"  "right")

	(define-key "#key-frame-resumed" "\e[300z" "FrameResume" "novalue")
    )

)

; key names that are synonyms for standard keys (#key-up, #key-down, etc)
;
(if (= (ps-engine-type) 0) ; Sun
    (progn
	(map-standard-key #key-up	    #key-R8)
	(map-standard-key #key-down	    #key-R14)
	(map-standard-key #key-right	    #key-R12)
	(map-standard-key #key-left	    #key-R10)
    )
    (= (ps-engine-type) 1) ; SGI 4Sight
    (progn
	(map-standard-key #key-up	    #key-R8)
	(map-standard-key #key-down	    #key-R2)
	(map-standard-key #key-right	    #key-R6)
	(map-standard-key #key-left	    #key-R4)
    )
)

(set-default-frame-mapped "false")

(error-occurred
    (setq-option-default icon-y
		(- (framebuffer-height) 128))
    (setq-option-default pop-up-frame-y
		(- (framebuffer-height) 626))
)

(novalue)
