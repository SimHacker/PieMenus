;----------------------------------------------------------------------
; UI compiler
; Don Hopkins, Aug 27 1988
;----------------------------------------------------------------------
; 
; A buffer of default variables consists of a number of lines each
; specifying the name and the value of a default, 
; in the form of "name=value".
; 
; A template buffer contains literal text and magic cookies. When the 
; UI compiler instantiates a template, it eats up the magic cookies, 
; leaving text in their place.  The text replacing the cookies is
; determined by the set of default variables with which the template is
; instantiated, and the default values specified in the magic cookies
; themselves.
; 
; Magic cookies come in several basic forms:
;
;   <<>
; 
; Replaced with "<".  Used for quoting cookie delimiters into the text,
; in such a way that allows the emacs parenthesis traversing commands to
; function properly.
; 
;   <>>
;
; Replaced with ">", like "<<>".
; 
;   <name>
;
; The magic cookie is replaced by the value of the named default
; variable if it is defined.  Otherwise the cookie is left uneaten.
; If a cookie is eaten, and its value contains cookies, they are
; evaluated as well.
; 
;   <name1|name2|...|namen>
;
; The cookie is replaced by the value of the first named default
; variable that is defined.  If none of those names are defined, the
; cookie is left uneaten.
;
;   <=default>
;
; The magic cookie is replaced by the given default value, the text
; after the "=" and before the ">".
; 
;   <name=default>
;
; Like "<name>", except that if the default variable is not defined, the
; cookie is replaced by its own default value.
;
;   <name1|name2|...=default>
;
; Like "<name=default>", except that the cookie is replaced by the value
; of the first named default variable that is defined, or its own
; default value if none of those names are defined.
;
;   <:mlisp>
;   <name:mlisp>
;   <name1|name2|...:mlisp>
; 
; Like the corresponding cookies with "=", except that the mlisp code
; between the ":" and the ">" is executed to obtain the default value
; with which to replace the magic cookie.
;
; In the case of magic cookies with mlisp defaults, the mlisp is
; executed with the buffer narrowed to the body of the cookie (including
; the "<>"'s), and the region wrapped around the mlisp code being executed.
; 
; The mlisp code can be the name of an mlisp variable, or a call to an
; mlisp function.
; 
; If the mlisp code returns a string value, then the UI compiler
; replaces the cookie with that value.  Otherwise, if the code returns
; "novalue", or encounters an error, the compiler skips over the cookie, 
; uneaten.  A function can compute a string to return to the UI compiler, 
; or make its own changes and return "novalue".  It can replace the
; entire cookie, or edit the cookie's body, or recursivly instantiate 
; another template in place of the cookie, or even save the restrictions, 
; widen the region, and make local editing changes to the surrounding text!


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
	(if (error-occurred (search-forward (concat "\n" &pd-name "=")))
	    ; add new default
	    (insert-string (concat "\n" &pd-name "=" &pd-value))
	    ; else replace old default
	    (progn
		(if (! (looking-at "\n"))
		    (kill-to-end-of-line))
		(insert-string &pd-value)))
    ))
))

; (fetch-default "*defaults-buffer*" "name")
; Return the value of a default, if it exists, otherwise "*undefined*".
(defun (fetch-default &fd-buffer &fd-name
    (setq &fd-buffer (arg 1 ": fetch-default (buffer) "))
    (setq &fd-name (arg 2 ": fetch-default (name) "))
    (save-excursion (temp-use-buffer &fd-buffer
	(beginning-of-file)
	(if (! (looking-at "\n"))
	    (newline-and-backup)) ; Force leading newline
	(if (error-occurred (search-forward (concat "\n" &fd-name "=")))
	    "*undefined*"
	    (progn
		(set-mark)
		(end-of-line)
		(region-to-string)))
    ))
))


; (instantiate-template "*defaults-buffer*")
; Instantiate the template in the current buffer by eating its magic cookies.

