;----------------------------------------------------------------------
; Panel compiler
;----------------------------------------------------------------------

; A template buffer contains magic cookies, each specifying a name and a 
; default value, in the form of "<name|default>", or
; "<name1|name2|...|default>", or <name>.

; A defaults buffer contains default bindings, each specifying a name and
; a value, in the form on "name|value", each in a line.

; Instantiating a template buffer with a defaults buffer will replace each 
; named magic cookie in the template buffer with the value to which its 
; name is bound in the defaults buffer, or its default value if its name 
; is unbound. If there are several names in the magic cookie, the value of
; the first name that is bound is used, or the default value if all the 
; names are unbound. If there is just one name and no default, the name''s
; value is used if the name is bound, otherwise the name its self is used.

; Example:
;   defaults:
;     foo|123
;     bar|456
; 
;   transformations:
;     "<foo>" => "123"
;     "<foo|foo>" => "123"
;     "<fum>" => "fum"
;     "<fum|fum>" => "fum"
;     "<foo|bar>" => "123"
;     "<yow|bar>" => "bar"
;     "<yow|baz>" => "baz"
;     "<yow|bar|baz>" => "456"
;     "<yow|baz|rag>" => "rag"
;     "<<>" => "<"
;     "<>>" => ">"

; (put-default "*defaults-buffer*" "name" "value")
; Add a default, or change already existing default.
(defun (put-default &pd-buffer &pd-name &pd-value
    (setq &pd-buffer (arg 1 ": put-default (buffer) "))
    (setq &pd-name (arg 2 ": put-default (name) "))
    (setq &pd-value (arg 3 ": put-default (value) "))
    (save-excursion (temp-use-buffer &pd-buffer
	(beginning-of-file)
	(if (! (looking-at "\n"))
	    (newline-and-backup)) ; Force leading newline
	(if (error-occurred (search-forward (concat "\n" &pd-name "|")))
	    ; add new default
	    (insert-string (concat "\n" &pd-name "|" &pd-value))
	    ; else replace old default
	    (progn
		(if (! (looking-at "\n"))
		    (kill-to-end-of-line))
		(insert-string &pd-value)))
    ))
))

; (fetch-default "*defaults-buffer*" "name")
; Return the value of a default, if it exists, otherwise "*undefined*".
(defun (fetch-default &gb-buffer &gb-name
    (setq &gb-buffer (arg 1 ": fetch-default (buffer) "))
    (setq &gb-name (arg 2 ": fetch-default (name) "))
    (save-excursion (temp-use-buffer &gb-buffer
	(beginning-of-file)
	(if (! (looking-at "\n"))
	    (newline-and-backup)) ; Force leading newline
	(if (error-occurred (search-forward (concat "\n" &gb-name "|")))
	    "*undefined*"
	    (progn
		(set-mark)
		(end-of-line)
		(region-to-string)))
    ))
))


; (instantiate-template "*template-buffer*" "*defaults-buffer*")
; Insert the template into the current buffer, and replaces magic cookies
; with their defaults.

(defun (instantiate-template &it-template &it-defaults
    &it-name &it-value &it-delim
    (setq &it-template (arg 1 ": instantiate-template (template buffer) "))
    (setq &it-defaults (arg 2 ": instantiate-template (defaults buffer) "))
    (save-restriction
	(set-mark)
	(narrow-region)
	(yank-buffer &it-template)
	(beginning-of-file)
	(while (! (error-occurred (search-forward "<")))
	    (if (| (looking-at "<>") ; "<<>" => "<"
		   (looking-at ">>")) ; "<>>" => ">"
		(progn (delete-previous-character)
		       (forward-character)
		       (delete-next-character))
		(looking-at "\\([^|>]*\\)\\([|>]\\)")
		(progn (region-around-match 1)
		       (setq &it-name (region-to-string))
		       (region-around-match 2)
		       (setq &it-delim (region-to-string))
		       (region-around-match 0)
		       (erase-region)
		       (setq &it-value (fetch-default &it-defaults &it-name))
		       (if (= &it-delim ">")
			   ; foo unbound: "<foo>" => "<foo>"
			   ; foo bound: "<foo>" => "value"
			   (progn
			       (delete-previous-character)
			       (if (= &it-value "*undefined*")
				   (insert-string &it-name)
			           (progn (insert-string &it-value)
			                  (exchange-dot-and-mark))))
		           (!= &it-value "*undefined*")
			   (progn ; foo bound: "<foo|...>" => "value"
			       (delete-previous-character)
			       (if (! (looking-at "[^>]*>"))
				   (error-message "Can't find matching >!"))
			       (region-around-match 0)
			       (erase-region)
			       (insert-string &it-value)
			       (exchange-dot-and-mark))
			   (progn ; foo unbound: "<foo|bar...>" => "<bar...>"
			       (if (looking-at "[^|>]*>")
				   (progn
				       (region-around-match 0)
				       (exchange-dot-and-mark)
				       (delete-previous-character)
				       (exchange-dot-and-mark)
				       (delete-previous-character))
			           (backward-character)))))
		(error-message "Can't find matching | or >!"))
	)
	(end-of-file)
    )
))

