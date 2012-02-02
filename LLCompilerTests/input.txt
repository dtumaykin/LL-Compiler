(defun f (x)
    (if (> x 1) (* x (f (- x 1))) 1 ))

(defun f1 (x) x)