\ SPDX-License-Identifier: LGPL-3.0-or-later

BUILTIN-WORDS
INCLUDE core-words.fs
LATEST @
INCLUDE tools-words.fs
LATEST @

( At this point the data stack has name tokens for all the new words
  stacked up in reverse order:

      builtin core tools

  We work back through this a pair at a time generating the code needed
  to have a complete run-from-ROM sytem.
)

EXUDE tools-words.h
." // Auto-generated from tools.fs" CR
." #ifndef RF_WORDS_TOOLS_H_" CR
." #define RF_WORDS_TOOLS_H_" CR
2DUP LIST>ROM DROP
." #endif /* RF_WORDS_TOOLS_H_*/" CR

EXUDE core-words.h
." // Auto-generated from core.fs" CR
." #ifndef RF_WORDS_CORE_H_" CR
." #define RF_WORDS_CORE_H_" CR
LIST>ROM
." #endif /* RF_WORDS_CORE_H_*/" CR