(defun (instantiate-template &it-defaults
    &it-name &it-value &it-delim
    (setq &it-defaults (arg 1 ": instantiate-template (defaults buffer) "))
    (save-excursion
	(save-restriction
	    (beginning-of-file)
	    (while (! (error-occurred (search-forward "<")))
		   (if (| (looking-at "<>") ; "<<>" => "<"
			  (looking-at ">>")) ; "<>>" => ">"
		       (progn (delete-previous-character)
			      (forward-character)
			      (delete-next-character))
		       (save-restriction
			     (backward-character)
			     (set-mark)
			     (search-forward ">")
			     (narrow-region)
			     (beginning-of-file)
			     (forward-character)
			     (munch-cookie))
		   )
	    )
	)
    )
))

; (lookup "name") => "value" or "*undefined*"
(defun (lookup
    (fetch-default &it-defaults (arg 1 ": lookup "))
))

; (def "name" "value")
(defun (def
    (put-default &it-defaults
		 (arg 1 ": def (name) ")
		 (arg 2 ": def (value) "))
))

; <name>
; <name1|name2|name3>
; <=default>
; <name=default>
; <name1|name2=default>
; <:mlisp>
; <name:mlisp>
; <name1|name2:mlisp>

(defun (chomp
    (yank-buffer "*template*")
    (instantiate-template "*defaults*")))

(defun (munch-cookie cookie-name cookie-byte cookie-value
    (if (! (looking-at "\\([^>|=:]*\\)\\([>|=:]\\)"))
	(error-message
	    (concat "Choked on cookie " (buffer-to-string)))
	(progn
	    (region-around-match 1)
	    (setq cookie-name (region-to-string))
	    (region-around-match 2)
	    (setq cookie-byte (region-to-string))
	    (if (& (!= cookie-name "")
		   (!= (setq cookie-value (lookup cookie-name))
		       "*undefined*"))
		(progn ; if cookie-value defined
		    (erase-buffer)
		    (insert-string cookie-value)
		    (beginning-of-file) ; For recursive evaluation
		)
		(progn ; if cookie-value undefined
		    (if (= cookie-byte ">")
			(progn ; end of cookie
			    (end-of-file) ; Not tasty. Skip over it
			)
			(= cookie-byte "|")
			(progn ; cookie conjunction
			    (munch-cookie)
			)
			(= cookie-byte "=")
			(progn ; cookie default
			    (set-mark)
			    (beginning-of-file)
			    (erase-region)
			    (end-of-file)
			    (delete-previous-character)
			)
			(= cookie-byte ":")
			(progn ; mlisp cookie
			    (set-mark)
			    (search-forward ">")
			    (backward-character)
			    (if (error-occurred ; (! (is-string-like ...))
				  (string-to-char
				    (setq cookie-value 
				          (execute-mlisp-line
					     (region-to-string)))))
				(end-of-file) ; skip over the cookie
				(progn ; replace it with mlisp return value
				    (erase-buffer)
				    (insert-string cookie-value)
				    (beginning-of-file) ; for recursive eval
				)
			    )
			)
			(error-message
			    (concat "Choked on cookie " (buffer-to-string)))
		    )
		)
	    )
	)
    )
))

; Conditional text line:
; /ItemBorder <border:(zap-line)> def
; If a conditional line is the last in a buffer, it must end with a newline.

(defun (zap-line
    (save-restriction
	(widen-region)
	(beginning-of-line)
	(set-mark)
	(end-of-line)
	(if (looking-at "\n")
	    (forward-character))
	(erase-region)
    )
    (set-mark)
    (narrow-region)
    (novalue)
))

; Conditional text region:
; <HeaderBegin:(zap-to "HeaderEnd")>
; This is the header.
; <HeaderEnd=[header supressed]>
; <BodyBegin:(zap-to "BodyEnd")>
; this is the body.
; <BodyEnd=[body supressed]>

(defun (zap-to &zap-target
    (setq &zap-target (arg 1 ": zap-to "))
    (save-restriction
	    (beginning-of-file)
	    (widen-region)
	    (push-mark)
	    (search-forward (concat "<" &zap-target))
	    (provide-prefix-argument (+ 1 (length &zap-target))
		(backward-character))
	    (erase-region)
    )
    (set-mark)
    (narrow-region)
    (novalue)
))

