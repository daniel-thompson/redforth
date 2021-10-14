CC = gcc
CFLAGS = -Wall -Werror -g

ifdef NO_OPT
CFLAGS += -O0
else
CFLAGS += -O2
endif

all : redforth

SRCS = libred.c main.c vm-gnuc.c
CROSS_OBJS = $(patsubst %.c,build-cross/%.o,$(SRCS))

crossforth : build-cross $(CROSS_OBJS)
	$(CC) $(CFLAGS) -o $@ $(CROSS_OBJS)

build-cross/%.o : %.c
	$(CC) $(CFLAGS) -c -o $@ $<

OBJS = $(patsubst %.c,build/%.o,$(SRCS))

redforth : build $(OBJS)
	$(CROSS_COMPILE)$(CC) $(CFLAGS) -o $@ $(OBJS)

build/%.o : %.c
	$(CROSS_COMPILE)$(CC) $(CFLAGS) -DHAVE_CODEGEN_WORDS -c -o $@ $<

words-codegen.h : crossforth
	echo '/*' > words-codegen.h
	cat core.fs tools.fs codegen.fs | ./crossforth >> words-codegen.h

build build-cross :
	mkdir $@

clean :
	$(RM) -r build-cross/ build/
	$(RM) redforth crossforth words-codegen.h debug.fs filetest.txt

eyeball : redforth
	cat eyeball.fs | ./redforth

cross-check : crossforth
	cat core.fs tools.fs eyeball.fs | ./crossforth | uniq -u > eyeball.log
	-[ `cat eyeball.log | wc -l` -eq 2 ] || (cat eyeball.log; false)
	cat core.fs tools.fs selftest.fs | ./crossforth
	@$(RM) filetest.txt eyeball.log

check : redforth
	cat eyeball.fs | ./redforth | uniq -u > eyeball.log
	[ `cat eyeball.log | wc -l` -eq 1 ] || (cat eyeball.log; false)
	cat selftest.fs | ./redforth
	@$(RM) filetest.txt eyeball.log

debug : redforth
	cat core.fs tools.fs selftest.fs > debug.fs
	gdb redforth -ex "break rf_forth_exec" -ex "run < debug.fs"

$(CROSS_OBJS) $(OBJS) : Makefile $(wildcard *.h)
build/vm-gnuc.o : words-codegen.h