(defun (use-item-template &uit-name &uit-template
    (setq &uit-name (arg 1 ": use-item-template "))
    (setq &uit-template (info-get-resource (concat &uit-name "Template")))
    (save-excursion
	(temp-use-buffer "*panel-item-template*"
	    (erase-buffer)
	    (insert-string &uit-template))
	(wipe-item-defaults)
    )
))

(defun (wipe-item-defaults
  (temp-use-buffer "*panel-item-defaults*"
    (erase-buffer))
  (put-default "*panel-item-defaults*"
    "panel-name" (concat "panel:" &info-current-node))
))

(defun (compile-panel
    &cp-editing &cp-info-panel &cp-key-number &cp-body-prefix
    &cp-name &cp-value &cp-action &cp-arg &cp-ps
    (message-for 0 (concat "Compiling " &info-target-node "..."))
    (if (! (setq &cp-editing &info-editing))
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
		   (setq &cp-info-panel (region-to-string))
	       )
	))
	(progn
	      (if (is-bound &cps-key-number) ; compile-panel-set
		  (#with-raw-resource "PanelKeyBase" 1
		      (erase-buffer)
		      (insert-string (setq &cp-key-number &cps-key-number)))
		  (setq &cp-key-number (info-get-resource "PanelKeyBase")))
	      (use-item-template "Button") ; start out w/ good default

	      (#with-raw-resource "PanelBindings" 1
		  (erase-buffer)
		  (insert-string
		      (concat
			     (info-get-resource "PanelBindingsHeaderTemplate") "\n"
			     (info-get-resource "PanelBindingsBodyTemplate") "\n"
			     (info-get-resource "PanelBindingsTrailerTemplate"))))
              (setq &cp-ps
		      (concat
			     (info-get-resource "PanelPSHeaderTemplate") "\n"
			     (info-get-resource "PanelPSBodyTemplate") "\n"
			     (info-get-resource "PanelPSTrailerTemplate")))
	      (temp-use-buffer "*panel-item-template*"
		  (erase-buffer)
		  (insert-string &cp-ps)
	      )
	      (#with-raw-resource "PanelPS" 1
		  (erase-buffer)
		  (instantiate-template
		      "*panel-item-template*" "*panel-item-defaults*")
		  (beginning-of-file)
		  (re-search-forward "^\\(\\([\t ]*\\)<body>\\)$")
		  (region-around-match 2)
		  (setq &cp-body-prefix (region-to-string))
		  (region-around-match 1)
		  (narrow-region)
		  (erase-buffer)
		  (insert-string &cp-info-panel)
		  (beginning-of-file)
		  (while (! (eobp))
			 (if (| (looking-at "\n")
				(looking-at "^[#;%]"))
			     ; skip blank lines & comments
			     (progn (end-of-line)
				    (forward-character))

			     (looking-at "^[^\t *]*$")
			     ; ItemTemplateName
			     (progn
				    (region-around-match 0)
				    (setq &cp-name (region-to-string))
				    (erase-region)
				    (delete-next-character)
				    (use-item-template &cp-name))

			     (looking-at "^[\t ]+\\([^:]*\\):[\t ]*\\(.*\\)$")
			     ;         name: value
			     (progn (region-around-match 1)
				    (setq &cp-name (region-to-string))
				    (region-around-match 2)
				    (setq &cp-value (region-to-string))
				    (region-around-match 0)
				    (erase-region)
				    (delete-next-character)
				    (put-default "*panel-item-defaults*" 
					         &cp-name &cp-value))

			     (looking-at "^\*[\t ]*\\([^:]*\\):[\t ]*\\([^:]*\\):[\t ]*\\(.*\\)$")
			     ; * name: action: arg
			     (progn (region-around-match 1)
				    (setq &cp-name (region-to-string))
				    (region-around-match 2)
				    (setq &cp-action (region-to-string))
				    (region-around-match 3)
				    (setq &cp-arg (region-to-string))
				    (region-around-match 0)
				    (erase-region)
				    (make-action-defaults)
				    (instantiate-template
					"*panel-item-template*"
					"*panel-item-defaults*")
				    (wipe-item-defaults)
			     )
			     (progn (message "Unknown stuff!") 
				    (recursive-edit))
			 )
		  )
	      )
	      (#with-raw-resource "PanelBindings" 1
		  (beginning-of-file)
		  (search-forward "<bindings>")
		  (end-of-line)
		  (set-mark)
		  (beginning-of-line)
		  (if (! (bobp))
		      (backward-character))
		  (erase-region)
	      )
	      (if (is-bound &cps-key-number)
		  (setq &cps-key-number &cp-key-number))
	)
    )
    (if (! &cp-editing)
	(#info-exit-edit)
	(point-save))
))