(defun (compile-panel
    &cp-editing &cp-info-panel &cp-key-number
    &cp-ps-header &cp-ps-template &cp-ps-trailer
    &cp-bindings-header &cp-bindings-template &cp-bindings-trailer
    (message-for 0 (concat "Compiling " &info-current-node "..."))
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
		   (setq &cp-info-panel (region-to-string))
	       )
	))
	(progn
	      (init-panel-defaults)

	      (if (is-bound &cps-key-number) ; compile-panel-set
		  (#with-raw-resource "KeyBase" 1
		      (erase-buffer)
		      (insert-string (setq &cp-key-number &cps-key-number)))
		  (setq &cp-key-number (info-get-resource "KeyBase")))

	      (setq &cp-bindings-header 
		    (info-get-resource "BindingsHeader"))
	      (setq &cp-bindings-template
		    (info-get-resource "BindingsTemplate"))
	      (setq &cp-bindings-trailer
		    (info-get-resource "BindingsTrailer"))
	      (#with-raw-resource "Bindings" 1
		  (erase-buffer)
		  (insert-string
		      &cp-bindings-header "\n"
		      &cp-bindings-template "\n"
		      &cp-bindings-trailer "\n"))

              (setq &cp-ps-header (info-get-resource "PSHeader"))
              (setq &cp-ps-template (info-get-resource "PSTemplate"))
              (setq &cp-ps-trailer (info-get-resource "PSTrailer"))
	      (#with-raw-resource "PS" 1
		  (erase-buffer)
		  (insert-string
		      &cp-ps-header "\n"
		      &cp-ps-template "\n"
		      &cp-ps-trailer "\n")
		  (beginning-of-file)
		  (instantiate-template "*panel-defaults*"))

	      ; Nuke last binding insertion point cookie. bletch. 
	      (#with-raw-resource "Bindings" 1
		  (beginning-of-file)
		  (search-forward "<bindings>")
		  (end-of-line)
		  (set-mark)
		  (beginning-of-line)
		  (if (! (bobp))
		      (backward-character))
		  (erase-region)
	      )
	      (if (is-bound &cps-key-number) ; compile-panel-set
		  (setq &cps-key-number &cp-key-number))
	)
    )
    (if (! &cp-editing)
	(#info-exit-edit)
	(point-save))
))

(defun (load-panel
  (execute-ps-string (resource-to-string "PS"))
  (execute-resource "Bindings")
))

(defun (compile-panel-body &panel-body
   &template-name &template &item-name &item-action &item-arg
   (setq &panel-body (arg 1 ": compile-panel-body "))
   (setq &template-name "")
   (erase-buffer)
   (insert-string &panel-body)
   (beginning-of-file)
   (gobble-defaults "*panel-defaults*")
   (while (! (eobp))
	  (if (| (looking-at "\n")
		 (looking-at "^[#;%]"))
	      ; skip blank lines & comments
	      (progn (end-of-line)
		     (forward-character))
	      (looking-at "^[^\t *]*$")
	      ; TemplateName
	      (progn
		    (region-around-match 0)
		    (setq &template-name (region-to-string))
		    (erase-region)
		    (delete-next-character)
; (popmsg (concat "Template " &template-name))
		    (setq &template
			  (info-get-resource (concat &template-name
						     "Template")))
		    (if (error-occurred (string-to-char &template))
			(setq &template-name "")) ; if not found
		    (init-template-defaults)
		    (gobble-defaults "*template-defaults*")
	      )
	      (looking-at "^\*[\t ]*\\([^:]*\\):[\t ]*\\([^:]*\\):[\t ]*\\(.*\\)$")
	      ; * name: action: arg
	      (if (= &template-name "")
		  (error-message "Panel item encountered before template name!")
		  (progn
		     (region-around-match 1)
		     (setq &item-name (region-to-string))
		     (region-around-match 2)
		     (setq &item-action (region-to-string))
		     (region-around-match 3)
		     (setq &item-arg (region-to-string))
		     (region-around-match 0)
		     (erase-region)
		     (delete-next-character)
; (popmsg (concat "Item " &item-name " " &item-action " " &item-arg))
		     (init-defaults)
		     (gobble-defaults "*defaults*")
		     (make-action-defaults)
		     (save-restriction
			 (set-mark)
			 (narrow-region)
			 (insert-string &template "\n")
		         (instantiate-template "*defaults*")
			 (end-of-file))
		  )
	      )
	      (progn (message "Unknown stuff!") 
		     (recursive-edit))
	  )
   )
   (novalue)
))

