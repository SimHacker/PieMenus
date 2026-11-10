;----------------------------------------------------------------------
; Menu compiler
;----------------------------------------------------------------------

(defun (compile-menu
    &cm-info-menu &cm-body-prefix &cm-key-number
    &cm-label &cm-action &cm-arg &cm-editing
    (message-for 0 (concat "Compiling " &info-target-node "..."))
    (if (! (setq &cm-editing &info-editing))
	(#info-enter-edit))
    (beginning-of-file)
    (if (! (error-occurred 
	       (save-excursion 
		   (beginning-of-file)
		   (re-search-forward "^-+$")
		   (forward-character)
		   (set-mark)
		   (re-search-forward "^-+$")
		   (beginning-of-line)
		   ;(backward-character)
		   (setq &cm-info-menu (region-to-string))
	       )
	))
	(progn
	       (if (is-bound &cmt-key-number)
		   (edit-resource "MenuKeyBase"
		       (erase-buffer)
		       (insert-string (setq &cm-key-number &cmt-key-number)))
		   (setq &cm-key-number (info-get-resource "MenuKeyBase")))
;	       (edit-resource "MenuBindings"
	       (#with-raw-resource "MenuBindings" 1
		   (erase-buffer)
		   (insert-string
		       (concat
		           (info-get-resource "MenuBindingsHeader") "\n"
		           (info-get-resource "MenuBindingsTemplate") "\n"
		   	   (info-get-resource "MenuBindingsTrailer")))
	       )
;	       (edit-resource "MenuPS"
	       (#with-raw-resource "MenuPS" 1
		   (erase-buffer)
		   (insert-string 
		       (concat
			   (info-get-resource "MenuPSHeader") "\n"
			   (info-get-resource "MenuPSTemplate") "\n"
		           (info-get-resource "MenuPSTrailer")))
		   (beginning-of-file)
		   (replace-string "<name>"
		     (concat "menu:" &info-current-node))
		   (re-search-forward "^\\(\\([\t ]*\\)<body>.*\n\\)")
		   (region-around-match 2)
		   (setq &cm-body-prefix (region-to-string))
		   (region-around-match 1)
		   (narrow-region)
		   (erase-buffer)
		   (insert-string &cm-info-menu)
		   (beginning-of-file)
		   (while (! (error-occurred
				 (re-search-forward "^\\([\t ]*\*[\t ]*\\([^:]*\\):[\t ]*\\([^:]*\\):[\t ]*\\(.*\\)\\)$")))
			  (region-around-match 2)
			  (setq &cm-label (region-to-string))
			  (region-around-match 3)
			  (setq &cm-action (region-to-string))
			  (region-around-match 4)
			  (setq &cm-arg (region-to-string))
			  (region-around-match 1)
			  (erase-region)
			  (insert-string (concat &cm-body-prefix
						 "(" &cm-label ")\t"))
			  (if (= &cm-action "literal")
			        (compile-menu-literal)
			      (= &cm-action "bind")
				(compile-menu-binding 0)
			      (= &cm-action "bindpoint")
				(compile-menu-binding 1)
			      (= &cm-action "call")
				(compile-menu-call)
			      (= &cm-action "indirect")
				(compile-menu-indirect)
			      (= &cm-action "type")
				(compile-menu-type)
			      (= &cm-action "popmsg")
				(compile-menu-popmsg)
				(compile-submenu))
		   )
	       )
;	       (edit-resource "MenuBindings"
	       (#with-raw-resource "MenuBindings" 1
		   (beginning-of-file)
		   (search-forward "<bindings>")
		   (end-of-line)
		   (set-mark)
		   (beginning-of-line)
		   (if (! (bobp))
		       (backward-character))
		   (erase-region)
	       )
	       (if (is-bound &cmt-key-number)
		   (setq &cmt-key-number &cm-key-number))
    ))
    (if (! &cm-editing)
	(#info-exit-edit)
	(point-save))
))

(defun (compile-menu-literal
    (insert-string &cm-arg)
))

; (compile-menu-binding point?)
(defun (compile-menu-binding
    &cmb-prefix &cmb-point
    (setq &cmb-point (arg 1))
    (insert-string "{ " &cm-key-number " menukey }\t% " &cm-arg)
    ; make the mlisp binding!
    (save-excursion 
;	(edit-resource "MenuBindings"
	(#with-raw-resource "MenuBindings" 1
	    (beginning-of-file)
	    (re-search-forward "^\\([\t ]*\\)<bindings>.*")
	    (region-around-match 1)
	    (setq &cmb-prefix (region-to-string))
	    (beginning-of-line)
	    (insert-string (concat &cmb-prefix
				   "(define-key \"#menu-"
				   (if &cmb-point "point-" "")
				   &cm-key-number
				   "\"\t\"\\e["
				   (if &cmb-point "p" "m")
				   &cm-key-number
				   "\\n\"\t\"menu:" &info-target-node
				   ":" &cm-label "\"\n"
				   &cmb-prefix "    \"" &cm-arg "\")\n"))
	)
    )
    (++ &cm-key-number)
))

(defun (compile-menu-call
    (insert-string "{ " &cm-arg " }")
))

(defun (compile-menu-indirect
    (insert-string "/" &cm-arg " cvx")
))

(defun (compile-submenu
    (insert-string "/menu:" &cm-action " cvx")
    (if (!= (length &cm-arg) 0)
	(insert-string "\t% " &cm-arg))
))

(defun (compile-menu-type
    (insert-string "{ (" &cm-arg ") menustring }")
))

(defun (compile-menu-popmsg
    (insert-string "{ currentcursorlocation [(" &cm-arg ")] popmsg }")
))

(defun (compile-menu-tree
    &cmt-menu &cmt-submenus &cmt-key-number &cmt-buf &cmt-submenu &cmt-editing
    &info-target-file &info-target-node
    (setq &cmt-menu (arg 1 ": compile-menu-tree (menu) "))
    (setq &info-target-file "menu")
    (setq &info-target-node &cmt-menu)
    (setq &cmt-buf (current-buffer-name))
    (temp-use-buffer "*menu-tree-ps*"
	(erase-buffer))
    (temp-use-buffer "*menu-tree-bindings*"
	(erase-buffer))
    (temp-use-buffer "*menu-resource*"
	(#info-fetch-node)
	(setq &cmt-submenus
	      (resource-to-string "SubMenus"))
	(setq &cmt-key-number
	      (info-get-resource "MenuKeyBase"))
    )
    (compile-menu)
    (append-string-to-buffer "*menu-tree-ps*"
	(resource-to-string "MenuPS"))
    (append-string-to-buffer "*menu-tree-bindings*"
	(resource-to-string "MenuBindings"))
    (temp-use-buffer "*compile-menu-tree-scratch*"
	(erase-buffer)
	(insert-string &cmt-submenus)
	(beginning-of-file)
	(while (! (error-occurred (re-search-forward "^[\t ]*\\(.*\\)$")))
	       (region-around-match 1)
	       (setq &cmt-submenu (region-to-string))
	       (temp-use-buffer &cmt-buf
		   (setq &info-target-node &cmt-submenu)
		   (setq &info-target-file "menu")
		   (#info-go-target-node)
		   (compile-menu)
		   (append-string-to-buffer "*menu-tree-ps*"
		       (resource-to-string "MenuPS"))
		   (append-string-to-buffer "*menu-tree-bindings*"
		       (resource-to-string "MenuBindings"))
	       )
	)
    )
    (setq &info-target-file "menu")
    (setq &info-target-node &cmt-menu)
    (#info-go-target-node)
    (if (! (setq &cmt-editing &info-editing))
	(#info-enter-edit))
    (#with-raw-resource "MenuTreePS" 1
      (erase-buffer)
      (yank-buffer "*menu-tree-ps*")
    )
    (#with-raw-resource "MenuTreeBindings" 1
      (erase-buffer)
      (yank-buffer "*menu-tree-bindings*")
    )
    (if (! &cmt-editing)
	(#info-exit-edit)
	(point-save))
    (message-for 0 "All done!")
))


(defun (goto-menu &gm-index &gm-prefix &gm-name
  (setq &gm-name (arg 1 ": goto-menu "))
  (setq &gm-index (index &gm-name ":"))
  (setq &gm-prefix (substr &gm-name 0 (- &gm-index 1)))
  (setq &gm-name (substr &gm-name (+ &gm-index 1) -1))
  (info-go-node (concat "(" &gm-prefix ")" &gm-name))
))

(defun (goto-menu-key &gmk-name
  (setq &gmk-name (arg 1 ": goto-menu-key "))
  (beginning-of-file)
  (search-forward (concat "* " &gmk-name ":"))
  (beginning-of-line)
))

(define-key "#menu-go"		"\e[ms"		"MenuGo"
      "goto-menu")
(define-key "#menu-key"		"\e[mk"		"MenuKey"
      "goto-menu-key")