(defun (make-action-defaults
    (put-default "*panel-item-defaults*"
	"name" &cp-name)
    (put-default "*panel-item-defaults*"
	"action" &cp-action)
    (put-default "*panel-item-defaults*"
	"arg" &cp-arg)
    (if (= &cp-action "literal")
	(compile-panel-literal)
	(= &cp-action "bind")
	(compile-panel-binding)
	(= &cp-action "call")
	(compile-panel-call)
	(= &cp-action "type")
	(compile-panel-type)
	(= &cp-action "popmsg")
	(compile-panel-popmsg)
	(= &cp-action "menu")
	(compile-panel-menu)
	(error-message (concat "Unrecognized action: " &cp-action)))
))

(defun (compile-panel-literal
    (put-default "*panel-item-defaults*"
	"notify" &cp-arg)))

(defun (compile-panel-binding
    &cpb-prefix
    (put-default "*panel-item-defaults*"
	"keycode" (concat &cp-key-number))
    (put-default "*panel-item-defaults*"
	"notify" (concat "{ ItemNotifyMsg menustring }"))
    ; make the mlisp binding!
    (save-excursion 
	(#with-raw-resource "PanelBindings" 1
	    (beginning-of-file)
	    (re-search-forward "^\\([\t ]*\\)<bindings>.*")
	    (region-around-match 1)
	    (setq &cpb-prefix (region-to-string))
	    (beginning-of-line)
	    (insert-string (concat &cpb-prefix
				   "(define-key \"#panel-"
				   &cp-key-number
				   "\"\t\"\\e[P"
				   &cp-key-number
				   "\\n\"\t\"panel:" &info-target-node
				   ":" &cp-name "\"\n"
				   &cpb-prefix "    \"" &cp-arg "\")\n"))
	)
    )
    (++ &cp-key-number)
))

(defun (compile-panel-call
    (put-default "*panel-item-defaults*"
	"notify" (concat "{ " &cp-arg " }"))
))

(defun (compile-panel-menu
    (put-default "*panel-item-defaults*"
        "notify"
	(concat "{ currentcursorlocation /showat " &cp-arg " send }"))
))

(defun (compile-panel-type
    (put-default "*panel-item-defaults*"
        "notify" (concat "{ (" &cp-arg ") menustring }"))
))

(defun (compile-panel-popmsg
    (put-default "*panel-item-defaults*"
        "notify" (concat "{ currentcursorlocation [(" &cp-arg ")] popmsg }"))
))
