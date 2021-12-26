\ SPDX-License-Identifier: LGPL-3.0-or-later

BUILTIN-WORDS
INCLUDE core-words.fs
LATEST @
INCLUDE debug-words.fs
LATEST @
INCLUDE file-words.fs
LATEST @
INCLUDE tools-words.fs
LATEST @

( At this point the data stack has name tokens for all the new words
  stacked up in reverse order:

      builtin core debug tools

  We work back through this a pair at a time generating the code needed
  to have a complete run-from-ROM sytem.
)

EXUDE tools-words.h
." // Auto-generated from tools-words.fs" CR
." #ifndef RF_TOOLS_WORDS_H_" CR
." #define RF_TOOLS_WORDS_H_" CR
2DUP LIST>ROM DROP
." #endif /* RF_TOOLS_WORDS_H_*/" CR

EXUDE file-words.h
." // Auto-generated from file-words.fs" CR
." #ifndef RF_FILE_WORDS_H_" CR
." #define RF_FILE_WORDS_H_" CR
2DUP LIST>ROM DROP
." #endif /* RF_FILE_WORDS_H_*/" CR

EXUDE debug-words.h
." // Auto-generated from debug-words.fs" CR
." #ifndef RF_DEBUG_WORDS_H_" CR
." #define RF_DEBUG_WORDS_H_" CR
2DUP LIST>ROM DROP
." #endif /* RF_DEBUG_WORDS_H_*/" CR

EXUDE core-words.h
." // Auto-generated from core.fs" CR
." #ifndef RF_CORE_WORDS_H_" CR
." #define RF_CORE_WORDS_H_" CR
LIST>ROM
." #endif /* RF_CORE_WORDS_H_*/" CR

( Generate Raspberry Pi RP2 words.

  First we must define some of the RP2 native words otherwise they will be
  missing words in the forth code!
)
: LFS ;
: LFS_INFO_SIZE ;
: LFS_INFO_NAME ;
: LFS_DIR_SIZE ;
: LFS_FILE_SIZE ;
: LFS_O_RDONLY 1 ;
: LFS_O_WRONLY 2 ;
: LFS_O_RDWR 3 ;
: LFS_O_CREAT 256 ;
: LFS_O_EXCL 512 ;
: LFS_O_TRUNC 1024 ;
: LFS_O_APPEND 2048 ;
: FN-lfs_file_open ;
: FN-lfs_file_close ;
: FN-lfs_file_read ;
: FN-lfs_file_write ;
: FN-lfs_dir_open ;
: FN-lfs_dir_close ;
: FN-lfs_dir_read ;

LATEST @
INCLUDE rp2-file-words.fs
LATEST @

EXUDE rp2-file-words.h
." // Auto-generated from rp2-file-words.fs" CR
." #ifndef RF_RP2_FILE_WORDS_H_" CR
." #define RF_RP2_FILE_WORDS_H_" CR
LIST>ROM
." #endif /* RF_RP2_FILE_WORDS_H_*/" CR

( Forget all the RP2 words! )
FORGET LFS

( Generate Unix words.

  First we must define some of the Unix native words otherwise they will be
  missing words in the forth code!
)
: O_RDONLY 1 ;
: O_WRONLY 2 ;
: O_RDWR 3 ;
: O_CREAT 256 ;
: O_EXCL 512 ;
: O_TRUNC 1024 ;
: O_APPEND 2048 ;
: FN-open ;
: FN-close ;
: FN-read ;
: FN-unlink ;
: FN-write ;
: FN-opendir ;
: FN-closedir ;
: FN-readdir ;
: DIRENT_D_NAME ;

( the XXX-file-words.fs family of libraries redefine INCLUDE and we don't want
  to use the new definition during codegen so we must provide an alternative
)
: #INCLUDE INCLUDE ;

LATEST @
#INCLUDE unix-file-words.fs
LATEST @
#INCLUDE shell-words.fs
LATEST @

EXUDE shell-words.h
." // Auto-generated from shell-words.fs" CR
." #ifndef RF_SHELL_WORDS_H_" CR
." #define RF_SHELL_WORDS_H_" CR
2DUP LIST>ROM DROP
." #endif /* RF_SHELL_WORDS_H_*/" CR

EXUDE unix-file-words.h
." // Auto-generated from unix-file-words.fs" CR
." #ifndef RF_UNIX_FILE_WORDS_H_" CR
." #define RF_UNIX_FILE_WORDS_H_" CR
LIST>ROM
." #endif /* RF_UNIX_FILE_WORDS_H_*/" CR

( Forget all the Unix words! )
FORGET O_RDONLY
