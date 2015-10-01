define hook-quit
    set confirm off
end
set listsize 10
set filename-display basename
set target-charset ASCII
set output-radix 16 
set print address  off
set print pretty on
set print symbol off 
set print symbol-filename off
set  disassemble-next-line on
br DebugLogPrint
commands
silent
printf "DebugLog: %s\n", message
cont
end