(defun (gobble-defaults &gd-defaults
    &gd-name &gd-value
    (setq &gd-defaults (arg 1 ": gobble-defaults "))
    (while (looking-at "^[\t ]+\\([^:]*\\):[\t ]*\\(.*\\)$")
	(progn (region-around-match 1)
	       (setq &gd-name (region-to-string))
	       (region-around-match 2)
	       (setq &gd-value (region-to-string))
	       (region-around-match 0)
	       (erase-region)
	       (delete-next-character)
	       (put-default &gd-defaults &gd-name &gd-value)))
))

(defun (init-panel-defaults &panel-defaults
    (setq &panel-defaults (info-get-resource "PanelDefaults"))
    (temp-use-buffer "*panel-defaults*"
	(erase-buffer)
	(insert-string &panel-defaults))
    (put-default "*panel-defaults*"
	"panel-body" "<:(compile-panel-body &cp-info-panel)>") ; magic
;    (init-template-defaults)
))

(defun (init-template-defaults
    (temp-use-buffer "*template-defaults*"
	(erase-buffer)
	(yank-buffer "*panel-defaults*"))
;    (init-defaults)
))

(defun (init-defaults
    (temp-use-buffer "*defaults*"
	(erase-buffer)
	(yank-buffer "*template-defaults*"))
))

(defun (make-action-defaults
    (put-default "*defaults*"
	"name" &item-name)
    (put-default "*defaults*"
	"action" &item-action)
    (put-default "*defaults*"
	"arg" &item-arg)
    (if (= &item-action "literal")
	(compile-panel-literal)
	(= &item-action "bind")
	(compile-panel-binding)
	(= &item-action "call")
	(compile-panel-call)
	(= &item-action "type")
	(compile-panel-type)
	(= &item-action "popmsg")
	(compile-panel-popmsg)
	(= &item-action "menu")
	(compile-panel-menu)
	(error-message (concat "Unrecognized action: " &item-action)))
))

(defun (compile-panel-literal
    (put-default "*defaults*"
	"notify" &item-arg)))

(defun (compile-panel-binding
    &cpb-prefix
    (put-default "*defaults*"
	"keycode" (concat &cp-key-number))
    (put-default "*defaults*"
	"notify" (concat "{ ItemNotifyMsg menustring }"))
    ; make the mlisp binding! bletch.
    (save-excursion 
	(#with-raw-resource "Bindings" 1
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
				   ":" &item-name "\"\n"
				   &cpb-prefix "    \"" &item-arg "\")\n"))
	)
    )
    (++ &cp-key-number)
))

(defun (compile-panel-call
    (put-default "*defaults*"
	"notify" (concat "{ " &item-arg " }"))
))

(defun (compile-panel-menu
    (put-default "*defaults*"
        "notify"
	(concat "{ gsave framebuffer setcanvas "
		"    currentcursorlocation /showat " &item-arg " send "
		"  grestore }"))
))

(defun (compile-panel-type
    (put-default "*defaults*"
        "notify" (concat "{ (" &item-arg ") menustring }"))
))

(defun (compile-panel-popmsg
    (put-default "*defaults*"
        "notify" (concat "{ gsave framebuffer setcanvas"
			 "    currentcursorlocation [(" &item-arg ")] popmsg"
			 "  pop grestore }"))
))

