CC = gcc
CFLAGS = -Wall -Werror -g

ifdef NO_OPT
CFLAGS += -O0
else
CFLAGS += -O2
endif

all : redforth

OBJS = libred.o main.o vm-gnuc.o
$(OBJS) : $(wildcard *.h)

redforth : $(OBJS)
	$(CC) $(CFLAGS) -o $@ $(OBJS)

clean :
	$(RM) redforth debug.fs filetest.txt $(OBJS)

check : redforth
	cat core.fs tools.fs eyeball.fs | ./redforth | uniq -u > eyeball.log
	[ `cat eyeball.log | wc -l` -eq 2 ] || (cat eyeball.log; false)
	cat core.fs tools.fs selftest.fs | ./redforth
	@$(RM) filetest.txt eyeball.log

debug : redforth
	cat core.fs tools.fs selftest.fs > debug.fs
	gdb redforth -ex "break rf_forth_exec" -ex "run < debug.fs"
